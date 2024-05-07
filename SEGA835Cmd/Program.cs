using CommandLine;
using Haruka.Arcade.SEGA835Cmd.Modules.AimeReader;
using Haruka.Arcade.SEGA835Cmd.Modules.IO4Con;
using Haruka.Arcade.SEGA835Cmd.Modules.Printer;
using Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher;
using Haruka.Arcade.SEGA835Cmd.Modules.RFID;
using Haruka.Arcade.SEGA835Cmd.Modules.VFD;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Cmd {
    internal class Program {

        static int Main(string[] args) {
            try {
                return (int)Parser.Default.ParseArguments
                    <Modules.IO4Con.Options, Modules.AimeReader.Options, Modules.VFD.Options, Modules.Printer.Options, Modules.PrinterWatcher.Options, Modules.RFID.Options>(args)
                    .MapResult<Modules.IO4Con.Options, Modules.AimeReader.Options, Modules.VFD.Options, Modules.Printer.Options, Modules.PrinterWatcher.Options, Modules.RFID.Options, DeviceStatus>(
                  IO4Controller.Run,
                  AimeReader.Run,
                  VFDRunner.Run,
                  PrinterRunner.Run,
                  PrinterWatcherRunner.Run,
                  RFIDRunner.Run,
                  errs => DeviceStatus.ERR_OTHER);
            }catch(Exception ex) {
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
