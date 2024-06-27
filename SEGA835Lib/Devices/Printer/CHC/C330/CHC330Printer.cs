#if NET8_0_OR_GREATER

using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330 {

    /// <summary>
    /// A CHC-330 Card Printer for Fate/Grand Order Arcade.
    /// </summary>
    public class CHC330Printer : CHCSeriesCardPrinter {

        private RFIDRWPrinter_837_15347 rfid;

        /// <summary>
        /// Creates a new CHC-330 printer.
        /// </summary>
        /// <param name="rfid">The 837-15347 integrated RFID board to use (or null to not use RFID features)</param>
        public CHC330Printer(RFIDRWPrinter_837_15347 rfid) : base(new Native(), rfid?.Backend, new Size(662, 1024)) {
            this.rfid = rfid;
        }

        /// <inheritdoc/>
        public override DeviceStatus ConnectRFID() {
            DeviceStatus ret;
            if (rfid != null) {

                int attempt = 1;
                do {
                    rfid.Disconnect();
                    ret = rfid.Connect();
                    Log.Write("Attempt to connect: " + attempt);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("RFID Connect failed");
                        continue;
                    }

                    ret = rfid.ResetWriter();
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("RFID ResetWriter failed");
                        continue;
                    }
                } while (attempt++ < 3 && ret != DeviceStatus.OK);

            } else {
                ret = DeviceStatus.OK;
            }
            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public override DeviceStatus DisconnectRFID() {
            return SetLastError(rfid?.Disconnect() ?? DeviceStatus.OK);
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
        public RFIDRWPrinter_837_15347 GetRFIDBoard() {
            return rfid;
        }

        /// <inheritdoc/>
        public override void VerifyRFIDData(byte[] payload, bool overrideCardId) {
            if (payload != null && rfid == null) {
                throw new InvalidOperationException("Can't write RFID data to card if no RFID board was set when initializing printer");
            }
            if (payload != null && payload.Length != rfid.GetCardPayloadSize() - (overrideCardId ? 0 : CARD_ID_LEN)) {
                throw new ArgumentException("RFID data size (" + payload.Length + ") must be equal to RFID device's expected data size (" + (rfid.GetCardPayloadSize() - (overrideCardId ? 0 : CARD_ID_LEN)) + ", card ID override = "+overrideCardId+")");
            }
        }

        /// <inheritdoc/>
        public override DeviceStatus WriteRFID(ref ushort rc, byte[] payload, bool overrideCardId, out byte[] writtenCardId) {
            DeviceStatus ret = DeviceStatus.OK;
            writtenCardId = null;
            if (rfid != null) {
                Log.Write("Initializing RFID Board");

                ret = rfid.ResetWriter();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("RFID ResetWriter failed");
                    return PrintExitThreadError(ret, RESULT_CARDRFID_CommandError);
                }

                Log.Write("Reading Card ID from RFID board");
                byte[] cardId = null;
                ret = PrintWaitFor(ref rc, (ref ushort rc) => {
                    ret = GetLoadedCardId(out cardId);
                    if (cardId != null) {
                        rc = RESULT_NOERROR;
                        return CHCUSB_RC_OK;
                    } else {
                        return CHCUSB_RC_BUSY;
                    }
                }, 20000);
                if (ret != DeviceStatus.OK || cardId == null) {
                    Log.WriteError("RFID Read failed");
                    return PrintExitThreadError(ret, RESULT_CARDRFID_CommandError);
                }

                Job.JobStatus = PrintStatus.RFIDWrite;

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
                    Log.WriteWarning("No RFID data to write");
                }
            } else {
                Log.Write("No RFID board was initialized");
            }
            return ret;
        }

        /// <inheritdoc/>
        protected override ushort GetStartPageParameter() {
            return StartPage_Standby_RFID;
        }
    }
}

#endif