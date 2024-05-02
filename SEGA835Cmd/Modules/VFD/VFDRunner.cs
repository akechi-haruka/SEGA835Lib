using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoy.Wrapper;

namespace Haruka.Arcade.SEGA835Cmd.Modules.VFD {
    internal class VFDRunner {
        internal static DeviceStatus Main(Options opts) {
            Program.SetGlobalOptions(opts);

            VFD_GP1232A02A vfd = new VFD_GP1232A02A(opts.Port);

            DeviceStatus ret = vfd.Connect();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Connect failed");
                return ret;
            }

            vfd.SetUseExceptions(true);
            try {
                ret = vfd.SetEncoding(opts.Encoding);
                ret = vfd.SetOn(true);
                ret = vfd.SetTextPosition(20, 0, 120);
                ret = vfd.WriteScrollingText(opts.Text);
                ret = vfd.SetTextScroll(!opts.NoScroll);
                ret = vfd.SetBrightness(opts.Brightness);
            } catch {
                Log.WriteError("VFD setup failed");
                return (DeviceStatus)vfd.GetLastError();
            }

            return ret;
        }
    }
}
