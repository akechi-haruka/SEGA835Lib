#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320 {
    /// <summary>
    /// A CHC-320 Card Printer for Sangokushi Taisen.
    /// </summary>
    public class Chc320Printer : ChcSeriesCardPrinter {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc320Printer));

        /// <summary>
        /// Creates a new CHC-320 printer.
        /// </summary>
        public Chc320Printer() : base(new Native(), null, new Size(664, 1036)) {
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
            LOG.LogError("RFID data cannot be read from a CHC-320!");
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
            LOG.LogError("RFID data cannot be written to a CHC-320!");
            writtenCardId = null;
            return DeviceStatus.ErrorIncompatible;
        }

        /// <inheritdoc/>
        protected override ushort GetStartPageParameter() {
            return START_PAGE_EXIT;
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