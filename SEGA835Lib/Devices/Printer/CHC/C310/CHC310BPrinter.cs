#if NET8_0_OR_GREATER

using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;

using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Drawing;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310 {

    /// <summary>
    /// A CHC-310B Card Printer for CardMaker.
    /// Fully inherits functions from the CHC-310.
    /// </summary>
    public class CHC310BPrinter : CHC310Printer {

        private static readonly NativeB native = new NativeB(); // hack to pass the same Native to both parameters

        /// <summary>
        /// Creates a new CHC-310B printer.
        /// </summary>
        public CHC310BPrinter() : base(native, new RFIDBackendCHCDLL(native), new Size(768, 1052)) {
        }
        
    }
}

#endif