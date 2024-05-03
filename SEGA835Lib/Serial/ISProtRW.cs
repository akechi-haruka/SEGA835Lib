using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Misc;


namespace Haruka.Arcade.SEGA835Lib.Serial {

    /// <summary>
    /// An interface that declares that this device can read and write <see cref="SProtFrame"/> objects.
    /// This usually means the implementing class has an <see cref="SProtSerial"/> device that can receive and send those frames.
    /// </summary>
    /// <seealso cref="SProtSerial"/>
    public interface ISProtRW {

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
        public DeviceStatus Write(SProtFrame send);

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
        public DeviceStatus Read(out SProtFrame recv);

    }

    /// <summary>
    /// Miscellancelous helper methods for reading and writing <see cref="SProtFrame"/> objects.
    /// </summary>
    public static class IFrameRWExtensions {

        /// <summary>
        /// Writes the given frame to the device and then immediately reads a response.
        /// This call may block.
        /// </summary>
        /// <seealso cref="ISProtRW.Read(out SProtFrame)"/>
        /// <seealso cref="ISProtRW.Write(SProtFrame)"/>
        /// <param name="frame">The current frame object.</param>
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
        public static DeviceStatus WriteAndRead(this ISProtRW frame, SProtFrame send, out SProtFrame recv) {
            DeviceStatus ret = frame.Write(send);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            return frame.Read(out recv);
        }

        /// <summary>
        /// Writes the given <see cref="SProtPayload"/> to the device and then immediately reads a response.
        /// This call may block.
        /// </summary>
        /// <seealso cref="ISProtRW.Read(out SProtFrame)"/>
        /// <seealso cref="ISProtRW.Write(SProtFrame)"/>
        /// <typeparam name="In">The <see cref="SProtPayload"/> struct to be written.</typeparam>
        /// <typeparam name="Out">The <see cref="SProtPayload"/> struct to be read.</typeparam>
        /// <param name="frame">The current frame object.</param>
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
        public static DeviceStatus WriteAndRead<In, Out>(this ISProtRW frame, In send, out Out recv) where In : struct, SProtPayload where Out : struct, SProtPayload {
            return WriteAndRead(frame, send, out recv, out byte _, 0);
        }

        /// <summary>
        /// Writes the given <see cref="SProtPayload"/> to the device and then immediately reads a response.
        /// This call may block.
        /// </summary>
        /// <seealso cref="ISProtRW.Read(out SProtFrame)"/>
        /// <seealso cref="ISProtRW.Write(SProtFrame)"/>
        /// <typeparam name="In">The <see cref="SProtPayload"/> struct to be written.</typeparam>
        /// <typeparam name="Out">The <see cref="SProtPayload"/> struct to be read.</typeparam>
        /// <param name="frame">The current frame object.</param>
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
        public static DeviceStatus WriteAndRead<In, Out>(this ISProtRW frame, In send, out Out recv, out byte status, byte addr = 0x0) where In : struct, SProtPayload where Out : struct, SProtPayload {
            DeviceStatus ret = frame.Write(new SProtFrame(send, addr));
            if (ret != DeviceStatus.OK) {
                status = 0;
                recv = default;
                return ret;
            }
            ret = frame.Read(out SProtFrame recv_frame);
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