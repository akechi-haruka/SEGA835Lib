#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags {
    /// <summary>
    /// A structure returned by a call to <see cref="ChcSeriesCardPrinter.GetPrinterInfo(PrinterInfoTag, out byte[])" /> with <see cref="PrinterInfoTag.PrintCount"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public struct PrintCnt {
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint PrintCounter0;

        /// <summary>
        /// Unknown.
        /// </summary>
        public uint PrintCounter1;

        /// <summary>
        /// Unknown.
        /// </summary>
        public uint FeedRoller;

        /// <summary>
        /// Unknown.
        /// </summary>
        public uint CutterCount;

        /// <summary>
        /// Unknown.
        /// </summary>
        public uint HeadCount;

        /// <summary>
        /// Remaining prints on the color ribbon.
        /// </summary>
        public uint RibbonRemain;
    }
}

#endif