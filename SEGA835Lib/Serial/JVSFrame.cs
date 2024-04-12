using Haruka.Arcade.SEGA835Lib.Native;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    public class JVSFrame {

        private static int sequenceCount = 1;

        public byte Sequence { get; private set; }
        public byte Command { get; private set; }
        public byte Address { get; private set; }
        public byte Status { get; private set; }
        public byte[] Payload { get; private set; }

        public JVSFrame(byte command, byte[] payload, byte address = 0x0) {
            this.Command = command;
            this.Payload = payload;
            this.Address = address;
            this.Sequence = (byte)(sequenceCount++ % 0xFF);
        }

        public JVSFrame(JVSPayload payload, byte address = 0x0) {
            this.Command = payload.CommandID;
            this.Payload = StructUtils.GetBytes(payload);
            this.Address = address;
            this.Sequence = (byte)(sequenceCount++ % 0xFF);
        }

        public JVSFrame(byte sequence, byte command, byte address, byte status, byte[] payload) {
            this.Sequence = sequence;
            this.Command = command;
            this.Address = address;
            this.Status = status;
            this.Payload = payload;
        }
    }
}
