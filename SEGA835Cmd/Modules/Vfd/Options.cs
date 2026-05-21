using CommandLine;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using JetBrains.Annotations;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Vfd;

[Verb("vfd", HelpText = "Display text on a VFD")]
[UsedImplicitly]
class Options : GlobalOptions {
    [Option('p', "port", Required = false, HelpText = "Sets the VFD's port.", Default = 1)]
    public int Port { get; set; }

    [Option('b', "brightness", Required = false, HelpText = "Sets the display brightness. (Off,Level1,Level2,Level3,Level4)", Default = VfdBrightnessLevel.Level2)]
    public VfdBrightnessLevel Brightness { get; set; }

    [Option("scroll-speed", Required = false, HelpText = "Sets the scrolling speed. (Slow,Fast)", Default = VfdTextScrollSpeed.Slow)]
    public VfdTextScrollSpeed Speed { get; set; }

    [Option("scroll-line", Required = false, HelpText = "Select the line that should scroll (1 or 2, 0 for none)", Default = 1)]
    public int ScrollLine { get; set; }

    [Option('e', "encoding", Required = false, HelpText = "Sets the text encoding. (ShiftJis,Big5,Gb2312,Ksc5601)", Default = VfdEncoding.ShiftJis)]
    public VfdEncoding Encoding { get; set; }

    [Option("get-version", Required = false, HelpText = "Returns the board version instead of sending text")]
    public bool GetVersion { get; set; }

    [Value(0, MetaName = "Text Line 1", Required = true, HelpText = "The text to display on the VFD.")]
    public string Text { get; set; }

    [Value(1, MetaName = "Text Line 2", Required = false, HelpText = "The text to display on the VFD (second line).", Default = "")]
    public string Text2 { get; set; }
}