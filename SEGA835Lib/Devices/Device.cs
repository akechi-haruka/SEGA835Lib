using Haruka.Arcade.SEGA835Lib.Debugging;

namespace Haruka.Arcade.SEGA835Lib.Devices {
    public abstract class Device {

        private int lastError;
        private bool useExceptions;

        public abstract string GetName();

        public abstract string GetDeviceModel();

        public abstract DeviceStatus Connect();

        public abstract DeviceStatus Disconnect();

        public int GetLastError() {
            return lastError;
        }

        protected DeviceStatus SetLastError(DeviceStatus status, int? reportStatus = null) {
            if (status == DeviceStatus.OK || status == DeviceStatus.ERR_DEVICE) {
                lastError = reportStatus.GetValueOrDefault(0);
            } else {
                lastError = (int)status;
            }
            if (lastError > 0) {
                Log.WriteWarning("Recorded a device error ("+GetType()+"): " + (DeviceStatus)lastError);
                if (useExceptions) {
                    throw new IOException(GetType() + " device error: " + (DeviceStatus)lastError);
                }
            }
            return status;
        }

        public void SetUseExceptions(bool useExceptions) {
            this.useExceptions = useExceptions;
        }
    }

    public enum DeviceStatus {
        /**
         * The device request succeeded.
         */
        OK = -2,
        /**
         * The device is reporting that it's busy. Retry with the same parameters again in a moment.
         */
        BUSY = -1,
        /**
         * Begin of device status codes.
         */
        DEVICE_STATUS_CODES_START = 0,
        /**
         * End of device status codes.
         */
        DEVICE_STATUS_CODES_END = 99999,
        /**
         * The device is not connected or the device was disconnected in the middle of the call.
         */
        ERR_NOT_CONNECTED = 100000,
        /**
         * The device was not initialized (or never connected).
         */
        ERR_NOT_INITIALIZED,
        /**
         * The device is not compatible with this call.
         */
        ERR_INCOMPATIBLE,
        /**
         * The device reported a data checksum failure.
         */
        ERR_CHECKSUM,
        /**
         * The payload that was attempted to be sent is too large to fit in device constraints (overflow of length field, etc.)
         */
        ERR_PAYLOAD_TOO_LARGE,
        /**
         * The given buffer is too small to hold response data.
         */
        ERR_BUFFER_TOO_SMALL,
        /**
         * The device has reported an (undefined) error.
         */
        ERR_DEVICE,
        /**
         * An exception has occurred.
         */
        ERR_OTHER = 9999999,
    }
}
