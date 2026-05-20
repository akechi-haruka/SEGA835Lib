#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310 {
    /// <summary>
    /// A CHC-310B Card Printer for CardMaker.
    /// Fully inherits functions from the CHC-310.
    /// </summary>
    public class Chc310BPrinter : Chc310Printer {
        private static readonly NativeB NATIVE = new NativeB(); // hack to pass the same Native to both parameters

        /// <summary>
        /// Creates a new CHC-310B printer.
        /// </summary>
        public Chc310BPrinter() : base(NATIVE, null, new Size(768, 1052)) {
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
            Log.WriteError("RFID data cannot be read from a CHC-310B!");
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
            Log.WriteError("RFID data cannot be written to a CHC-310B!");
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