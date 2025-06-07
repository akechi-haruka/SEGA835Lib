#if NET8_0_OR_GREATER

using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {

    /// <summary>
    /// This class defines a "tag" of printer information that can be queried for on a CHC-series printer.
    /// </summary>
    public enum PrinterInfoTag : ushort {
        /// <summary>
        /// Unknown.
        /// </summary>
        [PrinterTagBufferSize(0x67)]
        PAPER = 0,
        /// <summary>
        /// Unknown.
        /// </summary>
        USBINQ = 2,
        /// <summary>
        /// Mostly unknown, firmware sizes.
        /// </summary>
        [PrinterTagBufferSize(0x99)]
        ENGID = 3,
        /// <summary>
        /// A <see cref="PrintCnt"/> structure.
        /// </summary>
        PRINTCNT = 4,
        /// <summary>
        /// A <see cref="PrintCnt2"/> structure.
        /// </summary>
        [PrinterTagBufferSize(0x28)]
        PRINTCNT2 = 5,
        /// <summary>
        /// A <see cref="PageStatus"/> structure.
        /// </summary>
        [PrinterTagBufferSize(0x20)]
        PAGESTATUS = 6,
        /// <summary>
        /// Unknown.
        /// </summary>
        SVCINFO = 7,
        /// <summary>
        /// Unknown.
        /// </summary>
        [PrinterTagBufferSize(0x1)]
        PRINTSTANDBY = 8,
        /// <summary>
        /// Unknown.
        /// </summary>
        MEMORY = 16,
        /// <summary>
        /// Unknown.
        /// </summary>
        PRINTMODE = 20,
        /// <summary>
        /// A single string of the printer's serial number.
        /// </summary>
        [PrinterTagBufferSize(0x8)]
        SERIALINFO = 26,
        /// <summary>
        /// The first three bytes return the printer temperature?
        /// </summary>
        [PrinterTagBufferSize(0xA)]
        TEMPERATURE = 40,
        /// <summary>
        /// Unknown.
        /// </summary>
        [PrinterTagBufferSize(0x3D)]
        ERRHISTORY = 50,
        /// <summary>
        /// Unknown.
        /// </summary>
        TONETABLE = 60
    }

    /// <summary>
    /// Helper class to allow Attribute retrieval.
    /// </summary>
    public static class PrinterInfoTagExtensions {

        /// <summary>
        /// Returns the size in bytes that the response buffer for this PrinterInfoTag will be.
        /// </summary>
        /// <param name="tag">The PrinterInfoTag.</param>
        /// <returns>The required buffer size or zero.</returns>
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

#endif