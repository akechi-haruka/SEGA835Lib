using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.LED {

    [Verb("led", HelpText = "Set outputs on LED or Monkey06 boards")]
    internal class Options : GlobalOptions {

        [Option('p', "port", Required = false, HelpText = "Sets the LED board's COM port.", Default = 9)]
        public int Port { get; set; }

        [Option('o', "offset", Required = false, HelpText = "The offset from which to start setting LEDs (note that existing will be cleared)")]
        public int Offset { get; set; }

        [Option('s', "src-addr", Required = false, HelpText = "Sets the client's address.", Default = 1)]
        public int SourceAddress { get; set; }

        [Option('d', "dest-addr", Required = false, HelpText = "Sets the destination's address.", Default = 2)]
        public int DestinationAddress { get; set; }

        [Option("monkey-reset", HelpText = "Resets configuration changes to MONKEY06 boards")]
        public bool MonkeyReset { get; set; }

        [Option("set-monkey-checksum", HelpText = "Sets the board checksum for MONKEY06 boards (this will persist until monkeyreset)")]
        public ushort MonkeyChecksum { get; set; }

        [Option("set-monkey-version", HelpText = "Sets the board firmware version for MONKEY06 boards (this will persist until monkey reset)")]
        public byte MonkeyVersion { get; set; }

        [Option("set-monkey-board-name", HelpText = "Sets the board name for MONKEY06 boards (this will persist until monkey reset, maximum 8 characters)")]
        public String MonkeyBoardName { get; set; }

        [Option("set-monkey-chip-number", HelpText = "Sets the chip number for MONKEY06 boards (this will persist until monkey reset, maximum 5 characters)")]
        public String MonkeyChipNumber { get; set; }

        [Option("set-monkey-translation", HelpText = "Sets the LED translation table for MONKEY06 boards (comma seperated list of numbers)")]
        public String MonkeyTable { get; set; }

        [Option("set-monkey-channels", Required = false, HelpText = "Sets the order of input data channels for a MONKEY06 board (comma seperated list of Red,Green,Blue with 3 entries)")]
        public String MonkeyChannels { get; set; }

        [Value(1, HelpText = "Set LEDs (comma seperated list of R,G,B,R,G,B,... - or if channels is set, the order of that parameter)")]
        public String LEDTable { get; set; }

    }
}
