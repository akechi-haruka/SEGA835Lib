#if !LINUX

using CommandLine;
using JetBrains.Annotations;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterInfo;

[Verb("printerinfo", HelpText = "Query information from CHC-series printers")]
[UsedImplicitly]
class Options : GlobalOptions {
    public enum PrinterModel {
        Chc310,
        Chc310B,
        Chc320,
        Chc330,
        Any
    }

    [Option('m', "model", Required = false, HelpText = "The printer model to use. (Chc310,Chc320,Chc330,Any)", Default = PrinterModel.Any)]
    public PrinterModel Model { get; set; }
}

#endif