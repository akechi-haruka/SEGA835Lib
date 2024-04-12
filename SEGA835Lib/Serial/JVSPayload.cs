using System.Runtime.InteropServices;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    public interface JVSPayload {
        public byte CommandID { get; }

    }

    public static class JVSPayloadExtensions {

        public static JVSFrame ToFrame(this JVSPayload payload) {
            return new JVSFrame(payload);
        }

    }
}
