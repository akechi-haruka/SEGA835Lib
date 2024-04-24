using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {
    internal class RFIDBackendSerial : RFIDBackend {

        private SProtSerial serial;

        public int Port { get; private set; }

        public RFIDBackendSerial(int port) {
            this.Port = port;
            this.serial = new SProtSerial(port, 115200, 3000, true, true, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Handshake.XOnXOff);
        }

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

        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        public override DeviceStatus Read(out byte[] packet) {
            return serial.ReadLenByOffset(3, out packet);
        }

        public override DeviceStatus Write(byte[] packet) {
            return serial.Write(packet);
        }
    }
}
