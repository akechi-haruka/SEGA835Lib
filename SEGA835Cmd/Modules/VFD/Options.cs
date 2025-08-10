using CommandLine;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;

namespace Haruka.Arcade.SEGA835Cmd.Modules.VFD {
    [Verb("vfd", HelpText = "Display text on a VFD")]
    internal class Options : GlobalOptions {
        [Option('p', "port", Required = false, HelpText = "Sets the VFD's port.", Default = 1)]
        public int Port { get; set; }

        [Option('b', "brightness", Required = false, HelpText = "Sets the display brightness. (OFF,LEVEL1,LEVEL2,LEVEL3,LEVEL4)", Default = VFDBrightnessLevel.LEVEL2)]
        public VFDBrightnessLevel Brightness { get; set; }

        [Option("scroll-speed", Required = false, HelpText = "Sets the scrolling speed. (SLOW,FAST)", Default = VFDTextScrollSpeed.SLOW)]
        public VFDTextScrollSpeed Speed { get; set; }

        [Option("scroll-line", Required = false, HelpText = "Select the line that should scroll (1 or 2, 0 for none)", Default = 1)]
        public int ScrollLine { get; set; }

        [Option('e', "encoding", Required = false, HelpText = "Sets the text encoding. (SHIFT_JIS,BIG5,GB2312,KSC5601)", Default = VFDEncoding.SHIFT_JIS)]
        public VFDEncoding Encoding { get; set; }

        [Option("get-version", Required = false, HelpText = "Returns the board version instead of sending text")]
        public bool GetVersion { get; set; }

        [Value(0, MetaName = "Text Line 1", Required = true, HelpText = "The text to display on the VFD.")]
        public string Text { get; set; }

        [Value(1, MetaName = "Text Line 2", Required = false, HelpText = "The text to display on the VFD (second line).", Default = "")]
        public string Text2 { get; set; }
    }
}