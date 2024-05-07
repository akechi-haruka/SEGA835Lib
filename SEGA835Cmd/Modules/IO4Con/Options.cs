using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd.Modules.IO4Con {

    [Verb("io4con", HelpText = "Use a IO4 board as a VJoy controller")]
    internal class Options : GlobalOptions {

        [Option("controller-id", Required = false, HelpText = "The vJoy controller ID to use.", Default = (uint)1)]
        public uint ControllerId { get; set; }

        [Option("x-adc", Required = false, HelpText = "The ADC to use for the X axis.", Default = (uint)0)]
        public uint XAxisADC { get; set; }

        [Option("y-adc", Required = false, HelpText = "The ADC to use for the Y axis.", Default = (uint)4)]
        public uint YAxisADC { get; set; }

        [Option("invert-x", Required = false, HelpText = "Inverts the X-Axis.")]
        public bool XFlip { get; set; }

        [Option("invert-y", Required = false, HelpText = "Inverts the Y-Axis.")]
        public bool YFlip { get; set; }

        [Option("poll-delay", Required = false, HelpText = "Sets the amount of milliseconds between polls", Default = 10)]
        public int PollDelay { get; set; }

        [Option("no-exit-button", Required = false, HelpText = "Disables pressing ESC to exit.")]
        public bool NoExitButton { get; set; }

    }
}
