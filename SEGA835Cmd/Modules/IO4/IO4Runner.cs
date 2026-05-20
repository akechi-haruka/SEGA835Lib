using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4;

static class Io4Runner {
    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        Io4Usb15257 dev = new Io4Usb15257();
        DeviceStatus ret = dev.Connect();
        if (ret != DeviceStatus.Ok) {
            Log.WriteError("Failed to connect to IO4 board.");
            return ret;
        }

        if (opts.Output == Options.OutputType.Gpio) {
            if (opts.Clear) {
                ret = dev.ClearGpio();
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Failed to clear.");
                    return ret;
                }
            }

            ret = dev.SetGpio(opts.Index, opts.Value != 0);
            if (ret != DeviceStatus.Ok) {
                Log.WriteError("Failed to set.");
                return ret;
            }
        } else if (opts.Output == Options.OutputType.Led) {
            if (opts.Clear) {
                ret = dev.ClearLed();
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Failed to clear.");
                    return ret;
                }
            }

            ret = dev.SetLed(opts.Index, (byte)opts.Value);
            if (ret != DeviceStatus.Ok) {
                Log.WriteError("Failed to set.");
                return ret;
            }
        }

        dev.Disconnect();

        return ret;
    }
}