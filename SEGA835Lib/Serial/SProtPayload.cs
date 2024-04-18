using System.Runtime.InteropServices;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    public interface SProtPayload {
        byte GetCommandID();
    }

    public static class JVSPayloadExtensions {

        public static SProtFrame ToFrame(this SProtPayload payload, byte addr = 0x0) {
            return new SProtFrame(payload, addr);
        }

    }
}
