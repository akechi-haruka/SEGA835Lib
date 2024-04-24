using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {
    internal class RFIDBackendCHCDLL : RFIDBackend {

        protected const int CHCUSB_RC_OK = 1;
        protected const int RESULT_NOERROR = 0;
        protected const int RESULT_CARDRFID_ReadA = 2405;

        private INativeTrampolineCHC Native;
        private byte[] LastResponse;

        public RFIDBackendCHCDLL(INativeTrampolineCHC native) {
            ArgumentNullException.ThrowIfNull(native);
            this.Native = native;
        }

        public override DeviceStatus Connect() {
            return DeviceStatus.OK; // not supported
        }

        public override DeviceStatus Disconnect() {
            return DeviceStatus.OK; // not supported
        }

        public unsafe override DeviceStatus Write(byte[] packet) {
            ushort rc = 0;
            uint wlen = 2048;
            byte[] resp = new byte[wlen];
            int ret;
            fixed(byte* wptr = packet, rptr = resp) {
                ret = Native.CHC_commCardRfidReader(wptr, rptr, (uint)packet.Length, ref wlen, ref rc);
            }
            if (ret != CHCUSB_RC_OK) {
                LastResponse = null;
                return (DeviceStatus)ret;
            }
            if (rc != RESULT_NOERROR && rc != RESULT_CARDRFID_ReadA) {
                LastResponse = null;
                return (DeviceStatus)ret;
            }
            LastResponse = new byte[wlen];
            Array.Copy(packet, LastResponse, wlen);
            return DeviceStatus.OK;
        }

        public override DeviceStatus Read(out byte[] packet) {
            packet = LastResponse;
            if (LastResponse == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }
            return DeviceStatus.OK;
        }

    }
}
