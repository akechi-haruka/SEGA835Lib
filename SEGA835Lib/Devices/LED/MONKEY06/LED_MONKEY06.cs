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
        /// <param name="host_addr">The address for the client. This may not actually matter.</param>
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
                checksum = checksum
            }, out RespPacketMonkeySetChecksum _, out byte status);
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
    }
}
