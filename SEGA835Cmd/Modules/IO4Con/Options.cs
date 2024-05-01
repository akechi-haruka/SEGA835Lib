using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4Con {

    [Verb("io4con", HelpText = "Use a IO4 board as a VJoy controller")]
    internal class Options : GlobalOptions {

        [Option("controller-id", Required = false, HelpText = "The vJoy controller ID to use (default: 1)", Default = 1)]
        public uint ControllerId { get; set; }

        [Option("x-adc", Required = false, HelpText = "The ADC to use for the X axis (default: 0)", Default = 0)]
        public uint XAxisADC { get; set; }

        [Option("y-adc", Required = false, HelpText = "The ADC to use for the Y axis (default: 4)", Default = 4)]
        public uint YAxisADC { get; set; }

    }
}
