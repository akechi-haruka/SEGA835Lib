using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID {

    /// <summary>
    /// A 837-15347 RFID card reader/writer that is found inside a CHC-330 printer.
    /// </summary>
    public class RFIDRWPrinter_837_15347 : RFIDDeckReader_837_20004 {

        private const byte COMMAND_WRITE_START_STOP = 0x02;
        private const byte COMMAND_WRITE_BLOCK = 0x03;

        /// <inheritdoc/>
        public RFIDRWPrinter_837_15347(int port) : base(port) {
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "837-15347";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "RFID Reader BD For Embedded";
        }

        /// <summary>
        /// Writes the given data to the currently loaded card.
        /// </summary>
        /// <remarks>
        /// To verify that a card is loaded, call <see cref="RFIDDeckReader_837_20004.Scan(out byte[][])"/> beforehand and check if the first dimension array length is equal to 1.
        /// </remarks>
        /// <param name="cardid">The card id to write (must be 12 bytes).</param>
        /// <param name="data">The card data to write (must be <see cref="RFIDRWDevice.GetCardPayloadSize"/> - 12 bytes).</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        /// <exception cref="ArgumentException">If cardid is not 12 bytes or the data is not <see cref="RFIDRWDevice.GetCardPayloadSize"/> - 12 bytes.</exception>
        public DeviceStatus Write(byte[] cardid, byte[] data) {
            ArgumentNullException.ThrowIfNull(cardid);
            ArgumentNullException.ThrowIfNull(data);
            if (cardid.Length != 12) {
                throw new ArgumentException("cardid must be 12 bytes in length (given: "+cardid.Length+")");
            }
            if (data.Length != GetCardPayloadSize() - 12) {
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

        /// <summary>
        /// Calls <see cref="RFIDRWDevice.GetUnknown81(out byte)"/> on the underlying RFID board. I do not know why this is needed, otherwise calling <see cref="RFIDDeckReader_837_20004.Scan(out byte[][])" /> will return code 3.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other DeviceStatus on error.</returns>
        public DeviceStatus ResetWriter() {
            Log.Write("ResetWriter");
            // I am not sure why this needs to be done, but it seems to be mandatory otherwise Scan will give RC 3
            return SetLastError(GetUnknown81(out byte _));
        }

    }

}
