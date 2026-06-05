#if NET8_0_OR_GREATER
using System;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310 {
    /// <summary>
    /// A CHC-310 Card Printer for Kantai Collection Arcade.
    /// </summary>
    public class Chc310Printer : ChcSeriesCardPrinter {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc310Printer));
        private const byte COMMAND_WRITE_START_STOP = 0x10;
        private const byte SUBCOMMAND_WRITE_START_STOP = 0x02;
        private const byte COMMAND_WRITE_BLOCK = 0x11;
        private const byte SUBCOMMAND_WRITE_BLOCK = 0x03;

        private static readonly Native NATIVE = new Native(); // hack to pass the same Native to both parameters

        /// <summary>
        /// Creates a new CHC-310 printer.
        /// </summary>
        public Chc310Printer() : base(NATIVE, new RfidBackendChcDll(NATIVE), new Size(768, 1052)) {
        }

        internal Chc310Printer(INativeTrampolineChc dllFunctions, RfidBackend rfidBackend, Size imageSize) : base(dllFunctions, rfidBackend, imageSize) {
        }

        /// <inheritdoc/>
        public override DeviceStatus ConnectRfid() {
            return SetLastError(ExecuteOnPrintThread((ref ushort _) => {
                DeviceStatus ret = RfidBackend.Connect();
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = SendRfidCommand(new ReqPacketReset(), out RespPacketReset _, out byte _);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                ret = SendRfidCommand(new ReqPacketUnknown81(), out RespPacketUnknown81 _, out byte _);
                return ret;
            }, true, true));
        }

        /// <inheritdoc/>
        public override DeviceStatus DisconnectRfid() {
            return SetLastError(RfidBackend.Disconnect());
        }

        private DeviceStatus SendRfidCommand<TIn, TOut>(TIn request, out TOut response, out byte status) where TIn : struct, ISProtPayload where TOut : struct, ISProtPayload {
            CheckCallingThread();

            SProtFrame reqFrame = new SProtFrame(request);

            DeviceStatus ret = SendRfidCommand(reqFrame.Command, 0, reqFrame.Payload, out byte[] payload, out status);
            if (ret != DeviceStatus.Ok) {
                response = default;
                return ret;
            }

            response = StructUtils.FromBytes<TOut>(payload);
            return ret;
        }

        private DeviceStatus SendRfidCommand(byte cmd, byte subCmd, byte[] payloadIn, out byte[] payloadOut, out byte status) {
            CheckCallingThread();
            ArgumentNullException.ThrowIfNull(payloadIn);

            SProtFrame reqFrame = new SProtFrame(cmd, payloadIn);
            byte[] payload = reqFrame.Payload;
            byte[] packet = new byte[payload.Length + 3];
            if (packet.Length > 0xFF) {
                payloadOut = null;
                status = 0;
                return DeviceStatus.ErrorPayloadTooLarge;
            }

            packet[0] = cmd;
            packet[1] = subCmd;
            packet[2] = (byte)payload.Length;
            Array.Copy(payload, 0, packet, 3, payload.Length);

            DeviceStatus ret = RfidBackend.Write(packet);
            if (ret != DeviceStatus.Ok) {
                payloadOut = null;
                status = 0;
                return ret;
            }

            ret = RfidBackend.Read(out byte[] data);
            if (ret != DeviceStatus.Ok) {
                payloadOut = null;
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

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "CHC330";
        }

        /// <inheritdoc/>
        public override unsafe DeviceStatus GetLoadedCardId(out byte[] cardid) {
            byte[] buf = new byte[CARD_ID_LEN];
            DeviceStatus ret = ExecuteOnPrintThread((ref ushort rc) => {
                DeviceStatus ret;
                fixed (byte* ptr = buf) {
                    ret = SetLastErrorByReturnCode(Native.CHC_getCardRfidTID(ptr, ref rc), rc);
                }

                if (rc != RESULT_STATUS_READY && rc != RESULT_CARDRFID_READ_A) {
                    buf = null;
                }

                return ret;
            });
            cardid = buf;
            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "SINFONIA Card Printer";
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <param name="payload">Ignored.</param>
        /// <param name="overrideCardId">Ignored.</param>
        public override void VerifyRfidData(byte[] payload, bool overrideCardId) {
        }

        /// <inheritdoc/>
        public override DeviceStatus WriteRfid(ref ushort rc, byte[] payload, bool overrideCardId, out byte[] writtenCardId) {
            DeviceStatus ret = DeviceStatus.Ok;

            if (payload != null) {
                writtenCardId = null;
                LOG.LogInformation("Initializing RFID Board");

                LOG.LogInformation("Reading Card ID from RFID board");
                byte[] cardId = new byte[CARD_ID_LEN];
                ret = PrintWaitFor(ref rc, (ref ushort rc) => {
                    unsafe {
                        fixed (byte* ptr = cardId) {
                            ret = SetLastErrorByReturnCode(Native.CHC_getCardRfidTID(ptr, ref rc), rc);
                        }
                    }

                    if (rc == RESULT_CARDRFID_READ_A) {
                        rc = RESULT_NOERROR;
                    }

                    return rc;
                }, 20000);
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("RFID Read failed");
                    return PrintExitThreadError(ret, RESULT_CARDRFID_COMMAND_ERROR);
                }

                Job.JobStatus = PrintStatus.CardDataWriteRfid;

                byte[] cardid = new byte[CARD_ID_LEN];
                if (overrideCardId) {
                    Array.Copy(payload, cardid, cardid.Length);
                    byte[] payloadWithoutId = new byte[payload.Length - cardid.Length];
                    Array.Copy(payload, cardid.Length, payloadWithoutId, 0, payloadWithoutId.Length);
                    payload = payloadWithoutId;
                    writtenCardId = cardid;
                } else {
                    Array.Copy(cardId, cardid, cardid.Length);
                    writtenCardId = cardid;
                }

                LOG.LogDebug("Write RFID ID:\n" + Hex.Dump(cardid));
                LOG.LogDebug("Write RFID Data:\n" + Hex.Dump(payload));

                ret = SetLastError(SendRfidCommand(COMMAND_WRITE_START_STOP, SUBCOMMAND_WRITE_START_STOP, cardid, out byte[] _, out byte status), status);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                for (int i = 0; i < payload.Length; i += 2) {
                    LOG.LogInformation("Write Block " + (i / 2));
                    ret = SetLastError(SendRfidCommand(COMMAND_WRITE_BLOCK, SUBCOMMAND_WRITE_BLOCK, new byte[] { payload[i], payload[i + 1] }, out byte[] _, out byte status2), status2);
                    if (ret != DeviceStatus.Ok) {
                        return ret;
                    }
                }

                return SetLastError(SendRfidCommand(COMMAND_WRITE_START_STOP, SUBCOMMAND_WRITE_START_STOP, Array.Empty<byte>(), out byte[] _, out byte status3), status3);
            }

            LOG.LogWarning("No RFID data to write");
            writtenCardId = null;

            return ret;
        }

        /// <inheritdoc/>
        protected override ushort GetStartPageParameter() {
            return START_PAGE_STANDBY_YMC;
        }

        /// <summary>
        /// Returns the RFID board's "app" version.
        /// </summary>
        /// <param name="version">The board version</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other status on failure.</returns>
        public virtual DeviceStatus GetRfidAppVersion(out byte version) {
            version = 0;

            DeviceStatus ret = SendRfidCommand(new ReqPacketGetAppVersion(), out RespPacketGetAppVersion packet, out byte status);
            if (ret != DeviceStatus.Ok) {
                return SetLastError(ret, status);
            }

            version = packet.version;
            return SetLastError(DeviceStatus.Ok, status);
        }

        /// <summary>
        /// Returns the RFID board's "boot" version.
        /// </summary>
        /// <param name="version">The board version</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other status on failure.</returns>
        public virtual DeviceStatus GetRfidBootVersion(out byte version) {
            version = 0;

            DeviceStatus ret = SendRfidCommand(new ReqPacketGetBootVersion(), out RespPacketGetBootVersion packet, out byte status);
            if (ret != DeviceStatus.Ok) {
                return SetLastError(ret, status);
            }

            version = packet.version;
            return SetLastError(DeviceStatus.Ok, status);
        }

        /// <summary>
        /// Returns the RFID board's board information.
        /// </summary>
        /// <param name="board">The board information</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other status on failure.</returns>
        public virtual DeviceStatus GetRfidBoardInfo(out string board) {
            board = null;

            DeviceStatus ret = SendRfidCommand(new ReqPacketGetBoardInfo(), out RespPacketGetBoardInfo packet, out byte status);
            if (ret != DeviceStatus.Ok) {
                return SetLastError(ret, status);
            }

            board = packet.version;
            return SetLastError(DeviceStatus.Ok, status);
        }

        /// <inheritdoc />
        protected override byte? GetPolishParameter(bool isHolo) {
            return (byte)(isHolo ? 5 : 2);
        }

        /// <inheritdoc/>
        protected override DeviceStatus PostProcessing(ref ushort rc) {
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        protected override ushort? GetInitialCardPosition() {
            return STANDBY_RFID;
        }

        /// <inheritdoc/>
        protected override byte? GetParameter19(bool isHolo) {
            return null;
        }

        /// <inheritdoc/>
        protected override DeviceStatus PreProcessing(ref ushort rc) {
            LOG.LogInformation("Get loaded card ID");
            // 310 only
            byte[] cardIdBuf = new byte[CARD_ID_LEN];
            DeviceStatus ret = PrintWaitFor(ref rc, (ref ushort rc) => {
                unsafe {
                    fixed (byte* ptr = cardIdBuf) {
                        return Native.CHC_getCardRfidTID(ptr, ref rc);
                    }
                }
            }, 20000, RESULT_STATUS_BUSY, RESULT_STATUS_OPERATION);
            if (ret != DeviceStatus.Ok) {
                return PrintExitThreadError(ret, rc);
            }

            if (rc != RESULT_CARDRFID_READ_A) {
                LOG.LogError("Unexpected result: " + rc);
                return PrintExitThreadError(SetLastErrorByReturnCode(CHCUSB_RC_OK, rc), rc);
            }

            return SetLastErrorByReturnCode((int)ret, rc);
        }
    }
}

#endif