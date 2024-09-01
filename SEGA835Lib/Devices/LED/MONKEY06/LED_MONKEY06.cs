using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06 {

    /// <summary>
    /// A bootleg 837-15093-06 board based on an Arduino or similar. See https://github.com/akechi-haruka/SuperMonkeyLEDs.
    /// </summary>
    /// <seealso cref="LED_837_15093_06"/>
    public class LED_MONKEY06 : LED_837_15093_06 {

        /// <summary>
        /// Creates a new LED board.
        /// </summary>
        /// <param name="port">The COM board to use.</param>
        /// <param name="host_addr">The address for the client. This might not actually matter.</param>
        /// <param name="board_addr">The address for the LED board.</param>
        public LED_MONKEY06(int port, byte host_addr = 0x01, byte board_addr = 0x02) : base(port, host_addr, board_addr) {
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "MONKEY06";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "MONKEY06 837-15093-06 EMULATOR";
        }

        /// <summary>
        /// Resets the monkey device state. This is not automatically called, neither will <see cref="LED_837_15093_06.Reset"/> reset monkey-specific switches and settings.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus ResetMonkey() {
            Log.Write("ResetMonkey");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketMonkeyReset(), out RespPacketMonkeyReset _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the board checksum on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="LED_837_15093_06.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetFirmwareChecksum(ushort checksum) {
            Log.Write("SetFirmwareChecksum(" + checksum + ")");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketMonkeySetChecksum() {
                fw_checksum_b1 = (byte)checksum,
                fw_checksum_b2 = (byte)(checksum >> 8)
            }, out RespPacketMonkeySetChecksum _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the board version on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="LED_837_15093_06.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetFirmwareVersion(byte version) {
            Log.Write("SetFirmwareVersion(" + version + ")");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketMonkeySetFirmwareVersion() {
                ver = version
            }, out RespPacketMonkeySetFirmwareVersion _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the chip number on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="LED_837_15093_06.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <param name="chip_no">The chip number to set. Maximum 5 characters. Missing characters are padded with the space character (0x20)</param>
        /// <exception cref="ArgumentException">If chip_no is too long.</exception>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetChipNumber(string chip_no) {
            Log.Write("SetChipNumber(" + chip_no + ")");
            NetStandardBackCompatExtensions.ThrowIfNull(chip_no, nameof(chip_no));
            if (chip_no.Length > 5) {
                throw new ArgumentException("chip_no is too long", nameof(chip_no));
            }
            if (chip_no.Length < 5) {
                chip_no = chip_no.PadRight(5);
            }
            DeviceStatus ret = this.WriteAndRead(new ReqPacketMonkeySetChipNumber() {
                chip_no = chip_no
            }, out RespPacketMonkeySetChipNumber _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the board name on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="LED_837_15093_06.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <param name="board_name">The board name to set. Maximum 8 characters. Missing characters are padded with the space character (0x20)</param>
        /// <exception cref="ArgumentException">If board_name is too long.</exception>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetBoardName(string board_name) {
            Log.Write("SetBoardName(" + board_name + ")");
            NetStandardBackCompatExtensions.ThrowIfNull(board_name, nameof(board_name));
            if (board_name.Length > 8) {
                throw new ArgumentException("board_name is too long", nameof(board_name));
            }
            if (board_name.Length < 8) {
                board_name = board_name.PadRight(8);
            }
            DeviceStatus ret = this.WriteAndRead(new ReqPacketMonkeySetBoardName() {
                board_name = board_name
            }, out RespPacketMonkeySetBoardName _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the order of channels for the data sent to <see cref="LED_837_15093_06.SetLEDs(IEnumerable{Color})"/>. This will be remembered until <see cref="ResetMonkey"/>. <see cref="LED_837_15093_06.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetChannels(Channel R, Channel G, Channel B) {
            Log.Write("SetChannels(" + R + ", " + G + ", " + B + ")");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketMonkeySetChannels() {
                r = (byte)R,
                g = (byte)G,
                b = (byte)B
            }, out RespPacketMonkeySetChannels _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the LED translation table.
        /// </summary>
        /// <remarks>
        /// The translation table works as follow:<br />
        /// By default, game input will be mapped 1:1 to LED output. ([0, 1, 2, 3, 4, ...])<br />
        /// If you set this, LEDs will be remapped in the way input->output, so for example if you set the translation table to [5, 5, 5, 5, 5, 2, 2, 2, 2, 2], the first 5 LEDs would be set to game LED index 5 and the next 5 LEDs to game LED index 2. Any LEDs past this would be turned off.<br />
        /// * values higher or equal to 66 are not allowed and will be ignored.
        /// * 0xFE can be used as a special value to keep the LED on instead of off.
        /// * missing values will be set to 0xFF (disabled).
        /// </remarks>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus SetLEDTranslationTable(IEnumerable<byte> mapping) {
            Log.Write("SetLEDTranslationTable(" + mapping.Count() + ")");
            ReqPacketMonkeySetTranslation req = new ReqPacketMonkeySetTranslation();
            StructUtils.Copy(mapping.ToArray(), req.translation, mapping.Count());
            DeviceStatus ret = this.WriteAndRead(req, out RespPacketMonkeySetTranslation _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Color channels for <see cref="SetChannels(Channel, Channel, Channel)"/>
        /// </summary>
        public enum Channel {
            /// <summary>
            /// Red.
            /// </summary>
            Red = 0,
            /// <summary>
            /// Green.
            /// </summary>
            Green = 1,
            /// <summary>
            /// Blue.
            /// </summary>
            Blue = 2
        }
    }
}
