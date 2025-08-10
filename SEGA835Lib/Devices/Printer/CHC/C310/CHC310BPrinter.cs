#if NET8_0_OR_GREATER

using Haruka.Arcade.SEGA835Lib.Debugging;
using System;
using System.Drawing;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310 {
    /// <summary>
    /// A CHC-310B Card Printer for CardMaker.
    /// Fully inherits functions from the CHC-310.
    /// </summary>
    public class CHC310BPrinter : CHC310Printer {
        private static readonly NativeB native = new NativeB(); // hack to pass the same Native to both parameters

        /// <summary>
        /// Creates a new CHC-310B printer.
        /// </summary>
        public CHC310BPrinter() : base(native, null, new Size(768, 1052)) {
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.OK"/>.</returns>
        public override DeviceStatus ConnectRFID() {
            return DeviceStatus.OK;
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.OK"/>.</returns>
        public override DeviceStatus DisconnectRFID() {
            return DeviceStatus.OK;
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <param name="payload">Ignored.</param>
        /// <param name="overrideCardId">Ignored.</param>
        public override void VerifyRFIDData(byte[] payload, bool overrideCardId) {
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="cardid">Always null.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ERR_INCOMPATIBLE"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override unsafe DeviceStatus GetLoadedCardId(out byte[] cardid) {
            Log.WriteError("RFID data cannot be read from a CHC-310B!");
            cardid = null;
            return DeviceStatus.ERR_INCOMPATIBLE;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="rc">Ignored.</param>
        /// <param name="payload">Ignored.</param>
        /// <param name="overrideCardId">Ignored.</param>
        /// <param name="writtenCardId">Always null.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ERR_INCOMPATIBLE"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus WriteRFID(ref ushort rc, byte[] payload, bool overrideCardId, out byte[] writtenCardId) {
            Log.WriteError("RFID data cannot be written to a CHC-310B!");
            writtenCardId = null;
            return DeviceStatus.ERR_INCOMPATIBLE;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="version">Always 0.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ERR_INCOMPATIBLE"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetRFIDAppVersion(out byte version) {
            version = 0;
            return DeviceStatus.ERR_INCOMPATIBLE;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="board">Always null.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ERR_INCOMPATIBLE"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetRFIDBoardInfo(out string board) {
            board = null;
            return DeviceStatus.ERR_INCOMPATIBLE;
        }

        /// <summary>
        /// Unsupported for this printer model.
        /// </summary>
        /// <param name="version">Always 0.</param>
        /// <returns>Always returns <see cref="DeviceStatus.ERR_INCOMPATIBLE"/>.</returns>
        [Obsolete("Unsupported for this printer model.")]
        public override DeviceStatus GetRFIDBootVersion(out byte version) {
            version = 0;
            return DeviceStatus.ERR_INCOMPATIBLE;
        }
    }
}

#endif