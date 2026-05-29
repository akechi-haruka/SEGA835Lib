#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using System.Threading;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320 {
    /// <summary>
    /// A CHC-320 Card Printer for Sangokushi Taisen.
    /// </summary>
    public class Chc320Printer : ChcSeriesCardPrinter {
        private const int MAX_CARD_SCAN_RETRIES = 10;
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc320Printer));

        private readonly Y3 printerCamera;

        /// <summary>
        /// Called when card data is successfully read from the printer camera.
        /// </summary>
        public event Action<Y3.CardInfo> CardDataRead;

        /// <summary>
        /// Creates a new CHC-320 printer.
        /// </summary>
        /// <param name="y3">The Y3 board to use as the printer camera, or null to not use.</param>
        public Chc320Printer(Y3 y3) : base(new Native(), null, new Size(662, 1024)) {
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
            return START_PAGE_STANDBY_RFID_OR_CARD_CAMERA;
        }

        /// <inheritdoc />
        protected override byte? GetPolishParameter(bool isHolo) {
            return (byte)(isHolo ? 0x08 : 0x02);
        }

        /// <summary>
        /// Reads the current card information from the connected Y3 board, and calls <see cref="CardDataRead"/> on success.
        /// </summary>
        /// <param name="rc">Ignored.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or if no Y3 board was specified, <see cref="DeviceStatus.ErrorDevice"/> otherwise.</returns>
        protected override DeviceStatus PostProcessing(ref ushort rc) {
            if (printerCamera != null) {
                DeviceStatus ret;
                DeviceStatus cardDetectionStatus = DeviceStatus.ErrorTimeout;

                if (printerCamera.GetStatus() == Y3.Native.Status.Idle) {
                    ret = printerCamera.SetParamsForPrinter();
                    if (ret != DeviceStatus.Ok) {
                        return ret;
                    }

                    ret = printerCamera.Start();
                    if (ret != DeviceStatus.Ok) {
                        return ret;
                    }
                }

                int retry = 0;
                do {
                    LOG.LogDebug("Retry " + retry);

                    ret = printerCamera.GetCards(out uint count, out Y3.CardInfo[] cards, out uint procTime);
                    LOG.LogTrace("Y3 card read took " + procTime);
                    if (ret != DeviceStatus.Ok) {
                        return ret;
                    }

                    if (count > 0) {
                        foreach (Y3.CardInfo card in cards) {
                            if (card.IsValidCard()) {
                                CardDataRead?.Invoke(card);
                                cardDetectionStatus = DeviceStatus.Ok;
                            }
                        }
                    } else {
                        LOG.LogWarning("Printer camera did not find a card");
                        Thread.Sleep(500);
                    }
                } while (retry++ < MAX_CARD_SCAN_RETRIES && cardDetectionStatus != DeviceStatus.Ok);

                if (cardDetectionStatus != DeviceStatus.Ok) {
                    LOG.LogError("Timed out trying to find card");
                }

                ret = printerCamera.Stop();
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                return cardDetectionStatus;
            }

            LOG.LogWarning("Printer camera not configured");
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        protected override ushort? GetInitialCardPosition() {
            return null;
        }

        /// <inheritdoc/>
        protected override byte? GetParameter19() {
            return 5;
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