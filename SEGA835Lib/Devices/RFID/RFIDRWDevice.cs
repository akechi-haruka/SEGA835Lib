using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    /// <summary>
    /// The base class for a device that can read (and possibly write) to SEGA RFID cards. The actual frequency or parameters these use are unknown.
    /// </summary>
    public abstract class RFIDRWDevice : Device {

        /// <summary>
        /// The backend that is used.
        /// </summary>
        public RFIDBackend Backend { get; private set; }

        /// <summary>
        /// Creates a new RFID device.
        /// </summary>
        /// <param name="backend">The backend being used.</param>
        protected RFIDRWDevice(RFIDBackend backend) {
            NetStandardBackCompatExtensions.ThrowIfNull(backend, nameof(backend));
            Backend = backend;
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            DeviceStatus ret = Backend.Connect();
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            ret = SetLastError(Reset());
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            Log.Write("Connected to RFID board successfully");
            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            return Backend.Disconnect();
        }

        /// <summary>
        /// Writes data to the device.
        /// </summary>
        /// <param name="cmd">The packet command.</param>
        /// <param name="payload">The packet payload.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> on success.<br />
        /// <see cref="DeviceStatus.ERR_PAYLOAD_TOO_LARGE"/> if the payload is larger than 253 bytes.<br />
        /// any other DeviceStatus on error.
        /// </returns>
        protected DeviceStatus Write(byte cmd, byte[] payload) {
            byte[] packet = new byte[payload.Length + 2];
            if (packet.Length > 0xFF) {
                return DeviceStatus.ERR_PAYLOAD_TOO_LARGE;
            }
            packet[0] = cmd;
            packet[1] = (byte)payload.Length;
            Array.Copy(payload, 0, packet, 2, payload.Length);
            return Backend.Write(packet);
        }

        /// <summary>
        /// Reads a packet from the device.
        /// </summary>
        /// <param name="cmd">The command that was received.</param>
        /// <param name="sub_cmd">The sub command that was received.</param>
        /// <param name="payload">The payload that was received.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        protected DeviceStatus Read(out byte cmd, out byte sub_cmd, out byte[] payload) {
            DeviceStatus ret = Backend.Read(out byte[] data);
            if (ret != DeviceStatus.OK) {
                cmd = 0;
                sub_cmd = 0;
                payload = null;
                return ret;
            }
            cmd = data[1];
            sub_cmd = data[2];
            byte len = data[3];
            payload = new byte[len];
            Array.Copy(data, 4, payload, 0, payload.Length);
            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public DeviceStatus Write(SProtFrame send) {
            return Write(send.Command, send.Payload);
        }

        /// <inheritdoc/>
        public DeviceStatus Read(out SProtFrame recv) {
            DeviceStatus ret = Read(out byte cmd, out byte status, out byte[] payload);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            recv = new SProtFrame(0, cmd, 0, status, payload);
            return ret;
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

        /// <summary>
        /// Resets the board.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus Reset() {
            Log.Write("Reset");
            DeviceStatus ret = WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out byte status);
            ret = SetLastError(ret, status);
            if (status == 3) {
                Log.WriteWarning("Board was already reset");
            }
            return ret;
        }

        /// <summary>
        /// Reads the "boot" version from the board.
        /// </summary>
        /// <param name="version">The version that was read from the device.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus GetBootVersion(out byte version) {
            Log.Write("GetBootVersion");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetBootVersion(), out RespPacketGetBootVersion resp, out byte status);
            if (ret != DeviceStatus.OK) {
                version = 0;
                return SetLastError(ret, status);
            }
            version = resp.version;
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Reads the "app" version from the board.
        /// </summary>
        /// <param name="version">The version that was read from the device.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus GetAppVersion(out byte version) {
            Log.Write("GetAppVersion");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetAppVersion(), out RespPacketGetAppVersion resp, out byte status);
            if (ret != DeviceStatus.OK) {
                version = 0;
                return SetLastError(ret, status);
            }
            version = resp.version;
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Reads the board information.
        /// </summary>
        /// <param name="version">The version that was read from the device.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus GetBoardInfo(out string version) {
            Log.Write("GetBoardInfo");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetBoardInfo(), out RespPacketGetBoardInfo resp, out byte status);
            if (ret != DeviceStatus.OK) {
                version = null;
                return SetLastError(ret, status);
            }
            version = resp.version;
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Unknown. This is required to be called for card reading.
        /// </summary>
        /// <param name="b">Unknown.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus GetUnknown81(out byte b) {
            Log.Write("GetUnknown81");
            DeviceStatus ret = WriteAndRead(new ReqPacketUnknown81(), out RespPacketUnknown81 resp, out byte status);
            if (ret != DeviceStatus.OK) {
                b = 0;
                return SetLastError(ret, status);
            }
            b = resp.unk;
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus SetUnknown4() {
            Log.Write("SetUnknown4");
            DeviceStatus ret = WriteAndRead(new ReqPacketUnknown4() {
                unk2 = 0x01
            }, out RespPacketUnknown4 _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus SetUnknown5() {
            Log.Write("SetUnknown5");
            DeviceStatus ret = WriteAndRead(new ReqPacketUnknown5() {
                unk = 0x10
            }, out RespPacketUnknown5 _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Returns the payload size for a card read by this board (including the 12-byte card ID).
        /// </summary>
        /// <returns>the number of bytes of the payload size</returns>
        public abstract int GetCardPayloadSize();
    }
}