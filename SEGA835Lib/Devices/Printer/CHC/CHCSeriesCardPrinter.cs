#if NET8_0_OR_GREATER
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    /// <summary>
    /// The base class for a CHC-series card printer.
    /// Note that all calls to the given <see cref="INativeTrampolineChc"/> must be done via <see cref="ExecuteOnPrintThread(PrinterThreadDelegate, bool, bool)"/> as the library checks from what thread any printer function is accessed.
    /// </summary>
    public abstract partial class ChcSeriesCardPrinter : Device {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(ChcSeriesCardPrinter));

        /// <summary>
        /// A function of the printer that waits until a non-busy status is returned.
        /// </summary>
        /// <param name="rc">The variable that the printer status will be passed to upon completion.</param>
        /// <returns>The printer function return code (CHCUSB_RC_*)</returns>
        public delegate int StatusWaitDelegate(ref ushort rc);

        /// <summary>
        /// A function that should be run on the printer thread.
        /// </summary>
        /// <param name="rc">The variable that the printer status will be passed to upon completion.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other status on error.</returns>
        public delegate DeviceStatus PrinterThreadDelegate(ref ushort rc);

        #region CHC constants

        internal const int CARD_ID_LEN = 12;

        // We are not documenting these... Again, 90% "Unknown".
#pragma warning disable CS1591
        public const int CHCUSB_RC_BUSY = 0;
        public const int CHCUSB_RC_OK = 1;
        public const int CHCUSB_RC_ERROR = -1;

        private const ushort PRINTER_ERROR_UNKNOWN = 6810;

        public const int RESULT_NOERROR = 0;
        protected const int RESULT_MEMFULLERR = 1000;
        protected const int RESULT_USBNOTFOUND = 1002;
        protected const int RESULT_ICCNOTFOUND = 1003;
        protected const int RESULT_OPENPORT = 1004;
        protected const int RESULT_FILEACCESS = 1005;
        protected const int RESULT_PRMERROR = 1006;
        protected const int RESULT_NOTFOUND = 1008;
        protected const int RESULT_IMAINGNOTFOUND = 1009;
        protected const int RESULT_MUTEX_WAIT = 1500;
        protected const int RESULT_THREAD_MAX = 1501;
        protected const int RESULT_THREAD_USED = 1502;
        protected const int RESULT_NOTCONNECT = 2000;
        protected const int RESULT_UNKNOWN = 2504;
        protected const int RESULT_USB_COMM_ERR = 2600;
        protected const int RESULT_STATUS_READY = 0;
        protected const int RESULT_STATUS_BUSY = 2100;
        protected const int RESULT_STATUS_MAIN_CPU_INITIALIZE = 2101;
        protected const int RESULT_STATUS_RIBBON_INITIALIZE = 2102;
        protected const int RESULT_STATUS_CARD_LOADING = 2103;
        protected const int RESULT_STATUS_THERMAL_PROTECT = 2104;
        protected const int RESULT_STATUS_OPERATION = 2105;
        protected const int RESULT_STATUS_SELFDIAGNOSIS = 2106;
        protected const int RESULT_STATUS_DOWN_LOADING = 2107;
        protected const int RESULT_STATUS_BOOT_MODE = 2150;
        protected const int RESULT_STATUS_HEAT = 2200;
        protected const int RESULT_STATUS_PRINT_LOADING = 2201;
        protected const int RESULT_STATUS_PRINT_START_POSITION = 2202;
        protected const int RESULT_STATUS_Y_PRINTTNG = 2203;
        protected const int RESULT_STATUS_M_PRINTTNG = 2204;
        protected const int RESULT_STATUS_C_PRINTTNG = 2205;
        protected const int RESULT_STATUS_K_PRINTTNG = 2206;
        protected const int RESULT_STATUS_OP_PRINTTNG = 2207;
        protected const int RESULT_STATUS_CUTTING = 2208;
        protected const int RESULT_STATUS_CARD_EJECTION = 2209;
        protected const int RESULT_STATUS_PRINTTING_COMPLETE = 2212;
        protected const int RESULT_STATUS_HOLO_HEAT = 2213;
        protected const int RESULT_STATUS_PRINT_START_POSITION_HOLO = 2214;
        protected const int RESULT_STATUS_HOLO_PRINTING = 2215;
        protected const int RESULT_STATUS_RFID_POSITION = 2216;
        protected const int RESULT_STATUS_NO_PRINTTING = 2300; // sic
        protected const int RESULT_STATUS_UNKNOWNPAPER = 2301;
        protected const int RESULT_STATUS_CARD_LOAD_ERR = 2303;
        protected const int RESULT_STATUS_CARD_UN_SET_ERR = 2304;
        protected const int RESULT_STATUS_CARD_EMPTY_ERR = 2305;
        protected const int RESULT_STATUS_INK_EMPTY_ERR = 2306;
        protected const int RESULT_STATUS_INK_INCORRECT_ERR = 2307;
        protected const int RESULT_STATUS_INK_EMPTY_HOLO_ERR = 2308;
        protected const int RESULT_STATUS_UPPER_COVER_OPEN_ERR = 2309;
        protected const int RESULT_STATUS_CARTRIDGE_OPEN_ERR = 2310;
        protected const int RESULT_STATUS_DUST_BOX_FULL_ERR = 2315;
        protected const int RESULT_STATUS_INK_UN_SET_ERR = 2316;
        protected const int RESULT_STATUS_INK_RIBBON_ERR = 2317;
        protected const int RESULT_STATUS_HOLO_RIBBON_ERR = 2318;
        protected const int RESULT_CARDRFID_COMM_ERROR = 2401;
        protected const int RESULT_CARDRFID_MEMORY_ERROR = 2403;
        protected const int RESULT_CARDRFID_COMMAND_ERROR = 2404;
        protected const int RESULT_CARDRFID_READ_A = 2405;
        protected const int RESULT_CARDRFID_READ_B = 2406;
        protected const int RESULT_CARDRFID_READ_C = 2407;
        protected const int RESULT_STATUS_INK_INCORRECT00 = 2420;
        protected const int RESULT_STATUS_INK_INCORRECT01 = 2421;
        protected const int RESULT_STATUS_INK_INCORRECT02 = 2422;
        protected const int RESULT_STATUS_INK_INCORRECT03 = 2423;
        protected const int RESULT_STATUS_INK_INCORRECT04 = 2424;
        protected const int RESULT_STATUS_INK_INCORRECT05 = 2425;
        protected const int RESULT_STATUS_INK_INCORRECT06 = 2426;
        protected const int RESULT_STATUS_INK_INCORRECT07 = 2427;
        protected const int RESULT_STATUS_INK_INCORRECT08 = 2428;
        protected const int RESULT_STATUS_INK_INCORRECT09 = 2429;
        protected const int RESULT_CARDRFID_COMM_TIMEOUT_ERROR = 4051;
        protected const int RESULT_PRINT_TIMEOUT = 5001;
        protected const int RESULT_UPDATE_RFIDFIRM = 5003;

        protected const int FORMAT_PIXEL_RGB = 3;
        protected const int FORMAT_PIXEL_BGR = 4;
        protected const int COMPONENT_RGB = 3;
        protected const int COLOR_DEPTH = 8;

        protected const ushort RENDERING_INTENTS_PERCEPTUAL = 0;
        protected const ushort RENDERING_INTENTS_RELATIVE_COLORMETRIC = 1;
        protected const ushort RENDERING_INTENTS_SATURATION = 2;

        protected const int START_PAGE_EXIT = 0;
        protected const int START_PAGE_STANDBY_YMC = 1;
        protected const int START_PAGE_STANDBY_HOLO = 2;
        protected const int START_PAGE_STANDBY_RFID = 3;

        protected const int STANDBY_YMC = 0;
        protected const int STANDBY_HOLO = 1;
        protected const int STANDBY_RFID = 2;
