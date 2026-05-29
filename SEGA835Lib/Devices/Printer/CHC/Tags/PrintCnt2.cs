#if NET8_0_OR_GREATER
namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags {
    /// <summary>
    /// A structure returned by a call to <see cref="ChcSeriesCardPrinter.GetPrinterInfo(PrinterInfoTag, out byte[])" /> with <see cref="PrinterInfoTag.PrintCount2"/>.
    /// </summary>
    public struct PrintCnt2 {
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
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint HoloCount;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint PaperCount;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint PrintCounter2;
        /// <summary>
        /// Unknown.
        /// </summary>
        public uint HoloPrintCounter;
    }
}

#endif