using Haruka.Arcade.SEGA835Lib.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01 {
    public class IO4USB835_15257_01 : JVSUSBIO {

        public IO4USB835_15257_01() : base(0x0ca3, 0x0021) {
        }

        public override string GetDeviceModel() {
            return "835-15257-01";
        }

        public override string GetName() {
            return "SEGA I/O CONTROL BD";
        }

        public DeviceStatus ResetBoardStatus() {
            Log.Write("ResetBoardStatus");
            return Write(new JVSUSBReportOut() {
                cmd = JVSUSBReports.ClearBoardStatus
            });
        }
    }
}
