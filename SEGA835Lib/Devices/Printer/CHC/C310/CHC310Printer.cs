using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;

using Haruka.Arcade.SEGA835Lib.Serial;
using System.Drawing;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310 {
    public class CHC310Printer : CHCSeriesCardPrinter {

        private const byte COMMAND_WRITE_START_STOP = 0x10;
        private const byte SUBCOMMAND_WRITE_START_STOP = 0x02;
        private const byte COMMAND_WRITE_BLOCK = 0x11;
        private const byte SUBCOMMAND_WRITE_BLOCK = 0x03;

        private static readonly Native native = new Native(); // hack to pass the same Native to both parameters

        public CHC310Printer() : base(native, new RFIDBackendCHCDLL(native), new Size(768, 1052)) {
        }

        public override DeviceStatus ConnectRFID() {
            return SetLastError(ExecuteOnPrintThread((ref ushort rc) => {
                DeviceStatus ret = RFIDBackend.Connect();
                if (ret != DeviceStatus.OK) {
                    return ret;
                }

                ret = SendRFIDCommand(new ReqPacketReset(), out RespPacketReset _, out byte _);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }

                ret = SendRFIDCommand(new ReqPacketUnknown81(), out RespPacketUnknown81 _, out byte _);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }

                return ret;
            }, true, true));
        }

        public override DeviceStatus DisconnectRFID() {
            return SetLastError(RFIDBackend.Disconnect());
        }

        private DeviceStatus SendRFIDCommand<In, Out>(In request, out Out response, out byte status) where In : struct, SProtPayload where Out : struct, SProtPayload {
            CheckCallingThread();
            ArgumentNullException.ThrowIfNull(request);

            DeviceStatus ret;

            SProtFrame reqFrame = new SProtFrame(request);

            ret = SendRFIDCommand(reqFrame.Command, 0, reqFrame.Payload, out byte[] payload, out status);
            if (ret != DeviceStatus.OK) {
                response = default;
                return ret;
            }

            response = StructUtils.FromBytes<Out>(payload);
            return ret;
        }

        private DeviceStatus SendRFIDCommand(byte cmd, byte subCmd, byte[] payloadIn, out byte[] payloadOut, out byte status) {
            CheckCallingThread();
            ArgumentNullException.ThrowIfNull(payloadIn);

            DeviceStatus ret;

            SProtFrame reqFrame = new SProtFrame(cmd, payloadIn);
            byte[] payload = reqFrame.Payload;
            byte[] packet = new byte[payload.Length + 2];
            if (packet.Length > 0xFF) {
                payloadOut = null;
                status = 0;
                return DeviceStatus.ERR_PAYLOAD_TOO_LARGE;
            }
            packet[0] = cmd;
            packet[1] = subCmd;
            packet[2] = (byte)payload.Length;
            Array.Copy(payload, 0, packet, 3, payload.Length);

            ret = RFIDBackend.Write(packet);
            if (ret != DeviceStatus.OK) {
                payloadOut = null;
                status = 0;
                return ret;
            }

            ret = RFIDBackend.Read(out byte[] data);
            if (ret != DeviceStatus.OK) {
                payloadOut = default;
                status = 0;
                return ret;
            }

            byte _ = data[0]; // cmd
            status = data[1];
            byte len = data[2];
            payloadOut = new byte[len];
            Array.Copy(data, 3, payload, 0, payload.Length);

            return ret;
        }

        public override string GetDeviceModel() {
            return "CHC330";
        }

        public unsafe override DeviceStatus GetLoadedCardId(out byte[] cardid) {
            byte[] buf = new byte[CARD_ID_LEN];
            DeviceStatus ret = ExecuteOnPrintThread((ref ushort rc) => {
                DeviceStatus ret;
                fixed (byte* ptr = buf) {
                    ret = SetLastErrorByRC(Native.CHC_getCardRfidTID(ptr, ref rc), rc);
                }
                if (rc != RESULT_STATUS_READY && rc != RESULT_CARDRFID_ReadA) {
                    buf = null;
                }
                return ret;
            });
            cardid = buf;
            return SetLastError(ret);
        }

        public override string GetName() {
            return "SINFONIA Card Printer";
        }

        public override void VerifyRFIDData(byte[] payload) {
        }

        public override DeviceStatus WriteRFID(ref ushort rc, byte[] payload, out byte[] writtenCardId) {
            DeviceStatus ret = DeviceStatus.OK;
            writtenCardId = null;
            Log.Write("Initializing RFID Board");

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

                Log.Dump(cardid, "Write RFID ID:");
                Log.Dump(payload, "Write RFID Data:");

                ret = SetLastError(SendRFIDCommand(COMMAND_WRITE_START_STOP, SUBCOMMAND_WRITE_START_STOP, cardid, out byte[] _, out byte status), status);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
                for (int i = 0; i < payload.Length; i += 2) {
                    Log.Write("Write Block " + (i / 2));
                    ret = SetLastError(SendRFIDCommand(COMMAND_WRITE_BLOCK, SUBCOMMAND_WRITE_BLOCK, new byte[] { payload[i], payload[i + 1] }, out byte[] _, out byte status2), status2);
                    if (ret != DeviceStatus.OK) {
                        return ret;
                    }
                }
                return SetLastError(SendRFIDCommand(COMMAND_WRITE_START_STOP, SUBCOMMAND_WRITE_START_STOP, new byte[0], out byte[] _, out byte status3), status3);
            } else {
                Log.WriteWarning("No RFID data to write");
            }
            return ret;
        }

        protected override ushort GetStartPageParameter() {
            return StartPage_Standby_YMC;
        }

        public DeviceStatus GetRFIDAppVersion(out byte version) {
            version = 0;

            DeviceStatus ret = SendRFIDCommand(new ReqPacketGetAppVersion(), out RespPacketGetAppVersion packet, out byte status);
            if (ret != DeviceStatus.OK) {
                return SetLastError(ret, status);
            }

            version = packet.version;
            return SetLastError(DeviceStatus.OK, status);
        }

        public DeviceStatus GetRFIDBootVersion(out byte version) {
            version = 0;

            DeviceStatus ret = SendRFIDCommand(new ReqPacketGetBootVersion(), out RespPacketGetBootVersion packet, out byte status);
            if (ret != DeviceStatus.OK) {
                return SetLastError(ret, status);
            }

            version = packet.version;
            return SetLastError(DeviceStatus.OK, status);
        }

        public DeviceStatus GetRFIDBoardInfo(out string board) {
            board = null;

            DeviceStatus ret = SendRFIDCommand(new ReqPacketGetBoardInfo(), out RespPacketGetBoardInfo packet, out byte status);
            if (ret != DeviceStatus.OK) {
                return SetLastError(ret, status);
            }

            board = packet.version;
            return SetLastError(DeviceStatus.OK, status);
        }
    }
}
