using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer {

    /// <summary>
    /// Enum defining printing progress.
    /// </summary>
    public enum PrintStatus {
        /// <summary>
        /// Not printing.
        /// </summary>
        None,
        /// <summary>
        /// Printer is preparing.
        /// </summary>
        Started,
        /// <summary>
        /// Card data is being read.
        /// </summary>
        RFIDRead,
        /// <summary>
        /// Card data is being written.
        /// </summary>
        RFIDWrite,
        /// <summary>
        /// Properties (MTF, ICC, ...) are being set.
        /// </summary>
        SetPrinterProperties,
        /// <summary>
        /// The image is being uploaded to the printer.
        /// </summary>
        SetImage,
        /// <summary>
        /// The printer is printing.
        /// </summary>
        Printing,
        /// <summary>
        /// The printing is being finished.
        /// </summary>
        Ending,
        /// <summary>
        /// The card is being ejected.
        /// </summary>
        Ejecting,
        /// <summary>
        /// The print was successfully finished.
        /// </summary>
        Finished,
        /// <summary>
        /// An error occurred.
        /// </summary>
        Errored
    }
}
