using CommandLine;

namespace Haruka.Arcade.SEGA835Cmd.Modules.RFID {
    [Verb("rfid", HelpText = "Read RFID cards from certain games")]
    internal class Options : GlobalOptions {
        [Option('p', "port", Required = false, HelpText = "Sets the RFID reader's port.", Default = 2)]
        public int Port { get; set; }

        [Option("wait-for-cards", Required = false, HelpText = "Waits for the specified number of cards (or more).", Default = 1)]
        public int WaitUntil { get; set; }
    }
}