#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags {
    /// <summary>
    /// A structure returned by a call to <see cref="ChcSeriesCardPrinter.GetPrinterInfo(PrinterInfoTag, out byte[])" /> with <see cref="PrinterInfoTag.PageStatus"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public unsafe struct PageStatus {
        /// <summary>
        /// Remaining holo prints
        /// </summary>
        public byte HoloRemain;

        private fixed byte padding[31];
    }
}

#endif