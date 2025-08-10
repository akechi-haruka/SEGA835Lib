using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06;
using Haruka.Arcade.SEGA835Lib.Misc;

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

            ret = led.GetBoardInfo(out string board_number, out string chip_number, out byte fw_ver);
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Getting board info failed");
                return ret;
            }

            Log.Write("Board Number: " + board_number);
            Log.Write("Chip Number: " + chip_number);
            Log.Write("FW version: " + fw_ver);

            ret = led.GetFirmwareChecksum(out ushort chk);
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Getting board info failed");
                return ret;
            }

            Log.Write("Board Checksum: " + chk);

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

            if (opts.MonkeyVersion > 0) {
                ret = led.SetFirmwareVersion(opts.MonkeyVersion);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting version failed");
                    return ret;
                }
            }

            if (opts.MonkeyBoardName != null) {
                ret = led.SetBoardName(opts.MonkeyBoardName);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting board name failed");
                    return ret;
                }
            }

            if (opts.MonkeyChipNumber != null) {
                ret = led.SetChipNumber(opts.MonkeyChipNumber);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting chip number failed");
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

            if (opts.MonkeyChannels != null) {
                string[] channels = opts.MonkeyChannels.Split(',');
                if (channels.Length != 3) {
                    Log.WriteError("Invalid argument for channels");
                    return ret;
                }

                led.SetChannels(Enum.Parse<LED_MONKEY06.Channel>(channels[0]), Enum.Parse<LED_MONKEY06.Channel>(channels[1]), Enum.Parse<LED_MONKEY06.Channel>(channels[2]));
            }

            if (opts.LEDTable != null) {
                List<Color> data = new List<Color>();
                string[] array = opts.LEDTable.Split(',');
                for (int i = 0; i < opts.Offset; i++) {
                    data.Add(Color.Black);
                }

                for (int i = 0; i < array.Length; i += 3) {
                    data.Add(Color.FromArgb(byte.Parse(array[i]), byte.Parse(array[i + 1]), byte.Parse(array[i + 2])));
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