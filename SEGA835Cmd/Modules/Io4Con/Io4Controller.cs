using System.Text;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Microsoft.Extensions.Logging;
using vJoy.Wrapper;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Io4Con;

static class Io4Controller {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Io4Controller));

    internal static unsafe DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        Io4Usb15257 dev = new Io4Usb15257();
        DeviceStatus ret = dev.Connect();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Failed to connect to IO4 board.");
            return ret;
        }

        ret = dev.ResetBoardStatus();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Failed to reset status.");
            return ret;
        }

        VirtualJoystick j = new VirtualJoystick(opts.ControllerId);
        try {
            j.Aquire();
        } catch {
            if (opts.IgnoreVJoyErrors) {
                j = null;
            } else {
                throw;
            }
        }

        if (!opts.NoExitButton) {
            Console.WriteLine("Press ESC to exit.");
        }

        while ((j?.Aquired ?? true) && dev.IsConnected()) {
            if (!opts.NoExitButton) {
                if (Console.KeyAvailable) {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) {
                        break;
                    }
                }
            }

            ret = dev.Poll(out JvsUsbReportIn report);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Poll failed: " + ret);
                break;
            }

            int x = report.adcs[opts.XAxisAdc] - short.MaxValue / 2;
            int y = report.adcs[opts.YAxisAdc] - short.MaxValue / 2;
            j?.SetJoystickAxis(opts.XFlip ? short.MaxValue - x : x, Axis.HID_USAGE_X);
            j?.SetJoystickAxis(opts.YFlip ? short.MaxValue - y : y, Axis.HID_USAGE_Y);
            Axis currentAxis = Axis.HID_USAGE_Z;
            for (int i = 0; i < JvsUsbReportIn.ADC_COUNT; i++) {
                if (i != opts.XAxisAdc && i != opts.YAxisAdc) {
                    j?.SetJoystickAxis(report.adcs[i] - short.MaxValue / 2, currentAxis++);
                }
            }

            uint buttonIndex = 0;
            for (int p = 0; p < JvsUsbReportIn.BUTTON_COUNT; p++) {
                for (int b = 0; b < 16; b++) {
                    j?.SetJoystickButton(((report.buttons[p] >> b) & 1) != 0, buttonIndex++);
                }
            }

            j?.Update();

            if (opts.DumpAxes) {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < JvsUsbReportIn.ADC_COUNT; i++) {
                    sb.Append(report.adcs[i]);
                    if (i + 1 < JvsUsbReportIn.ADC_COUNT) {
                        sb.Append(',');
                    }
                }

                LOG.LogInformation(sb.ToString());
            }

            Thread.Sleep(opts.PollDelay);
        }

        j?.Release();
        dev.Disconnect();

        return ret;
    }
}