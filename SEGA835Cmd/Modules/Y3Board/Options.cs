using CommandLine;
using JetBrains.Annotations;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Y3Board;

[Verb("y3", HelpText = "Read Taisen-series cards from a Y3 board")]
[UsedImplicitly]
class Options : GlobalOptions {
    [Option('p', "port", Required = false, HelpText = "Sets the board's COM port.", Default = 8)]
    public int Port { get; set; }

    [Option("find-one", Required = false, HelpText = "Exits after finding one card.")]
    public bool FindOne { get; set; }

    [Option("no-exit-button", Required = false, HelpText = "Disables pressing ESC to exit.")]
    public bool NoExitButton { get; set; }
}