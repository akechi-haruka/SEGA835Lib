using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.Misc {
    /// <summary>
    /// A Y3 601-13160(-01,-02) camera board, also known as playfield camera, printer camera or Y3CR BD SIE F720MM used in the Taisen series.
    /// TODO: get rid of the DLL dependency and analyze serial protocol
    /// </summary>
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Y3 : Device {
        /// <summary>
        /// The maximum number of cards the game can read.
        /// </summary>
        public const uint MAX_CARDS = 16;

        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Y3));

        static Y3() {
#if NET5_0_OR_GREATER
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        /// <summary>
        /// The COM port where this board is connected to.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The firmware version installed on this board.
        /// This is only set after <see cref="Connect"/> succeeded.
        /// </summary>
        public float FirmwareVersion { get; private set; }

        /// <summary>
        /// The firmware name installed on this board.
        /// This is only set after <see cref="Connect"/> succeeded.
        /// </summary>
        public String FirmwareName { get; private set; }

        /// <summary>
        /// Unknown.
        /// This is only set after <see cref="Connect"/> succeeded.
        /// </summary>
        public String TargetCode { get; private set; }

        private IntPtr handle;
        private CardInfo[] cards;

        /// <summary>
        /// Creates a new Y3 board.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        public Y3(int port) {
            Port = port;
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "Y3";
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "601-13160";
        }

        /// <summary>
        /// Connects to the device.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if connection was successful.
        /// <see cref="DeviceStatus.ErrorLibrary"/> if there is an error trying to load Y3CodeReaderNE.dll<br />
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board is not attached or if opening the device fails.<br />
        /// </returns>
        public override DeviceStatus Connect() {
            try {
                LOG.LogInformation("Y3 DLL version: " + Native.API_DLLVersion());
            } catch (Exception ex) {
                LOG.LogCritical(ex, "DLL initialization failed");
                return DeviceStatus.ErrorLibrary;
            }

            LOG.LogInformation("Opening Y3 board at port " + Port);

            handle = Native.API_Connect("COM" + Port);
            if (handle == IntPtr.Zero) {
                LOG.LogError("Could not open Y3 board at port " + Port);
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            LOG.LogInformation("Connected");

            UpdateBoardInfo();

            if (IsCalibrationNeeded) {
                LOG.LogWarning("Calibration is required!");
            }

            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            if (handle != IntPtr.Zero) {
                LOG.LogInformation("Disconnecting on Port " + Port);
                Native.API_Close(handle);
                LOG.LogInformation("Disconnected on Port " + Port);
            }

            handle = IntPtr.Zero;
            cards = null;

            return DeviceStatus.Ok;
        }

        private void UpdateBoardInfo() {
            FirmwareVersion = Native.API_GetFirmVersion(handle);
            FirmwareName = Encoding.ASCII.GetString(BitConverter.GetBytes(Native.API_GetFirmName(handle)));
            TargetCode = Encoding.ASCII.GetString(BitConverter.GetBytes(Native.API_GetTargetCode(handle)));
        }

        /// <summary>
        /// Returns the type of firmware that is installed on this Y3 board.
        /// </summary>
        /// <returns>a <see cref="FirmwareType"/> based on the <see cref="FirmwareName"/>.</returns>
        public FirmwareType GetFirmwareType() {
            switch (FirmwareName) {
                case "SFPR":
                    return FirmwareType.Field;
                case "SPRT":
                    return FirmwareType.Printer;
                default:
                    return FirmwareType.Unknown;
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        /// <returns>a <see cref="TargetCodeType"/> based on the <see cref="TargetCode"/>.</returns>
        public TargetCodeType GetTargetCodeType() {
            switch (TargetCode) {
                case "SFR0":
                    return TargetCodeType.Field;
                case "SPT0":
                    return TargetCodeType.Printer;
                case "ENON":
                    return TargetCodeType.None;
                default:
                    return TargetCodeType.Unknown;
            }
        }

        /// <summary>
        /// Returns true if calibration is needed for this board. The value of this is only valid after <see cref="Connect"/> was called.
        /// </summary>
        public bool IsCalibrationNeeded => GetFirmwareType() == FirmwareType.Field && GetTargetCodeType() != TargetCodeType.Field;

        /// <summary>
        /// Sets the Y3 board parameters to defaults that SEGA uses for Eiketsu Taisen's playfield camera.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called, or any other status on error.</returns>
        public DeviceStatus SetParamsForPlayfield() {
            LOG.LogInformation("SetParamsForPlayfield");

            DeviceStatus ret = SetCardCount(MAX_CARDS);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            ret = SetClbMode(false);

            return ret;
        }

        /// <summary>
        /// Sets the Y3 board parameters to defaults that SEGA uses for Sangokushi Taisen's Printer Camera.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called, or any other status on error.</returns>
        public DeviceStatus SetParamsForPrinter() {
            LOG.LogInformation("SetParamsForPrinter");
            return SetCardCount(1);
        }

        /// <summary>
        /// Sets the number of cards this board should concurrently read.
        /// </summary>
        /// <param name="count">The number of cards to read.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called, or any other status on error.</returns>
        public DeviceStatus SetCardCount(uint count) {
            LOG.LogInformation("SetCardCount(" + count + ")");
            if (handle == IntPtr.Zero) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            if (Native.API_SetParameter(handle, Native.ParameterNumber.CodeMax, new uint[] { count }) != 0) {
                return ReportBoardError();
            }

            cards = new CardInfo[count];

            return DeviceStatus.Ok;
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        /// <param name="set">true to enable "Clb", false to disable.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called, or any other status on error.</returns>
        public DeviceStatus SetClbMode(bool set) {
            LOG.LogInformation("SetClbMode(" + set + ")");
            if (handle == IntPtr.Zero) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            if (Native.API_SetParameter(handle, Native.ParameterNumber.ClbMode, new uint[] { set ? 1U : 0U }) != 0) {
                return ReportBoardError();
            }

            return DeviceStatus.Ok;
        }

        /// <summary>
        /// Starts reading cards.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called, or any other status on error.</returns>
        public DeviceStatus Start() {
            LOG.LogInformation("Start");
            if (handle == IntPtr.Zero) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            if (Native.API_Start(handle) != 0) {
                return ReportBoardError();
            }

            return DeviceStatus.Ok;
        }

        /// <summary>
        /// Stops reading cards.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called, or any other status on error.</returns>
        public DeviceStatus Stop() {
            LOG.LogInformation("Stop");
            if (handle == IntPtr.Zero) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            if (Native.API_Stop(handle) != 0) {
                return ReportBoardError();
            }

            return DeviceStatus.Ok;
        }

        /// <summary>
        /// Retrieves the cards that were detected by the camera.
        /// </summary>
        /// <param name="validCards">The number of cards that were detected.</param>
        /// <param name="cardData">An array with a size equal to the parameter passed to <see cref="SetCardCount"/> containing the scanned cards (including empty entries if not enough cards were scanned).</param>
        /// <param name="procTime">The time it took to detect the cards in milliseconds.</param>
        /// <returns>
        /// * <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called<br />
        /// * <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="SetCardCount"/> was not called, either directly or indirectly via <see cref="SetParamsForPlayfield"/>/<see cref="SetParamsForPrinter"/> <br />
        /// * or any other status on failure.
        /// </returns>
        public DeviceStatus GetCards(out uint validCards, out CardInfo[] cardData, out uint procTime) {
            cardData = null;
            procTime = 0;
            validCards = 0;

            if (handle == IntPtr.Zero) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            if (cards == null) {
                return SetLastError(DeviceStatus.ErrorNotInitialized);
            }

            int count = Native.API_GetCardInfo(handle, cards.Length, cards);
            if (count < 0) {
                return ReportBoardError();
            }

            validCards = (uint)count;
            cardData = cards;
            procTime = Native.API_GetProcTime(handle);

            return DeviceStatus.Ok;
        }

        /// <summary>
        /// Resets the board (without reconnecting)
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other status on error.</returns>
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public DeviceStatus Reset() {
            LOG.LogInformation("Reset");
            if (Native.API_Reset(handle, false) != 0) {
                return ReportBoardError();
            }

            return DeviceStatus.Ok;
        }

        /// <summary>
        /// Starts board calibration. This is normally never needed. This method blocks until completion. 
        /// </summary>
        /// <param name="mode">The mode to use for calibration. Use <see cref="CalibrationType.AutoParam"/> for the playfield and <see cref="CalibrationType.Led"/> for the printer camera.</param>
        /// <returns>
        /// * <see cref="DeviceStatus.ErrorNotConnected"/> if <see cref="Connect"/> was not called<br />
        /// * <see cref="DeviceStatus.ErrorTimeout"/> if the calibration was not finished in time or status changes didn't happen in time
        /// * <see cref="DeviceStatus.ErrorDevice"/> if settings were read back incorrectly or an unexpected <see cref="TargetCode"/> was encountered
        /// * or any other status on failure.
        /// </returns>
        public DeviceStatus Calibrate(CalibrationType mode) {
            LOG.LogInformation("Calibrate");
            if (handle == IntPtr.Zero) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            Reset();

            LOG.LogInformation("Waiting for idle status");

            Status lastStatus;
            DateTime start = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(5);
            do {
                lastStatus = GetStatus();
                LOG.LogInformation("Status = " + lastStatus);

                Thread.Sleep(250);
            } while (lastStatus != Status.Idle && DateTime.Now - start < timeout);

            if (DateTime.Now - start >= timeout) {
                LOG.LogError("Calibration timed out");
                return SetLastError(DeviceStatus.ErrorTimeout, (int)lastStatus);
            }

            LOG.LogInformation("SetParameter");
            uint targetValue = 0;
            if (Native.API_SetParameter(handle, Native.ParameterNumber.ClbMode, new uint[] { targetValue }) != 0) {
                return ReportBoardError();
            }

            uint[] readback = { 0x123456 };
            if (Native.API_GetParameter(handle, Native.ParameterNumber.ClbMode, readback) != 0) {
                return ReportBoardError();
            }

            if (readback[0] != targetValue) {
                LOG.LogError("Failed to write parameter, expected " + targetValue + ", got " + readback[0]);
                return SetLastError(DeviceStatus.ErrorDevice);
            }

            if (mode == CalibrationType.AutoParam) {
                LOG.LogInformation("Start AutoParam calibration");

                if (Native.API_Calibration(handle, Native.CalibrationMode.PARAM) != 0) {
                    return ReportBoardError();
                }

                do {
                    lastStatus = GetStatus();
                    Thread.Sleep(250);
                } while (lastStatus == Status.Calibration);

                LOG.LogInformation("AutoParam calibration done");

                if (lastStatus != 0) {
                    LOG.LogError("Calibration failed: " + lastStatus);
                    return ReportBoardError();
                }

                Reset();

                UpdateBoardInfo();

                if (GetTargetCodeType() != TargetCodeType.Field) {
                    LOG.LogError("Unexpected target code: " + GetTargetCodeType() + " / " + TargetCode);
                    return SetLastError(DeviceStatus.ErrorDevice);
                }
            } else if (mode == CalibrationType.Led) {
                LOG.LogInformation("Start Led calibration");

                if (Native.API_Calibration(handle, Native.CalibrationMode.LED_5x5) != 0) {
                    return ReportBoardError();
                }

                uint[] ledInfo = new uint[128];
                do {
                    lastStatus = GetStatus();
                    if (lastStatus == Status.ErrorCalibrationLed) {
                        Thread.Sleep(250);
                    } else if (lastStatus >= Status.ErrorBegin) {
                        return ReportBoardError();
                    }

                    if (Native.API_GetCalibrationResult(handle, Native.CalibrationMode.LED_5x5, ledInfo) == 0) {
                        LOG.LogInformation("Calibrated at X:" + ledInfo[0] + ",Y:" + ledInfo[1]); // TODO: what do we do with this?
                    }
                } while (lastStatus != Status.Calibration);

                if (Native.API_Calibration(handle, 0) != 0) {
                    return ReportBoardError();
                }

                LOG.LogInformation("Calibration complete");
            } else {
                LOG.LogError("Invalid calibration mode: " + mode);
                return SetLastError(DeviceStatus.ErrorIncompatible);
            }

            LOG.LogInformation("Calibration complete");
            return DeviceStatus.Ok;
        }

        [SuppressMessage("Performance", "CA1806")]
        private DeviceStatus ReportBoardError() {
            uint ec = Native.API_GetLastError(handle);
            byte[] str = new byte[512];
            Native.API_GetErrorMessage(ec, str, str.Length);
            LOG.LogError("Y3 board reported error (status=" + Native.API_GetStatus(handle) + "): " + ec + " / " + Encoding.GetEncoding("shift_jis").GetString(str));
            return SetLastError(DeviceStatus.ErrorDevice, (int)ec);
        }

        /// <summary>
        /// Returns the current board status.
        /// </summary>
        /// <returns>the current board status</returns>
        public Status GetStatus() {
            return (Status)Native.API_GetStatus(handle);
        }

        /// <summary>
        /// Checks that on a printer camera board if the camera marker is detected. As a convenience, this method starts and stops the board if the board is not started. If it is, no change in status will be performed.
        /// </summary>
        /// <param name="detected">Whether or not the marker was detected.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> if a read attempt was made, any other status on error</returns>
        public DeviceStatus DetectPrinterMarker(out bool detected) {
            LOG.LogInformation("Detecting printer marker");

            DeviceStatus ret;
            detected = false;
            bool isRunning = GetStatus() == Status.Active;

            if (!isRunning) {
                ret = SetParamsForPrinter();
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = Start();
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }
            }

            const int maxRetries = 10;
            int retry = 0;
            do {
                LOG.LogDebug("Retry " + retry);

                ret = GetCards(out uint count, out CardInfo[] data, out _);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                LOG.LogDebug("Count " + count);
                if (count > 0) {
                    foreach (CardInfo card in data) {
                        if (card.IsValidMarker()) {
                            detected = true;
                        }
                    }
                }

                if (!detected) {
                    Thread.Sleep(500);
                }
            } while (retry++ < maxRetries && !detected);

            if (!isRunning) {
                ret = Stop();
            }

            return ret;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static class Native {
            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern float API_DLLVersion();

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetLastError(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetErrorMessage(uint errNo, [Out] byte[] szMessage, int numBytes);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr API_Connect(string szPortName);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_Close(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_Start(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_Stop(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern float API_GetFirmVersion(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetFirmName(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetTargetCode(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetStatus(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetCounter(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_ClearError(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_Reset(IntPtr hDevice, [MarshalAs(UnmanagedType.Bool)] [In] bool isHardReset);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_GetCardInfo(IntPtr hDevice, int numCards, [Out] CardInfo[] pCardInfo);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_GetCardInfoCharSize();

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_FirmwareUpdate(IntPtr hDevice, uint address, uint size, byte[] buffer);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_Calibration(IntPtr hDevice, CalibrationMode calib);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_GetCalibrationResult(IntPtr hDevice, CalibrationMode calib, uint[] result);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetProcTime(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetMemStatus(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint API_GetMemCounter(IntPtr hDevice);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_SetParameter(IntPtr hDevice, ParameterNumber uParam, uint[] pParam);

            [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int API_GetParameter(IntPtr hDevice, ParameterNumber uParam, uint[] pParam);

            private const string DLL_NAME = "Y3CodeReaderNE";

            [Flags]
            public enum CalibrationMode {
                END = 0b0,
                CAMERA = 0b10000,
                PARAM = 0b100000,
                LED_DEFAULT = 0b10000000,
                LED_3x3 = 0b10000001,
                LED_4x4 = 0b10000010,
                LED_5x5 = 0b10000011,
                VIEW_CARD_8 = 0b10010000,
                VIEW_CARD_4 = 0b10010001,
                VIEW_CARD_1 = 0b10010010,
                VIEW_ADJ_8 = 0b10010011,
                VIEW_ALL_8 = 0b10010100,
                VIEW_ALL_4 = 0b10010101,
                VIEW_ALL_1 = 0b10010110,
                VIEW_DEBUG_1 = 0b10011000,
                VIEW_DEBUG_2 = 0b10011001,
                VIEW_DEBUG_3 = 0b10011010,
                VIEW_DEBUG_4 = 0b10011011,
                VIEW_HALF_8 = 0b10011100,
                VIEW_QTR_8 = 0b10011101,
                VIEW_ETH_8 = 0b10011110,
                VIEW_HALF_1 = 0b10011111
            }

            public enum ParameterNumber : uint {
                DebugMode = 0xE5,
                CodeMax = 0xF1,
                ProcMode = 0xF2,
                CardMode = 0xF4,
                CardType = 0xF5,
                ClbMode = 0x100
            }
        }

        /// <summary>
        /// Struct representing card data read from the Y3 board. 
        /// </summary>
        public struct CardInfo {
            /// <summary>
            /// The bit that is set if the "IV code" of a card belongs to Eiketsu Taisen.
            /// </summary>
            public const ulong EKT_IV_CODE = 0x200000000UL;

            /// <summary>
            /// X position of the card.
            /// </summary>
            public float X;

            /// <summary>
            /// Y position of the card.
            /// </summary>
            public float Y;

            /// <summary>
            /// Rotation of the card in [0.0, 360.0)
            /// </summary>
            public float Rotation;

            /// <summary>
            /// Unused.
            /// </summary>
            public Type UnknownType;

            /// <summary>
            /// What was detected in this card slot?
            /// </summary>
            public DetectionType CardType;

            /// <summary>
            /// The card ID. This is used for object tracking, as well as part of the card's IV code.
            /// </summary>
            /// <seealso cref="GetIvCode"/>
            public uint ID;

            /// <summary>
            /// The amount of data* fields that are filled.
            /// </summary>
            public int DataCount;

            /// <summary>
            /// Data field holding the title code.
            /// </summary>
            public CardByteData Data0;

            /// <summary>
            /// Possibly type of a marker card. Only value ever observed is 0x4000 for the printer camera.
            /// </summary>
            public CardByteData Data1;

            /// <summary>
            /// Unknown.
            /// </summary>
            public CardByteData Data2;

            /// <summary>
            /// Unknown. Only value ever observed is 0x0 for the printer camera.
            /// </summary>
            public CardByteData Data3;

            /// <summary>
            /// Unknown.
            /// </summary>
            public CardByteData Data4;

            /// <summary>
            /// Unknown.
            /// </summary>
            public CardByteData Data5;

            /// <summary>
            /// Unused.
            /// </summary>
            public enum Type {
                /// <summary>
                /// Unused.
                /// </summary>
                Value1,

                /// <summary>
                /// Unused.
                /// </summary>
                Value2,

                /// <summary>
                /// Unused.
                /// </summary>
                Value3,

                /// <summary>
                /// Unused.
                /// </summary>
                Value4,

                /// <summary>
                /// Unused.
                /// </summary>
                Value5,

                /// <summary>
                /// Unused.
                /// </summary>
                Value6,

                /// <summary>
                /// Unused.
                /// </summary>
                Value7,

                /// <summary>
                /// Unused.
                /// </summary>
                Value8
            }

            /// <summary>
            /// The object that was detected in this slot.
            /// </summary>
            public enum DetectionType {
                /// <summary>
                /// Nothing was detected.
                /// </summary>
                Invalid,

                /// <summary>
                /// A card was detected.
                /// </summary>
                Card,

                /// <summary>
                /// Infrared interference was detected. This is not a real object or card and must be ignored.
                /// </summary>
                Interference,

                /// <summary>
                /// The printer camera specific marker was detected.
                /// </summary>
                /// <seealso cref="IsValidMarker"/>
                Marker
            }

            /// <summary>
            /// "Title codes" that can be stored on a card.
            /// </summary>
            public enum TitleCode : byte {
                /// <summary>
                /// This card is for Eiketsu Taisen.
                /// </summary>
                EiketsuTaisen = 0x2
            }

            /// <summary>
            /// Returns the card's "IV code". This code is queried to the game server to load what this card actually is. (what general, etc.)
            /// </summary>
            /// <returns>the card's IV code.</returns>
            public long GetIvCode() {
                return (long)(((ulong)Data0.GetTitleCode() << 32) | ID);
            }

            /// <summary>
            /// Return the card's "title code".
            /// </summary>
            /// <returns></returns>
            public TitleCode GetTitleCode() {
                BitVector32 bitVector = new BitVector32((int)Data0.Data);
                return (TitleCode)(byte)bitVector[CardByteData.ID];
            }

            /// <summary>
            /// Returns true if this card's "IV code" is for Eiketsu taisen.
            /// </summary>
            /// <returns>true if this card's "IV code" is for Eiketsu taisen</returns>
            public bool IsEiketsuIvCode() {
                return ((ulong)GetIvCode() & EKT_IV_CODE) != 0;
            }

            /// <summary>
            /// Returns true if this card is a marker (the card attached to the inside of the printer camera).
            /// </summary>
            /// <returns>true if this card is a marker</returns>
            public bool IsValidMarker() {
                return CardType != DetectionType.Invalid && ID == 0x10 && Data0.Data > 0x0 && Data3.Data == 0x0;
            }

            /// <summary>
            /// Returns true if this card (slot) is valid.
            /// </summary>
            /// <returns>true if this card (slot) is valid</returns>
            public bool IsValidCard() {
                return CardType == DetectionType.Card;
            }

            /// <summary>
            /// Returns true if the object in this slot is valid. Consider using <see cref="IsValidCard"/> instead, as this function includes the marker card that is attached to the inside of the printer camera.
            /// </summary>
            /// <returns>true if the object in this slot is valid</returns>
            public bool IsValid() {
                return IsValidCard() || IsValidMarker();
            }

            /// <inheritdoc/>
            public override string ToString() {
                return ToCsv();
            }

            /// <summary>
            /// Converts this card data to a csv seperated by ";" (to work around decimal seperators)
            /// where the fields are: <see cref="CardType"/>;<see cref="UnknownType"/>;<see cref="X"/>;<see cref="Y"/>;<see cref="Rotation"/>;<see cref="ID"/>;<see cref="DataCount"/>;<see cref="GetIvCode"/>;<see cref="GetTitleCode"/>;<see cref="Data0"/>;<see cref="Data1"/>;<see cref="Data2"/>;<see cref="Data3"/>;<see cref="Data4"/>;<see cref="Data5"/>
            /// </summary>
            /// <remarks>any <see cref="CardByteData"/>s at indexes greater than <see cref="DataCount"/> are not </remarks>
            /// <example>Card;Type5;123;456;123.456;1234567;4;123456;EiketsuTaisen;1;2;3;4;0;0</example>
            /// <returns>A string representing this object in delimited format</returns>
            public String ToCsv() {
                return CardType + ";" +
                       UnknownType + ";" +
                       X + ";" +
                       Y + ";" +
                       Rotation + ";" +
                       ID + ";" +
                       DataCount + ";" +
                       GetIvCode() + ";" +
                       GetTitleCode() + ";" +
                       Data0 + ";" +
                       Data1 + ";" +
                       Data2 + ";" +
                       Data3 + ";" +
                       Data4 + ";" +
                       Data5 + ";";
            }
        }

        /// <summary>
        /// A data value stored on a Y3 card.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CardByteData {
            internal static readonly BitVector32.Section ID = BitVector32.CreateSection(4);
            internal static readonly BitVector32.Section CHECK_BIT = BitVector32.CreateSection(8, ID);
            internal static readonly BitVector32.Section CODE_TYPE = BitVector32.CreateSection(3, CHECK_BIT);
            internal static readonly BitVector32.Section MIRROR = BitVector32.CreateSection(1, CODE_TYPE);
            internal static readonly BitVector32.Section RESERVE = BitVector32.CreateSection(16, MIRROR);

            /// <summary>
            /// Interprets this data value as a title code. Preferably use <see cref="CardInfo.GetTitleCode"/> instead.
            /// </summary>
            /// <returns>the raw title code</returns>
            public byte GetTitleCode() {
                BitVector32 bitVector = new BitVector32((int)Data);
                return (byte)bitVector[ID];
            }

            /// <inheritdoc/>
            public override string ToString() {
                return Data.ToString();
            }

            /// <summary>
            /// The raw data value.
            /// </summary>
            public uint Data;
        }

        /// <summary>
        /// Type of the firmware installed on a Y3 board.
        /// </summary>
        public enum FirmwareType {
            /// <summary>
            /// The firmware on this Y3 board is for the playfield.
            /// The Y3 board's serial code is 601-13160-01.
            /// </summary>
            Field,

            /// <summary>
            /// The firmware on this Y3 board is for the printer camera inside the CHC-320 forn Sangokushi Taisen.
            /// The Y3 board's serial code is 601-13160-01.
            /// </summary>
            Printer,

            /// <summary>
            /// Unknown.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// "Target code" types.
        /// </summary>
        public enum TargetCodeType {
            /// <summary>
            /// Unknown.
            /// </summary>
            None,

            /// <summary>
            /// The target code for this Y3 board is for the playfield.
            /// The Y3 board's serial code is 601-13160-01.
            /// </summary>
            Field,

            /// <summary>
            /// The target code for this Y3 board is for the printer camera inside the CHC-320 forn Sangokushi Taisen.
            /// The Y3 board's serial code is 601-13160-01.
            /// </summary>
            Printer,

            /// <summary>
            /// Unknown.
            /// </summary>
            Unknown
        }

        /// <summary>
        /// The calibration that should be performed.
        /// </summary>
        public enum CalibrationType {
            /// <summary>
            /// Unknown. Used for the playfield firmware.
            /// </summary>
            AutoParam,

            /// <summary>
            /// Unknown. Used for the printer camera.
            /// </summary>
            Led
        }

        /// <summary>
        /// Status codes returned by <see cref="GetStatus"/>. Most of these are unknown.
        /// </summary>
        public enum Status : uint {
            /// <summary>
            /// Connected to the board, but not reading anything. Call <see cref="Start"/> to start reading.
            /// </summary>
            Idle = 0x0,

            /// <summary>
            /// The board is reading (asynchronously). Call <see cref="GetCards"/> to retrieve detected cards.
            /// </summary>
            Active = 0x1,
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            BlockingProcess = 0x2,
            Reboot = 0x3,

            /// <summary>
            /// Calibration is in progress.
            /// </summary>
            Calibration = 0x4,
            ParameterProcessing = 0x5,
            BoardChecking = 0x6,
            BoardChecked = 0x7,
            ErrorBegin = 0xF0000000,
            ErrorSystem = 0xF0000001,
            ErrorInput = 0xF0000002,
            ErrorFan = 0xF0000003,
            ErrorOther = 0xF0000004,
            ErrorInit = 0xF00000F0,
            ErrorInitHeader = 0xF00000F1,
            ErrorInitEeprom = 0xF00000F2,
            ErrorParamLoad = 0xF0000010,
            ErrorParamSave = 0xF0000020,
            ErrorEepromBlockRead = 0xF0000030,
            ErrorFlashBlockRead = 0xF0000040,
            ErrorFlashChecksum = 0xF0000048,
            ErrorDdrBlockRead = 0xF0000050,
            ErrorEepromBlockWriteW = 0xF0000060,
            ErrorEepromBlockWriteR = 0xF0000061,
            ErrorEepromBlockWriteV = 0xF0000062,
            ErrorEepromBlockWriteD = 0xF0000068,
            ErrorFlashBlockWriteW = 0xF0000070,
            ErrorFlashBlockWriteR = 0xF0000071,
            ErrorFlashBlockWriteV = 0xF0000072,
            ErrorFlashBlockWriteD = 0xF0000078,
            ErrorCameraSelect = 0xF0000080,
            ErrorCameraRegRead = 0xF00000A0,
            ErrorCameraRegWrite = 0xF00000A1,
            ErrorEepromRead = 0xF00000B0,
            ErrorEepromWrite = 0xF00000B1,
            ErrorCalibrationCamera = 0xF00000C1,
            ErrorCalibrationParam = 0xF00000C2,
            ErrorCalibrationLed = 0xF00000C8,
            ErrorCalibrationView = 0xF00000C9,
            ErrorCheckBoard = 0xF00000D0,
            ErrorTimeout = 0xF0000100
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}