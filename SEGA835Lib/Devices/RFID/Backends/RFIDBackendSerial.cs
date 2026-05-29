using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {
    /// <summary>
    /// A serial-based RFID backend (direct communication via a COM port)
    /// </summary>
    class RfidBackendSerial : RfidBackend {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(RfidBackendSerial));

        internal readonly SProtSerial Serial;

        /// <summary>
        /// The COM port that is being used.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Creates a new RFID backend.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        public RfidBackendSerial(int port) {
            Port = port;
            Serial = new SProtSerial(port, 115200, 3000, true, true);
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            if (Serial.IsConnected()) {
                return DeviceStatus.Ok;
            }

            LOG.LogInformation("Connecting on Port " + Port);
            if (!Serial.Connect()) {
                return DeviceStatus.ErrorNotConnected;
            }

            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            LOG.LogInformation("Disconnected on Port " + Port);
            Serial?.Disconnect();
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(out byte[] packet) {
            return Serial.ReadLenByOffset(3, out packet);
        }

        /// <inheritdoc/>
        public override DeviceStatus Write(byte[] packet) {
            return Serial.Write(packet);
        }
    }
}