using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {
    public abstract class RFIDBackend {

        public abstract DeviceStatus Connect();

        public abstract DeviceStatus Disconnect();

        public abstract DeviceStatus Read(out byte[] packet);

        public abstract DeviceStatus Write(byte[] packet);
    }
}
