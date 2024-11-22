using Haruka.Arcade.SEGA835Lib.Debugging;
using System.IO;

namespace Haruka.Arcade.SEGA835Lib.Devices {

    /// <summary>
    /// The base class for any device that this library can handle.
    /// </summary>
    public abstract class Device {

        private int lastError;
        private bool useExceptions;

        /// <summary>
        /// Whether exceptions are thrown rather than using status return codes.
        /// </summary>
        /// <seealso cref="SetUseExceptions(bool)"/>
        protected bool IsUsingExceptions {
            get {
                return useExceptions;
            }
        }

        /// <summary>
        /// Returns the user-friendly name of this device. (ex. Aime Card Reader)
        /// </summary>
        /// <returns>the user-friendly name of this device</returns>
        public abstract string GetName();

        /// <summary>
        /// Returns the model number of this device. (ex. 835-12345)
        /// </summary>
        /// <returns>the model number of this device</returns>
        public abstract string GetDeviceModel();

        /// <summary>
        /// Allocates device resources and connects to this device.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if connection was successful or the device is already connected.<br/>
        /// <see cref="DeviceStatus.BUSY"/> if the device uses asynchronous communication and the connection is in progress.<br/>
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not present.<br/>
        /// <see cref="DeviceStatus.ERR_LIBRARY"/> if required library files, DLLs or assemblies are missing.<br/>
        /// <see cref="DeviceStatus.ERR_INCOMPATIBLE"/> if the device is not compatible with this computer.<br/>
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was an error communicating with the device.<br/>
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an internal error occurred.
        /// </returns>
        public abstract DeviceStatus Connect();

        /// <summary>
        /// Disconnects from the device and frees device resources.
        /// </summary>
        /// <returns></returns>
        /// <see cref="DeviceStatus.OK"/> if connection was successful or the device is already connected.<br/>
        /// <see cref="DeviceStatus.BUSY"/> if the device uses asynchronous communication and the connection is in progress.<br/>
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was an error communicating with the device.<br/>
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an internal error occurred.
        public abstract DeviceStatus Disconnect();

        /// <summary>
        /// Returns the last error code that occurred on any function on this device.
        /// </summary>
        /// <returns>the last error code occurred</returns>
        public int GetLastError() {
            return lastError;
        }

        /// <summary>
        /// Sets the last error code of a method returning a <see cref="DeviceStatus"/> and if <see cref="IsUsingExceptions"/> is true, throw an exception.
        /// </summary>
        /// <param name="status">The last <see cref="DeviceStatus"/> that was obtained from a device call.</param>
        /// <param name="reportStatus">The last device status code that was obtained from a device call or null if the call didn't have one.</param>
        /// <returns>The value passed as "status".</returns>
        /// <exception cref="IOException">If <see cref="IsUsingExceptions"/> is true and status is not <see cref="DeviceStatus.OK"/> or reportStatus is set and is non-zero.</exception>
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

        /// <summary>
        /// Sets whether an <see cref="IOException"/> should be thrown if <see cref="SetLastError(DeviceStatus, int?)"/> is called with a non-success code.
        /// </summary>
        /// <remarks>
        /// This method can be used if any device operations should use exceptions instead of return codes. If this is true, only <see cref="DeviceStatus.OK"/> will be returned, or an exception.
        /// </remarks>
        /// <param name="useExceptions">true to enable exceptions</param>
        public void SetUseExceptions(bool useExceptions) {
            this.useExceptions = useExceptions;
        }
    }

    /// <summary>
    /// The enum of device status codes.
    /// </summary>
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
         * The device has not responded to a query.
         */
        ERR_TIMEOUT,
        /**
         * An error occurred trying to load a required library or assembly for this device.
         */
        ERR_LIBRARY,
        /**
         * An error occurred while trying to de- or encrypt data for this device.
         */
        ERR_CRYPT,
        /**
         * An exception has occurred.
         */
        ERR_OTHER = 9999999,
    }
}
