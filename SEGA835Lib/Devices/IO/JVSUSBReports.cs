using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO {

    /// <summary>
    /// Enum of possible outgoing report types for USB-based JVS devices. These have not yet been documented.
    /// </summary>
    public enum JVSUSBReports : byte {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unset = 0x00,
        /// <summary>
        /// Unknown.
        /// </summary>
        SetCommTimeout = 0x01,
        /// <summary>
        /// Unknown.
        /// </summary>
        SetSamplingCount = 0x02,
        /// <summary>
        /// Unknown.
        /// </summary>
        ClearBoardStatus = 0x03,
        /// <summary>
        /// Unknown.
        /// </summary>
        SetGeneralOutput = 0x04,
        /// <summary>
        /// Unknown.
        /// </summary>
        SetPWMOutput = 0x05,
        /// <summary>
        /// Super Unknown.
        /// </summary>
        UnknownChunithm = 0x41,
        /// <summary>
        /// Unknown.
        /// </summary>
        UpdateFirmware = 0x85
    }

    /// <summary>
    /// The structure that is sent to JVS-based USB I/O boards. This is not yet documented.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JVSUSBReportOut {
        /// <summary>
        /// The report type being sent.
        /// </summary>
        public JVSUSBReports cmd;
        /// <summary>
        /// Unknown.
        /// </summary>
        public fixed byte payload[62];
    }

    /// <summary>
    /// The structure that is received from JVS-based USB I/O boards.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JVSUSBReportIn {
        /// <summary>
        /// The maximum number of analog devices for this board.
        /// </summary>
        public const int ADC_COUNT = 8;
        /// <summary>
        /// The maximum number of spinner devices for this board.
        /// </summary>
        public const int SPINNER_COUNT = 4;
        /// <summary>
        /// The maximum number of (coin) chutes for this board.
        /// </summary>
        public const int CHUTE_COUNT = 2;
        /// <summary>
        /// The maximum number of players for this board.
        /// </summary>
        public const int BUTTON_COUNT = 2;

        /// <summary>
        /// The current values of the board's analog devices.
        /// </summary>
        public fixed ushort adcs[ADC_COUNT];
        /// <summary>
        /// The current values of the board's spinners.
        /// </summary>
        public fixed ushort spinners[SPINNER_COUNT];
        /// <summary>
        /// The current values of the board's chutes.
        /// </summary>
        public fixed ushort chutes[CHUTE_COUNT];
        /// <summary>
        /// The current values of the board's buttons. Individual buttons are encoded as bits, so ex. the 4th button of player 2 would be tested with (buttons[1] >> 3 &amp; 1) != 0
        /// </summary>
        public fixed ushort buttons[BUTTON_COUNT];
        /// <summary>
        /// Unknown.
        /// </summary>
        public byte system_status;
        /// <summary>
        /// Unknown.
        /// </summary>
        public byte usb_status;
        /// <summary>
        /// Unknown.
        /// </summary>
        public fixed byte unknown[29];
    }


}
