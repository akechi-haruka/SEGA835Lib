using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags {
    /// <summary>
    /// A structure returned by a call to <see cref="CHCSeriesCardPrinter.GetPrinterInfo(PrinterInfoTag, out byte[])" /> with <see cref="PrinterInfoTag.PRINTCNT2"/>.
    /// </summary>
    public struct PrintCnt2 {
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
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint holoCount;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint paperCount;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint printCounter2;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint holoPrintCounter;
    }
}
