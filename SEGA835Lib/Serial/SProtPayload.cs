using System.Runtime.InteropServices;

namespace Haruka.Arcade.SEGA835Lib.Serial {

    /// <summary>
    /// This interface defines a payload for <see cref="ISProtRW"/>. Implementors must be a struct.
    /// </summary>
    public interface SProtPayload {

        /// <summary>
        /// Gets the command ID of this payload.
        /// </summary>
        /// <returns>the command ID</returns>
        byte GetCommandID();
    }

    /// <summary>
    /// Helper functions for <see cref="SProtPayload"/>.
    /// </summary>
    public static class JVSPayloadExtensions {

        /// <summary>
        /// Converts this SProtPayload into a <see cref="SProtFrame"/>.
        /// </summary>
        /// <param name="payload">The payload to convert.</param>
        /// <param name="addr">The address of the receiving board. This is only needed for very specific boards and ignored on others.</param>
        /// <returns>The SProtFrame of the given SProtPayload</returns>
        public static SProtFrame ToFrame(this SProtPayload payload, byte addr = 0x0) {
            return new SProtFrame(payload, addr);
        }

    }
}
