using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;

namespace Haruka.Arcade.SEGA835Cmd.Modules.VFD {
    internal class VFDRunner {
        internal static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            VFD_GP1232A02A vfd = new VFD_GP1232A02A(opts.Port);

            DeviceStatus ret = vfd.Connect();
            if (ret != DeviceStatus.OK) {
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
                    ret = vfd.SetOn(true);
                    ret = vfd.SetText(opts.Text, opts.Text2, opts.ScrollLine == 1, opts.ScrollLine == 2);
                    ret = vfd.SetBrightness(opts.Brightness);
                }
            } catch (Exception ex) {
                Log.WriteFault(ex, "VFD setup failed");
                return (DeviceStatus)vfd.GetLastError();
            } finally {
                vfd?.Disconnect();
            }

            return ret;
        }
    }
}