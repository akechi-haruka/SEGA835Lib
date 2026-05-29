using System.Runtime.InteropServices;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketReset : ISProtPayload {
        public byte GetCommandID() {
            return 0x62;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketReset : ISProtPayload {
        public byte GetCommandID() {
            return 0x62;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketGetFirmwareVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetFirmwareVersion : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetFirmwareVersion1Byte : ISProtPayload {
        internal byte version;

        public byte GetCommandID() {
            return 0x30;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketGetHardwareVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0x32;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetHardwareVersion : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte GetCommandID() {
            return 0x32;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketRadioOn : ISProtPayload {
        public byte type;

        public byte GetCommandID() {
            return 0x40;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketRadioOn : ISProtPayload {
        public byte GetCommandID() {
            return 0x40;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketRadioOff : ISProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketRadioOff : ISProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketPoll : ISProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }
    }

    /* RespPacketPoll: dynamic */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketLedSetChannel : ISProtPayload {
        public byte rgb;
        public byte value;

        public byte GetCommandID() {
            return 0x80;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketLedSetColor : ISProtPayload {
        public byte red;
        public byte green;
        public byte blue;

        public byte GetCommandID() {
            return 0x81;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketLedGetInfo : ISProtPayload {
        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketLedGetInfo : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string info;

        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketLedHardwareVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketLedHardwareVersion : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        public string version;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketLedReset : ISProtPayload {
        public byte GetCommandID() {
            return 0xF5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketLedReset : ISProtPayload {
        public byte GetCommandID() {
            return 0xF5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketReadMifare : ISProtPayload {
        public uint uid;
        [MarshalAs(UnmanagedType.U1)] public byte block;

        public byte GetCommandID() {
            return 0x52;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct RespPacketReadMifare : ISProtPayload {
        public fixed byte data[16];

        public byte GetCommandID() {
            return 0x52;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct ReqPacketMifareSetKeySega : ISProtPayload {
        public fixed byte key[6];

        public byte GetCommandID() {
            return 0x54;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMifareSetKeySega : ISProtPayload {
        public byte GetCommandID() {
            return 0x54;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct ReqPacketMifareSetKeyNamco : ISProtPayload {
        public fixed byte key[6];

        public byte GetCommandID() {
            return 0x50;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMifareSetKeyNamco : ISProtPayload {
        public byte GetCommandID() {
            return 0x50;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketSelectMifare : ISProtPayload {
        public uint uid;

        public byte GetCommandID() {
            return 0x43;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct RespPacketSelectMifare : ISProtPayload {
        public fixed byte data[16];

        public byte GetCommandID() {
            return 0x43;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketAuthenticateMifare : ISProtPayload {
        public uint uid;
        [MarshalAs(UnmanagedType.U1)] public byte unk;

        public byte GetCommandID() {
            return 0x55;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct RespPacketAuthenticateMifare : ISProtPayload {
        public fixed byte data[16];

        public byte GetCommandID() {
            return 0x55;
        }
    }
}