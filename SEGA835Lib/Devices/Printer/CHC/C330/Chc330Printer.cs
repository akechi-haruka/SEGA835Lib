#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330 {
    /// <summary>
    /// A CHC-330 Card Printer for Fate/Grand Order Arcade.
    /// </summary>
    public class Chc330Printer : ChcSeriesCardPrinter {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc330Printer));

        private readonly RfidRwPrinter15347 rfid;

        /// <summary>
        /// Creates a new CHC-330 printer.
        /// </summary>
        /// <param name="rfid">The 837-15347 integrated RFID board to use (or null to not use RFID features)</param>
        public Chc330Printer(RfidRwPrinter15347 rfid) : base(new Native(), rfid?.Backend, new Size(662, 1024)) {
            this.rfid = rfid;
        }

        /// <inheritdoc/>
        public override DeviceStatus ConnectRfid() {
            DeviceStatus ret;
            if (rfid != null) {
                int attempt = 1;
                do {
                    rfid.Disconnect();
                    ret = rfid.Connect();
                    LOG.LogInformation("Attempt to connect: " + attempt);
                    if (ret != DeviceStatus.Ok) {
                        LOG.LogError("RFID Connect failed");
                        continue;
                    }

                    ret = rfid.ResetWriter();
                    if (ret != DeviceStatus.Ok) {
                        LOG.LogError("RFID ResetWriter failed");
                    }
                } while (attempt++ < 3 && ret != DeviceStatus.Ok);
            } else {
                ret = DeviceStatus.Ok;
            }

            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public override DeviceStatus DisconnectRfid() {
            return SetLastError(rfid?.Disconnect() ?? DeviceStatus.Ok);
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "CHC330";
        }

        /// <inheritdoc/>
        public override DeviceStatus GetLoadedCardId(out byte[] cardid) {
            DeviceStatus ret = rfid.Scan(out byte[][] card);
            if (card != null && card.Length == 1) {
                cardid = card[0];
                LOG.LogInformation("Loaded card ID:\n" + Hex.Dump(cardid));
            } else {
                cardid = null;
            }

            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "SINFONIA Card Printer";
        }

        /// <summary>
        /// Returns the 837-15347 RFID board used with this printer.
        /// </summary>
        /// <returns>the RFID board instance or null if not used.</returns>
        public RfidRwPrinter15347 GetRfidBoard() {
            return rfid;
        }

        /// <inheritdoc/>
        public override void VerifyRfidData(byte[] payload, bool overrideCardId) {
            if (payload != null && rfid == null) {
                throw new InvalidOperationException("Can't write RFID data to card if no RFID board was set when initializing printer");
            }

            if (payload != null && payload.Length != rfid.GetCardPayloadSize() - (overrideCardId ? 0 : CARD_ID_LEN)) {
                throw new ArgumentException("RFID data size (" + payload.Length + ") must be equal to RFID device's expected data size (" + (rfid.GetCardPayloadSize() - (overrideCardId ? 0 : CARD_ID_LEN)) + ", card ID override = " + overrideCardId + ")");
            }
        }

        /// <inheritdoc/>
        public override DeviceStatus WriteRfid(ref ushort rc, byte[] payload, bool overrideCardId, out byte[] writtenCardId) {
            DeviceStatus ret = DeviceStatus.Ok;
            writtenCardId = null;
            if (rfid != null) {
                LOG.LogInformation("Initializing RFID Board");

                ret = rfid.ResetWriter();
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("RFID ResetWriter failed");
                    return PrintExitThreadError(ret, RESULT_CARDRFID_COMMAND_ERROR);
                }

                LOG.LogInformation("Reading Card ID from RFID board");
                byte[] cardId = null;
                ret = PrintWaitFor(ref rc, (ref ushort rc) => {
                    ret = GetLoadedCardId(out cardId);
                    if (cardId != null) {
                        rc = RESULT_NOERROR;
                        return CHCUSB_RC_OK;
                    }

                    return CHCUSB_RC_BUSY;
                }, 20000);
                if (ret != DeviceStatus.Ok || cardId == null) {
                    LOG.LogError("RFID Read failed");
                    return PrintExitThreadError(ret, RESULT_CARDRFID_COMMAND_ERROR);
                }

                Job.JobStatus = PrintStatus.RfidWrite;

                if (payload != null) {
                    byte[] cardid = new byte[CARD_ID_LEN];
                    if (overrideCardId) {
                        Array.Copy(payload, cardid, cardid.Length);
                        byte[] payloadWithoutId = new byte[payload.Length - cardid.Length];
                        Array.Copy(payload, cardid.Length, payloadWithoutId, 0, payloadWithoutId.Length);

                        ret = rfid.Write(cardid, payloadWithoutId);
                    } else {
                        Array.Copy(cardId, cardid, cardid.Length);
                        writtenCardId = cardid;

                        ret = rfid.Write(cardid, payload);
                    }
                } else {
                    LOG.LogWarning("No RFID data to write");
                }
            } else {
                LOG.LogInformation("No RFID board was initialized");
            }

            return ret;
        }

        /// <inheritdoc/>
        protected override ushort GetStartPageParameter() {
            return START_PAGE_STANDBY_RFID;
        }

        /// <inheritdoc />
        protected override byte? GetPolishParameter(bool isHolo) {
            return (byte)(isHolo ? 0x11 : 0x02);
        }
    }
}

#endif