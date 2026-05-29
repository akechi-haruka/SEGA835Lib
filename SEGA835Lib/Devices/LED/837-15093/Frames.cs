using System;
using System.Runtime.InteropServices;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093 {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketReset : ISProtPayload {
        public byte reset_type;

        public byte GetCommandID() {
            return 0x10;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketReset : ISProtPayload {
        public byte GetCommandID() {
            return 0x10;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketGetBoardInfo : ISProtPayload {
        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetBoardInfo : ISProtPayload {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public String board_number;

        public byte padding;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public String chip_number;

        public byte padding_;
        public byte fw_ver;

        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketGetFirmwareChecksum : ISProtPayload {
        public byte GetCommandID() {
            return 0xF2;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetFirmwareChecksum : ISProtPayload {
        public byte fw_checksum_b1;
        public byte fw_checksum_b2;

        public byte GetCommandID() {
            return 0xF2;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketGetProtocolVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0xF3;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetProtocolVersion : ISProtPayload {
        public byte appli_mode;
        public byte major;
        public byte minor;

        public byte GetCommandID() {
            return 0xF3;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketSetTimeout : ISProtPayload {
        public ushort timeout;

        public byte GetCommandID() {
            return 0x11;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketSetTimeout : ISProtPayload {
        public ushort timeout;

        public byte GetCommandID() {
            return 0x11;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketGetBoardStatus : ISProtPayload {
        public byte flagclear;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketGetBoardStatus : ISProtPayload {
        public byte boardflag;
        public byte uartflag;
        public byte cmdflag;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketSetDisableResponse : ISProtPayload {
        public byte enable;

        public byte GetCommandID() {
            return 0x14;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketSetDisableResponse : ISProtPayload {
        public byte enable;

        public byte GetCommandID() {
            return 0x14;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    unsafe struct ReqPacketSetLeds : ISProtPayload {
        public fixed byte pixels[66 * 3];

        public byte GetCommandID() {
            return 0x82;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketSetLeds : ISProtPayload {
        public byte GetCommandID() {
            return 0x82;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct ReqPacketSetLedCount : ISProtPayload {
        public byte count;

        public byte GetCommandID() {
            return 0x86;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct RespPacketSetLedCount : ISProtPayload {
        public byte count;

        public byte GetCommandID() {
            return 0x86;
        }
    }
}