using System;
using System.Runtime.InteropServices;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06 {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketMonkeyReset : ISProtPayload {
        public byte GetCommandID() {
            return 0xA0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeyReset : ISProtPayload {
        public byte GetCommandID() {
            return 0xA0;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketMonkeySetChecksum : ISProtPayload {
        public byte fw_checksum_b1;
        public byte fw_checksum_b2;

        public byte GetCommandID() {
            return 0xA1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetChecksum : ISProtPayload {
        public byte GetCommandID() {
            return 0xA1;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct ReqPacketMonkeySetTranslation : ISProtPayload {
        public byte offset;
        public fixed byte translation[66];

        public byte GetCommandID() {
            return 0xA2;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetTranslation : ISProtPayload {
        public byte GetCommandID() {
            return 0xA2;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketMonkeySetChannels : ISProtPayload {
        public byte r;
        public byte g;
        public byte b;

        public byte GetCommandID() {
            return 0xA3;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetChannels : ISProtPayload {
        public byte GetCommandID() {
            return 0xA3;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketMonkeySetFirmwareVersion : ISProtPayload {
        public byte ver;

        public byte GetCommandID() {
            return 0xA5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetFirmwareVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0xA5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketMonkeySetChipNumber : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public String chip_no;

        public byte GetCommandID() {
            return 0xA4;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetChipNumber : ISProtPayload {
        public byte GetCommandID() {
            return 0xA4;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketMonkeySetBoardName : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public String board_name;

        public byte GetCommandID() {
            return 0xA6;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetBoardName : ISProtPayload {
        public byte GetCommandID() {
            return 0xA6;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct ReqPacketMonkeySetAuxiliaryLeds : ISProtPayload {
        public fixed byte pixels[66 * 3];

        public byte GetCommandID() {
            return 0xA7;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketMonkeySetAuxiliaryLeds : ISProtPayload {
        public byte GetCommandID() {
            return 0xA7;
        }
    }
}