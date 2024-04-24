using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;

using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    public abstract partial class CHCSeriesCardPrinter : Device {

        public delegate int StatusWaitDelegate(ref ushort rc);
        public delegate DeviceStatus PrinterThreadDelegate(ref ushort rc);

        #region CHC constants
        protected const int CHCUSB_RC_BUSY = 0;
        protected const int CHCUSB_RC_OK = 1;
        protected const int CHCUSB_RC_ERROR = -1;
        internal const int CARD_ID_LEN = 12;

        private const ushort PRINTER_ERROR_UNKNOWN = 6810;

        protected const int RESULT_NOERROR = 0;
        protected const int RESULT_MEMFULLERR = 1000;
        protected const int RESULT_USBNOTFOUND = 1002;
        protected const int RESULT_ICCNOTFOUND = 1003;
        protected const int RESULT_OPENPORT = 1004;
        protected const int RESULT_FILEACCESS = 1005;
        protected const int RESULT_PRMERROR = 1006;
        protected const int RESULT_NOTFOUND = 1008;
        protected const int RESULT_IMAINGNOTFOUND = 1009;
        protected const int RESULT_MUTEX_Wait = 1500;
        protected const int RESULT_THREAD_MAX = 1501;
        protected const int RESULT_THREAD_Used = 1502;
        protected const int RESULT_NOTCONNECT = 2000;
        protected const int RESULT_UNKNOWN = 2504;
        protected const int RESULT_USB_CommErr = 2600;
        protected const int RESULT_STATUS_READY = 0;
        protected const int RESULT_STATUS_BUSY = 2100;
        protected const int RESULT_STATUS_MainCpuInitialize = 2101;
        protected const int RESULT_STATUS_RibbonInitialize = 2102;
        protected const int RESULT_STATUS_CardLoading = 2103;
        protected const int RESULT_STATUS_ThermalProtect = 2104;
        protected const int RESULT_STATUS_Operation = 2105;
        protected const int RESULT_STATUS_Selfdiagnosis = 2106;
        protected const int RESULT_STATUS_DownLoading = 2107;
        protected const int RESULT_STATUS_BootMode = 2150;
        protected const int RESULT_STATUS_Heat = 2200;
        protected const int RESULT_STATUS_PrintLoading = 2201;
        protected const int RESULT_STATUS_PrintStartPosition = 2202;
        protected const int RESULT_STATUS_Y_Printtng = 2203;
        protected const int RESULT_STATUS_M_Printtng = 2204;
        protected const int RESULT_STATUS_C_Printtng = 2205;
        protected const int RESULT_STATUS_K_Printtng = 2206;
        protected const int RESULT_STATUS_OP_Printtng = 2207;
        protected const int RESULT_STATUS_Cutting = 2208;
        protected const int RESULT_STATUS_CardEjection = 2209;
        protected const int RESULT_STATUS_PrinttingComplete = 2212;
        protected const int RESULT_STATUS_Holo_Heat = 2213;
        protected const int RESULT_STATUS_PrintStartPositionHolo = 2214;
        protected const int RESULT_STATUS_Holo_Printing = 2215;
        protected const int RESULT_STATUS_RFIDPosition = 2216;
        protected const int RESULT_STATUS_NoPrintting = 2300;
        protected const int RESULT_STATUS_UNKNOWNPAPER = 2301;
        protected const int RESULT_STATUS_CardLoadErr = 2303;
        protected const int RESULT_STATUS_CardUnSetErr = 2304;
        protected const int RESULT_STATUS_CardEmptyErr = 2305;
        protected const int RESULT_STATUS_InkEmptyErr = 2306;
        protected const int RESULT_STATUS_InkIncorrectErr = 2307;
        protected const int RESULT_STATUS_InkEmptyHoloErr = 2308;
        protected const int RESULT_STATUS_UpperCoverOpenErr = 2309;
        protected const int RESULT_STATUS_CartridgeOpenErr = 2310;
        protected const int RESULT_STATUS_DustBoxFullErr = 2315;
        protected const int RESULT_STATUS_InkUnSetErr = 2316;
        protected const int RESULT_STATUS_InkRibbonErr = 2317;
        protected const int RESULT_STATUS_HoloRibbonErr = 2318;
        protected const int RESULT_CARDRFID_CommError = 2401;
        protected const int RESULT_CARDRFID_MemoryError = 2403;
        protected const int RESULT_CARDRFID_CommandError = 2404;
        protected const int RESULT_CARDRFID_ReadA = 2405;
        protected const int RESULT_CARDRFID_ReadB = 2406;
        protected const int RESULT_CARDRFID_ReadC = 2407;
        protected const int RESULT_STATUS_InkIncorrect00 = 2420;
        protected const int RESULT_STATUS_InkIncorrect01 = 2421;
        protected const int RESULT_STATUS_InkIncorrect02 = 2422;
        protected const int RESULT_STATUS_InkIncorrect03 = 2423;
        protected const int RESULT_STATUS_InkIncorrect04 = 2424;
        protected const int RESULT_STATUS_InkIncorrect05 = 2425;
        protected const int RESULT_STATUS_InkIncorrect06 = 2426;
        protected const int RESULT_STATUS_InkIncorrect07 = 2427;
        protected const int RESULT_STATUS_InkIncorrect08 = 2428;
        protected const int RESULT_STATUS_InkIncorrect09 = 2429;
        protected const int RESULT_CARDRFID_CommTimeoutError = 4051;
        protected const int RESULT_PRINT_TIMEOUT = 5001;
        protected const int RESULT_UPDATE_RFIDFIRM = 5003;

        protected const byte POLISHTYPE_GLOSSY = 18;
        protected const byte POLISHTYPE_GLOSSY_HOLO = 21;

        protected const int FORMAT_PIXEL_RGB = 3;
        protected const int FORMAT_PIXEL_BGR = 4;
        protected const int COMPONENT_RGB = 3;
        protected const int COLOR_DEPTH = 8;

        protected const ushort RENDERING_INTENTS_PERCEPTUAL = 0;
        protected const ushort RENDERING_INTENTS_RELATIVE_COLORMETRIC = 1;
        protected const ushort RENDERING_INTENTS_SATURATION = 2;

        protected const int StartPage_Exit = 0;
        protected const int StartPage_Standby_YMC = 1;
        protected const int StartPage_Standby_Holo = 2;
        protected const int StartPage_Standby_RFID = 3;

        protected const int Standby_YMC = 0;
        protected const int Standby_Holo = 1;
        protected const int Standby_RFID = 2;
        #endregion

        #region CHC error table
        private static readonly PrinterError[] ERROR_TABLE = new PrinterError[] {
            new PrinterError(RESULT_NOERROR, 0, "OK"),
            new PrinterError(RESULT_STATUS_CardLoadErr, 0, "Card tray failed loading card"),
            new PrinterError(RESULT_STATUS_CardUnSetErr, 6801, "Card tray is empty"),
            new PrinterError(RESULT_STATUS_CardEmptyErr, 6801, "Card tray is empty"),
            new PrinterError(RESULT_STATUS_InkEmptyErr, 6802, 1, "No color ribbon is present"),
            new PrinterError(RESULT_STATUS_InkIncorrectErr, 6803, "Color ribbon data could not be read"),
            new PrinterError(RESULT_STATUS_InkEmptyHoloErr, 6802, 2, "No holo ribbon is present"),
            new PrinterError(RESULT_STATUS_UpperCoverOpenErr, 6807, "Printer lid is open"),
            new PrinterError(RESULT_STATUS_CartridgeOpenErr, 6808, "Card compartment is open"),
            new PrinterError(RESULT_STATUS_InkIncorrect00, 6820, "Color ribbon is not compatible (0)"),
            new PrinterError(RESULT_STATUS_InkIncorrect01, 6820, "Color ribbon is not compatible (1)"),
            new PrinterError(RESULT_STATUS_InkIncorrect02, 6820, "Color ribbon is not compatible (2)"),
            new PrinterError(RESULT_STATUS_InkIncorrect03, 6820, "Color ribbon is not compatible (3)"),
            new PrinterError(RESULT_STATUS_InkIncorrect04, 6820, "Color ribbon is not compatible (4)"),
            new PrinterError(RESULT_STATUS_InkIncorrect05, 6820, "Color ribbon is not compatible (5)"),
            new PrinterError(RESULT_STATUS_InkIncorrect06, 6820, "Color ribbon is not compatible (6)"),
            new PrinterError(RESULT_STATUS_InkIncorrect07, 6820, "Color ribbon is not compatible (7)"),
            new PrinterError(RESULT_STATUS_InkIncorrect08, 6820, "Color ribbon is not compatible (8)"),
            new PrinterError(RESULT_STATUS_InkIncorrect09, 6820, "Color ribbon is not compatible (9)"),
            new PrinterError(RESULT_STATUS_BUSY, 0, "Printer is busy"),
            new PrinterError(RESULT_STATUS_MainCpuInitialize, 0, "Printer is initializing"),
            new PrinterError(RESULT_STATUS_RibbonInitialize, 0, "Printer is checking ribbons"),
            new PrinterError(RESULT_STATUS_CardLoading, 0, "Printer is loading card"),
            new PrinterError(RESULT_STATUS_Operation, 0, "Printer is operating"),
            new PrinterError(RESULT_STATUS_Selfdiagnosis, 0, "Printer is diagnosing"),
            new PrinterError(RESULT_STATUS_ThermalProtect, 6810, "Printer is overheated"),
            new PrinterError(RESULT_USBNOTFOUND, 0, "Initialization failed (missing DLLs?)"),
            new PrinterError(RESULT_OPENPORT, 0, "No printer connected"),
            new PrinterError(RESULT_MUTEX_Wait, 6810, "Other printer error"),
            new PrinterError(RESULT_THREAD_MAX, 0, "Thread access error"),
            new PrinterError(RESULT_PRMERROR, 0, "Invalid argument"),
            new PrinterError(RESULT_UNKNOWN, 6810, "Other printer error"),
            new PrinterError(RESULT_STATUS_UNKNOWNPAPER, 0, "Attempted to set unknown paper type"),
            new PrinterError(2406, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2407, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(2401, 6817, "Loaded card(s) do not match the dedicated card"),
            new PrinterError(RESULT_STATUS_BootMode, 0, "NG"),
            new PrinterError(RESULT_USB_CommErr, 6804, "Communication error with printer"),
            new PrinterError(RESULT_CARDRFID_CommTimeoutError, 6818, "Error communicating with the RFID board of the printer"),
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
            new PrinterError(RESULT_STATUS_DustBoxFullErr, 6809, "The printer scrap box is full"),
            new PrinterError(RESULT_STATUS_InkUnSetErr, 6812, "Color ribbon can not be recongized"),
            new PrinterError(RESULT_STATUS_InkRibbonErr, 6813, 1, "Failed to cut or wind the color ribbon"),
            new PrinterError(RESULT_STATUS_HoloRibbonErr, 6813, 2, "Failed to cut or wind the holo ribbon"),
            new PrinterError(RESULT_NOTCONNECT, 0, 0, "Printer is not connected"),
            new PrinterError(9999, 9999, "No return code")
        };
        #endregion

        public Size ImageDimensions { get; private set; }
        protected INativeTrampolineCHC Native { get; private set; }
        protected RFIDBackend RFIDBackend { get; private set; }

        // Printer properties

        public string IccTable1FileName { get; private set; }
        public string IccTable2FileName { get; private set; }
        public string MtfFileName { get; private set; }
        public StretchMode ImageStretchMode { get; set; } = StretchMode.SizeMustMatch;

        // Instance properties

        protected PrintJob Job { get; private set; }
        private Thread printThread;
        private bool isConnected;
        private PrinterThreadDelegate threadFunc;
        private DeviceStatus? threadCallStatus;
        private ushort threadStatusCode;
        private ushort currentStatusCode;

        protected CHCSeriesCardPrinter(INativeTrampolineCHC dllFunctions, RFIDBackend rfidBackend, Size imageSize) {
            ArgumentNullException.ThrowIfNull(dllFunctions);
            ArgumentNullException.ThrowIfNull(imageSize);
            ImageDimensions = imageSize;
            RFIDBackend = rfidBackend;
            Native = dllFunctions;
        }

        public ushort GetPrinterStatusCode() {
            return currentStatusCode;
        }

        public abstract DeviceStatus GetLoadedCardId(out byte[] cardid);

        public abstract DeviceStatus ConnectRFID();

        public abstract DeviceStatus DisconnectRFID();

        public abstract DeviceStatus WriteRFID(ref ushort rc, byte[] payload, out byte[] writtenCardId);

        public abstract void VerifyRFIDData(byte[] payload);

        protected abstract ushort GetStartPageParameter();

        protected DeviceStatus SetLastErrorByRC(int operationReturnCode, ushort rc = RESULT_STATUS_READY) {
            if (operationReturnCode != CHCUSB_RC_OK && rc != RESULT_STATUS_READY) {
                rc = GetPrinterStatusCode();
            }
            if (rc != RESULT_STATUS_BUSY && rc != RESULT_STATUS_READY) {
                Log.WriteError("Printer Error (" + rc + "): " + RCToString(rc));
                if (Debugger.IsAttached) {
                    Debugger.Break();
                }
            }
            if (rc == CHCUSB_RC_OK || rc == RESULT_STATUS_READY) {
                return SetLastError(DeviceStatus.OK);
            } else if (rc == CHCUSB_RC_BUSY || rc == RESULT_STATUS_BUSY) {
                return DeviceStatus.BUSY;
            }
            return SetLastError((DeviceStatus)rc);
        }

        protected DeviceStatus PrintWaitFor(ref ushort rc, StatusWaitDelegate printerFunction, int timeout, params ushort[] wait_codes) {
            DateTime start = DateTime.Now;
            while (printerFunction(ref rc) == CHCUSB_RC_BUSY || wait_codes.Contains(rc)) {
                if ((DateTime.Now - start).TotalMilliseconds > timeout) {
                    Log.WriteError("Timeout was hit: " + timeout);
                    Log.WriteError("Status was: " + RCToString(rc));
                    return SetLastError((DeviceStatus)rc);
                }
                Thread.Sleep(1000);
            }
            return SetLastError(DeviceStatus.OK, rc);
        }

        public void SetIccTables(string icc1Filename, string icc2Filename) {
            ArgumentNullException.ThrowIfNull(icc1Filename);
            ArgumentNullException.ThrowIfNull(icc2Filename);
            if (!File.Exists(icc1Filename)) {
                throw new FileNotFoundException(null, icc1Filename);
            }
            if (!File.Exists(icc2Filename)) {
                throw new FileNotFoundException(null, icc2Filename);
            }
            IccTable1FileName = icc1Filename;
            IccTable2FileName = icc2Filename;
            Log.Write("ICC tables set to: " + IccTable1FileName + ", " + IccTable2FileName);
        }

        public void SetMtfFile(string mtfFilename) {
            ArgumentNullException.ThrowIfNull(mtfFilename);
            if (!File.Exists(mtfFilename)) {
                throw new FileNotFoundException(null, mtfFilename);
            }
            MtfFileName = mtfFilename;
            Log.Write("MTF set to: " + MtfFileName);
        }

        public sealed override DeviceStatus Connect() {
            DeviceStatus ret;

            Log.Write("Connect");

            ResetState();
            isConnected = true;
            printThread = new Thread(PrintCommandExecutor) {
                Name = "Printer Command Executor"
            };
            printThread.Start();

            ret = ExecuteOnPrintThread((ref ushort rc) => {
                DeviceStatus ret = SetLastErrorByRC(Native.CHC_open(ref rc), rc);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Open failed");
                    return ret;
                }

                ret = SelectById();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Select failed");
                    return ret;
                }

                Log.Write("Waiting until printer has finished initalizing");
                return PrintWaitFor(ref rc, Native.CHC_status, 120_000, RESULT_STATUS_MainCpuInitialize, RESULT_STATUS_RibbonInitialize, RESULT_STATUS_CardLoading, RESULT_STATUS_Operation, RESULT_STATUS_Selfdiagnosis, RESULT_STATUS_DownLoading, RESULT_STATUS_BootMode);
            });

            if (ret != DeviceStatus.OK) {
                return ret;
            }

            return SetLastError(ConnectRFID());
        }

        private void PrintCommandExecutor() {
            Log.Write("Thread started");

            DateTime lastStatusQuery = DateTime.MinValue;

            while (isConnected) {

                ushort rc = 0;

                Monitor.Enter(this);
                try {
                    if (threadFunc != null) {
                        Log.Write("Printer Query: " + threadFunc);
                        try {
                            threadCallStatus = threadFunc(ref rc);
                            threadStatusCode = rc;
                        } catch (Exception ex) {
                            Log.WriteFault(ex, "Printer query failed with exception");
                            threadCallStatus = DeviceStatus.ERR_OTHER;
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

            Log.Write("Thread exited");
        }
        protected DeviceStatus ExecuteOnPrintThread(PrinterThreadDelegate func, bool waitForCompletion = true, bool waitForStart = false) {
            if (printThread == null) {
                throw new ThreadStateException("printer thread has shut down");
            }
            if (printThread.ManagedThreadId == Environment.CurrentManagedThreadId) {
#if DEBUG
                Log.WriteWarning("Calling ExecuteOnPrintThread on the printer thread!");
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
                    return DeviceStatus.BUSY;
                }
            } else {
                threadCallStatus = null;
                threadFunc = func;
            }
            lock (printThread) {
                Monitor.Pulse(printThread);
            }
            if (!waitForCompletion) {
                return DeviceStatus.OK;
            }
            while (threadCallStatus == null && isConnected) {
                Thread.Sleep(10);
            }
            if (!isConnected || threadCallStatus == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }
            if (threadStatusCode != RESULT_NOERROR) {
                Log.WriteWarning("Printer Query returned: " + RCToString(threadStatusCode));
            }
            return threadCallStatus.Value;
        }

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
            Array.Fill<byte>(idArray, 0xFF);
            unsafe {
                fixed (byte* idArrayPtr = idArray) {
                    ret = SetLastErrorByRC(Native.CHC_listupPrinter(idArrayPtr));
                }
            }
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Listup failed");
                return ret;
            }

            byte printerId = 0xFF;
            foreach (byte id in idArray) {
                if (id != 0xFF) {
                    printerId = id;
                }
            }
            if (printerId == 0xFF) {
                return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
            }
            Log.Write("Select Printer: " + printerId);
            Native.CHC_selectPrinter(printerId, ref rc); // this seems to return 0 for some obscure reason
            return SetLastErrorByRC(CHCUSB_RC_OK, rc);
        }

        [Obsolete("This causes an access violation", true)]
        private DeviceStatus SelectBySN() {
            CheckCallingThread();

            const ulong NO_ENTRY = 0xFFFFFFFFFFFFFFFF;
            DeviceStatus ret;
            ushort rc = 0;

            ulong[] idArray = new ulong[0x400];
            Array.Fill(idArray, NO_ENTRY);
            unsafe {
                fixed (ulong* idArrayPtr = idArray) {
                    ret = SetLastErrorByRC(Native.CHC_listupPrinterSN(idArrayPtr));
                }
            }
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            ulong printerId = NO_ENTRY;
            foreach (ulong id in idArray) {
                if (id != NO_ENTRY) {
                    printerId = id;
                }
            }
            if (printerId == NO_ENTRY) {
                return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
            }
            Log.Write("Select Printer (SN): " + printerId);
            return SetLastErrorByRC(Native.CHC_selectPrinterSN(printerId, ref rc), rc);
        }

        public sealed override DeviceStatus Disconnect() {
            isConnected = false;
            if (printThread != null) {
                lock (printThread) {
                    Monitor.Pulse(printThread);
                }
                Log.Write("Waiting for thread completion");
                printThread.Join();
                printThread = null;
            }

            DeviceStatus ret = SetLastError(DisconnectRFID());
            if (ret == DeviceStatus.OK) {
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
            Log.Write("GetPrinterInfo: " + tag);
            uint len = tag.GetBufferSize();
            byte[] buf = new byte[len];
            DeviceStatus ret = ExecuteOnPrintThread((ref ushort rc) => {
                DeviceStatus ret;
                unsafe {
                    fixed (byte* ptr = buf) {
                        ret = SetLastErrorByRC(Native.CHC_getPrinterInfo((ushort)tag, ptr, ref len));
                    }
                }
                if (ret != DeviceStatus.OK) {
                    buf = null;
                }
                return ret;
            });
            data = buf;
            return ret;
        }

        public DeviceStatus GetPrinterSerial(out string serialno) {
            Log.Write("GetPrinterSerial");
            serialno = null;
            DeviceStatus ret = GetPrinterInfo(PrinterInfoTag.SERIALINFO, out byte[] buf);
            if (ret == DeviceStatus.OK) {
                serialno = UnsafeUtils.BytesToString(buf);
            }
            return ret;
        }
        public DeviceStatus GetPrintIDStatus(out int status) {
            Log.Write("GetPrintIDStatus");

            int statusQ = 0;

            DeviceStatus ret = ExecuteOnPrintThread((ref ushort rc) => {
                byte[] buf = new byte[8];
                unsafe {
                    fixed (byte* ptr = buf) {
                        ret = SetLastErrorByRC(Native.CHC_getPrintIDStatus(0, ptr, ref rc), rc);
                    }
                }

                if (ret == DeviceStatus.OK) {
                    statusQ = buf[7] << 8 | buf[6];
                }

                return ret;
            });

            status = statusQ;

            return ret;
        }

        public DeviceStatus GetPrintJobResult() {
            PrintStatus status = Job?.JobStatus ?? PrintStatus.None;
            if (status == PrintStatus.None) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            } else if (status == PrintStatus.Errored) {
                if (IsUsingExceptions) {
                    throw new Exception("An exception occurred during printing", Job.JobException);
                } else {
                    return Job.JobResult;
                }
            } else if (status < PrintStatus.Finished) {
                return DeviceStatus.BUSY;
            } else {
                return Job.JobResult;
            }
        }

        public DeviceStatus GetWrittenRFIDCardId(out byte[] cardid) {
            PrintStatus status = Job?.JobStatus ?? PrintStatus.None;
            if (status == PrintStatus.None) {
                cardid = null;
                return DeviceStatus.ERR_NOT_INITIALIZED;
            } else if (status > PrintStatus.RFIDWrite) {
                cardid = Job.WrittenRFIDCardId;
                return DeviceStatus.OK;
            } else if (status != PrintStatus.Errored) {
                cardid = null;
                return DeviceStatus.ERR_DEVICE;
            } else {
                cardid = null;
                return DeviceStatus.BUSY;
            }
        }

        public DeviceStatus StartPrinting(Bitmap image, byte[] rfidPayload = null, Bitmap holo = null, bool waitForCompletion = false) {
            ArgumentNullException.ThrowIfNull(image);
            if (image.PhysicalDimension != ImageDimensions) {
                if (ImageStretchMode == StretchMode.Stretch) {
                    image = image.CopyStretched(ImageDimensions);
                } else if (ImageStretchMode == StretchMode.Center) {
                    image = image.CopyCentered(ImageDimensions);
                } else {
                    throw new ArgumentException("Image to print with size " + image.PhysicalDimension + " does not match expected printer size of " + ImageDimensions);
                }
            }
            if (holo != null && holo.PhysicalDimension != ImageDimensions) {
                if (ImageStretchMode == StretchMode.Stretch) {
                    image = image.CopyStretched(ImageDimensions);
                } else if (ImageStretchMode == StretchMode.Center) {
                    image = image.CopyCentered(ImageDimensions);
                } else {
                    throw new ArgumentException("Holo image to print with size " + holo.PhysicalDimension + " does not match expected printer size of " + ImageDimensions);
                }
            }
            VerifyRFIDData(rfidPayload);
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
                    Log.WriteWarning("Rejecting StartPrinting, previous print is not complete");
                    return SetLastError(DeviceStatus.BUSY);
                }
            }

            Job = new PrintJob(this, Native, image, holo, rfidPayload);

            Log.Write("Start Printing");
            Log.Write("Current Status: " + RCToString(GetPrinterStatusCode()));

            return SetLastError(ExecuteOnPrintThread(Job.Run, waitForCompletion));
        }

        protected DeviceStatus PrintExitThreadError(DeviceStatus ret, ushort rc, ushort? pageId = null) {
            if (Job == null) {
                throw new ThreadStateException("Can't exit print job thread while no thread is running");
            }
            return Job.PrintExitThreadError(ret, rc, pageId);
        }

        public static string RCToErrorMessage(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).Message;
        }

        public static int RCToSegaError(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).ErrorCodeExt;
        }

        public static string RCToString(int error) {
            return ERROR_TABLE.Where(pe => pe.ErrorCodeInt == error).FirstOrDefault(new PrinterError(error, PRINTER_ERROR_UNKNOWN, "Unknown Printer Error")).ToString();
        }
    }
}