namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    /// <summary>
    /// Definition of a printer error.
    /// </summary>
    public readonly struct PrinterError {
        /// <summary>
        /// The (DLL-) internal error code.
        /// </summary>
        public readonly int ErrorCodeInt;

        /// <summary>
        /// The SEGA external error code.
        /// </summary>
        public readonly int ErrorCodeExt;

        /// <summary>
        /// The SEGA external sub-error code.
        /// </summary>
        public readonly int ErrorCodeExtSub;

        /// <summary>
        /// A message describing the error.
        /// </summary>
        public readonly string Message;

        internal PrinterError(int errorCodeInt, int errorCodeExt, string message) {
            ErrorCodeInt = errorCodeInt;
            ErrorCodeExt = errorCodeExt;
            ErrorCodeExtSub = 0;
            Message = message;
        }

        internal PrinterError(int errorCodeInt, int errorCodeExt, int errorCodeExtSub, string message) {
            ErrorCodeInt = errorCodeInt;
            ErrorCodeExt = errorCodeExt;
            ErrorCodeExtSub = errorCodeExtSub;
            Message = message;
        }

        /// <inheritdoc />
        public override string ToString() {
            return "[" + ErrorCodeExt + (ErrorCodeExtSub != 0 ? "-" + ErrorCodeExtSub : "") + "] " + Message + " (" + ErrorCodeInt + ")";
        }
    }
}