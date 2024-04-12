using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Card._837_15396 {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketReset : JVSPayload {
        public byte CommandID => 0x62;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketReset : JVSPayload {
        public byte CommandID => 0x62;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketGetFWVersion : JVSPayload {
        public byte CommandID => 0x30;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketGetFWVersion : JVSPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte CommandID => 0x30;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketGetHWVersion : JVSPayload {
        public byte CommandID => 0x32;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketGetHWVersion : JVSPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        internal string version;

        public byte CommandID => 0x32;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketRadioOn : JVSPayload {
        public byte type;
        public byte CommandID => 0x40;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketRadioOn : JVSPayload {

        public byte CommandID => 0x40;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketRadioOff : JVSPayload {
        public byte CommandID => 0x41;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketRadioOff : JVSPayload {

        public byte CommandID => 0x41;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketPoll : JVSPayload {
        public byte CommandID => 0x42;
    }

    /* RespPacketPoll: dynamic */



}
