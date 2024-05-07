using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {

    /// <summary>
    /// A DLL based RFID backend (used by the CHC-310).
    /// </summary>
    internal class RFIDBackendCHCDLL : RFIDBackend {

        protected const int CHCUSB_RC_OK = 1;
        protected const int RESULT_NOERROR = 0;
        protected const int RESULT_CARDRFID_ReadA = 2405;

        private INativeTrampolineCHC Native;
        private byte[] LastResponse;

        /// <summary>
        /// Creates a new RFID backend.
        /// </summary>
        /// <param name="native">The DLL trampoline to use.</param>
        public RFIDBackendCHCDLL(INativeTrampolineCHC native) {
            NetStandardBackCompatExtensions.ThrowIfNull(native, nameof(native));
            this.Native = native;
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            return DeviceStatus.OK; // not supported
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            return DeviceStatus.OK; // not supported
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override DeviceStatus Read(out byte[] packet) {
            packet = LastResponse;
            if (LastResponse == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }
            return DeviceStatus.OK;
        }

    }
}
