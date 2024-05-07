using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    /// <summary>
    /// The base class for a device that can read (and possibly write) to SEGA RFID cards. The actual frequency or parameters these use are unknown.
    /// </summary>
    public abstract class RFIDRWDevice : Device, ISProtRW {

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
        /// Resets the board.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus Reset() {
            Log.Write("Reset");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out byte status);
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
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetBootVersion(), out RespPacketGetBootVersion resp, out byte status);
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
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetAppVersion(), out RespPacketGetAppVersion resp, out byte status);
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
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetBoardInfo(), out RespPacketGetBoardInfo resp, out byte status);
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
            DeviceStatus ret = this.WriteAndRead(new ReqPacketUnknown81(), out RespPacketUnknown81 resp, out byte status);
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
            DeviceStatus ret = this.WriteAndRead(new ReqPacketUnknown4() {
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
            DeviceStatus ret = this.WriteAndRead(new ReqPacketUnknown5() {
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