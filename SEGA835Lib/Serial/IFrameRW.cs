using Haruka.Arcade.SEGA835Lib.Native;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    public interface IFrameRW {

        public DeviceStatus Write(JVSFrame send);

        public DeviceStatus Read(out JVSFrame recv);

    }

    public static class  IFrameRWExtensions {

        public static DeviceStatus WriteAndRead(this IFrameRW frame, JVSFrame send, out JVSFrame recv) {
            DeviceStatus ret = frame.Write(send);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            return frame.Read(out recv);
        }

        public static DeviceStatus WriteAndRead<In, Out>(this IFrameRW frame, In send, out Out recv) where In : struct, JVSPayload where Out : struct, JVSPayload {
            return WriteAndRead(frame, send, out recv, out byte _);
        }

        public static DeviceStatus WriteAndRead<In, Out>(this IFrameRW frame, In send, out Out recv, out byte status) where In : struct, JVSPayload where Out : struct, JVSPayload {
            DeviceStatus ret = frame.Write(new JVSFrame(send));
            if (ret != DeviceStatus.OK) {
                status = 0;
                recv = default;
                return ret;
            }
            ret = frame.Read(out JVSFrame recv_frame);
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