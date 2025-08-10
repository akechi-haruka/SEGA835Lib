#if !LINUX

using CommandLine;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterInfo {
    [Verb("printerinfo", HelpText = "Query information from CHC-series printers")]
    internal class Options : GlobalOptions {
        public enum PrinterModel {
            CHC310, CHC310B, CHC330, Any
        }

        [Option('m', "model", Required = false, HelpText = "The printer model to use. (CHC310,CHC330,Any)", Default = PrinterModel.Any)]
        public PrinterModel Model { get; set; }
    }
}

#endif