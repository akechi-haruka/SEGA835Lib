#if NET8_0_OR_GREATER
using System;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Misc;

namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {

    /// <summary>
    /// A DLL based RFID backend (used by the CHC-310).
    /// </summary>
    class RfidBackendChcDll : RfidBackend {

        protected const int CHCUSB_RC_OK = 1;
        protected const int RESULT_NOERROR = 0;
        protected const int RESULT_CARDRFID_READ_A = 2405;

        private readonly INativeTrampolineChc native;
        private byte[] lastResponse;

        /// <summary>
        /// Creates a new RFID backend.
        /// </summary>
        /// <param name="native">The DLL trampoline to use.</param>
        public RfidBackendChcDll(INativeTrampolineChc native) {
            NetStandardBackCompatExtensions.ThrowIfNull(native, nameof(native));
            this.native = native;
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            return DeviceStatus.Ok; // not supported
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            return DeviceStatus.Ok; // not supported
        }

        /// <inheritdoc/>
        public override unsafe DeviceStatus Write(byte[] packet) {
            ushort rc = 0;
            uint wlen = 2048;
            byte[] resp = new byte[wlen];
            int ret;
            fixed(byte* wptr = packet, rptr = resp) {
                ret = native.CHC_commCardRfidReader(wptr, rptr, (uint)packet.Length, ref wlen, ref rc);
            }
            if (ret != CHCUSB_RC_OK) {
                lastResponse = null;
                return (DeviceStatus)ret;
            }
            if (rc != RESULT_NOERROR && rc != RESULT_CARDRFID_READ_A) {
                lastResponse = null;
                return (DeviceStatus)ret;
            }
            lastResponse = new byte[wlen];
            Array.Copy(packet, lastResponse, wlen);
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(out byte[] packet) {
            packet = lastResponse;
            if (lastResponse == null) {
                return DeviceStatus.ErrorNotInitialized;
            }
            return DeviceStatus.Ok;
        }

    }
}

#endif