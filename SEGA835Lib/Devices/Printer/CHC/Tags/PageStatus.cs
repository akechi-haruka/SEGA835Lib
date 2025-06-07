#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags {
    /// <summary>
    /// A structure returned by a call to <see cref="CHCSeriesCardPrinter.GetPrinterInfo(PrinterInfoTag, out byte[])" /> with <see cref="PrinterInfoTag.PAGESTATUS"/>.
    /// </summary>
    public unsafe struct PageStatus {
        /// <summary>
        /// Remaining holo prints
        /// </summary>
        public byte holoRemain;
        private fixed byte padding[31];
    }
}

#endif