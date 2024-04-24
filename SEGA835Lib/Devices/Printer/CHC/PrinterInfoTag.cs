using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    public enum PrinterInfoTag : ushort {
        [PrinterTagBufferSize(0x67)]
        PAPER = 0,
        USBINQ = 2,
        [PrinterTagBufferSize(0x99)]
        ENGID = 3,
        PRINTCNT = 4,
        [PrinterTagBufferSize(0x28)]
        PRINTCNT2 = 5,
        [PrinterTagBufferSize(0x20)]
        UNKNOWN = 6,
        SVCINFO = 7,
        [PrinterTagBufferSize(0x1)]
        PRINTSTANDBY = 8,
        MEMORY = 16,
        PRINTMODE = 20,
        [PrinterTagBufferSize(0x8)]
        SERIALINFO = 26,
        [PrinterTagBufferSize(0xA)]
        TEMPERATURE = 40,
        [PrinterTagBufferSize(0x3D)]
        ERRHISTORY = 50,
        TONETABLE = 60
    }

    internal static class PrinterInfoTagExtensions {

        public static uint GetBufferSize(this PrinterInfoTag tag) {
            return tag.GetAttribute<PrinterTagBufferSize>()?.Value ?? 0;
        }

    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    internal class PrinterTagBufferSize : Attribute {

        private readonly uint value;

        public PrinterTagBufferSize(uint value) {
            this.value = value;
        }

        public uint Value {
            get { return this.value; }
        }

    }
}
