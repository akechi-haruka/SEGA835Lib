using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    public class RFIDDeckReader_837_20004 : RFIDRWDevice {

        private const byte COMMAND_SCAN = 0x06;
        private const byte SUBCOMMAND_CARD_DATA_START = 0x81;
        private const byte SUBCOMMAND_CARD_DATA = 0x82;
        private const byte SUBCOMMAND_CARD_DATA_END = 0x83;

        public RFIDDeckReader_837_20004(int port) : base(new RFIDBackendSerial(port)) {
        }

        public override string GetDeviceModel() {
            return "837-20004";
        }

        public override string GetName() {
            return "RFID Deck Reader BD Half TKK";
        }

        public DeviceStatus Scan(out byte[][] cards) {
            Log.Write("Scan for cards");
            cards = null;
            DeviceStatus ret = SetLastError(Write(COMMAND_SCAN, new byte[0])); // start scanning
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            ret = SetLastError(Read(out byte cmd, out byte sub_cmd, out byte[] _)); // card data start packet
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            if (cmd != COMMAND_SCAN || sub_cmd != SUBCOMMAND_CARD_DATA_START) {
                Log.WriteError("Unexpected response to Scan packet: " + cmd + "/" + sub_cmd);
                return SetLastError(DeviceStatus.ERR_DEVICE, sub_cmd);
            }
            List<byte[]> cardlist = new List<byte[]>();
            while (true) {
                ret = SetLastError(Read(out cmd, out sub_cmd, out byte[] payload));
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
                if (cmd != COMMAND_SCAN && (sub_cmd != SUBCOMMAND_CARD_DATA && sub_cmd != SUBCOMMAND_CARD_DATA_END)) {
                    Log.WriteError("Unexpected response while reading cards: " + cmd + "/" + sub_cmd);
                    return SetLastError(DeviceStatus.ERR_DEVICE, sub_cmd);
                }
                if (sub_cmd == SUBCOMMAND_CARD_DATA) { // card data packet
                    cardlist.Add(payload);
                } else { // card data end packet
                    break;
                }
            }
            cards = cardlist.ToArray();
            Log.Write("Found " + cards.Length + " card(s)");
            return DeviceStatus.OK;
        }

        public override int GetCardPayloadSize() {
            return 44;
        }
    }
}
