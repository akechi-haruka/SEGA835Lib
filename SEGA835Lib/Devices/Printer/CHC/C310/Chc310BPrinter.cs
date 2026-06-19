#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310 {
    /// <summary>
    /// A CHC-310B Card Printer for CardMaker.
    /// Fully inherits functions from the CHC-310.
    /// </summary>
    public class Chc310BPrinter : Chc310Printer {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc310BPrinter));
        private static readonly NativeB NATIVE = new NativeB(); // hack to pass the same Native to both parameters
        private static readonly byte[] CARD_MAKER_OUTTONE_TABLE = {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
            33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
            49, 50, 51, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63,
            64, 65, 66, 67, 68, 69, 70, 70, 71, 72, 73, 74, 75, 76, 77, 78,
            79, 80, 81, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 91, 92,
            93, 94, 95, 96, 97, 98, 99, 99, 100, 101, 102, 103, 104, 105, 106, 107,
            108, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 117, 118, 119, 120, 121,
            122, 123, 124, 125, 126, 127, 128, 128, 129, 130, 131, 132, 133, 134, 135, 136,
            137, 138, 139, 140, 141, 142, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151,
            152, 153, 154, 155, 156, 157, 158, 158, 159, 160, 161, 162, 163, 164, 165, 166,
            167, 168, 169, 170, 171, 172, 173, 174, 175, 175, 176, 177, 178, 179, 180, 181,
            182, 183, 184, 185, 186, 187, 188, 188, 189, 190, 191, 192, 193, 194, 195, 196,
            197, 198, 199, 200, 201, 202, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211,
            212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 224, 225, 226, 227, 229,
            230, 231, 233, 234, 236, 238, 239, 241, 243, 244, 246, 248, 250, 251, 253, 255,
        };
        private static readonly int[] CARD_MAKER_MTF_200 = {
            0, -25, 0,
            -25, 200, -25,
            0, -25, 0
        };

        /// <summary>
        /// Creates a new CHC-310B printer.
        /// </summary>
        public Chc310BPrinter() : base(NATIVE, null, new Size(768, 1052)) {
            SetDefaultRgbOutputToneTable(CARD_MAKER_OUTTONE_TABLE);
            SetMtfValues(CARD_MAKER_MTF_200);
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.Ok"/>.</returns>
        public override DeviceStatus ConnectRfid() {
            return DeviceStatus.Ok;
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.Ok"/>.</returns>
        public override DeviceStatus DisconnectRfid() {
            return DeviceStatus.Ok;
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <param name="payload">Ignored.</param>
        /// <param name="overrideCardId">Ignored.</param>
        public override void VerifyRfidData(byte[] payload, bool overrideCardId) {
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="cardid">Always null.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ErrorIncompatible"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetLoadedCardId(out byte[] cardid) {
            LOG.LogError("RFID data cannot be read from a CHC-310B!");
            cardid = null;
            return DeviceStatus.ErrorIncompatible;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="rc">Ignored.</param>
        /// <param name="payload">Ignored.</param>
        /// <param name="overrideCardId">Ignored.</param>
        /// <param name="writtenCardId">Always null.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ErrorIncompatible"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus WriteRfid(ref ushort rc, byte[] payload, bool overrideCardId, out byte[] writtenCardId) {
            LOG.LogError("RFID data cannot be written to a CHC-310B!");
            writtenCardId = null;
            return DeviceStatus.ErrorIncompatible;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="version">Always 0.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ErrorIncompatible"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetRfidAppVersion(out byte version) {
            version = 0;
            return DeviceStatus.ErrorIncompatible;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="board">Always null.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ErrorIncompatible"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetRfidBoardInfo(out string board) {
            board = null;
            return DeviceStatus.ErrorIncompatible;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="version">Always 0.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ErrorIncompatible"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetRfidBootVersion(out byte version) {
            version = 0;
            return DeviceStatus.ErrorIncompatible;
        }
    }
}

#endif
