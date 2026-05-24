using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Y3Board;

static class Y3Runner {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Y3Runner));

    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        Y3 y3 = new Y3(opts.Port);
        DeviceStatus ret;

        try {
            ret = y3.Connect();
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Connecting to Y3 board failed");
                return ret;
            }

            LOG.LogInformation("Device Model:" + y3.GetDeviceModel());
            LOG.LogInformation("Device Firmware Type:" + y3.GetFirmwareType());
            LOG.LogInformation("Device Target Code:" + y3.GetTargetCodeType());
            LOG.LogInformation("Status: " + y3.GetStatus());

            ret = y3.SetParamsForPlayfield();
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting parameters failed");
                return ret;
            }

            LOG.LogInformation("Status: " + y3.GetStatus());

            ret = y3.Start();
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Start failed");
                return ret;
            }

            LOG.LogInformation("Status: " + y3.GetStatus());

            if (!opts.NoExitButton) {
                Console.WriteLine("Press ESC to exit.");
            }

            bool found = false;
            DateTime lastinfo = DateTime.Now;
            do {
                if (!opts.NoExitButton) {
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape) {
                            break;
                        }
                    }
                }

                ret = y3.GetCards(out uint count, out Y3.CardInfo[] data, out uint _);
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("Error reading from board: " + ret);
                    return ret;
                }

                if (count > 0) {
                    foreach (Y3.CardInfo card in data) {
                        if (card.IsValidCard() || (opts.IgnoreCardType && card.IsValid())) {
                            Console.WriteLine(card.CardType + ":" + card.UnknownType + ":" + card.X + ":" + card.Y + ":" + card.Rotation + ":" + card.GetTitleCode() + ":" + card.GetIvCode() + ":" + card.DataCount + ":" + card.Data0 + ":" + card.Data1 + ":" + card.Data2 + ":" + card.Data3 + ":" + card.Data4 + ":" + card.Data5);
                            found = true;
                        }
                    }

                    Thread.Sleep(250);
                }

                if (DateTime.Now - lastinfo > TimeSpan.FromSeconds(3)) {
                    LOG.LogInformation("Status: " + y3.GetStatus());
                    lastinfo = DateTime.Now;
                }
            } while (!opts.FindOne || !found);
        } finally {
            y3.Disconnect();
        }

        return ret;
    }
}