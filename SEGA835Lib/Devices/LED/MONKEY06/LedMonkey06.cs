using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06 {
    /// <summary>
    /// A bootleg 837-15093-06 board based on an Arduino or similar. See https://github.com/akechi-haruka/SuperMonkeyLEDs.
    /// </summary>
    /// <seealso cref="Led15093"/>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class LedMonkey06 : Led15093 {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(LedMonkey06));

        /// <summary>
        /// Creates a new LED board.
        /// </summary>
        /// <param name="port">The COM board to use.</param>
        /// <param name="hostAddr">The address for the client. This might not actually matter.</param>
        /// <param name="boardAddr">The address for the LED board.</param>
        public LedMonkey06(int port, byte hostAddr = 0x01, byte boardAddr = 0x02) : base(port, hostAddr, boardAddr) {
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
        /// Resets the monkey device state. This is not automatically called, neither will <see cref="Led15093.Reset"/> reset monkey-specific switches and settings.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus ResetMonkey() {
            LOG.LogInformation("ResetMonkey");
            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeyReset(), out RespPacketMonkeyReset _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the board checksum on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="Led15093.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetFirmwareChecksum(ushort checksum) {
            LOG.LogInformation("SetFirmwareChecksum(" + checksum + ")");
            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeySetChecksum() {
                fw_checksum_b1 = (byte)checksum,
                fw_checksum_b2 = (byte)(checksum >> 8)
            }, out RespPacketMonkeySetChecksum _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the board version on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="Led15093.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetFirmwareVersion(byte version) {
            LOG.LogInformation("SetFirmwareVersion(" + version + ")");
            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeySetFirmwareVersion() {
                ver = version
            }, out RespPacketMonkeySetFirmwareVersion _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the "appli mode" on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="Led15093.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetAppliMode(byte appliMode) {
            LOG.LogInformation("SetAppliMode(" + appliMode + ")");
            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeySetAppliMode() {
                appliMode = appliMode
            }, out RespPacketMonkeySetAppliMode _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the chip number on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="Led15093.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <param name="chipNo">The chip number to set. Maximum 5 characters. Missing characters are padded with the space character (0x20)</param>
        /// <exception cref="ArgumentException">If chip_no is too long.</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetChipNumber(string chipNo) {
            LOG.LogInformation("SetChipNumber(" + chipNo + ")");
            NetStandardBackCompatExtensions.ThrowIfNull(chipNo, nameof(chipNo));
            if (chipNo.Length > 5) {
                throw new ArgumentException("chip_no is too long", nameof(chipNo));
            }

            if (chipNo.Length < 5) {
                chipNo = chipNo.PadRight(5);
            }

            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeySetChipNumber() {
                chip_no = chipNo
            }, out RespPacketMonkeySetChipNumber _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the board name on the monkey device. This will be remembered until <see cref="ResetMonkey"/>. <see cref="Led15093.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <param name="boardName">The board name to set. Maximum 8 characters. Missing characters are padded with the space character (0x20)</param>
        /// <exception cref="ArgumentException">If board_name is too long.</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetBoardName(string boardName) {
            LOG.LogInformation("SetBoardName(" + boardName + ")");
            NetStandardBackCompatExtensions.ThrowIfNull(boardName, nameof(boardName));
            if (boardName.Length > 8) {
                throw new ArgumentException("board_name is too long", nameof(boardName));
            }

            if (boardName.Length < 8) {
                boardName = boardName.PadRight(8);
            }

            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeySetBoardName() {
                board_name = boardName
            }, out RespPacketMonkeySetBoardName _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets the order of channels for the data sent to <see cref="Led15093.SetLeds"/>. This will be remembered until <see cref="ResetMonkey"/>. <see cref="Led15093.Reset"/> does NOT reset the parameters for the monkey device.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public DeviceStatus SetChannels(Channel r, Channel g, Channel b) {
            LOG.LogInformation("SetChannels(" + r + ", " + g + ", " + b + ")");
            DeviceStatus ret = WriteAndRead(new ReqPacketMonkeySetChannels() {
                r = (byte)r,
                g = (byte)g,
                b = (byte)b
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
        /// * The maximum possible numbers of values is 255.
        /// * 0xFE can be used as a special value to keep the LED on instead of off.
        /// * missing values will be set to 0xFF (disabled).
        /// </remarks>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus SetLedTranslationTable(IEnumerable<byte> mapping) {
            byte[] data = mapping.ToArray();
            if (data.Length > byte.MaxValue) {
                throw new ArgumentException("given translation table contains " + data.Length + " entries, however only " + byte.MaxValue + " are possible");
            }

            DeviceStatus ret = DeviceStatus.Ok;
            byte status = 0;

            const int blockSize = 66;
            for (byte i = 0; i < data.Length; i += blockSize) {
                LOG.LogInformation("SetLEDTranslationTable(" + i + "/" + data.Length + ")");
                ReqPacketMonkeySetTranslation req = new ReqPacketMonkeySetTranslation {
                    offset = i
                };
                StructUtils.Copy(data, i, req.translation, 0, blockSize);
                ret = WriteAndRead(req, out RespPacketMonkeySetTranslation _, out status);
                if (ret != DeviceStatus.Ok) {
                    return SetLastError(ret, status);
                }
            }

            return SetLastError(ret, status);
        }

        /// <summary>
        /// Sets auxiliary LED colors. This is a second output pin or LED strip that can be fed in addition to the SEGA-supported protocol. If the output is a singular PIN, only colors[0] will be set. If the output is a LED strip, it will behave like <see cref="Led15093.SetLeds"/>.
        /// </summary>
        /// <param name="colors">A list of colors to set.</param>
        /// <exception cref="ArgumentException">If more than 66 colors are given.</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus SetAuxiliaryLeds(IEnumerable<Color> colors) {
            IEnumerable<Color> colorArray = colors.ToArray();
            int cnt = colorArray.Count();
            LOG.LogInformation("SetAuxiliaryLEDs(" + cnt + ")");

            if (colorArray.Count() > 66) {
                throw new ArgumentException("too many colors: " + cnt);
            }

            ReqPacketMonkeySetAuxiliaryLeds req = new ReqPacketMonkeySetAuxiliaryLeds();
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