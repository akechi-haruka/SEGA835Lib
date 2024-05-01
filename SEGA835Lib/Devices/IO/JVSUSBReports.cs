using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO {

    public enum JVSUSBReports : byte {
        Unset = 0x00,
        SetCommTimeout = 0x01,
        SetSamplingCount = 0x02,
        ClearBoardStatus = 0x03,
        SetGeneralOutput = 0x04,
        SetPWMOutput = 0x05,
        UnknownChunithm = 0x41,
        UpdateFirmware = 0x85
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JVSUSBReportOut {
        public JVSUSBReports cmd;
        public fixed byte payload[62];
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JVSUSBReportIn {
        public const int ADC_COUNT = 8;
        public const int SPINNER_COUNT = 4;
        public const int CHUTE_COUNT = 2;
        public const int BUTTON_COUNT = 2;

        public fixed ushort adcs[ADC_COUNT];
        public fixed ushort spinners[SPINNER_COUNT];
        public fixed ushort chutes[CHUTE_COUNT];
        public fixed ushort buttons[BUTTON_COUNT];
        public byte system_status;
        public byte usb_status;
        public fixed byte unknown[29];
    }


}
