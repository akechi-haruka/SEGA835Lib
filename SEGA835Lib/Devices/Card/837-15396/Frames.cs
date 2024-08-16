using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x62;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x62;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketGetFWVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketGetFWVersion : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketGetFWVersion1Byte : SProtPayload {

        internal byte version;

        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketGetHWVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x32;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketGetHWVersion : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte GetCommandID() {
            return 0x32;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketRadioOn : SProtPayload {
        public byte type;

        public byte GetCommandID() {
            return 0x40;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketRadioOn : SProtPayload {
        public byte GetCommandID() {
            return 0x40;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketRadioOff : SProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketRadioOff : SProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketPoll : SProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }
    }

    /* RespPacketPoll: dynamic */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketLEDSetChannel : SProtPayload {
        public byte rgb;
        public byte value;

        public byte GetCommandID() {
            return 0x80;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketLEDSetColor : SProtPayload {
        public byte red;
        public byte green;
        public byte blue;

        public byte GetCommandID() {
            return 0x81;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketLEDGetInfo : SProtPayload {
        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketLEDGetInfo : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string info;

        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketLEDHWVersion : SProtPayload {
        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketLEDHWVersion : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        public string version;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketLEDReset : SProtPayload {
        public byte GetCommandID() {
            return 0xF5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketLEDReset : SProtPayload {
        public byte GetCommandID() {
            return 0xF5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketReadMIFARE : SProtPayload {

        public uint uid;
        [MarshalAs(UnmanagedType.U1)]
        public byte block;

        public byte GetCommandID() {
            return 0x52;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct RespPacketReadMIFARE : SProtPayload {

        public fixed byte data[16];

        public byte GetCommandID() {
            return 0x52;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct ReqPacketMIFARESetKeySega : SProtPayload {

        public fixed byte key[6];

        public byte GetCommandID() {
            return 0x54;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMIFARESetKeySega : SProtPayload {

        public byte GetCommandID() {
            return 0x54;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct ReqPacketMIFARESetKeyNamco : SProtPayload {

        public fixed byte key[6];

        public byte GetCommandID() {
            return 0x50;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMIFARESetKeyNamco : SProtPayload {

        public byte GetCommandID() {
            return 0x50;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketSelectMIFARE : SProtPayload {

        public uint uid;

        public byte GetCommandID() {
            return 0x43;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct RespPacketSelectMIFARE : SProtPayload {

        public fixed byte data[16];

        public byte GetCommandID() {
            return 0x43;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketAuthenticateMIFARE : SProtPayload {

        public uint uid;
        [MarshalAs(UnmanagedType.U1)]
        public byte unk;

        public byte GetCommandID() {
            return 0x55;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct RespPacketAuthenticateMIFARE : SProtPayload {

        public fixed byte data[16];

        public byte GetCommandID() {
            return 0x55;
        }
    }
}
