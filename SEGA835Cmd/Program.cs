using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC330;

using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Cmd {
    internal class Program {

        static void Main(string[] args) {
            Log.Init(false, 0);
            CHC330Printer printer = new CHC330Printer(new SEGA835Lib.Devices.RFID.RFIDRWPrinter_837_15347(4));
            printer.Connect();
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
