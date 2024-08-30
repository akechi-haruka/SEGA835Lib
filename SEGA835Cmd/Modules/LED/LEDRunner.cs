using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoy.Wrapper;

namespace Haruka.Arcade.SEGA835Cmd.Modules.LED {
    internal class LEDRunner {
        internal unsafe static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            LED_MONKEY06 led = new LED_MONKEY06(opts.Port, (byte)opts.SourceAddress, (byte)opts.DestinationAddress);

            DeviceStatus ret = led.Connect();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Connecting to LED board failed");
                return ret;
            }

            if (opts.MonkeyReset) {
                ret = led.ResetMonkey();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Reset failed");
                    return ret;
                }
            }

            if (opts.MonkeyChecksum > 0) {
                ret = led.SetFirmwareChecksum(opts.MonkeyChecksum);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting checksum failed");
                    return ret;
                }
            }

            if (opts.MonkeyTable != null) {
                List<byte> data = new List<byte>();
                foreach (String s in opts.MonkeyTable.Split(',')) {
                    data.Add(Byte.Parse(s));
                }
                ret = led.SetLEDTranslationTable(data);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting translation table failed");
                    return ret;
                }
            }

            if (opts.LEDTable != null) {
                List<Color> data = new List<Color>();
                string[] array = opts.LEDTable.Split(',');
                for (int i = 0; i < opts.Offset; i++) {
                    data.Add(Color.Black);
                }
                for (int i = 0; i < array.Length; i+=3) {
                    data.Add(Color.FromArgb(byte.Parse(array[i]), byte.Parse(array[i+1]), byte.Parse(array[i+2])));
                }
                ret = led.SetLEDs(data);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting LEDs failed");
                    return ret;
                }
            }

            return ret;
        }
    }
}
