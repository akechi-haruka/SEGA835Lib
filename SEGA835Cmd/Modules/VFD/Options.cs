using CommandLine;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.VFD {

    [Verb("vfd", HelpText = "Use a IO4 board as a VJoy controller")]
    internal class Options : GlobalOptions {

        [Option('p', "port", Required = false, HelpText = "Sets the VFD's port.", Default = 1)]
        public int Port { get; set; }

        [Option('b', "brightness", Required = false, HelpText = "Sets the display brightness. (OFF,LEVEL1,LEVEL2,LEVEL3,LEVEL4)", Default = VFDBrightnessLevel.LEVEL2)]
        public VFDBrightnessLevel Brightness { get; set; }

        [Option("scroll-speed", Required = false, HelpText = "Sets the scrolling speed. (SLOW,FAST)", Default = VFDTextScrollSpeed.SLOW)]
        public VFDTextScrollSpeed Speed { get; set; }

        [Option("no-scroll", Required = false, HelpText = "Disable scrolling.")]
        public bool NoScroll { get; set; }

        [Option('e', "encoding", Required = false, HelpText = "Sets the text encoding. (SHIFT_JIS,BIG5,GB2312,KSC5601)", Default = VFDEncoding.SHIFT_JIS)]
        public VFDEncoding Encoding { get; set; }

        [Value(0, MetaName = "Text", Required = true, HelpText = "The text to display on the VFD.")]
        public string Text { get; set; }

    }
}
