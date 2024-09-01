using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06 {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketMonkeyReset : SProtPayload {

        public byte GetCommandID() {
            return 0xA0;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeyReset : SProtPayload {

        public byte GetCommandID() {
            return 0xA0;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketMonkeySetChecksum : SProtPayload {

        public byte fw_checksum_b1;
        public byte fw_checksum_b2;

        public byte GetCommandID() {
            return 0xA1;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeySetChecksum : SProtPayload {

        public byte GetCommandID() {
            return 0xA1;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal unsafe struct ReqPacketMonkeySetTranslation : SProtPayload {

        public fixed byte translation[66];

        public byte GetCommandID() {
            return 0xA2;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeySetTranslation : SProtPayload {

        public byte GetCommandID() {
            return 0xA2;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketMonkeySetChannels : SProtPayload {

        public byte r;
        public byte g;
        public byte b;

        public byte GetCommandID() {
            return 0xA3;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeySetChannels : SProtPayload {

        public byte GetCommandID() {
            return 0xA3;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketMonkeySetFirmwareVersion : SProtPayload {

        public byte ver;

        public byte GetCommandID() {
            return 0xA5;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeySetFirmwareVersion : SProtPayload {

        public byte GetCommandID() {
            return 0xA5;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketMonkeySetChipNumber : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public String chip_no;

        public byte GetCommandID() {
            return 0xA4;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeySetChipNumber : SProtPayload {

        public byte GetCommandID() {
            return 0xA4;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ReqPacketMonkeySetBoardName : SProtPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public String board_name;

        public byte GetCommandID() {
            return 0xA6;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RespPacketMonkeySetBoardName : SProtPayload {

        public byte GetCommandID() {
            return 0xA6;
        }
    }

}
