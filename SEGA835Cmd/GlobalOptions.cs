using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Cmd {
    internal class GlobalOptions {

        [Option('s', "silent", Required = false, HelpText = "Disables log output to console.")]
        public bool Silent { get; set; }

        [Option('f', "log-file", Required = false, HelpText = "Enables logging to the specified file.")]
        public string LogFile { get; set; }

    }
}
