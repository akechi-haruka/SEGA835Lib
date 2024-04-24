namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    public struct PrinterError {
        public int ErrorCodeInt;
        public int ErrorCodeExt;
        public int ErrorCodeExtSub;
        public string Message;

        public PrinterError(int errorCodeInt, int errorCodeExt, string message) {
            this.ErrorCodeInt = errorCodeInt;
            this.ErrorCodeExt = errorCodeExt;
            this.Message = message;
        }

        public PrinterError(int errorCodeInt, int errorCodeExt, int errorCodeExtSub, string message) {
            this.ErrorCodeInt = errorCodeInt;
            this.ErrorCodeExt = errorCodeExt;
            this.ErrorCodeExtSub = errorCodeExtSub;
            this.Message = message;
        }

        public override string ToString() {
            return "[" + ErrorCodeExt + (ErrorCodeExtSub != 0 ? "-" + ErrorCodeExtSub : "") + "] " + Message + " (" + ErrorCodeInt + ")";
        }
    }
}