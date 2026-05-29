using System.Runtime.InteropServices;
using Haruka.Arcade.SEGA835Lib.Serial;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketReset : ISProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketReset : ISProtPayload {
        public byte GetCommandID() {
            return 0x41;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketGetBootVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0x84;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketGetBootVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0x84;
        }

        public byte version;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketGetBoardInfo : ISProtPayload {
        public byte GetCommandID() {
            return 0x85;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketGetBoardInfo : ISProtPayload {
        public byte GetCommandID() {
            return 0x85;
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string version;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketUnknown81 : ISProtPayload {
        public byte GetCommandID() {
            return 0x81;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketUnknown81 : ISProtPayload {
        public byte GetCommandID() {
            return 0x81;
        }

        public byte unk;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketGetAppVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketGetAppVersion : ISProtPayload {
        public byte GetCommandID() {
            return 0x42;
        }

        public byte version;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketUnknown4 : ISProtPayload {
        public byte unk;
        public byte unk2;

        public byte GetCommandID() {
            return 0x04;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketUnknown4 : ISProtPayload {
        public byte GetCommandID() {
            return 0x04;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ReqPacketUnknown5 : ISProtPayload {
        public byte unk;

        public byte GetCommandID() {
            return 0x05;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct RespPacketUnknown5 : ISProtPayload {
        public byte GetCommandID() {
            return 0x05;
        }
    }
}