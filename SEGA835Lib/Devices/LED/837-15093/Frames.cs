using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093 {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketReset : SProtPayload {

        public byte reset_type;

        public byte GetCommandID() {
            return 0x10;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x10;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketGetBoardInfo : SProtPayload {

        public byte GetCommandID() {
            return 0xF0;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct RespPacketGetBoardInfo : SProtPayload {

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
    internal struct ReqPacketGetFirmwareChecksum : SProtPayload {

        public byte GetCommandID() {
            return 0xF2;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketGetFirmwareChecksum : SProtPayload {

        public ushort fw_checksum;

        public byte GetCommandID() {
            return 0xF2;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketGetProtocolVersion : SProtPayload {

        public byte GetCommandID() {
            return 0xF3;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketGetProtocolVersion : SProtPayload {

        public byte appli_mode;
        public byte major;
        public byte minor;

        public byte GetCommandID() {
            return 0xF3;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketSetTimeout : SProtPayload {

        public ushort timeout;

        public byte GetCommandID() {
            return 0x11;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketSetTimeout : SProtPayload {

        public ushort timeout;

        public byte GetCommandID() {
            return 0x11;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketGetBoardStatus : SProtPayload {

        public byte flagclear;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketGetBoardStatus : SProtPayload {

        public byte boardflag;
        public byte uartflag;
        public byte cmdflag;

        public byte GetCommandID() {
            return 0xF1;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketSetDisableResponse : SProtPayload {

        public byte enable;

        public byte GetCommandID() {
            return 0x14;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketSetDisableResponse : SProtPayload {

        public byte enable;

        public byte GetCommandID() {
            return 0x14;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct ReqPacketSetLEDs : SProtPayload {

        public fixed byte pixels[66 * 3];

        public byte GetCommandID() {
            return 0x82;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketSetLEDs : SProtPayload {

        public byte GetCommandID() {
            return 0x82;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketSetLEDCount : SProtPayload {

        public byte count;

        public byte GetCommandID() {
            return 0x86;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketSetLEDCount : SProtPayload {

        public byte count;

        public byte GetCommandID() {
            return 0x86;
        }
    }

}
