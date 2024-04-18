using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC330.Native;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC330 {
    public class CHC330_Printer : CHCSeriesCardPrinter {

        private _837_15347_RFIDRWPrinter rfid;

        public CHC330_Printer(_837_15347_RFIDRWPrinter rfid) {
            this.rfid = rfid;
        }

        public override DeviceStatus Connect() {
            Log.Write("Connect");

            ushort rc = 0;
            DeviceStatus ret = SetLastErrorByRC(chcusb_open(ref rc), rc);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            SelectById();
            //SelectBySN();

            if (ret != DeviceStatus.OK) {
                return ret;
            }

            return SetLastError(rfid?.Connect() ?? DeviceStatus.OK);
        }

        private DeviceStatus SelectById() {
            DeviceStatus ret;
            ushort rc = 0;

            byte[] idArray = new byte[0x80];
            Array.Fill<byte>(idArray, 0xFF);
            unsafe {
                fixed (byte* idArrayPtr = idArray) {
                    ret = SetLastErrorByRC(chcusb_listupPrinter(idArrayPtr));
                }
            }
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            byte printerId = 0xFF;
            foreach (byte id in idArray) {
                if (id != 0xFF) {
                    printerId = id;
                }
            }
            if (printerId == 0xFF) {
                return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
            }
            Log.Write("Select Printer: " + printerId);
            chcusb_selectPrinter(printerId, ref rc); // this seems to return 0 for some obscure reason
            return SetLastErrorByRC(1, rc);
        }

        private DeviceStatus SelectBySN() {
            const ulong NO_ENTRY = 0xFFFFFFFFFFFFFFFF;
            DeviceStatus ret;
            ushort rc = 0;

            ulong[] idArray = new ulong[0x100];
            Array.Fill(idArray, NO_ENTRY);
            unsafe {
                fixed (ulong* idArrayPtr = idArray) {
                    ret = SetLastErrorByRC(chcusb_listupPrinterSN(idArrayPtr));
                }
            }
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            ulong printerId = NO_ENTRY;
            foreach (ulong id in idArray) {
                if (id != NO_ENTRY) {
                    printerId = id;
                }
            }
            if (printerId == NO_ENTRY) {
                return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
            }
            Log.Write("Select Printer (SN): " + printerId);
            return SetLastErrorByRC(chcusb_selectPrinterSN(printerId, ref rc), rc);
        }

        public override DeviceStatus Disconnect() {
            chcusb_close();
            return SetLastError(rfid?.Disconnect() ?? DeviceStatus.OK);
        }

        public override string GetDeviceModel() {
            return "CHC330";
        }

        public override string GetName() {
            return "SINFONIA Card Printer";
        }

        public override ushort GetPrinterStatusCode() {
            ushort status = 0xFFFF;
            if (chcusb_status(ref status) != PRINTER_STATUS_OK) {
                Log.WriteWarning("chcusb_status failed");
            }
            return status;
        }

        public DeviceStatus GetPrinterSerial(out string serialno) {
            DeviceStatus ret;
            Log.Write("GetPrinterSerial");
            const ushort PINFTAG_SERIALINFO = 26;
            uint len = 16;
            byte[] buf = new byte[len];
            unsafe {
                fixed (byte* ptr = buf) {
                    ret = SetLastErrorByRC(chcusb_getPrinterInfo(PINFTAG_SERIALINFO, ptr, ref len));
                }
            }
            if (ret != DeviceStatus.OK) {
                serialno = null;
                return ret;
            }
            serialno = Unsafe.BytesToString(buf);
            return ret;
        }
    }
}
