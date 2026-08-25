using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    /// <summary>
    /// A serial device that uses "SProt" (Sega-Protocol).
    /// </summary>
    /// <remarks>
    /// The name is not official, it's the 0xE0 JVS-like protocol that many SEGA boards use.
    /// Note that if any of the Read/Write commands fail, the device may be in an inconsistent state, therefore a hard reset (<see cref="SerialComm.Disconnect"/> + <see cref="SerialComm.Connect"/> is highly recommended.
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SProtSerial : SerialComm {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(SProtSerial));

        /// <summary>
        /// The constant synchronization byte. All packets start with this constant.
        /// </summary>
        public const byte SYNC_BYTE = 0xE0;

        /// <summary>
        /// The byte used to escape the synchronization or other escape bytes.
        /// </summary>
        private const byte ESCAPE_BYTE = 0xD0;

        /// <summary>
        /// Whether read and written bytes should be printed to the log.
        /// </summary>
        public bool DumpBytesToLog { get; set; }

        private readonly object locker = new object();

        /// <inheritdoc/>
        public SProtSerial(int portNumber, int baudrate = 115200, int timeout = 1000, bool dtr = false, bool rts = false, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake flowControl = Handshake.None) : base(portNumber, baudrate, timeout, dtr, rts, parity, dataBits, stopBits, flowControl) {
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(int len, out byte[] data) {
            lock (locker) {
                if (DumpReadWriteCommandsToLog) {
                    LOG.LogInformation("SProtSerial Port " + Port + ", Read Len=" + len);
                }

                int pos = 0;
                List<byte> bytes = new List<byte>();
                data = null;
                int checksum = 0;
                bool escapeFlag = false;
                DeviceStatus ret = DeviceStatus.Ok;
                while (pos < len) {
                    ret = base.ReadByte(out byte b);
                    if (ret != DeviceStatus.Ok) {
                        return ret;
                    }

                    if (pos == 0 && b != SYNC_BYTE) {
                        LOG.LogError("SProtSerial Read failed, expected sync byte, got " + b);
                        return DeviceStatus.ErrorChecksum;
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
                    LOG.LogInformation("SProtSerial Read:");
                    LOG.LogInformation(Hex.Dump(data));
                }

                checksum %= 0x100;
                byte dataChecksum = data[data.Length - 1];
                if (checksum != dataChecksum) {
                    LOG.LogError("SProtSerial Read failed, checksum mismatch, expected " + dataChecksum + ", got " + checksum);
                    ret = DeviceStatus.ErrorChecksum;
                }

                return ret;
            }
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
        /// <see cref="DeviceStatus.Ok"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="SerialComm.Connect"/> was never called.
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the device is not/no longer connected, the thread was interrupted or <see cref="SerialComm.Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ErrorTimeout"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.
        /// <see cref="DeviceStatus.ErrorChecksum"/> if data verification fails.
        /// <see cref="DeviceStatus.ErrorOther"/> if an exception occurred.
        /// </returns>
        /// <exception cref="ArgumentException">if the byte at lenByteOffset minus the bytes read up to this point is negative</exception>
        public DeviceStatus ReadLenByOffset(int lenByteOffset, out byte[] data, bool lenIncludesSelf = false, bool lenIncludesChecksumByte = false) {
            lock (locker) {
                if (DumpReadWriteCommandsToLog) {
                    LOG.LogInformation("SProtSerial Port " + Port + ", Read Len By Offset=" + lenByteOffset);
                }

                int pos = 0;
                int? len = null;
                List<byte> bytes = new List<byte>();
                data = null;
                int checksum = 0;
                bool escapeFlag = false;
                DeviceStatus ret = DeviceStatus.Ok;
                while (len == null) {
                    ret = base.ReadByte(out byte b);
                    if (ret != DeviceStatus.Ok) {
                        if (DumpReadWriteCommandsToLog) {
                            LOG.LogInformation("Error occurred after reading " + pos);
                        }

                        return ret;
                    }

                    if (pos == 0 && b != SYNC_BYTE) {
                        LOG.LogError("SProtSerial ReadLenByOffset failed, expected sync byte, got " + b);
                        return DeviceStatus.ErrorChecksum;
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
                if (DumpReadWriteCommandsToLog) {
                    LOG.LogInformation("SProtSerial Port " + Port + ", Read Len Remaining=" + (len - pos));
                }

                if (len - pos < 0) {
                    throw new ArgumentException("Bytes to read from stream are negative (len: " + len + ", pos: " + pos + ")");
                }

                while (pos < len) {
                    ret = base.ReadByte(out byte b);
                    if (ret != DeviceStatus.Ok) {
                        return ret;
                    }

                    if (b == ESCAPE_BYTE && pos + 1 < len) { // do not unescape the checksum byte
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
                    LOG.LogInformation("SProtSerial Read:");
                    LOG.LogInformation(Hex.Dump(data));
                }

                byte dataChecksum = data[data.Length - 1];
                if (checksum != dataChecksum) {
                    LOG.LogError("SProtSerial ReadLenByOffset failed, checksum mismatch, expected " + dataChecksum + ", got " + checksum);
                    ret = DeviceStatus.ErrorChecksum;
                }

                return ret;
            }
        }

        /// <inheritdoc/>
        public override DeviceStatus Write(byte[] data) {
            lock (locker) {
                List<byte> bytes = new List<byte> {
                    SYNC_BYTE
                };
                int checksum = 0;
                foreach (byte b in data) {
                    AppendSingleByte(bytes, b);
                    checksum += b;
                }

                AppendSingleByte(bytes, (byte)(checksum % 0x100));

                byte[] encoded = bytes.ToArray();
                if (DumpBytesToLog) {
                    LOG.LogInformation("SProtSerial Write:");
                    LOG.LogInformation(Hex.Dump(encoded));
                }

                return base.Write(encoded);
            }
        }

#if !NET35
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void AppendSingleByte(List<byte> bytes, byte b) {
            if (b == ESCAPE_BYTE || b == SYNC_BYTE) {
                bytes.Add(ESCAPE_BYTE);
                bytes.Add((byte)(b - 1));
            } else {
                bytes.Add(b);
            }
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
        /// <see cref="DeviceStatus.Ok"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="SerialComm.Connect"/> was never called.
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the device is not/no longer connected, the thread was interrupted or <see cref="SerialComm.Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ErrorTimeout"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.
        /// <see cref="DeviceStatus.ErrorChecksum"/> if data verification fails.
        /// <see cref="DeviceStatus.ErrorOther"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndRead(byte[] send, int recvLen, out byte[] recv) {
            lock (locker) {
                DeviceStatus ret = Write(send);
                if (ret != DeviceStatus.Ok) {
                    recv = null;
                    return ret;
                }

                return Read(recvLen, out recv);
            }
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
        /// <see cref="DeviceStatus.Ok"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="SerialComm.Connect"/> was never called.
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the device is not/no longer connected, the thread was interrupted or <see cref="SerialComm.Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ErrorTimeout"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.
        /// <see cref="DeviceStatus.ErrorChecksum"/> if data verification fails.
        /// <see cref="DeviceStatus.ErrorOther"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndReadByOffset(byte[] send, int lenByteOffset, out byte[] recv) {
            lock (locker) {
                DeviceStatus ret = Write(send);
                if (ret != DeviceStatus.Ok) {
                    recv = null;
                    return ret;
                }

                return ReadLenByOffset(lenByteOffset, out recv);
            }
        }
    }
}