using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x62;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x62;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketGetFWVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketGetFWVersion : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketGetHWVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x32;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketGetHWVersion : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte GetCommandID() {
            return 0x32;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketRadioOn : SProtPayload {
        public byte type;

        public byte GetCommandID() {
            return 0x40;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketRadioOn : SProtPayload {
        public byte GetCommandID() {
            return 0x40;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketRadioOff : SProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketRadioOff : SProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketPoll : SProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }
    }

    /* RespPacketPoll: dynamic */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketLEDSetChannel : SProtPayload {
        public byte rgb;
        public byte value;

        public byte GetCommandID() {
            return 0x80;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketLEDSetColor : SProtPayload {
        public byte red;
        public byte green;
        public byte blue;

        public byte GetCommandID() {
            return 0x81;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketLEDGetInfo : SProtPayload {
        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketLEDGetInfo : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string info;

        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketLEDHWVersion : SProtPayload {
        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketLEDHWVersion : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        public string version;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct ReqPacketLEDReset : SProtPayload {
        public byte GetCommandID() {
            return 0xF5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 0)]
    internal struct RespPacketLEDReset : SProtPayload {
        public byte GetCommandID() {
            return 0xF5;
        }
    }
}
