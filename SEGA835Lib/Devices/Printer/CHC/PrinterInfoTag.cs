#if NET8_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags;
using Haruka.Arcade.SEGA835Lib.Misc;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    /// <summary>
    /// This class defines a "tag" of printer information that can be queried for on a CHC-series printer.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum PrinterInfoTag : ushort {
        /// <summary>
        /// Unknown.
        /// </summary>
        [PrinterTagBufferSize(0x67)] Paper = 0,

        /// <summary>
        /// Unknown.
        /// </summary>
        Usbinq = 2,

        /// <summary>
        /// Mostly unknown, firmware sizes.
        /// </summary>
        [PrinterTagBufferSize(0x99)] EngID = 3,

        /// <summary>
        /// A <see cref="PrintCnt"/> structure.
        /// </summary>
        PrintCount = 4,

        /// <summary>
        /// A <see cref="PrintCnt2"/> structure.
        /// </summary>
        [PrinterTagBufferSize(0x28)] PrintCount2 = 5,

        /// <summary>
        /// A <see cref="Tags.PageStatus"/> structure.
        /// </summary>
        [PrinterTagBufferSize(0x20)] PageStatus = 6,

        /// <summary>
        /// Unknown.
        /// </summary>
        SvcInfo = 7,

        /// <summary>
        /// Unknown.
        /// </summary>
        [PrinterTagBufferSize(0x1)] PrintStandby = 8,

        /// <summary>
        /// Unknown.
        /// </summary>
        Memory = 16,

        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 19,

        /// <summary>
        /// Unknown.
        /// </summary>
        PrintMode = 20,

        /// <summary>
        /// A single string of the printer's serial number.
        /// </summary>
        [PrinterTagBufferSize(0x8)] SerialInfo = 26,

        /// <summary>
        /// The first three bytes return the printer temperature?
        /// </summary>
        [PrinterTagBufferSize(0xA)] Temperature = 40,

        /// <summary>
        /// Unknown.
        /// </summary>
        [PrinterTagBufferSize(0x3D)] ErrorHistory = 50,

        /// <summary>
        /// Unknown.
        /// </summary>
        ToneTable = 60
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

    [AttributeUsage(AttributeTargets.Field)]
    class PrinterTagBufferSize : Attribute {
        public PrinterTagBufferSize(uint value) {
            Value = value;
        }

        public uint Value { get; }
    }
}

#endif