using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.LED {

    [Verb("led", HelpText = "Set outputs on LED or Monkey06 boards")]
    internal class Options : GlobalOptions {

        [Option('p', "port", Required = false, HelpText = "Sets the LED board's COM port.", Default = 10)]
        public int Port { get; set; }

        [Option('s', "src-addr", Required = false, HelpText = "Sets the client's address.", Default = 1)]
        public int SourceAddress { get; set; }

        [Option('d', "dest-addr", Required = false, HelpText = "Sets the destination's address.", Default = 2)]
        public int DestinationAddress { get; set; }

        [Option("monkey-reset", HelpText = "Resets configuration changes to MONKEY06 boards")]
        public bool MonkeyReset { get; set; }

        [Option("set-monkey-checksum", HelpText = "Sets the board checksum for MONKEY06 boards (this will persist until reset)")]
        public ushort MonkeyChecksum { get; set; }

        [Option("set-monkey-translation", HelpText = "Sets the LED translation table for MONKEY06 boards (comma seperated list of numbers)")]
        public String MonkeyTable { get; set; }

        [Value(1, HelpText = "Set LEDs (comma seperated list of R,G,B,R,G,B,...)")]
        public String LEDTable { get; set; }

    }
}