#pragma warning restore CS1591

        #endregion

        #region CHC error table

        private static readonly PrinterError[] ERROR_TABLE = new PrinterError[] {
            new PrinterError(RESULT_NOERROR, 0, "OK"),
            new PrinterError(RESULT_STATUS_CARD_LOAD_ERR, 0, "Card tray failed loading card"),
            new PrinterError(RESULT_STATUS_CARD_UN_SET_ERR, 6801, "Card tray is empty"),
            new PrinterError(RESULT_STATUS_CARD_EMPTY_ERR, 6801, "Card tray is empty"),
            new PrinterError(RESULT_STATUS_INK_EMPTY_ERR, 6802, 1, "No color ribbon is present"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT_ERR, 6803, "Color ribbon data could not be read"),
            new PrinterError(RESULT_STATUS_INK_EMPTY_HOLO_ERR, 6802, 2, "No holo ribbon is present"),
            new PrinterError(RESULT_STATUS_UPPER_COVER_OPEN_ERR, 6807, "Printer lid is open"),
            new PrinterError(RESULT_STATUS_CARTRIDGE_OPEN_ERR, 6808, "Card compartment is open"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT00, 6820, "Color ribbon is not compatible (0)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT01, 6820, "Color ribbon is not compatible (1)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT02, 6820, "Color ribbon is not compatible (2)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT03, 6820, "Color ribbon is not compatible (3)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT04, 6820, "Color ribbon is not compatible (4)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT05, 6820, "Color ribbon is not compatible (5)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT06, 6820, "Color ribbon is not compatible (6)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT07, 6820, "Color ribbon is not compatible (7)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT08, 6820, "Color ribbon is not compatible (8)"),
            new PrinterError(RESULT_STATUS_INK_INCORRECT09, 6820, "Color ribbon is not compatible (9)"),
            new PrinterError(RESULT_STATUS_BUSY, 0, "Printer is busy"),
            new PrinterError(RESULT_STATUS_MAIN_CPU_INITIALIZE, 0, "Printer is initializing"),
            new PrinterError(RESULT_STATUS_RIBBON_INITIALIZE, 0, "Printer is checking ribbons"),
            new PrinterError(RESULT_STATUS_CARD_LOADING, 0, "Printer is loading card"),
            new PrinterError(RESULT_STATUS_OPERATION, 0, "Printer is operating"),
            new PrinterError(RESULT_STATUS_SELFDIAGNOSIS, 0, "Printer is diagnosing"),
            new PrinterError(RESULT_STATUS_THERMAL_PROTECT, 6810, "Printer is overheated"),
            new PrinterError(RESULT_USBNOTFOUND, 0, "Initialization failed (missing DLLs?)"),
            new PrinterError(RESULT_OPENPORT, 0, "No printer connected"),
            new PrinterError(RESULT_MUTEX_WAIT, 6810, "Other printer error"),
            new PrinterError(RESULT_THREAD_MAX, 0, "Thread access error"),
            new PrinterError(RESULT_PRMERROR, 0, "Invalid argument"),
            new PrinterError(RESULT_UNKNOWN, 6810, "Other printer error"),
            new PrinterError(RESULT_STATUS_UNKNOWNPAPER, 0, "Attempted to set unknown paper type"),
            new PrinterError(2406, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2407, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2401, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(RESULT_STATUS_BOOT_MODE, 0, "NG"),
            new PrinterError(RESULT_USB_COMM_ERR, 6804, "Communication error with printer"),
            new PrinterError(RESULT_CARDRFID_COMM_TIMEOUT_ERROR, 6818, "Error communicating with the RFID board of the printer"),
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
            new PrinterError(RESULT_STATUS_DUST_BOX_FULL_ERR, 6809, "The printer scrap box is full"),
            new PrinterError(RESULT_STATUS_INK_UN_SET_ERR, 6812, "Color ribbon can not be recongized"),
            new PrinterError(RESULT_STATUS_INK_RIBBON_ERR, 6813, 1, "Failed to cut or wind the color ribbon"),
            new PrinterError(RESULT_STATUS_HOLO_RIBBON_ERR, 6813, 2, "Failed to cut or wind the holo ribbon"),
            new PrinterError(RESULT_NOTCONNECT, 0, 0, "Printer is not connected"),
            new PrinterError(9999, 9999, "No return code")
        };

        #endregion

        /// <summary>
        /// The image dimensions that this printer expects.
        /// </summary>
        public Size ImageDimensions { get; }

        /// <summary>
        /// The DLL trampoline that this printer uses.
        /// </summary>
        protected INativeTrampolineChc Native { get; }

        /// <summary>
        /// The RFID backend that this printer uses.
        /// </summary>
        protected RfidBackend RfidBackend { get; private set; }

        // Printer properties

        /// <summary>
        /// The file name for the first .icc file.
        /// </summary>
        public string IccTable1FileName { get; private set; }

        /// <summary>
        /// The file name for the second .icc file.
        /// </summary>
        public string IccTable2FileName { get; private set; }

        /// <summary>
        /// The file name for the mtf.txt file.
        /// </summary>
        public string MtfFileName { get; private set; }

        /// <summary>
        /// How the image being printed should be stretched.
        /// </summary>
        public StretchMode ImageStretchMode { get; set; } = StretchMode.SizeMustMatch;

        // Instance properties

        /// <summary>
        /// The current print job being executed (or null).
        /// </summary>
        protected PrintJob Job { get; private set; }

        private Thread printThread;
        private bool isConnected;
        private PrinterThreadDelegate threadFunc;
        private DeviceStatus? threadCallStatus;
        private ushort threadStatusCode;
        private ushort currentStatusCode;
        internal int InitTimeout = 120_000;

        /// <summary>
        /// Creates a new CHCSeriesCardPrinter.
        /// </summary>
        /// <param name="dllFunctions">The native function trampoline with a matching DLL that this printer uses.</param>
        /// <param name="rfidBackend">The RFID backend this printer uses (or null to disable RFID functions).</param>
        /// <param name="imageSize">The image dimensions this printer expects.</param>
        protected ChcSeriesCardPrinter(INativeTrampolineChc dllFunctions, RfidBackend rfidBackend, Size imageSize) {
            NetStandardBackCompatExtensions.ThrowIfNull(dllFunctions, nameof(dllFunctions));
            NetStandardBackCompatExtensions.ThrowIfNull(imageSize, nameof(imageSize));
            ImageDimensions = imageSize;
            RfidBackend = rfidBackend;
            Native = dllFunctions;
        }

        /// <summary>
        /// Returns the last reported printer status code (RESULT_* constants).
        /// </summary>
        /// <returns>the last reported printer status code</returns>
        public ushort GetPrinterStatusCode() {
            return currentStatusCode;
        }

        /// <summary>
        /// Reads the ID of the currently loaded card in the printer.
        /// </summary>
        /// <param name="cardid">The cardid that was read or null on failure.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other status on error.</returns>
        public abstract DeviceStatus GetLoadedCardId(out byte[] cardid);

        /// <summary>
        /// Connects to the RFID board.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if connection was successful or the board was already connected.<br />
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board is not attached.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.
        /// </returns>
        public abstract DeviceStatus ConnectRfid();

        /// <summary>
        /// Disconnects from the RFID board.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the board was disconnected successfuly or was not connected.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.
        /// </returns>
        public abstract DeviceStatus DisconnectRfid();

        /// <summary>
        /// Writes a payload to the card being printed.
        /// </summary>
        /// <param name="rc">The printer status code being returned.</param>
        /// <param name="payload">The data to write.</param>
        /// <param name="writtenCardId">The card ID of the card that was being written to or null on error or if no RFID board was configured.</param>
        /// <param name="overrideCardId">Whether or not the payload contains the card ID.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the data was written successfully or payload was null.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="ConnectRfid"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board was disconnected during the operation.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.<br />
        /// any other <see cref="DeviceStatus.DeviceStatusCodesEnd"/> >= DeviceStatus >= <see cref="DeviceStatus.DeviceStatusCodesStart"/> to represent device error codes.
        /// </returns>
        public abstract DeviceStatus WriteRfid(ref ushort rc, byte[] payload, bool overrideCardId, out byte[] writtenCardId);

        /// <summary>
        /// Verifies the RFID data being written to and throws an exception if this payload can not be written
        /// </summary>
        /// <param name="payload">The payload to be verified.</param>
        /// <param name="overrideCardId">Whether the card ID should be contained in the payload or not.</param>
        /// <exception cref="InvalidOperationException">If a verification error occurrs.</exception>
        /// <exception cref="ArgumentException">If a verification error occurrs.</exception>
        public abstract void VerifyRfidData(byte[] payload, bool overrideCardId);

        /// <summary>
        /// Returns the StartPage_* constant that is required for this printer's call to <see cref="INativeTrampolineChc.CHC_startpage(ushort, ref ushort, ref ushort)"/>.
        /// </summary>
        /// <returns>the constant required for StartPage.</returns>
        protected abstract ushort GetStartPageParameter();

        /// <summary>
        /// Returns the POLISH_* constant that tells the printer if the print is normal/holo/laminate.
        /// </summary>
        /// <param name="isHolo">Whether the print job includes a holo image or not.</param>
        /// <returns>the constant required for setPrinterInfo(20), or null if this should not be called</returns>
        protected abstract byte? GetPolishParameter(bool isHolo);

        /// <summary>
        /// Sets the last error code of a printer function that returns a status code and a return code and if <see cref="Device.IsUsingExceptions"/> is true, throw an exception.
        /// </summary>
        /// <remarks>
        /// A non-ready non-busy status code will trigger a <see cref="Debugger.Break"/> if running in debug mode.
        /// </remarks>
        /// <param name="operationReturnCode">The return code of the printer method (any CHCUSB_RC_* constant).</param>
        /// <param name="rc">The last printer status code that was obtained from a device call.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok" /> if <paramref name="operationReturnCode"/> == <see cref="CHCUSB_RC_OK"/> or <paramref name="rc"/> == <see cref="RESULT_STATUS_READY"/>.<br />
        /// <see cref="DeviceStatus.Busy" /> if <paramref name="operationReturnCode"/> == <see cref="CHCUSB_RC_BUSY"/> and <paramref name="rc"/> == <see cref="RESULT_STATUS_BUSY"/>.<br />
        /// otherwise <paramref name="rc"/> cast to a <see cref="DeviceStatus"/>.
        /// </returns>
        /// <exception cref="IOException">If <see cref="Device.IsUsingExceptions"/> is true and the return value would neither be <see cref="DeviceStatus.Ok"/> or <see cref="DeviceStatus.Busy"/>.</exception>
        protected DeviceStatus SetLastErrorByReturnCode(int operationReturnCode, ushort rc = RESULT_STATUS_READY) {
            if (operationReturnCode != CHCUSB_RC_OK && rc != RESULT_STATUS_READY) {
                rc = GetPrinterStatusCode();
            }

            if (rc != RESULT_STATUS_BUSY && rc != RESULT_STATUS_READY) {
                LOG.LogError("Printer Error (" + rc + "): " + RcToString(rc));
                if (Debugger.IsAttached) {
                    Debugger.Break();
                }
            }

            if (rc == CHCUSB_RC_OK || rc == RESULT_STATUS_READY) {
                return SetLastError(DeviceStatus.Ok);
            }

            if (rc == CHCUSB_RC_BUSY || rc == RESULT_STATUS_BUSY) {
                return DeviceStatus.Busy;
            }

            return SetLastError((DeviceStatus)rc);
        }

        /// <summary>
        /// Waits for a printer function to return anything that is not <see cref="CHCUSB_RC_BUSY"/> or within <paramref name="waitCodes"/>. The function is repeatedly called (thus this call will block) as long it returns <see cref="CHCUSB_RC_BUSY"/> or the status code is no longer part of <paramref name="waitCodes"/>.
        /// </summary>
        /// <param name="rc">The printer status code that was returned.</param>
        /// <param name="printerFunction">The function to call.</param>
        /// <param name="timeout">The amount in millisecond to wait for completion.</param>
        /// <param name="waitCodes">Additional RESULT_* values that should be considered as "busy".</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> is <paramref name="printerFunction"/> completed successfully.<br />
        /// If the timeout was hit, the last printer status code cast to a <see cref="DeviceStatus"/> will be returned.<br />
        /// If an error occurs, the last printer status code cast to a <see cref="DeviceStatus"/> will be returned.
        /// </returns>
        protected DeviceStatus PrintWaitFor(ref ushort rc, StatusWaitDelegate printerFunction, int timeout, params ushort[] waitCodes) {
            DateTime start = DateTime.Now;
            while (printerFunction(ref rc) == CHCUSB_RC_BUSY || waitCodes.Contains(rc)) {
                if ((DateTime.Now - start).TotalMilliseconds > timeout) {
                    LOG.LogError("Timeout was hit: " + timeout);
                    LOG.LogError("Status was: " + RcToString(rc));
                    return SetLastError((DeviceStatus)rc);
                }

                Thread.Sleep(1000);
            }

            return SetLastError(DeviceStatus.Ok, rc);
        }

        /// <summary>
        /// Sets the .icc files used for printing.
        /// </summary>
        /// <param name="icc1Filename">The first .icc file to be used.</param>
        /// <param name="icc2Filename">The second .icc file to be used.</param>
        /// <exception cref="FileNotFoundException">If one of the files cannot be found.</exception>
        public void SetIccTables(string icc1Filename, string icc2Filename) {
            NetStandardBackCompatExtensions.ThrowIfNull(icc1Filename, nameof(icc1Filename));
            NetStandardBackCompatExtensions.ThrowIfNull(icc2Filename, nameof(icc2Filename));
            if (!File.Exists(icc1Filename)) {
                throw new FileNotFoundException(null, icc1Filename);
            }

            if (!File.Exists(icc2Filename)) {
                throw new FileNotFoundException(null, icc2Filename);
            }

            IccTable1FileName = icc1Filename;
            IccTable2FileName = icc2Filename;
            LOG.LogInformation("ICC tables set to: " + IccTable1FileName + ", " + IccTable2FileName);
        }

        /// <summary>
        /// Sets the mtf.txt file used for printing.
        /// </summary>
        /// <param name="mtfFilename">The file to be used.</param>
        /// <exception cref="FileNotFoundException">If the file cannot be found.</exception>
        public void SetMtfFile(string mtfFilename) {
            NetStandardBackCompatExtensions.ThrowIfNull(mtfFilename, nameof(mtfFilename));
            if (!File.Exists(mtfFilename)) {
                throw new FileNotFoundException(null, mtfFilename);
            }

            MtfFileName = mtfFilename;
            LOG.LogInformation("MTF set to: " + MtfFileName);
        }

        /// <inheritdoc/>
        public sealed override DeviceStatus Connect() {
            LOG.LogInformation("Connect");

            ResetState();

            try {
                Native.CHC_close(); // test for DLL presence
            } catch (Exception ex) {
                LOG.LogCritical(ex, "DLL initialization failed");
                return DeviceStatus.ErrorLibrary;
            }

            isConnected = true;
            printThread = new Thread(PrintCommandExecutor) {
                Name = "Printer Command Executor"
            };
            printThread.Start();

            DeviceStatus ret = ExecuteOnPrintThread((ref ushort rc) => {
                DeviceStatus ret2 = SetLastErrorByReturnCode(Native.CHC_open(ref rc), rc);
                if (ret2 != DeviceStatus.Ok) {
                    LOG.LogError("Open failed");
                    return ret2;
                }

                ret2 = SelectById();
                if (ret2 != DeviceStatus.Ok) {
                    LOG.LogError("Select failed");
                    return ret2;
                }

                LOG.LogInformation("Waiting until printer has finished initalizing");
                return PrintWaitFor(ref rc, Native.CHC_status, InitTimeout, RESULT_STATUS_MAIN_CPU_INITIALIZE, RESULT_STATUS_RIBBON_INITIALIZE, RESULT_STATUS_CARD_LOADING, RESULT_STATUS_OPERATION, RESULT_STATUS_SELFDIAGNOSIS, RESULT_STATUS_DOWN_LOADING, RESULT_STATUS_BOOT_MODE);
            });

            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            return SetLastError(ConnectRfid());
        }

        private void PrintCommandExecutor() {
            LOG.LogInformation("Thread started");

            DateTime lastStatusQuery = DateTime.MinValue;

            while (isConnected) {
                ushort rc = 0;

                Monitor.Enter(this);
                try {
                    if (threadFunc != null) {
                        LOG.LogInformation("Printer Query: " + threadFunc);
                        try {
                            threadCallStatus = threadFunc(ref rc);
                            threadStatusCode = rc;
                        } catch (Exception ex) {
                            LOG.LogCritical(ex, "Printer query failed with exception");
                            threadCallStatus = DeviceStatus.ErrorOther;
                            threadStatusCode = 0;
                        }

                        threadFunc = null;
                    }
                } finally {
                    Monitor.Exit(this);
                }

                if (DateTime.Now - lastStatusQuery > TimeSpan.FromSeconds(5)) {
                    Native.CHC_status(ref currentStatusCode);
                    lastStatusQuery = DateTime.Now;
                }

                if (printThread != null && isConnected) {
                    lock (printThread) {
                        Monitor.Wait(printThread, 1000);
                    }
                }
            }

            Native.CHC_close();

            LOG.LogInformation("Thread exited");
        }

        /// <summary>
        /// Runs the given function on the printer thread. This must be used for every call on a function contained in <see cref="Native"/>.
        /// </summary>
        /// <param name="func">The function to call</param>
        /// <param name="waitForCompletion">Whether or not to wait for thread completion. If this is true, this call will block until then.</param>
        /// <param name="waitForStart">Whether to wait for an already existing call. If this is true, this will block until this function can be started. If this is false, <see cref="DeviceStatus.Busy"/> will be immediately returned instead.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if <paramref name="waitForCompletion"/> is false and the function was successfully started on the printer thread.<br />
        /// <see cref="DeviceStatus.Busy"/> if <paramref name="waitForStart"/> is false and something is already executing on the printer thread.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// otherwise the return value of <paramref name="func"/> will be returned.
        /// </returns>
        /// <exception cref="ThreadStateException">If the printer thread is not running (<see cref="Connect"/> was not called, <see cref="Disconnect"/> was called or an internal error occurred), or if this function itself is called from inside the printer thread.</exception>
        protected DeviceStatus ExecuteOnPrintThread(PrinterThreadDelegate func, bool waitForCompletion = true, bool waitForStart = false) {
            if (printThread == null) {
                throw new ThreadStateException("printer thread has shut down");
            }

            if (printThread.ManagedThreadId == Environment.CurrentManagedThreadId) {
#if DEBUG
                LOG.LogWarning("Calling ExecuteOnPrintThread on the printer thread!");
                ushort _ = 0;
                return func(ref _);
#else
                throw new ThreadStateException("calling ExecuteOnPrintThread on the printer thread is not allowed");
#endif
            }

            if (threadFunc != null && threadCallStatus == null) {
                if (waitForStart) {
                    Monitor.Enter(this);
                    threadCallStatus = null;
                    threadFunc = func;
                    Monitor.Exit(this);
                } else {
                    return DeviceStatus.Busy;
                }
            } else {
                threadCallStatus = null;
                threadFunc = func;
            }

            lock (printThread) {
                Monitor.Pulse(printThread);
            }

            if (!waitForCompletion) {
                return DeviceStatus.Ok;
            }

            while (threadCallStatus == null && isConnected) {
                Thread.Sleep(10);
            }

            if (!isConnected || threadCallStatus == null) {
                return DeviceStatus.ErrorNotInitialized;
            }

            if (threadStatusCode != RESULT_NOERROR) {
                LOG.LogWarning("Printer Query returned: " + RcToString(threadStatusCode));
            }

            return threadCallStatus.Value;
        }

        /// <summary>
        /// Checks if we are running on the printer thread, otherwise throw an exception.
        /// </summary>
        /// <param name="callerFunc">Auto-generated.</param>
        /// <exception cref="ThreadStateException">if the printer thread has shut down or this function is not being called on the printer thread.</exception>
        protected void CheckCallingThread([CallerMemberName] string callerFunc = null) {
            if (printThread == null) {
                throw new ThreadStateException(callerFunc + " must be called on printer thread, which is not active!");
            }

            if (printThread.ManagedThreadId != Environment.CurrentManagedThreadId) {
                throw new ThreadStateException(callerFunc + " must be called on printer thread (" + printThread.ManagedThreadId + ")! Called from " + Environment.CurrentManagedThreadId);
            }
        }

        private DeviceStatus SelectById() {
            CheckCallingThread();

            DeviceStatus ret;
            ushort rc = 0;

            byte[] idArray = new byte[0x80];
            idArray.Populate<byte>(0xFF);
            unsafe {
                fixed (byte* idArrayPtr = idArray) {
                    ret = SetLastErrorByReturnCode(Native.CHC_listupPrinter(idArrayPtr));
                }
            }

            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Listup failed");
                return ret;
            }

            byte printerId = 0xFF;
            foreach (byte id in idArray) {
                if (id != 0xFF) {
                    printerId = id;
                }
            }

            if (printerId == 0xFF) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            LOG.LogInformation("Select Printer: " + printerId);
            Native.CHC_selectPrinter(printerId, ref rc); // this seems to return 0 for some obscure reason
            return SetLastErrorByReturnCode(CHCUSB_RC_OK, rc);
        }

        [Obsolete("This causes an access violation", true)]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private DeviceStatus SelectBySerialNumber() {
            CheckCallingThread();

            const ulong invalidEntry = 0xFFFFFFFFFFFFFFFF;
            DeviceStatus ret;
            ushort rc = 0;

            ulong[] idArray = new ulong[0x400];
            idArray.Populate(invalidEntry);
            unsafe {
                fixed (ulong* idArrayPtr = idArray) {
                    ret = SetLastErrorByReturnCode(Native.CHC_listupPrinterSN(idArrayPtr));
                }
            }

            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            ulong printerId = invalidEntry;
            foreach (ulong id in idArray) {
                if (id != invalidEntry) {
                    printerId = id;
                }
            }

            if (printerId == invalidEntry) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            LOG.LogInformation("Select Printer (SN): " + printerId);
            return SetLastErrorByReturnCode(Native.CHC_selectPrinterSN(printerId, ref rc), rc);
        }

        /// <inheritdoc/>
        public sealed override DeviceStatus Disconnect() {
            isConnected = false;
            if (printThread != null) {
                lock (printThread) {
                    Monitor.Pulse(printThread);
                }

                LOG.LogInformation("Waiting for thread completion");
                printThread.Join();
                printThread = null;
            }

            DeviceStatus ret = SetLastError(DisconnectRfid());
            if (ret == DeviceStatus.Ok) {
                ResetState();
            }

            return ret;
        }

        private void ResetState() {
            printThread = null;
            isConnected = false;
            threadCallStatus = null;
            threadStatusCode = RESULT_NOTCONNECT;
            threadFunc = null;
            currentStatusCode = RESULT_NOTCONNECT;
        }

        private DeviceStatus GetPrinterInfo(PrinterInfoTag tag, out byte[] data) {
            LOG.LogInformation("GetPrinterInfo: " + tag);
            uint len = tag.GetBufferSize();
            byte[] buf = new byte[len];
            DeviceStatus ret = ExecuteOnPrintThread((ref ushort _) => {
                DeviceStatus ret2;
                unsafe {
                    fixed (byte* ptr = buf) {
                        ret2 = SetLastErrorByReturnCode(Native.CHC_getPrinterInfo((ushort)tag, ptr, ref len));
                    }
                }

                if (ret2 != DeviceStatus.Ok) {
                    buf = null;
                }

                return ret2;
            });
            data = buf;
            return ret;
        }

        /// <summary>
        /// Queries the printer's serial number.
        /// </summary>
        /// <param name="serialno">The printer serial number or null on error.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.Busy"/> if the printer is busy with another operation (ex. printing), any other status on error.</returns>
        public DeviceStatus GetPrinterSerial(out string serialno) {
            LOG.LogInformation("GetPrinterSerial");
            serialno = null;
            DeviceStatus ret = GetPrinterInfo(PrinterInfoTag.SerialInfo, out byte[] buf);
            if (ret == DeviceStatus.Ok) {
                serialno = UnsafeUtils.BytesToString(buf);
            }

            return ret;
        }


        /// <summary>
        /// Queries various printer statistics.
        /// </summary>
        /// <param name="printcnt">The printer statistics or null on error.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.Busy"/> if the printer is busy with another operation (ex. printing), any other status on error.</returns>
        public DeviceStatus GetPrintCnt(out PrintCnt? printcnt) {
            LOG.LogInformation("GetPrintCnt");
            printcnt = null;
            DeviceStatus ret = GetPrinterInfo(PrinterInfoTag.PrintCount, out byte[] buf);
            if (ret == DeviceStatus.Ok) {
                printcnt = StructUtils.FromBytes<PrintCnt>(buf);
            }

            return ret;
        }

        /// <summary>
        /// Queries various printer statistics.
        /// </summary>
        /// <param name="printcnt">The printer statistics or null on error.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.Busy"/> if the printer is busy with another operation (ex. printing), any other status on error.</returns>
        public DeviceStatus GetPrintCnt2(out PrintCnt2? printcnt) {
            LOG.LogInformation("GetPrintCnt2");
            printcnt = null;
            DeviceStatus ret = GetPrinterInfo(PrinterInfoTag.PrintCount2, out byte[] buf);
            if (ret == DeviceStatus.Ok) {
                printcnt = StructUtils.FromBytes<PrintCnt2>(buf);
            }

            return ret;
        }

        /// <summary>
        /// Queries various page statistics.
        /// </summary>
        /// <param name="pageStatus">The page statistics or null on error.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.Busy"/> if the printer is busy with another operation (ex. printing), any other status on error.</returns>
        public DeviceStatus GetPageStatus(out PageStatus? pageStatus) {
            LOG.LogInformation("GetPageStatus");
            pageStatus = null;
            DeviceStatus ret = GetPrinterInfo(PrinterInfoTag.PageStatus, out byte[] buf);
            if (ret == DeviceStatus.Ok) {
                pageStatus = StructUtils.FromBytes<PageStatus>(buf);
            }

            return ret;
        }

        /// <summary>
        /// Queries the printer's "print ID" status. The only observed values are <see cref="RESULT_STATUS_PRINTTING_COMPLETE"/> and <see cref="RESULT_STATUS_NO_PRINTTING"/>.
        /// </summary>
        /// <param name="status">The print ID status.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.Busy"/> if the printer is busy with another operation (ex. printing), any other status on error.</returns>
        public DeviceStatus GetPrintIDStatus(out int status) {
            LOG.LogInformation("GetPrintIDStatus");

            int statusQ = 0;

            DeviceStatus ret = ExecuteOnPrintThread((ref ushort rc) => {
                byte[] buf = new byte[8];
                unsafe {
                    fixed (byte* ptr = buf) {
                        ret = SetLastErrorByReturnCode(Native.CHC_getPrintIDStatus(0, ptr, ref rc), rc);
                    }
                }

                if (ret == DeviceStatus.Ok) {
                    statusQ = buf[7] << 8 | buf[6];
                }

                return ret;
            });

            status = statusQ;

            return ret;
        }

        /// <summary>
        /// Returns the status of the currently running print job.
        /// </summary>
        /// <remarks>
        /// If this returns non-OK, non-BUSY, non-ERROR_NOT_INITALIZED you must call <see cref="Disconnect"/> and <see cref="Connect"/> to reset the job and printer state.
        /// </remarks>
        /// <returns>
        /// On success, returns the status of the currently running (or finished/errored) printer job.<br />
        /// <see cref="DeviceStatus.Busy"/> if the job is still running.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if no print job is running.
        /// </returns>
        /// <exception cref="Exception">If <see cref="Device.IsUsingExceptions"/> is true and a printer job exception occurred</exception>
        public DeviceStatus GetPrintJobResult() {
            if (Job == null) {
                return DeviceStatus.ErrorNotInitialized;
            }

            PrintStatus status = Job.JobStatus;
            if (status == PrintStatus.None) {
                return DeviceStatus.ErrorNotInitialized;
            }

            if (status == PrintStatus.Errored) {
                if (IsUsingExceptions) {
                    throw new Exception("An exception occurred during printing", Job.JobException);
                }

                return Job.JobResult;
            }

            if (status < PrintStatus.Finished) {
                return DeviceStatus.Busy;
            }

            return Job.JobResult;
        }

        /// <summary>
        /// Returns the last written RFID card ID.
        /// </summary>
        /// <param name="cardid">The last written card ID or null if none is available.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok" /> if the print job has successfully passed <see cref="PrintStatus.RfidWrite"/>.<br />
        /// <see cref="DeviceStatus.Busy"/> if it has not.
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if no print job was ever started.
        /// <see cref="DeviceStatus.ErrorDevice"/> if the print job is in error state.
        /// </returns>
        public DeviceStatus GetWrittenRfidCardId(out byte[] cardid) {
            PrintStatus status = Job?.JobStatus ?? PrintStatus.None;
            if (status == PrintStatus.None) {
                cardid = null;
                return DeviceStatus.ErrorNotInitialized;
            }

            if (status > PrintStatus.RfidWrite && status <= PrintStatus.Finished) {
                cardid = Job?.WrittenRfidCardId;
                return DeviceStatus.Ok;
            }

            if (status == PrintStatus.Errored) {
                cardid = null;
                return DeviceStatus.ErrorDevice;
            }

            cardid = null;
            return DeviceStatus.Busy;
        }

        /// <summary>
        /// Starts printing an image with optional holo image and RFID data.
        /// </summary>
        /// <param name="frontImage">The image to print.</param>
        /// <param name="rfidPayload">The RFID payload data (without card ID) to write or null for no RFID data.</param>
        /// <param name="holo">The holo image to print or null for no holo.</param>
        /// <param name="waitForCompletion">if this is true, this function will block until the print completes (or errors).</param>
        /// <param name="overrideCardId">if this is true, the card ID of the loaded card will be ignored and instead expected as an added 12 bytes in the rfidPayload.</param>
        /// <param name="backImage">The image to print on the back side. Only certain printers support this.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the print completed successfully.<br />
        /// <see cref="DeviceStatus.Busy"/> if the print started successfully and <paramref name="waitForCompletion"/> is false or if a print is already in progress.<br />
        /// any other status on various printer errors.
        /// </returns>
        /// <seealso cref="GetPrintJobResult"/>
        /// <exception cref="ArgumentException">If RFID payload verification fails or if <see cref="ImageStretchMode"/> is <see cref="StretchMode.SizeMustMatch"/> and the image dimensions do not match <see cref="ImageDimensions"/>.</exception>
        /// <exception cref="InvalidOperationException">If <see cref="SetMtfFile(string)"/> was not called, <see cref="SetIccTables(string, string)"/> was not called or the last print job result is <see cref="PrintStatus.Errored"/></exception>
        public DeviceStatus StartPrinting(Bitmap frontImage, byte[] rfidPayload = null, Bitmap holo = null, bool waitForCompletion = false, bool overrideCardId = false, Bitmap backImage = null) {
            NetStandardBackCompatExtensions.ThrowIfNull(frontImage, nameof(frontImage));
            if (frontImage.PhysicalDimension != ImageDimensions) {
                if (ImageStretchMode == StretchMode.Stretch) {
                    frontImage = frontImage.CopyStretched(ImageDimensions);
                } else if (ImageStretchMode == StretchMode.Center) {
                    frontImage = frontImage.CopyCentered(ImageDimensions);
                } else {
                    throw new ArgumentException("Image to print with size " + frontImage.PhysicalDimension + " does not match expected printer size of " + ImageDimensions);
                }
            }

            if (holo != null && holo.PhysicalDimension != ImageDimensions) {
                if (ImageStretchMode == StretchMode.Stretch) {
                    holo = holo.CopyStretched(ImageDimensions);
                } else if (ImageStretchMode == StretchMode.Center) {
                    holo = holo.CopyCentered(ImageDimensions);
                } else {
                    throw new ArgumentException("Holo image to print with size " + holo.PhysicalDimension + " does not match expected printer size of " + ImageDimensions);
                }
            }

            VerifyRfidData(rfidPayload, overrideCardId);
            if (MtfFileName == null) {
                throw new InvalidOperationException("MTF file must be set before attempting to print, call SetMtfFile");
            }

            if (IccTable1FileName == null) {
                throw new InvalidOperationException("ICC table files must be set before attempting to print, call SetIccTables");
            }

            if (Job != null) {
                if (Job.JobStatus == PrintStatus.Errored) {
                    throw new InvalidOperationException("Printer is in error state. Call Disconnect() then Connect() before attempting to print again");
                }

                if (Job.JobStatus != PrintStatus.None) {
                    LOG.LogWarning("Rejecting StartPrinting, previous print is not complete");
                    return SetLastError(DeviceStatus.Busy);
                }
            }

            Job = new PrintJob(this, Native, frontImage, backImage, holo, rfidPayload, overrideCardId);

            LOG.LogInformation("Start Printing");
            LOG.LogInformation("Current Status: " + RcToString(GetPrinterStatusCode()));

            return SetLastError(ExecuteOnPrintThread(Job.Run, waitForCompletion));
        }

        /// <summary>
        /// Aborts the currently running print job with the given status codes.
        /// </summary>
        /// <param name="ret">The DeviceStatus that the job will be terminated with.</param>
        /// <param name="rc">The printer status code that the job will be terminated with.</param>
        /// <param name="pageId">The id of the page to cancel (or null) to not cancel.</param>
        /// <returns><paramref name="ret"/></returns>
        /// <exception cref="ThreadStateException">if no job is running</exception>
        protected DeviceStatus PrintExitThreadError(DeviceStatus ret, ushort rc, ushort? pageId = null) {
            CheckCallingThread();
            if (Job == null) {
                throw new ThreadStateException("Can't exit print job thread while no thread is running");
            }

            return Job.PrintExitThreadError(ret, rc, pageId);
        }

        /// <summary>
        /// Retrieves the printer error message for the given internal printer error code.
        /// </summary>
        /// <param name="error">the error code</param>
        /// <returns>The printer error message or "Unknown Printer Error".</returns>
        public static string RcToErrorMessage(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).Message;
        }

        /// <summary>
        /// Retrieves the printer external (SEGA) printer error code for the given internal printer error code.
        /// </summary>
        /// <param name="error">the error code</param>
        /// <returns>The printer error code, or 6810 if the error code is not known.</returns>
        public static int RcToSegaError(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).ErrorCodeExt;
        }

        /// <summary>
        /// Formats the given internal printer error code to a string for internal/logging use.
        /// </summary>
        /// <param name="error">the error code</param>
        /// <returns>A string in the format "[ExternalErrorCode] Message (InternalErrorCode)"</returns>
        public static string RcToString(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).ToString();
        }
    }
}

#endif