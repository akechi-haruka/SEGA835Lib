using CommandLine;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Haruka.Arcade.SEGA835Cmd.Modules.Printer.Options;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher {

    [Verb("printerwatch", HelpText = "Monitor (a) folder(s) for created files and sends them to a CHC-series printer")]
    internal class Options : GlobalOptions {

        [Option('p', "port", Required = false, HelpText = "Sets the RFID writer's port.", Default = 4)]
        public int Port { get; set; }

        [Option('m', "model", Required = false, HelpText = "The printer model to use. (CHC310,CHC330,Any)", Default = PrinterModel.Any)]
        public PrinterModel Model { get; set; }

        [Option("size", Required = false, HelpText = "Set how the image should be stretched it's size doesn't match the paper. (SizeMustMatch,Stretch,Center)", Default = StretchMode.SizeMustMatch)]
        public StretchMode Stretch { get; set; }

        [Option("rfid-dir", Required = false, HelpText = "The RFID .bin file directory to monitor.")]
        public string RFIDDirectory { get; set; }

        [Option("holo-dir", Required = false, HelpText = "The holo image directory to monitor.")]
        public string HoloDirectory { get; set; }

        [Option("delete-files", Required = false, HelpText = "Deletes all source files after printing.")]
        public bool DeleteAfterPrint { get; set; }

        [Option("continue-on-fail", Required = false, HelpText = "Resumes watching for new files, even after an error occurrs.")]
        public bool ContinueOnFail { get; set; }

        [Option("holo-pattern", Required = false, HelpText = "The file pattern to search the holo directory for.", Default = "*holo.bmp")]
        public string HoloFilePattern { get; set; }

        [Option("rfid-pattern", Required = false, HelpText = "The file pattern to search the RFID directory for.", Default = "*.bin")]
        public string RFIDFilePattern { get; set; }

        [Option("image-pattern", Required = false, HelpText = "The file pattern to search the image directory for.", Default = "*_p0.bmp")]
        public string ImageFilePattern { get; set; }

        [Value(0, MetaName = "ImageDirectory", Required = true, HelpText = "The image directory to monitor.")]
        public string ImageDirectory { get; set; }

        [Value(1, MetaName = "ICC1FileName", Required = true, HelpText = "The input .icc table file.")]
        public string ICC1FileName { get; set; }

        [Value(2, MetaName = "ICC2FileName", Required = true, HelpText = "The output .icc table file.")]
        public string ICC2FileName { get; set; }

        [Value(3, MetaName = "MtfFileName", Required = true, HelpText = "The MTF file used for printing.")]
        public string MtfFileName { get; set; }

    }
}
