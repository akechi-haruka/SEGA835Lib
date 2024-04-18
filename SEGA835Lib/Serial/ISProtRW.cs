using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Native;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    public interface ISProtRW {

        public DeviceStatus Write(SProtFrame send);

        public DeviceStatus Read(out SProtFrame recv);

    }

    public static class IFrameRWExtensions {

        public static DeviceStatus WriteAndRead(this ISProtRW frame, SProtFrame send, out SProtFrame recv) {
            DeviceStatus ret = frame.Write(send);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            return frame.Read(out recv);
        }

        public static DeviceStatus WriteAndRead<In, Out>(this ISProtRW frame, In send, out Out recv) where In : struct, SProtPayload where Out : struct, SProtPayload {
            return WriteAndRead(frame, send, out recv, out byte _, 0);
        }

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