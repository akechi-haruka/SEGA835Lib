#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320 {
    /// <summary>
    /// A CHC-320 Card Printer for Sangokushi Taisen.
    /// </summary>
    public class Chc320Printer : ChcSeriesCardPrinter {
        private readonly Y3 printerCamera;
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc320Printer));

        /// <summary>
        /// Called when card data is successfully read from the printer camera.
        /// </summary>
        public event Action<Y3.CardInfo> CardDataRead;

        /// <summary>
        /// Creates a new CHC-320 printer.
        /// </summary>
        /// <param name="y3">The Y3 board to use as the printer camera, or null to not use.</param>
        public Chc320Printer(Y3 y3) : base(new Native(), null, new Size(664, 1036)) {
            printerCamera = y3;
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

        /// <summary>
        /// Reads the current card information from the connected Y3 board, and calls <see cref="CardDataRead"/> on success.
        /// </summary>
        /// <param name="rc">Ignored.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or if no Y3 board was specified, <see cref="DeviceStatus.ErrorDevice"/> otherwise.</returns>
        protected override DeviceStatus ReadCardInformation(ref ushort rc) {
            // TODO: read from Y3
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        protected override ushort GetInitialCardPosition() {
            return STANDBY_CARD_CAMERA;
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