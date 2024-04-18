using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Native;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Cmd {
    internal class Program {

        static void Main(string[] args) {
            Log.Init(false, 0);
            IO4USB835_15257_01 io4 = new IO4USB835_15257_01();
            ThrowOnDeviceError(io4.Connect());
            ThrowOnDeviceError(io4.ResetBoardStatus());
            while (true) {
                io4.Poll(out JVSUSBReportIn report);
                unsafe {
                    Log.Write(report.system_status + "," + report.usb_status);
                    for (int i = 0; i < 2; i++) {
                        Log.Write("B" + i + ":" +report.buttons[i].ToString());
                    }
                    for (int i = 0; i < 8; i++) {
                        Log.Write("A" +i + ":" + report.adcs[i].ToString());
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /*static void Main(string[] args) {
            Log.Init(false, 0);
            AimeCardReader_837_15396 aime = new AimeCardReader_837_15396(Int32.Parse(args[0]));
            ThrowOnDeviceError(aime.Connect());
            ThrowOnDeviceError(aime.GetFWVersion(out string fw));
            ThrowOnDeviceError(aime.GetHWVersion(out string hw));
            Log.Write("Reader FW: " + fw + " / HW: " + hw);
            ThrowOnDeviceError(aime.RadioOn(RadioOnType.MIFARE));
            ThrowOnDeviceError(aime.StartPolling());
            while (aime.IsPolling() && !aime.HasDetectedCard()) {
                Thread.Sleep(100);
            }
            ThrowOnDeviceError(aime.StopPolling());
            Log.Write("Card Type: " + aime.GetCardType());
            Log.Write("Card ID: " + String.Join(",", aime.GetCardUID()));
        }*/

        static void ThrowOnDeviceError(DeviceStatus ret) {
            if (ret != DeviceStatus.OK) {
                throw new Exception("Device access failed with: " + ret);
            }
        }
    }
}
