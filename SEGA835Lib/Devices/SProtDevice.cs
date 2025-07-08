using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Lib.Devices {

    /// <summary>
    /// The base class for any device that uses a serial connection with <see cref="SProtSerial"/>.
    /// </summary>
    public abstract class SProtDevice : Device {

        /// <summary>
        /// The COM port being used.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The object used to synchronize Serial reads and writes. You must use this in your implementation of <see cref="Read(out SProtFrame)"/> and <see cref="Write(SProtFrame)"/> to stay thread-safe.
        /// </summary>
        protected object SerialLocker { get; private set; } = new object();

        internal readonly SProtSerial serial;

        /// <summary>
        /// Creates a new SProtDevice.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        /// <param name="baudrate">The baudrate to use.</param>
        public SProtDevice(int port, int baudrate) : this(new SProtSerial(port, baudrate)) {
        }

        /// <summary>
        /// Creates a new SProtDevice.
        /// </summary>
        /// <param name="serial">The SProtSerial object to use for communication.</param>
        public SProtDevice(SProtSerial serial) {
            this.serial = serial;
            Port = serial.Port;
        }

        /// <summary>
        /// Writes the given frame to the device. This call may block.
        /// </summary>
        /// <param name="send">The frame to send.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the bytes were successfully written.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if "Connect" on the device was never called.<br />
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or "Disconnect" was called while this call was waiting.<br />
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if a timeout occurred during writing.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public abstract DeviceStatus Write(SProtFrame send);

        /// <summary>
        /// Reads a frame from the device. This call may block.
        /// </summary>
        /// <param name="recv">The frame that was received.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the frame was successfully read.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if "Connect" on the device was never called.<br />
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or "Disconnect" was called while this call was waiting.<br />
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if a timeout occurred during writing.<br />
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public abstract DeviceStatus Read(out SProtFrame recv);

        /// <summary>
        /// Writes the given frame to the device and then immediately reads a response.
        /// This call may block.
        /// </summary>
        /// <seealso cref="Read(out SProtFrame)"/>
        /// <seealso cref="Write(SProtFrame)"/>
        /// <param name="send">The frame to send.</param>
        /// <param name="recv">The frame that was received in response, or null if an error occurred.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the object was successfully sent and received.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if "Connect" was never called.<br />
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or "Disconnect" was called while this call was waiting.<br />
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.<br />
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndRead(SProtFrame send, out SProtFrame recv) {
            lock (SerialLocker) {
                DeviceStatus ret = Write(send);
                if (ret != DeviceStatus.OK) {
                    recv = null;
                    return ret;
                }
                return Read(out recv);
            }
        }

        /// <summary>
        /// Writes the given <see cref="SProtPayload"/> to the device and then immediately reads a response.
        /// This call may block.
        /// </summary>
        /// <seealso cref="Read(out SProtFrame)"/>
        /// <seealso cref="Write(SProtFrame)"/>
        /// <typeparam name="In">The <see cref="SProtPayload"/> struct to be written.</typeparam>
        /// <typeparam name="Out">The <see cref="SProtPayload"/> struct to be read.</typeparam>
        /// <param name="send">The object to send.</param>
        /// <param name="recv">The object that was received in response, or null if an error occurred.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the object was successfully sent and received.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if "Connect" was never called.<br />
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or "Disconnect" was called while this call was waiting.<br />
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.<br />
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndRead<In, Out>(In send, out Out recv) where In : struct, SProtPayload where Out : struct, SProtPayload {
            return WriteAndRead(send, out recv, out byte _, 0);
        }

        /// <summary>
        /// Writes the given <see cref="SProtPayload"/> to the device and then immediately reads a response.
        /// This call may block.
        /// </summary>
        /// <seealso cref="Read(out SProtFrame)"/>
        /// <seealso cref="Write(SProtFrame)"/>
        /// <typeparam name="In">The <see cref="SProtPayload"/> struct to be written.</typeparam>
        /// <typeparam name="Out">The <see cref="SProtPayload"/> struct to be read.</typeparam>
        /// <param name="send">The object to send.</param>
        /// <param name="recv">The object that was received in response, or null if an error occurred.</param>
        /// <param name="status">The device status code received in the response. Non-zero indicates error. This is independent from the return code, as the device itself may return different status codes.</param>
        /// <param name="addr">The bus address of the device to communicate with. This is only used for very specific SProt devices, ignored otherwise.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the object was successfully sent and received.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if "Connect" was never called.<br />
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or "Disconnect" was called while this call was waiting.<br />
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="SerialComm.Timeout"/> ms.<br />
        /// <see cref="DeviceStatus.ERR_CHECKSUM"/> if data verification fails.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public DeviceStatus WriteAndRead<In, Out>(In send, out Out recv, out byte status, byte addr = 0x0) where In : struct, SProtPayload where Out : struct, SProtPayload {
            lock (SerialLocker) {
                DeviceStatus ret = Write(new SProtFrame(send, addr));
                if (ret != DeviceStatus.OK) {
                    status = 0;
                    recv = default;
                    return ret;
                }
                ret = Read(out SProtFrame recv_frame);
                if (ret != DeviceStatus.OK) {
                    recv = default;
                    status = 0;
                    return ret;
                }
                recv = StructUtils.FromBytes<Out>(recv_frame.Payload);
                status = recv_frame.Status;
                return ret;
            }
        }

    }
}
