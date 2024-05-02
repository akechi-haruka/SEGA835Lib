using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoy.Wrapper;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4Con {
    internal class IO4Controller {
        internal unsafe static DeviceStatus Main(Options opts) {
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
            j.Aquire();

            if (!opts.NoExitButton) {
                Console.WriteLine("Press ESC to exit.");
            }

            while (j.Aquired && dev.IsConnected()) {

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

                j.SetJoystickAxis(report.adcs[opts.XAxisADC], Axis.HID_USAGE_X);
                j.SetJoystickAxis(report.adcs[opts.YAxisADC], Axis.HID_USAGE_Y);
                Axis currentAxis = Axis.HID_USAGE_Z;
                for (int i = 0; i < JVSUSBReportIn.ADC_COUNT; i++) {
                    if (i != opts.XAxisADC && i != opts.YAxisADC) {
                        j.SetJoystickAxis(report.adcs[i], currentAxis++);
                    }
                }
                uint button_index = 0;
                for (int p = 0; p < JVSUSBReportIn.BUTTON_COUNT; p++) {
                    for (int b = 0; b < 16; b++) {
                        j.SetJoystickButton(((report.buttons[p] >> b) & 1) != 0, button_index++);
                    }
                }

                j.Update();

                Thread.Sleep(2);
            }

            j.Release();
            dev.Disconnect();

            return ret;
        }
    }
}
