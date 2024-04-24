using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330 {
    public class CHC330Printer : CHCSeriesCardPrinter {

        private RFIDRWPrinter_837_15347 rfid;

        public CHC330Printer(RFIDRWPrinter_837_15347 rfid) : base(new Native(), rfid?.Backend, new Size(662, 1024)) {
            this.rfid = rfid;
        }

        public override DeviceStatus ConnectRFID() {
            DeviceStatus ret;
            if (rfid != null) {
                ret = rfid.Connect();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("RFID Connect failed");
                    return SetLastError(ret);
                }

                ret = rfid.ResetWriter();
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("RFID ResetWriter failed");
                    return SetLastError(ret);
                }

            } else {
                ret = DeviceStatus.OK;
            }
            return SetLastError(ret);
        }

        public override DeviceStatus DisconnectRFID() {
            return SetLastError(rfid?.Disconnect() ?? DeviceStatus.OK);
        }

        public override string GetDeviceModel() {
            return "CHC330";
        }

        public override DeviceStatus GetLoadedCardId(out byte[] cardid) {
            DeviceStatus ret = rfid.Scan(out byte[][] card);
            if (card != null && card.Length == 1) {
                cardid = card[0];
            } else {
                cardid = null;
            }
            return SetLastError(ret);
        }

        public override string GetName() {
            return "SINFONIA Card Printer";
        }

        public RFIDRWPrinter_837_15347 GetRFIDBoard() {
            return rfid;
        }

        public override void VerifyRFIDData(byte[] payload) {
            if (payload != null && rfid == null) {
                throw new InvalidOperationException("Can't write RFID data to card if no RFID board was set when initializing printer");
            }
            if (payload != null && payload.Length != rfid.GetCardPayloadSize() - CARD_ID_LEN) {
                throw new ArgumentException("RFID data size (" + payload.Length + ") must be equal to RFID device's expected data size (" + (rfid.GetCardPayloadSize() - CARD_ID_LEN) + ")");
            }
        }

        public override DeviceStatus WriteRFID(ref ushort rc, byte[] payload, out byte[] writtenCardId) {
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
                    Array.Copy(cardId, cardid, cardid.Length);
                    writtenCardId = cardid;

                    ret = rfid.Write(cardid, payload);
                } else {
                    Log.WriteWarning("No RFID data to write");
                }
            } else {
                Log.Write("No RFID board was initialized");
            }
            return ret;
        }

        protected override ushort GetStartPageParameter() {
            return StartPage_Standby_RFID;
        }
    }
}
