using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093 {

    /// <summary>
    /// A 837-15093-06 LED board.
    /// </summary>
    public class LED_837_15093_06 : Device, ISProtRW {

        /// <summary>
        /// The COM port being used.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// The address being used by the client.
        /// </summary>
        public byte HostAddress { get; private set; }
        /// <summary>
        /// The address being used by the board.
        /// </summary>
        public byte BoardAddress { get; private set; }

        internal SProtSerial serial;
        private bool responseDisabled;

        /// <summary>
        /// Creates a new LED board.
        /// </summary>
        /// <param name="port">The COM board to use.</param>
        /// <param name="host_addr">The address for the client. This may not actually matter.</param>
        /// <param name="board_addr">The address for the LED board.</param>
        public LED_837_15093_06(int port, byte host_addr = 0x02, byte board_addr = 0x01) {
            Port = port;
            HostAddress = host_addr;
            BoardAddress = board_addr;
            serial = new SProtSerial(port, dtr: true, rts: true);
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            if (serial != null && serial.IsConnected()) {
                return DeviceStatus.OK;
            }
            Log.Write("Connecting on Port " + Port);
            if (!serial.Connect()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }

            DeviceStatus ret = SetResponseDisabled(false);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            return Reset();
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "837-15093-06";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "IC BD I/O 7CH CONT RS232 12V";
        }

        private DeviceStatus Write(byte dest, byte src, byte cmd, byte[] payload) {
            byte[] packet = new byte[payload.Length + 4];
            if (packet.Length > 0xFF) {
                return DeviceStatus.ERR_PAYLOAD_TOO_LARGE;
            }
            packet[0] = dest;
            packet[1] = src;
            packet[2] = (byte)(payload.Length + 1);
            packet[3] = cmd;
            Array.Copy(payload, 0, packet, 4, payload.Length);
            return serial.Write(packet);
        }

        private DeviceStatus Read(out byte src, out byte dest, out byte cmd, out byte status, out byte report, out byte[] payload) {
            if (responseDisabled) {
                Log.Write("Responses are disabled");
                src = 0;
                dest = 0;
                cmd = 0;
                status = 0;
                report = 0;
                payload = new byte[0];
                return DeviceStatus.OK;
            }
            DeviceStatus ret = serial.ReadLenByOffset(3, out byte[] data, false, false);
            if (ret != DeviceStatus.OK) {
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
            payload = new byte[data[3] - 3];
            Array.Copy(data, 7, payload, 0, payload.Length);
            if (status != 0) {
                ret = DeviceStatus.ERR_DEVICE;
                SetLastError(ret, status);
            }
            return ret;
        }

        /// <inheritdoc/>
        public DeviceStatus Write(SProtFrame send) {
            return Write(BoardAddress, HostAddress, send.Command, send.Payload);
        }

        /// <inheritdoc/>
        public DeviceStatus Read(out SProtFrame recv) {
            DeviceStatus ret = Read(out byte addr, out byte _, out byte cmd, out byte status, out byte _, out byte[] payload);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            recv = new SProtFrame(0, cmd, addr, status, payload);
            return ret;
        }

        /// <summary>
        /// Resets the device state. This is implicitely called on <see cref="Connect"/>.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or if the reader was already reset (which will log a warning), or any other DeviceStatus on failure.</returns>
        public DeviceStatus Reset() {
            Log.Write("Reset");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketReset() {
                reset_type = 0xD9
            }, out RespPacketReset _, out byte status);

            responseDisabled = false;

            if (ret == DeviceStatus.ERR_DEVICE) { // error on double reset, ignore
                return SetLastError(DeviceStatus.OK, status);
            }
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's hardware versions.
        /// </summary>
        /// <param name="board_number">The LED Board Number (ex. "15093-06") or null on failure</param>
        /// <param name="chip_number">The LED Chip Number (ex. "6710") or null on failure</param>
        /// <param name="firmware_version">The LED Board Firmware Version (ex. 0xA0) or 0 on failure</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetBoardInfo(out String board_number, out String chip_number, out byte firmware_version) {
            Log.Write("GetBoardInfo");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetBoardInfo(), out RespPacketGetBoardInfo resp, out byte status);
            if (ret == DeviceStatus.OK) {
                board_number = resp.board_number;
                chip_number = resp.chip_number;
                firmware_version = resp.fw_ver;
            } else {
                board_number = null;
                chip_number = null;
                firmware_version = 0;
            }
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's firmware checksum.
        /// </summary>
        /// <param name="checksum">The LED Firmware checksum or 0 on failure.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFirmwareChecksum(out ushort checksum) {
            Log.Write("GetFirmwareChecksum");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetFirmwareChecksum(), out RespPacketGetFirmwareChecksum resp, out byte status);
            if (ret == DeviceStatus.OK) {
                checksum = resp.fw_checksum;
            } else {
                checksum = 0;
            }
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's protocol version.
        /// </summary>
        /// <param name="appli_mode">Unknown or 0 on failure.</param>
        /// <param name="major">The major protocol version or 0 on failure.</param>
        /// <param name="minor">The minor protocol version or 0 on failure.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetProtocolVersion(out byte appli_mode, out byte major, out byte minor) {
            Log.Write("GetProtocolVersion");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetProtocolVersion(), out RespPacketGetProtocolVersion resp, out byte status);
            if (ret == DeviceStatus.OK) {
                appli_mode = resp.appli_mode;
                major = resp.major;
                minor = resp.minor;
            } else {
                appli_mode = 0;
                major = 0;
                minor = 0;
            }
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the LED board timeout (when no new LED data is received after X ms, LEDs will turn off.
        /// </summary>
        /// <param name="timeout">The timeout to set.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetTimeout(ushort timeout) {
            Log.Write("SetTimeout("+timeout+")");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketSetTimeout() {
                timeout = timeout
            }, out RespPacketSetTimeout _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets if the LED board should not respond with ACK packets for the <see cref="SetLEDs"/> command.
        /// </summary>
        /// <param name="enabled">If "enabled" is set, responses will be DISABLED.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetResponseDisabled(bool enabled) {
            Log.Write("SetResponseDisabled(" + enabled + ")");
            responseDisabled = enabled;
            DeviceStatus ret = this.WriteAndRead(new ReqPacketSetDisableResponse() {
                enable = (byte)(enabled ? 1 : 0)
            }, out RespPacketSetDisableResponse _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets LED colors.
        /// </summary>
        /// <param name="colors">A list of colors to set.</param>
        /// <exception cref="ArgumentException">If more than 66 colors are given.</exception>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus SetLEDs(IEnumerable<Color> colors) {
            int cnt = colors.Count();
            Log.Write("SetLEDs(" + cnt + ")");

            if (colors.Count() > 66) {
                throw new ArgumentException("too many colors " + cnt);
            }

            ReqPacketSetLEDs req = new ReqPacketSetLEDs();
            byte* ledptr = req.pixels;
            int i = 0;
            foreach (Color c in colors){
                ledptr[i] = c.R;
                ledptr[i + 1] = c.G;
                ledptr[i + 2] = c.B;
                i += 3;
            }
            DeviceStatus ret = SetLastError(Write(BoardAddress, HostAddress, req.GetCommandID(), StructUtils.GetBytes(req)));
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            if (!responseDisabled) {
                ret = SetLastError(Read(out SProtFrame f), f.Status);
            }

            return ret;
        }

        /// <summary>
        /// Sets the number of connected LEDs.
        /// </summary>
        /// <param name="count">The number of LEDs.</param>
        /// <exception cref="ArgumentOutOfRangeException">if count is outside [0,66].</exception>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetLEDCount(int count) {
            Log.Write("SetLEDCount(" + count + ")");

            if (count > 66) {
                throw new ArgumentOutOfRangeException("count is too high: " + count);
            }

            DeviceStatus ret = this.WriteAndRead(new ReqPacketSetLEDCount() {
                count = (byte)count
            }, out RespPacketSetLEDCount _, out byte status);
            return SetLastError(ret, status);
        }
    }
}
