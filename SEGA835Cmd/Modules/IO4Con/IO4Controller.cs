using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using System.Text;
using vJoy.Wrapper;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4Con {
    internal class IO4Controller {
        internal unsafe static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            IO4USB_835_15257_01 dev = new IO4USB_835_15257_01();
            DeviceStatus ret = dev.Connect();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Failed to connect to IO4 board.");
                return ret;
            }

            ret = dev.ResetBoardStatus();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Failed to reset status.");
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

            while (j?.Aquired ?? true && dev.IsConnected()) {
                if (!opts.NoExitButton) {
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape) {
                            break;
                        }
                    }
                }

                ret = dev.Poll(out JVSUSBReportIn report);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Poll failed: " + ret);
                    break;
                }

                int x = report.adcs[opts.XAxisADC] - short.MaxValue / 2;
                int y = report.adcs[opts.YAxisADC] - short.MaxValue / 2;
                j?.SetJoystickAxis(opts.XFlip ? short.MaxValue - x : x, Axis.HID_USAGE_X);
                j?.SetJoystickAxis(opts.YFlip ? short.MaxValue - y : y, Axis.HID_USAGE_Y);
                Axis currentAxis = Axis.HID_USAGE_Z;
                for (int i = 0; i < JVSUSBReportIn.ADC_COUNT; i++) {
                    if (i != opts.XAxisADC && i != opts.YAxisADC) {
                        j?.SetJoystickAxis(report.adcs[i] - short.MaxValue / 2, currentAxis++);
                    }
                }

                uint button_index = 0;
                for (int p = 0; p < JVSUSBReportIn.BUTTON_COUNT; p++) {
                    for (int b = 0; b < 16; b++) {
                        j?.SetJoystickButton(((report.buttons[p] >> b) & 1) != 0, button_index++);
                    }
                }

                j?.Update();

                if (opts.DumpAxes) {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < JVSUSBReportIn.ADC_COUNT; i++) {
                        sb.Append(report.adcs[i]);
                        if (i + 1 < JVSUSBReportIn.ADC_COUNT) {
                            sb.Append(',');
                        }
                    }

                    Log.Write(sb.ToString());
                }

                Thread.Sleep(opts.PollDelay);
            }

            j.Release();
            dev.Disconnect();

            return ret;
        }
    }
}