using Haruka.Arcade.SEGA835Lib.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.AimeReader {
    internal class AimeReader {

        public static DeviceStatus Main(Options opts) {
            Program.SetGlobalOptions(opts);
            return 0;
        }
    }
}
