#if NET8_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {

    /// <summary>
    /// Enum how the size of an image sent to the printer will be modified.
    /// </summary>
    public enum StretchMode {
        /// <summary>
        /// The image dimensions must match what the printer expects. If they are not, <see cref="CHCSeriesCardPrinter.StartPrinting(System.Drawing.Bitmap, byte[], System.Drawing.Bitmap, bool, bool)"/> will throw an exception.
        /// </summary>
        SizeMustMatch,
        /// <summary>
        /// The image will be stretched (ignoring aspect ratio) to the printer dimensions.
        /// </summary>
        Stretch,
        /// <summary>
        /// The image will be centered (thus cropped on the edges) to match printer dimensions. If the image is smaller than what the printer expects, the excess space will be blank.
        /// </summary>
        Center
    }
}

#endif