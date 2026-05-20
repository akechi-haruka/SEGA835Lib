using System.Collections.Generic;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    /// <summary>
    /// A 837-20004 RFID card reader that is found in Kantai Collection Arcade and Fate/Grand Order Arcade.
    /// </summary>
    public class RfidDeckReader20004 : RfidReadWriteDevice {
        private const byte COMMAND_SCAN = 0x06;
        private const byte SUBCOMMAND_CARD_DATA_START = 0x81;
        private const byte SUBCOMMAND_CARD_DATA = 0x82;
        private const byte SUBCOMMAND_CARD_DATA_END = 0x83;

        /// <summary>
        /// Creates a new card reader.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        public RfidDeckReader20004(int port) : base(new RfidBackendSerial(port)) {
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "837-20004";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "RFID Deck Reader BD Half TKK";
        }

        /// <summary>
        /// Scans for cards. This method blocks until all cards were read.
        /// </summary>
        /// <param name="cards">A 2-dimensional array where the first dimension is the list of cards, and the second dimension is the card's content in bytes, length equal to <see cref="GetCardPayloadSize()"/>.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus Scan(out byte[][] cards) {
            Log.Write("Scan for cards");
            cards = null;
            DeviceStatus ret = SetLastError(Write(COMMAND_SCAN, new byte[0])); // start scanning
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            ret = SetLastError(Read(out byte cmd, out byte subCmd, out byte[] _)); // card data start packet
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            if (cmd != COMMAND_SCAN || subCmd != SUBCOMMAND_CARD_DATA_START) {
                Log.WriteError("Unexpected response to Scan packet: " + cmd + "/" + subCmd);
                return SetLastError(DeviceStatus.ErrorDevice, subCmd);
            }

            List<byte[]> cardlist = new List<byte[]>();
            while (true) {
                ret = SetLastError(Read(out cmd, out subCmd, out byte[] payload));
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                if (cmd != COMMAND_SCAN && (subCmd != SUBCOMMAND_CARD_DATA && subCmd != SUBCOMMAND_CARD_DATA_END)) {
                    Log.WriteError("Unexpected response while reading cards: " + cmd + "/" + subCmd);
                    return SetLastError(DeviceStatus.ErrorDevice, subCmd);
                }

                if (subCmd == SUBCOMMAND_CARD_DATA) { // card data packet
                    cardlist.Add(payload);
                } else { // card data end packet
                    break;
                }
            }

            cards = cardlist.ToArray();
            Log.Write("Found " + cards.Length + " card(s)");
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override int GetCardPayloadSize() {
            return 44;
        }
    }
}