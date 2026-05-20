using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396;

namespace Haruka.Arcade.SEGA835Cmd.Modules.AimeReader;

static class AimeReader {
    public static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        AimeCardReader15396 aime = new AimeCardReader15396(opts.Port, !opts.SlowBaudrate);
        try {
            DeviceStatus ret = aime.Connect();
            if (ret != DeviceStatus.Ok) {
                Log.WriteError("Connecting to card reader failed");
                return ret;
            }

            if (opts.ResetLeds) {
                ret = aime.LedReset();
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Resetting LEDs failed");
                    return ret;
                }
            }

            if (opts.LedRed > 0 || opts.LedGreen > 0 || opts.LedBlue > 0) {
                ret = aime.LedSetColor(opts.LedRed, opts.LedGreen, opts.LedBlue);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Setting LEDs failed");
                    return ret;
                }
            }

            if (opts.GetFirmware) {
                ret = aime.GetFirmwareVersion(out string version, out byte versionByte);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Operation failed");
                }

                if (versionByte > 0) {
                    Console.WriteLine("0x" + versionByte.ToString("X2"));
                } else {
                    Console.WriteLine(version);
                }

                return ret;
            } else if (opts.GetFirmwareChecksum) {
                ret = aime.GetFirmwareChecksum(out ushort checksum);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Operation failed");
                }

                Console.WriteLine("0x" + checksum.ToString("X2"));
                return ret;
            } else if (opts.GetHardware) {
                ret = aime.GetHardwareVersion(out string version);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Operation failed");
                }

                Console.WriteLine(version);
                return ret;
            }

            ret = aime.RadioOn(opts.CardType);
            if (ret != DeviceStatus.Ok) {
                Log.WriteError("Failed to start scanning");
                return ret;
            }

            ret = aime.StartPolling();
            if (ret != DeviceStatus.Ok) {
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
                        Console.WriteLine(BitConverter.ToString(aime.GetCardUid()).Replace("-", ""));
                        break;
                    }

                    Thread.Sleep(50);
                } while (DateTime.Now - start < timeout);

                aime.ClearCard();
            } while (aime.IsPolling() && ++scan <= maxScan);

            ret = aime.StopPolling();
            if (ret != DeviceStatus.Ok) {
                Log.WriteError("Failed to stop scanning");
                return ret;
            }

            ret = aime.RadioOff();
            if (ret != DeviceStatus.Ok) {
                Log.WriteError("Failed to stop scanning");
            }

            return ret;
        } finally {
            aime.Disconnect();
        }
    }
}