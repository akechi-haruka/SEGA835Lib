

using Haruka.Arcade.SEGA835Lib.Misc;

namespace Haruka.Arcade.SEGA835Lib.Serial {

    /// <summary>
    /// This class holds all relevant information for a single frame of a SProt device.
    /// </summary>
    /// <seealso cref="SProtSerial"/>
    public class SProtFrame {

        private static int sequenceCount = 1;

        /// <summary>
        /// The sequence ID of the frame. Some SProt devices require this, but not all.
        /// </summary>
        public byte Sequence { get; private set; }
        /// <summary>
        /// The command ID of the frame.
        /// </summary>
        public byte Command { get; private set; }

        /// <summary>
        /// The bus address of the frame. This is only used by very specific devices.
        /// </summary>
        public byte Address { get; private set; }

        /// <summary>
        /// The response status of the frame. If this frame is a request, this is always zero. Non-zero means error and is device specific.
        /// </summary>
        public byte Status { get; private set; }

        /// <summary>
        /// The payload data of the frame.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Creates a new SProtFrame. The sequence byte is auto-incremented.
        /// </summary>
        /// <param name="command">The command ID to set.</param>
        /// <param name="payload">The payload to send.</param>
        /// <param name="address">The bus address to send this frame to. This is only used by very specific devices.</param>
        public SProtFrame(byte command, byte[] payload, byte address = 0x0) {
            this.Command = command;
            this.Payload = payload;
            this.Address = address;
            this.Sequence = (byte)(sequenceCount++ % 0x100);
        }

        /// <summary>
        /// Creates a new SProtFrame. The sequence byte is auto-incremented.
        /// </summary>
        /// <param name="payload">The payload to send.</param>
        /// <param name="address">The bus address to send this frame to. This is only used by very specific devices.</param>
        public SProtFrame(SProtPayload payload, byte address = 0x0) {
            this.Command = payload.GetCommandID();
            this.Payload = StructUtils.GetBytes(payload);
            this.Address = address;
            this.Sequence = (byte)(sequenceCount++ % 0x100);
        }

        /// <summary>
        /// Creates a new SProtFrame.
        /// </summary>
        /// <param name="sequence">The sequence byte to set.</param>
        /// <param name="command">The command ID to set.</param>
        /// <param name="address">The bus address to send this frame to. This is only used by very specific devices.</param>
        /// <param name="status">The response status that was received.</param>
        /// <param name="payload">The payload to send.</param>
        public SProtFrame(byte sequence, byte command, byte address, byte status, byte[] payload) {
            this.Sequence = sequence;
            this.Command = command;
            this.Address = address;
            this.Status = status;
            this.Payload = payload;
        }
    }
}
