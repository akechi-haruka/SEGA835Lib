#if LINUX
using System.Text;
#else
using Haruka.Arcade.SEGA835Cmd.Modules.Printer;
using Haruka.Arcade.SEGA835Cmd.Modules.PrinterInfo;
using Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher;
using Haruka.Arcade.SEGA835Cmd.Modules.Y3Board;
#endif
using CommandLine;
using Haruka.Arcade.SEGA835Cmd.Modules.AimeReader;
using Haruka.Arcade.SEGA835Cmd.Modules.Io4;
using Haruka.Arcade.SEGA835Cmd.Modules.Io4Con;
using Haruka.Arcade.SEGA835Cmd.Modules.Led;
using Haruka.Arcade.SEGA835Cmd.Modules.Rfid;
using Haruka.Arcade.SEGA835Cmd.Modules.Vfd;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Options = Haruka.Arcade.SEGA835Cmd.Modules.Io4Con.Options;

namespace Haruka.Arcade.SEGA835Cmd;

static class Program {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Program));

    private static int Main(string[] args) {
        try {
#if LINUX
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                return (int)Parser.Default.ParseArguments
                    <Modules.Io4Con.Options, Modules.AimeReader.Options, Modules.Vfd.Options, Modules.Rfid.Options, Modules.Io4.Options, Modules.Led.Options>(args)
                    .MapResult<Modules.Io4Con.Options, Modules.AimeReader.Options, Modules.Vfd.Options, Modules.Rfid.Options, Modules.Io4.Options, Modules.Led.Options, DeviceStatus>(
                  Io4Controller.Run,
                  AimeRunner.Run,
                  VfdRunner.Run,
                  RfidRunner.Run,
                  Io4Runner.Run,
                  LedRunner.Run,
                  _ => DeviceStatus.ErrorOther);
#else
            return (int)Parser.Default.ParseArguments
                    <Options, Modules.AimeReader.Options, Modules.Vfd.Options, Modules.Printer.Options, Modules.PrinterInfo.Options, Modules.PrinterWatcher.Options, Modules.Rfid.Options, Modules.Io4.Options, Modules.Led.Options, Modules.Y3Board.Options>(args)
                .MapResult<Options, Modules.AimeReader.Options, Modules.Vfd.Options, Modules.Printer.Options, Modules.PrinterInfo.Options, Modules.PrinterWatcher.Options, Modules.Rfid.Options, Modules.Io4.Options, Modules.Led.Options, Modules.Y3Board.Options, DeviceStatus>(
                    Io4Controller.Run,
                    AimeRunner.Run,
                    VfdRunner.Run,
                    PrinterRunner.Run,
                    PrinterInfoRunner.Run,
                    PrinterWatcherRunner.Run,
                    RfidRunner.Run,
                    Io4Runner.Run,
                    LedRunner.Run,
                    Y3Runner.Run,
                    _ => DeviceStatus.ErrorOther);
#endif
        } catch (Exception ex) {
            LOG.LogCritical(ex, "An error has occurred");
            return Int32.MinValue;
        } finally {
            LOG.LogInformation("Exiting");
        }
    }

    internal static void SetGlobalOptions(GlobalOptions options) {
        LogManager.Initialize(LoggerFactory.Create(builder => {
            builder.SetMinimumLevel(LogLevel.Trace);
            if (!options.Silent) {
                builder.AddConsole();
            }

            builder.AddDebug();

            if (options.LogFile != null) {
                builder.AddFile(options.LogFile);
            }
        }));
    }
}