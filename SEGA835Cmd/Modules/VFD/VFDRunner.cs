using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;

namespace Haruka.Arcade.SEGA835Cmd.Modules.VFD;

static class VfdRunner {
    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        VfdGp1232A02A vfd = new VfdGp1232A02A(opts.Port);

        DeviceStatus ret = vfd.Connect();
        if (ret != DeviceStatus.Ok) {
            Log.WriteError("Connect failed");
            return ret;
        }

        vfd.SetUseExceptions(true);
        try {
            if (opts.GetVersion) {
                ret = vfd.GetVersion(out string ver);
                Console.WriteLine(ver);
            } else {
                ret = vfd.SetEncoding(opts.Encoding);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = vfd.SetOn(true);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = vfd.SetTextScrollSpeed(opts.Speed);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = vfd.SetText(opts.Text, opts.Text2, opts.ScrollLine == 1, opts.ScrollLine == 2);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = vfd.SetBrightness(opts.Brightness);
            }
        } catch (Exception ex) {
            Log.WriteFault(ex, "VFD setup failed");
            return (DeviceStatus)vfd.GetLastError();
        } finally {
            vfd.Disconnect();
        }

        return ret;
    }
}