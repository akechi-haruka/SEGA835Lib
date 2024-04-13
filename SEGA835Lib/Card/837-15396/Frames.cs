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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketLEDSetChannel : JVSPayload {
        public byte rgb;
        public byte value;
        public byte CommandID => 0x80;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketLEDSetChannel : JVSPayload {
        public byte CommandID => 0x80;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketLEDSetColor : JVSPayload {
        public byte red;
        public byte green;
        public byte blue;
        public byte CommandID => 0x81;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketLEDSetColor : JVSPayload {
        public byte CommandID => 0x81;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketLEDGetInfo : JVSPayload {
        public byte CommandID => 0xF0;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketLEDGetInfo : JVSPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string info;
        public byte CommandID => 0xF0;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketLEDHWVersion : JVSPayload {
        public byte CommandID => 0xF1;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketLEDHWVersion : JVSPayload {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 23)]
        public string version;
        public byte CommandID => 0xF1;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ReqPacketLEDReset : JVSPayload {
        public byte CommandID => 0xF5;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct RespPacketLEDReset : JVSPayload {
        public byte CommandID => 0xF5;
    }
}
