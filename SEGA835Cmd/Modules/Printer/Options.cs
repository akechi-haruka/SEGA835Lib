#if !RASPBERRY

using CommandLine;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using System.Drawing;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Printer {
    [Verb("printer", HelpText = "Print things to CHC-series printers")]
    internal class Options : GlobalOptions {
        public enum PrinterModel {
            CHC310, CHC310B, CHC330, Any
        }

        [Option('p', "port", Required = false, HelpText = "Sets the RFID writer's port.", Default = 4)]
        public int Port { get; set; }

        [Option('m', "model", Required = false, HelpText = "The printer model to use. (CHC310,CHC330,Any)", Default = PrinterModel.Any)]
        public PrinterModel Model { get; set; }

        [Option("no-wait", Required = false, HelpText = "Does not wait for successful print completion, will return immediately with exit code BUSY (-1).")]
        public bool NoWait { get; set; }

        [Option("print-card-id", Required = false, HelpText = "Prints the written RFID card ID to stdout.")]
        public bool PrintCardId { get; set; }

        [Option("size", Required = false, HelpText = "Set how the image should be stretched it's size doesn't match the paper. (SizeMustMatch,Stretch,Center)", Default = StretchMode.SizeMustMatch)]
        public StretchMode Stretch { get; set; }

        [Option("rfid-data", Required = false, HelpText = "The RFID .bin file to write.")]
        public string RFIDFileName { get; set; }

        [Option("rfid-override-card-id", Required = false, Hidden = true)]
        public bool RFIDOverrideCardId { get; set; }

        [Option("holo-simplify", Required = false, HelpText = "Strips everything except fully black (0,0,0) pixels from the holo")]
        public bool HoloSimplify { get; set; }

        [Option("holo-rf", Required = false, HelpText = "Rotate and/or flip the holo image", Default = RotateFlipType.RotateNoneFlipNone)]
        public RotateFlipType HoloRotateFlip { get; set; }

        [Option("image-rf", Required = false, HelpText = "Rotate and/or flip the image", Default = RotateFlipType.RotateNoneFlipNone)]
        public RotateFlipType ImageRotateFlip { get; set; }

        [Option("holo", Required = false, HelpText = "The holo image file to print.")]
        public string HoloFileName { get; set; }

        [Value(0, MetaName = "ImageFileName", Required = true, HelpText = "The image file to print.")]
        public string ImageFileName { get; set; }

        [Value(1, MetaName = "ICC1FileName", Required = true, HelpText = "The input .icc table file.")]
        public string ICC1FileName { get; set; }

        [Value(2, MetaName = "ICC2FileName", Required = true, HelpText = "The output .icc table file.")]
        public string ICC2FileName { get; set; }

        [Value(3, MetaName = "MtfFileName", Required = true, HelpText = "The MTF file used for printing.")]
        public string MtfFileName { get; set; }
    }
}

#endif