using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib {
    public interface IDevice {
        
        public String GetName();

        public String GetDeviceModel();

        public DeviceStatus Connect();

        public DeviceStatus Disconnect();

        public int GetLastError();

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
        ERR_BUFFER_TO_SMALL, 
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
