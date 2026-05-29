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
        CardDataRead,

        /// <summary>
        /// RFID Card data is being written before printing.
        /// </summary>
        CardDataWriteRfid,

        /// <summary>
        /// Properties (MTF, ICC, ...) are being set.
        /// </summary>
        SetPrinterProperties,

        /// <summary>
        /// The image is being uploaded to the printer.
        /// </summary>
        SetImage,

        /// <summary>
        /// IR data is being written.
        /// </summary>
        SetImageIr,

        /// <summary>
        /// The printer is printing.
        /// </summary>
        Printing,

        /// <summary>
        /// User-defined post-processing functions are running.
        /// </summary>
        Postprocessing,

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