using CommandLine;
using Haruka.Arcade.SEGA835Cmd.Modules.AimeReader;
using Haruka.Arcade.SEGA835Cmd.Modules.IO4;
using Haruka.Arcade.SEGA835Cmd.Modules.IO4Con;
using Haruka.Arcade.SEGA835Cmd.Modules.LED;
#if !RASPBERRY
using Haruka.Arcade.SEGA835Cmd.Modules.Printer;
using Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher;
#endif
using Haruka.Arcade.SEGA835Cmd.Modules.RFID;
using Haruka.Arcade.SEGA835Cmd.Modules.VFD;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Serial;
using System.Text;

namespace Haruka.Arcade.SEGA835Cmd {
    internal class Program {

        static int Main(string[] args) {
            try {
#if RASPBERRY
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                return (int)Parser.Default.ParseArguments
                    <Modules.IO4Con.Options, Modules.AimeReader.Options, Modules.VFD.Options, Modules.RFID.Options, Modules.IO4.Options, Modules.LED.Options>(args)
                    .MapResult<Modules.IO4Con.Options, Modules.AimeReader.Options, Modules.VFD.Options, Modules.RFID.Options, Modules.IO4.Options, Modules.LED.Options, DeviceStatus>(
                  IO4Controller.Run,
                  AimeReader.Run,
                  VFDRunner.Run,
                  RFIDRunner.Run,
                  IO4Runner.Run,
                  LEDRunner.Run,
                  errs => DeviceStatus.ERR_OTHER);
#else
                return (int)Parser.Default.ParseArguments
                    <Modules.IO4Con.Options, Modules.AimeReader.Options, Modules.VFD.Options, Modules.Printer.Options, Modules.PrinterWatcher.Options, Modules.RFID.Options, Modules.IO4.Options, Modules.LED.Options>(args)
                    .MapResult<Modules.IO4Con.Options, Modules.AimeReader.Options, Modules.VFD.Options, Modules.Printer.Options, Modules.PrinterWatcher.Options, Modules.RFID.Options, Modules.IO4.Options, Modules.LED.Options, DeviceStatus>(
                  IO4Controller.Run,
                  AimeReader.Run,
                  VFDRunner.Run,
                  PrinterRunner.Run,
                  PrinterWatcherRunner.Run,
                  RFIDRunner.Run,
                  IO4Runner.Run,
                  LEDRunner.Run,
                  errs => DeviceStatus.ERR_OTHER);
#endif
            } catch(Exception ex) {
                Log.WriteFault(ex, "An error has occurred");
                return Int32.MinValue;
            } finally {
                Log.Write("Exiting");
            }
        }

        internal static void SetGlobalOptions(GlobalOptions options) {
            Log.Mute = options.Silent;
            if (options.LogFile != null) {
                Log.LogFileName = options.LogFile;
                Log.Init(true, 0);
            }
        }

    }
}
