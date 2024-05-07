using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Serial {

    /// <summary>
    /// A serial device that uses "SProt" (Sega-Protocol).
    /// </summary>
    /// <remarks>
    /// The name is not official, it's the 0xE0 JVS-like protocol that many SEGA boards use.
    /// Note that if any of the Read/Write commands fail, the device may be in an inconsistent state, therefore a hard reset (<see cref="SerialComm.Disconnect"/> + <see cref="SerialComm.Connect"/> is highly recommended.
    /// </remarks>
    public class SProtSerial : SerialComm {

        /// <summary>
        /// The constant synchronization byte. All packets start with this constant.
        /// </summary>
        public const byte SYNC_BYTE = 0xE0;
        /// <summary>
        /// The byte used to escape the synchronization or other escape bytes.
        /// </summary>
        private const byte ESCAPE_BYTE = 0xD0;

        /// <summary>
        /// Whether read and written bytes should be printed to the <see cref="Log"/>.
        /// </summary>
        public bool DumpBytesToLog { get; set; }

        /// <inheritdoc/>
        public SProtSerial(int portNumber, int baudrate = 115200, int timeout = 1000, bool dtr = false, bool rts = false, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake flowControl = Handshake.None) : base(portNumber, baudrate, timeout, dtr, rts, parity, dataBits, stopBits, flowControl) {
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(int len, out byte[] data) {
            if (DumpRWCommandsToLog) {
                Log.Write("SProtSerial Port " + Port + ", Read Len=" + len);
            }
            int pos = 0;
            List<byte> bytes = new List<byte>();
            data = null;
            int checksum = 0;
            bool escapeFlag = false;
            DeviceStatus ret = DeviceStatus.OK;
            while (pos < len) {
                ret = base.ReadByte(out byte b);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
                if (pos == 0 && b != SYNC_BYTE) {
                    Log.WriteError("SProtSerial Read failed, expected sync byte, got " + b);
                    return DeviceStatus.ERR_CHECKSUM;
                }
                if (b == ESCAPE_BYTE) {
                    escapeFlag = true;
                } else {
                    if (escapeFlag) {
                        bytes.Add((byte)(b + 1));
                        checksum += 1;
                        escapeFlag = false;
                    } else {
                        bytes.Add(b);
                    }
                    pos++;
                }
                if (pos > 1 && pos < len && !escapeFlag) { // don't add sync and checksum byte
                    checksum += b;
                }
            }

            data = bytes.ToArray();

            if (DumpBytesToLog) {
                Log.Dump(data, "SProtSerial Read:");
            }

            checksum %= 0x100;
            byte data_checksum = data[data.Length - 1];
            if (checksum != data_checksum) {
                Log.WriteError("SProtSerial Read failed, checksum mismatch, expected " + data_checksum + ", got " + checksum);
                ret = DeviceStatus.ERR_CHECKSUM;
            }

            return ret;
        }

        /// <summary>
        /// Reads a number of bytes from the device, where the data length is part of the data. All bytes preceding the length byte are also read and returned.
        /// This call may block up to <see cref="SerialComm.Timeout"/> ms.
        /// </summary>
        /// <param name="lenByteOffset">
        /// The offset where the number of bytes to be read from the input data can be found within the input data.
        /// For example if the data[3] in the response data is the length byte, lenByteOffset should be 3.
        /// </param>
        /// <param name="data">The bytes that were read or null any error occurred.</param>
        /// <param name="lenIncludesSelf">Whether or not the length byte inside the data includes itself in the length.</param>
        /// <param name="lenIncludesChecksumByte">Whether or not the length byte inside the data includes the trailing checksum byte in the length.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="SerialComm.Connect"/> was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or <see cref="SerialComm.Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        /// <exception cref="ArgumentException">if the byte at lenByteOffset minus the bytes read up to this point is negative</exception>
        public DeviceStatus ReadLenByOffset(int lenByteOffset, out byte[] data, bool lenIncludesSelf = false, bool lenIncludesChecksumByte = false) {
            if (DumpRWCommandsToLog) {
                Log.Write("SProtSerial Port " + Port + ", Read Len By Offset=" + lenByteOffset);
            }
            int pos = 0;
            int? len = null;
            List<byte> bytes = new List<byte>();
            data = null;
            int checksum = 0;
            bool escapeFlag = false;
            DeviceStatus ret = DeviceStatus.OK;
            while (len == null) {
                ret = base.ReadByte(out byte b);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
                if (pos == 0 && b != SYNC_BYTE) {
                    Log.WriteError("SProtSerial ReadLenByOffset failed, expected sync byte, got " + b);
                    return DeviceStatus.ERR_CHECKSUM;
                }
                if (b == ESCAPE_BYTE) {
                    escapeFlag = true;
                } else {
                    if (escapeFlag) {
                        bytes.Add((byte)(b + 1));
                        checksum += 1;
                        escapeFlag = false;
                    } else {
                        bytes.Add(b);
                    }
                    if (pos++ == lenByteOffset) {
                        len = b; // checksum byte
                    }
                }
                if (pos > 1 && !escapeFlag) { // don't add sync byte
                    checksum += b;
                }
            }
            pos = lenIncludesSelf ? 1 : 0;
            len += lenIncludesChecksumByte ? 0 : 1;
            if (DumpRWCommandsToLog) {
                Log.Write("SProtSerial Port " + Port + ", Read Len Remaining=" + (len - pos));
            }
            if (len - pos < 0) {
                throw new ArgumentException("Bytes to read from stream are negative (len: " + len + ", pos: " + pos + ")");
            }
            while (pos < len) {
                ret = base.ReadByte(out byte b);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
                if (b == ESCAPE_BYTE) {
                    escapeFlag = true;
                } else {
                    if (escapeFlag) {
                        bytes.Add((byte)(b + 1));
                        checksum += 1;
                        escapeFlag = false;
                    } else {
                        bytes.Add(b);
                    }
                    pos++;
                }
                if (pos < len && !escapeFlag) { // don't add sync and checksum byte
                    checksum += b;
                }
            }

            checksum %= 0x100;

            data = bytes.ToArray();
            if (DumpBytesToLog) {
                Log.Dump(data, "SProtSerial Read:");
            }

            byte data_checksum = data[data.Length - 1];
            if (checksum != data_checksum) {
                Log.WriteError("SProtSerial ReadLenByOffset failed, checksum mismatch, expected " + data_checksum + ", got " + checksum);
                ret = DeviceStatus.ERR_CHECKSUM;
            }

            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus Write(byte[] data) {
            List<byte> bytes = new List<byte>();
            bytes.Add(SYNC_BYTE);
            int checksum = 0;
            foreach (byte b in data) {
                if (b == ESCAPE_BYTE || b == SYNC_BYTE) {
                    bytes.Add(ESCAPE_BYTE);
                    bytes.Add((byte)(b - 1));
                } else {
                    bytes.Add(b);
                }
                checksum += b;
            }
            bytes.Add((byte)(checksum % 0x100));
            byte[] encoded = bytes.ToArray();
            if (DumpBytesToLog) {
                Log.Dump(encoded, "SProtSerial Write:");
            }
            return base.Write(encoded);
        }

        /// <summary>
        /// Writes the given bytes to the device and then immediately reads a response.
        /// This call may block up to <see cref="SerialComm.Timeout"/> ms.
        /// </summary>
        /// <seealso cref="Read(int, out byte[])"/>
        /// <seealso cref="Write(byte[])"/>
        /// <param name="send">The data to send.</param>
        /// <param name="recvLen">The amount of bytes to read.</param>
        /// <param name="recv">The bytes that were received or null if any error occurred.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="SerialComm.Connect"/> was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or <see cref="SerialComm.Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndRead(byte[] send, int recvLen, out byte[] recv) {
            DeviceStatus ret = Write(send);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            return Read(recvLen, out recv);
        }

        /// <summary>
        /// Writes the given bytes to the device and then immediately reads a number of bytes from the device, where the data length is part of the data. All bytes preceding the length byte are also read and returned.
        /// This call may block up to <see cref="SerialComm.Timeout"/> ms.
        /// </summary>
        /// <seealso cref="ReadLenByOffset(int, out byte[], bool, bool)"/>
        /// <seealso cref="Write(byte[])"/>
        /// <param name="send">The data to send.</param>
        /// <param name="lenByteOffset">
        /// The offset where the number of bytes to be read from the input data can be found within the input data.
        /// For example if the data[3] in the response data is the length byte, lenByteOffset should be 3.
        /// </param>
        /// <param name="recv">The bytes that were received or null if any error occurred.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="SerialComm.Connect"/> was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or <see cref="SerialComm.Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndReadByOffset(byte[] send, int lenByteOffset, out byte[] recv) {
            DeviceStatus ret = Write(send);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            return ReadLenByOffset(lenByteOffset, out recv);
        }
    }
}
