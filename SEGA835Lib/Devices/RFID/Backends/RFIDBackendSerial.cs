using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System.IO.Ports;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {
    /// <summary>
    /// A serial-based RFID backend (direct communication via a COM port)
    /// </summary>
    internal class RFIDBackendSerial : RFIDBackend {
        internal readonly SProtSerial serial;

        /// <summary>
        /// The COM port that is being used.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Creates a new RFID backend.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        public RFIDBackendSerial(int port) {
            this.Port = port;
            this.serial = new SProtSerial(port, 115200, 3000, true, true, Parity.None, 8, StopBits.One, Handshake.None);
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            if (serial != null && serial.IsConnected()) {
                return DeviceStatus.OK;
            }

            Log.Write("Connecting on Port " + Port);
            if (!serial.Connect()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }

            return DeviceStatus.OK;
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(out byte[] packet) {
            return serial.ReadLenByOffset(3, out packet);
        }

        /// <inheritdoc/>
        public override DeviceStatus Write(byte[] packet) {
            return serial.Write(packet);
        }
    }
}