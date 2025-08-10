using CommandLine;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4 {
    [Verb("io4", HelpText = "Set outputs on IO4 boards")]
    internal class Options : GlobalOptions {
        internal enum OutputType {
            GPIO, LED
        }

        [Option("clear", Required = false, HelpText = "Clears all other outputs of the same type.")]
        public bool Clear { get; set; }

        [Value(1, Required = true, HelpText = "Select the type to output (GPIO, LED)")]
        public OutputType Output { get; set; }

        [Value(2, Required = true, HelpText = "The output index [0,32)")]
        public int Index { get; set; }

        [Value(3, Required = true, HelpText = "The output value (GPIO = 0/1, LED = 0~255)")]
        public int Value { get; set; }
    }
}