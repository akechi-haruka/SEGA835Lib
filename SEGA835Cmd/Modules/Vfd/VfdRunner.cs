using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Vfd;

static class VfdRunner {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(VfdRunner));

    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        VfdGp1232A02A vfd = new VfdGp1232A02A(opts.Port);

        DeviceStatus ret = vfd.Connect();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Connect failed");
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
            LOG.LogCritical(ex, "VFD setup failed");
            return (DeviceStatus)vfd.GetLastError();
        } finally {
            vfd.Disconnect();
        }

        return ret;
    }
}