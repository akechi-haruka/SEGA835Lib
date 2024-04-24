using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    public abstract class RFIDRWDevice : Device, ISProtRW {

        public RFIDBackend Backend { get; private set; }

        protected RFIDRWDevice(RFIDBackend backend) {
            ArgumentNullException.ThrowIfNull(backend);
            Backend = backend;
        }

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

        public override DeviceStatus Disconnect() {
            return Backend.Disconnect();
        }

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

        public DeviceStatus Write(SProtFrame send) {
            return Write(send.Command, send.Payload);
        }

        public DeviceStatus Read(out SProtFrame recv) {
            DeviceStatus ret = Read(out byte cmd, out byte status, out byte[] payload);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            recv = new SProtFrame(0, cmd, 0, status, payload);
            return ret;
        }

        public DeviceStatus Reset() {
            Log.Write("Reset");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out byte status);
            ret = SetLastError(ret, status);
            if (status == 3) {
                Log.WriteWarning("Board was already reset");
            }
            return ret;
        }

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

        // TODO: ???
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

        // TODO: ???
        public DeviceStatus SetUnknown4() {
            Log.Write("SetUnknown4");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketUnknown4() {
                unk2 = 0x01
            }, out RespPacketUnknown4 _, out byte status);
            return SetLastError(ret, status);
        }

        // TODO: ???
        public DeviceStatus SetUnknown5() {
            Log.Write("SetUnknown5");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketUnknown5() {
                unk = 0x10
            }, out RespPacketUnknown5 _, out byte status);
            return SetLastError(ret, status);
        }

        public abstract int GetCardPayloadSize();
    }
}