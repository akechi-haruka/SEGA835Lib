#if LINUX
using System.Text;
#else
using System.Drawing;
using Haruka.Arcade.SEGA835Cmd.Modules.Printer;
using Haruka.Arcade.SEGA835Cmd.Modules.PrinterInfo;
using Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher;
using Haruka.Arcade.SEGA835Cmd.Modules.Y3Board;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
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
            Parser parser = new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.AutoHelp = true;
                settings.HelpWriter = Console.Error;
            });

            args = NormalizeArguments(args);
#if LINUX
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                return (int)parser.ParseArguments
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
            return (int)parser.ParseArguments
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
            LogManager.FlushAndClose();
        }
    }

    private static string[] NormalizeArguments(string[] args) {
        if (args.Length == 0) {
            return args;
        }

        string[] normalized = (string[])args.Clone();
        NormalizeVerb(normalized);

#if !LINUX
        NormalizeOptionEnum<Modules.Printer.Options.PrinterModel>(normalized, "-m", "--model");
        NormalizeOptionEnum<StretchMode>(normalized, null, "--size");
        NormalizeOptionEnum<RotateFlipType>(normalized, null, "--holo-rf");
        NormalizeOptionEnum<RotateFlipType>(normalized, null, "--image-rf");
#endif

        return normalized;
    }

    private static void NormalizeVerb(string[] args) {
        string[] verbs = {
            "aime",
            "io4",
            "io4con",
            "led",
            "rfid",
            "vfd",
#if !LINUX
            "printer",
            "printer-info",
            "printer-watcher",
            "y3",
#endif
        };

        foreach (string verb in verbs) {
            if (String.Equals(args[0], verb, StringComparison.OrdinalIgnoreCase)) {
                args[0] = verb;
                return;
            }
        }
    }

    private static void NormalizeOptionEnum<TEnum>(string[] args, string shortOption, string longOption) where TEnum : struct, Enum {
        for (int i = 0; i < args.Length; i++) {
            if (longOption != null && args[i].StartsWith(longOption + "=", StringComparison.OrdinalIgnoreCase)) {
                int valueStart = longOption.Length + 1;
                args[i] = longOption + "=" + NormalizeEnumValue<TEnum>(args[i].Substring(valueStart));
                continue;
            }

            if ((shortOption != null && String.Equals(args[i], shortOption, StringComparison.OrdinalIgnoreCase))
                || (longOption != null && String.Equals(args[i], longOption, StringComparison.OrdinalIgnoreCase))) {
                if (i + 1 < args.Length) {
                    args[i + 1] = NormalizeEnumValue<TEnum>(args[i + 1]);
                }
            }
        }
    }

    private static string NormalizeEnumValue<TEnum>(string value) where TEnum : struct, Enum {
        foreach (string name in Enum.GetNames<TEnum>()) {
            if (String.Equals(value, name, StringComparison.OrdinalIgnoreCase)) {
                return name;
            }
        }

        return value;
    }

    internal static void SetGlobalOptions(GlobalOptions options) {
        LogManager.Initialize(LoggerFactory.Create(builder => {
            builder.SetMinimumLevel(LogLevel.Trace);
            if (!options.Silent) {
                builder.AddSimpleConsole(console => { console.SingleLine = true; });
            }

            builder.AddDebug();

            if (options.LogFile != null) {
                builder.AddFile(options.LogFile);
            }
        }));
    }
}