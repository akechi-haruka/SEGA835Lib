using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Y3Board {
    internal class Y3Runner {
        internal static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            Y3 y3 = new Y3(opts.Port);
            DeviceStatus ret;

            try {
                ret = y3.Connect();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Connecting to Y3 board failed");
                    return ret;
                }

                Log.Write("Device Model:" + y3.GetDeviceModel());
                Log.Write("Device Firmware Type:" + y3.GetFirmwareType());
                Log.Write("Device Target Code:" + y3.GetTargetCodeType());
                Log.Write("Status: " + y3.GetStatus());

                ret = y3.SetParamsForPlayfield();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Setting parameters failed");
                    return ret;
                }

                if (!opts.NoExitButton) {
                    Console.WriteLine("Press ESC to exit.");
                }

                bool found = false;
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
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Error reading from board: " + ret);
                        return ret;
                    }

                    if (count > 0) {
                        foreach (Y3.CardInfo card in data) {
                            if (card.IsValid()) {
                                Console.Write(card.CardType + "," + card.UnknownType + "," + card.X + "," + card.Y + "," + card.Rotation + "," + card.GetTitleCode() + "," + card.GetIvCode());
                                found = true;
                            }
                        }
                    }
                } while (!opts.FindOne || !found);
            } finally {
                y3.Disconnect();
            }

            return ret;
        }
    }
}