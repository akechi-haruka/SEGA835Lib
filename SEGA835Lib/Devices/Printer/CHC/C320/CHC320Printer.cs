#if NET8_0_OR_GREATER

using Haruka.Arcade.SEGA835Lib.Debugging;
using System;
using System.Drawing;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320 {

    /// <summary>
    /// A CHC-320 Card Printer for Sangokushi Taisen.
    /// </summary>
    public class CHC320Printer : CHCSeriesCardPrinter {

        /// <summary>
        /// Creates a new CHC-320 printer.
        /// </summary>
        public CHC320Printer() : base(new Native(), null, new Size(664, 1036)) {
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
            Log.WriteError("RFID data cannot be read from a CHC-320!");
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
            Log.WriteError("RFID data cannot be written to a CHC-320!");
            writtenCardId = null;
            return DeviceStatus.ERR_INCOMPATIBLE;
        }

        /// <inheritdoc/>
        protected override ushort GetStartPageParameter() {
            return StartPage_Exit;
        }

        /// <inheritdoc />
        protected override byte? GetPolishParameter(bool isHolo) {
            return null;
        }
        
        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "CHC320";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "SINFONIA Card Printer";
        }
    }
}

#endif