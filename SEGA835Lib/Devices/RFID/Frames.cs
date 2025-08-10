using Haruka.Arcade.SEGA835Lib.Serial;
using System.Runtime.InteropServices;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketReset : SProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketGetBootVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x84;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketGetBootVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x84;
        }

        public byte version;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketGetBoardInfo : SProtPayload {
        public byte GetCommandID() {
            return 0x85;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketGetBoardInfo : SProtPayload {
        public byte GetCommandID() {
            return 0x85;
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string version;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketUnknown81 : SProtPayload {
        public byte GetCommandID() {
            return 0x81;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketUnknown81 : SProtPayload {
        public byte GetCommandID() {
            return 0x81;
        }

        public byte unk;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketGetAppVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketGetAppVersion : SProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }

        public byte version;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketUnknown4 : SProtPayload {
        public byte unk;
        public byte unk2;

        public byte GetCommandID() {
            return 0x04;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketUnknown4 : SProtPayload {
        public byte GetCommandID() {
            return 0x04;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketUnknown5 : SProtPayload {
        public byte unk;

        public byte GetCommandID() {
            return 0x05;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketUnknown5 : SProtPayload {
        public byte GetCommandID() {
            return 0x05;
        }
    }
}