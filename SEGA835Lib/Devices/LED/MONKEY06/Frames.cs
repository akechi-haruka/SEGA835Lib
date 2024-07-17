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

        public ushort checksum;

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

}
