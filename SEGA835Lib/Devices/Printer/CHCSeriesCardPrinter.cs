using Haruka.Arcade.SEGA835Lib.Debugging;
using System.Linq;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer {
    public abstract class CHCSeriesCardPrinter : Device {

        protected const ushort PRINTER_STATUS_OK = 0;
        private const ushort PRINTER_ERROR_UNKNOWN = 6810;
        private const ushort PRINTER_ERROR_NOT_CONNECTED = 1004;
        private static readonly PrinterError[] ERROR_TABLE = new PrinterError[] {
            new PrinterError(PRINTER_STATUS_OK, 0, "OK"),
            new PrinterError(2303, 0, "NG"),
            new PrinterError(2304, 6801, "Card tray is empty"),
            new PrinterError(2305, 6801, "Card tray is empty"),
            new PrinterError(2306, 6802, 1, "No color ribbon is present"),
            new PrinterError(2307, 6803, "Color ribbon data could not be read"),
            new PrinterError(2308, 6802, 2, "No holo ribbon is present"),
            new PrinterError(2309, 6807, "Printer lid is open"),
            new PrinterError(2310, 6808, "Card compartment is open"),
            new PrinterError(2420, 6820, "Color ribbon is not compatible"),
            new PrinterError(2421, 6820, "Color ribbon is not compatible"),
            new PrinterError(2422, 6820, "Color ribbon is not compatible"),
            new PrinterError(2423, 6820, "Color ribbon is not compatible"),
            new PrinterError(2424, 6820, "Color ribbon is not compatible"),
            new PrinterError(2425, 6820, "Color ribbon is not compatible"),
            new PrinterError(2426, 6820, "Color ribbon is not compatible"),
            new PrinterError(2427, 6820, "Color ribbon is not compatible"),
            new PrinterError(2428, 6820, "Color ribbon is not compatible"),
            new PrinterError(2429, 6820, "Color ribbon is not compatible"),
            new PrinterError(2100, 0, "NG"),
            new PrinterError(2101, 0, "NG"),
            new PrinterError(2102, 0, "NG"),
            new PrinterError(2103, 0, "NG"),
            new PrinterError(2105, 0, "NG"),
            new PrinterError(2107, 0, "NG"),
            new PrinterError(2104, 6810, "Other printer error"),
            new PrinterError(1002, 0, "Initialization failed (missing DLLs?)"),
            new PrinterError(PRINTER_ERROR_NOT_CONNECTED, 0, "No printer connected"),
            new PrinterError(1500, 6810, "Other printer error"),
            new PrinterError(1501, 6810, "Other printer error"),
            new PrinterError(1502, 0, "Invalid argument"),
            new PrinterError(2504, 6810, "Other printer error"),
            new PrinterError(2406, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2407, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2401, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2150, 0, "NG"),
            new PrinterError(2600, 6804, "Communication error with printer"),
            new PrinterError(4051, 6818, "Printer's RFID board is not recognized"),
            new PrinterError(3041, 6805, "A card is jammed"),
            new PrinterError(3042, 6805, "A card is jammed"),
            new PrinterError(3043, 6805, "A card is jammed"),
            new PrinterError(3044, 6805, "A card is jammed"),
            new PrinterError(3045, 6805, "A card is jammed"),
            new PrinterError(3046, 6805, "A card is jammed"),
            new PrinterError(3047, 6805, "A card is jammed"),
            new PrinterError(3048, 6805, "A card is jammed"),
            new PrinterError(3049, 6805, "A card is jammed"),
            new PrinterError(3050, 6805, "A card is jammed"),
            new PrinterError(3051, 6805, "A card is jammed"),
            new PrinterError(3053, 6805, "A card is jammed"),
            new PrinterError(3054, 6805, "A card is jammed"),
            new PrinterError(3055, 6805, "A card is jammed"),
            new PrinterError(3056, 6805, "A card is jammed"),
            new PrinterError(3057, 6805, "A card is jammed"),
            new PrinterError(3058, 6805, "A card is jammed"),
            new PrinterError(3059, 6805, "A card is jammed"),
            new PrinterError(3060, 6805, "A card is jammed"),
            new PrinterError(3061, 6805, "A card is jammed"),
            new PrinterError(3082, 6805, "A card is jammed"),
            new PrinterError(3083, 6805, "A card is jammed"),
            new PrinterError(3084, 6805, "A card is jammed"),
            new PrinterError(3085, 6805, "A card is jammed"),
            new PrinterError(3086, 6805, "A card is jammed"),
            new PrinterError(3087, 6805, "A card is jammed"),
            new PrinterError(3089, 6805, "A card is jammed"),
            new PrinterError(3090, 6805, "A card is jammed"),
            new PrinterError(3052, 6814, "The card could not be ejected properly"),
            new PrinterError(3088, 6815, "A card or object is stuck in the dispensing slot"),
            new PrinterError(2315, 6809, "The printer scrap box is full"),
            new PrinterError(2316, 6812, "Color ribbon can not be recongized"),
            new PrinterError(2317, 6813, 1, "Failed to cut or wind the color ribbon"),
            new PrinterError(2318, 6813, 2, "Failed to cut or wind the holo ribbon"),
            new PrinterError(9999, 9999, "No return code")
        };

        public abstract ushort GetPrinterStatusCode();

        protected DeviceStatus SetLastErrorByRC(int i, ushort rc = 0xFFFF) {
            if (i == 1) {
                return SetLastError(DeviceStatus.OK);
            } else {
                if (rc == 0xFFFF) {
                    rc = GetPrinterStatusCode();
                }
                Log.WriteError("Printer Error (" + rc + "): " + GetPrinterErrorAsString(rc));
                if (rc == PRINTER_ERROR_NOT_CONNECTED) {
                    return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
                }
                return SetLastError((DeviceStatus)rc);
            }
        }

        public static String GetPrinterErrorText(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).Message;
        }

        public static int GetExternalPrinterErrorCode(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).ErrorCodeExt;
        }

        public static string GetPrinterErrorAsString(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).ToString();
        }
    }
}