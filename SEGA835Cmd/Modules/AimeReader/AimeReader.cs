using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.AimeReader {
    internal class AimeReader {

        public static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            AimeCardReader_837_15396 aime = new AimeCardReader_837_15396(opts.Port, !opts.SlowBaudrate);
            try {
                DeviceStatus ret = aime.Connect();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Connecting to card reader failed");
                    return ret;
                }

                if (opts.ResetLEDs) {
                    ret = aime.LEDReset();
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Resetting LEDs failed");
                        return ret;
                    }
                }
                if (opts.LEDRed > 0 || opts.LEDGreen > 0 || opts.LEDBlue > 0) {
                    ret = aime.LEDSetColor(opts.LEDRed, opts.LEDGreen, opts.LEDBlue);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Setting LEDs failed");
                        return ret;
                    }
                }

                if (opts.GetFirmware) {
                    ret = aime.GetFWVersion(out string version);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Operation failed");
                    }
                    Console.WriteLine(version);
                    return ret;
                } else if (opts.GetHardware) {
                    ret = aime.GetHWVersion(out string version);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Operation failed");
                    }
                    Console.WriteLine(version);
                    return ret;
                }

                ret = aime.RadioOn(opts.CardType);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Failed to start scanning");
                    return ret;
                }
                ret = aime.StartPolling();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Failed to start scanning");
                    return ret;
                }

                int scan = 1;
                int maxScan = opts.Continous ? Int32.MaxValue : 1;
                TimeSpan timeout = TimeSpan.FromMilliseconds(opts.Timeout);

                do {
                    DateTime start = DateTime.Now;
                    do {
                        if (aime.HasDetectedCard()) {
                            Console.WriteLine(BitConverter.ToString(aime.GetCardUID()).Replace("-", ""));
                            break;
                        }
                        Thread.Sleep(50);
                    } while (DateTime.Now - start < timeout);
                    aime.ClearCard();
                } while (aime.IsPolling() && ++scan <= maxScan);

                ret = aime.StopPolling();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Failed to stop scanning");
                    return ret;
                }
                ret = aime.RadioOff();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Failed to stop scanning");
                    return ret;
                }

                return ret;
            } finally {
                aime?.Disconnect();
            }
        }
    }
}
