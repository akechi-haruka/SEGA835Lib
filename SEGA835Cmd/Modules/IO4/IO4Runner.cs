using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4 {
    internal class IO4Runner {
        internal unsafe static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            IO4USB_835_15257_01 dev = new IO4USB_835_15257_01();
            DeviceStatus ret = dev.Connect();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Failed to connect to IO4 board.");
                return ret;
            }

            if (opts.Output == Options.OutputType.GPIO) {
                if (opts.Clear) {
                    ret = dev.ClearGPIO();
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Failed to clear.");
                        return ret;
                    }
                }

                ret = dev.SetGPIO(opts.Index, opts.Value != 0);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Failed to set.");
                    return ret;
                }
            } else if (opts.Output == Options.OutputType.LED) {
                if (opts.Clear) {
                    ret = dev.ClearLED();
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Failed to clear.");
                        return ret;
                    }
                }

                ret = dev.SetLED(opts.Index, (byte)opts.Value);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Failed to set.");
                    return ret;
                }
            }

            dev.Disconnect();

            return ret;
        }
    }
}