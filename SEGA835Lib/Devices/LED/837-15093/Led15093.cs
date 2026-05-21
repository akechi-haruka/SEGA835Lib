using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093 {
    /// <summary>
    /// A 837-15093-06 LED board.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Led15093 : SProtDevice {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Led15093));

        /// <summary>
        /// The address being used by the client.
        /// </summary>
        public byte HostAddress { get; }

        /// <summary>
        /// The address being used by the board.
        /// </summary>
        public byte BoardAddress { get; }

        /// <summary>
        /// Whether or not board responses are currently disabled. If this is true, <see cref="Read(out SProtFrame)" /> should not be called until <see cref="SetResponseDisabled(bool)"/> is called with false.
        /// </summary>
        public bool ResponsesDisabled { get; protected set; }

        /// <summary>
        /// Creates a new LED board.
        /// </summary>
        /// <param name="port">The COM board to use.</param>
        /// <param name="hostAddr">The address for the client. This may not actually matter.</param>
        /// <param name="boardAddr">The address for the LED board.</param>
        public Led15093(int port, byte hostAddr = 0x02, byte boardAddr = 0x01) : base(new SProtSerial(port, dtr: true, rts: true)) {
            HostAddress = hostAddr;
            BoardAddress = boardAddr;
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            lock (SerialLocker) {
                if (Serial.IsConnected()) {
                    return DeviceStatus.Ok;
                }

                LOG.LogInformation("Connecting on Port " + Port);
                if (!Serial.Connect()) {
                    return DeviceStatus.ErrorNotConnected;
                }
            }

            DeviceStatus ret = SetResponseDisabled(false);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            return Reset();
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            LOG.LogInformation("Disconnected on Port " + Port);
            lock (SerialLocker) {
                Serial?.Disconnect();
            }

            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "837-15093-06";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "IC BD I/O 7CH CONT RS232 12V";
        }

        /// <inheritdoc/>
        protected DeviceStatus Write(byte dest, byte src, byte cmd, byte[] payload) {
            lock (SerialLocker) {
                byte[] packet = new byte[payload.Length + 4];
                if (packet.Length > 0xFF) {
                    return DeviceStatus.ErrorPayloadTooLarge;
                }

                packet[0] = dest;
                packet[1] = src;
                packet[2] = (byte)(payload.Length + 1);
                packet[3] = cmd;
                Array.Copy(payload, 0, packet, 4, payload.Length);
                return Serial.Write(packet);
            }
        }

        private DeviceStatus Read(out byte src, out byte dest, out byte cmd, out byte status, out byte report, out byte[] payload) {
            if (ResponsesDisabled) {
                LOG.LogInformation("Responses are disabled");
                src = 0;
                dest = 0;
                cmd = 0;
                status = 0;
                report = 0;
                payload = new byte[0];
                return DeviceStatus.Ok;
            }

            byte[] data;
            DeviceStatus ret;
            lock (SerialLocker) {
                ret = Serial.ReadLenByOffset(3, out data);
            }

            if (ret != DeviceStatus.Ok) {
                src = 0;
                dest = 0;
                cmd = 0;
                status = 0;
                report = 0;
                payload = null;
                return ret;
            }

            dest = data[1];
            src = data[2];
            cmd = data[5];
            status = (byte)(data[4] - 1); // 1 here means success
            report = (byte)(data[6] - 1);
            if (report != 0) {
                LOG.LogWarning("Report received from LED board: " + report);
            }

            payload = new byte[data[3] - 3];
            Array.Copy(data, 7, payload, 0, payload.Length);
            if (status != 0) {
                ret = DeviceStatus.ErrorDevice;
                SetLastError(ret, status);
            }

            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus Write(SProtFrame send) {
            return Write(BoardAddress, HostAddress, send.Command, send.Payload);
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(out SProtFrame recv) {
            DeviceStatus ret = Read(out byte addr, out byte _, out byte cmd, out byte status, out byte _, out byte[] payload);
            if (ret != DeviceStatus.Ok) {
                recv = null;
                return ret;
            }

            recv = new SProtFrame(0, cmd, addr, status, payload);
            return ret;
        }

        /// <summary>
        /// Resets the device state. This is implicitely called on <see cref="Connect"/>.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or if the reader was already reset (which will log a warning), or any other DeviceStatus on failure.</returns>
        public DeviceStatus Reset() {
            LOG.LogInformation("Reset");
            DeviceStatus ret = WriteAndRead(new ReqPacketReset() {
                reset_type = 0xD9
            }, out RespPacketReset _, out byte status);

            ResponsesDisabled = false;

            if (ret == DeviceStatus.ErrorDevice) { // error on double reset, ignore
                return SetLastError(DeviceStatus.Ok, status);
            }

            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's hardware versions.
        /// </summary>
        /// <param name="boardNumber">The LED Board Number (ex. "15093-06") or null on failure</param>
        /// <param name="chipNumber">The LED Chip Number (ex. "6710") or null on failure</param>
        /// <param name="firmwareVersion">The LED Board Firmware Version (ex. 0xA0) or 0 on failure</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetBoardInfo(out String boardNumber, out String chipNumber, out byte firmwareVersion) {
            LOG.LogInformation("GetBoardInfo");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetBoardInfo(), out RespPacketGetBoardInfo resp, out byte status);
            if (ret == DeviceStatus.Ok) {
                boardNumber = resp.board_number;
                chipNumber = resp.chip_number;
                firmwareVersion = resp.fw_ver;
            } else {
                boardNumber = null;
                chipNumber = null;
                firmwareVersion = 0;
            }

            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's firmware checksum.
        /// </summary>
        /// <param name="checksum">The LED Firmware checksum or 0 on failure.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFirmwareChecksum(out ushort checksum) {
            LOG.LogInformation("GetFirmwareChecksum");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetFirmwareChecksum(), out RespPacketGetFirmwareChecksum resp, out byte status);
            if (ret == DeviceStatus.Ok) {
                checksum = (ushort)(resp.fw_checksum_b2 << 8 | resp.fw_checksum_b1);
            } else {
                checksum = 0;
            }

            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's protocol version.
        /// </summary>
        /// <param name="appliMode">Unknown or 0 on failure.</param>
        /// <param name="major">The major protocol version or 0 on failure.</param>
        /// <param name="minor">The minor protocol version or 0 on failure.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetProtocolVersion(out byte appliMode, out byte major, out byte minor) {
            LOG.LogInformation("GetProtocolVersion");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetProtocolVersion(), out RespPacketGetProtocolVersion resp, out byte status);
            if (ret == DeviceStatus.Ok) {
                appliMode = resp.appli_mode;
                major = resp.major;
                minor = resp.minor;
            } else {
                appliMode = 0;
                major = 0;
                minor = 0;
            }

            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the LED board timeout (when no new LED data is received after X ms, LEDs will turn off.
        /// </summary>
        /// <param name="timeout">The timeout to set.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetTimeout(ushort timeout) {
            LOG.LogInformation("SetTimeout(" + timeout + ")");
            DeviceStatus ret = WriteAndRead(new ReqPacketSetTimeout() {
                timeout = timeout
            }, out RespPacketSetTimeout _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets if the LED board should not respond with ACK packets for the <see cref="SetLeds"/> command.
        /// </summary>
        /// <param name="enabled">If "enabled" is set, responses will be DISABLED.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetResponseDisabled(bool enabled) {
            LOG.LogInformation("SetResponseDisabled(" + enabled + ")");
            ResponsesDisabled = enabled;
            DeviceStatus ret = WriteAndRead(new ReqPacketSetDisableResponse() {
                enable = (byte)(enabled ? 1 : 0)
            }, out RespPacketSetDisableResponse _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets LED colors.
        /// </summary>
        /// <param name="colors">A list of colors to set.</param>
        /// <exception cref="ArgumentException">If more than 66 colors are given.</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus SetLeds(IEnumerable<Color> colors) {
            IEnumerable<Color> colorArray = colors.ToArray();
            int cnt = colorArray.Count();
            LOG.LogInformation("SetLEDs(" + cnt + ")");

            if (colorArray.Count() > 66) {
                throw new ArgumentException("too many colors " + cnt);
            }

            ReqPacketSetLeds req = new ReqPacketSetLeds();
            byte* ledptr = req.pixels;
            int i = 0;
            foreach (Color c in colorArray) {
                ledptr[i] = c.R;
                ledptr[i + 1] = c.G;
                ledptr[i + 2] = c.B;
                i += 3;
            }

            DeviceStatus ret = SetLastError(Write(BoardAddress, HostAddress, req.GetCommandID(), StructUtils.GetBytes(req)));
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            if (!ResponsesDisabled) {
                ret = SetLastError(Read(out SProtFrame f), f.Status);
            }

            return ret;
        }

        /// <summary>
        /// Sets the number of connected LEDs.
        /// </summary>
        /// <param name="count">The number of LEDs.</param>
        /// <exception cref="ArgumentOutOfRangeException">if count is outside [0,66].</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetLedCount(int count) {
            LOG.LogInformation("SetLEDCount(" + count + ")");

            if (count > 66) {
                throw new ArgumentOutOfRangeException("count is too high: " + count);
            }

            DeviceStatus ret = WriteAndRead(new ReqPacketSetLedCount() {
                count = (byte)count
            }, out RespPacketSetLedCount _, out byte status);
            return SetLastError(ret, status);
        }
    }
}