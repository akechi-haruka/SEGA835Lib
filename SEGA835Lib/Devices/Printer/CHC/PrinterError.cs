namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {

    /// <summary>
    /// Definition of a printer error.
    /// </summary>
    public struct PrinterError {
        /// <summary>
        /// The (DLL-) internal error code.
        /// </summary>
        public int ErrorCodeInt;
        /// <summary>
        /// The SEGA external error code.
        /// </summary>
        public int ErrorCodeExt;
        /// <summary>
        /// The SEGA external sub-error code.
        /// </summary>
        public int ErrorCodeExtSub;
        /// <summary>
        /// A message describing the error.
        /// </summary>
        public string Message;

        internal PrinterError(int errorCodeInt, int errorCodeExt, string message) {
            this.ErrorCodeInt = errorCodeInt;
            this.ErrorCodeExt = errorCodeExt;
            this.ErrorCodeExtSub = 0;
            this.Message = message;
        }

        internal PrinterError(int errorCodeInt, int errorCodeExt, int errorCodeExtSub, string message) {
            this.ErrorCodeInt = errorCodeInt;
            this.ErrorCodeExt = errorCodeExt;
            this.ErrorCodeExtSub = errorCodeExtSub;
            this.Message = message;
        }

        /// <inheritdoc />
        public override string ToString() {
            return "[" + ErrorCodeExt + (ErrorCodeExtSub != 0 ? "-" + ErrorCodeExtSub : "") + "] " + Message + " (" + ErrorCodeInt + ")";
        }
    }
}