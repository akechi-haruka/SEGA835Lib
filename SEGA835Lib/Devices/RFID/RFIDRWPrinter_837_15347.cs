using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {
    public class RFIDRWPrinter_837_15347 : RFIDDeckReader_837_20004 {

        private const byte COMMAND_WRITE_START_STOP = 0x02;
        private const byte COMMAND_WRITE_BLOCK = 0x03;


        public RFIDRWPrinter_837_15347(int port) : base(port) {
        }

        public override string GetDeviceModel() {
            return "837-15347";
        }

        public override string GetName() {
            return "RFID Reader BD For Embedded";
        }

        public DeviceStatus Write(byte[] cardid, byte[] data) {
            ArgumentNullException.ThrowIfNull(cardid);
            ArgumentNullException.ThrowIfNull(data);
            if (cardid.Length != 12) {
                throw new ArgumentException("cardid must be 12 bytes in length (given: "+cardid.Length+")");
            }
            if (data.Length != 32) {
                throw new ArgumentException("data must be 32 bytes in length (given: " + data.Length + ")");
            }
            Log.Dump(cardid, "Write RFID ID:");
            Log.Dump(data, "Write RFID Data:");

            // todo: these actually return something

            DeviceStatus ret = SetLastError(Write(COMMAND_WRITE_START_STOP, cardid));
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            ret = SetLastError(Read(out SProtFrame _));
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            for (int i = 0; i < data.Length; i+=2) {
                Log.Write("Write Block " + (i/2));
                ret = SetLastError(Write(COMMAND_WRITE_BLOCK, new byte[] { data[i], data[i + 1] }));
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
                ret = SetLastError(Read(out SProtFrame _));
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
            }
            ret = SetLastError(Write(COMMAND_WRITE_START_STOP, new byte[0]));
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            return SetLastError(Read(out SProtFrame _));
        }

        public DeviceStatus ResetWriter() {
            Log.Write("ResetWriter");
            // I am not sure why this needs to be done, but it seems to be mandatory otherwise Scan will give RC 3
            return SetLastError(GetUnknown81(out byte _));
        }

    }

}
