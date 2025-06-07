#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags {
    /// <summary>
    /// A structure returned by a call to <see cref="CHCSeriesCardPrinter.GetPrinterInfo(PrinterInfoTag, out byte[])" /> with <see cref="PrinterInfoTag.PRINTCNT"/>.
    /// </summary>
    public struct PrintCnt {
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint printCounter0;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint printCounter1;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint feedRoller;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint cutterCount;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint headCount;
        /// <summary>
        /// Remaining prints on the color ribbon.
        /// </summary>
        public uint ribbonRemain;
    }
}

#endif