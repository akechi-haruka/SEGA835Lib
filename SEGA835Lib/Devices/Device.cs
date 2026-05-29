using System.Diagnostics.CodeAnalysis;
using System.IO;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices {
    /// <summary>
    /// The base class for any device that this library can handle.
    /// </summary>
    public abstract class Device {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Device));

        private int lastError;
        private bool useExceptions;

        /// <summary>
        /// Whether exceptions are thrown rather than using status return codes.
        /// </summary>
        /// <seealso cref="SetUseExceptions(bool)"/>
        protected bool IsUsingExceptions => useExceptions;

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
        /// <see cref="DeviceStatus.Ok"/> if connection was successful or the device is already connected.<br/>
        /// <see cref="DeviceStatus.Busy"/> if the device uses asynchronous communication and the connection is in progress.<br/>
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the device is not present.<br/>
        /// <see cref="DeviceStatus.ErrorLibrary"/> if required library files, DLLs or assemblies are missing.<br/>
        /// <see cref="DeviceStatus.ErrorIncompatible"/> if the device is not compatible with this computer.<br/>
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was an error communicating with the device.<br/>
        /// <see cref="DeviceStatus.ErrorOther"/> if an internal error occurred.
        /// </returns>
        public abstract DeviceStatus Connect();

        /// <summary>
        /// Disconnects from the device and frees device resources.
        /// </summary>
        /// <returns></returns>
        /// <see cref="DeviceStatus.Ok"/> if connection was successful or the device is already connected.<br/>
        /// <see cref="DeviceStatus.Busy"/> if the device uses asynchronous communication and the connection is in progress.<br/>
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was an error communicating with the device.<br/>
        /// <see cref="DeviceStatus.ErrorOther"/> if an internal error occurred.
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
        /// <exception cref="IOException">If <see cref="IsUsingExceptions"/> is true and status is not <see cref="DeviceStatus.Ok"/> or reportStatus is set and is non-zero.</exception>
        protected DeviceStatus SetLastError(DeviceStatus status, int? reportStatus = null) {
            if (status == DeviceStatus.Ok || status == DeviceStatus.ErrorDevice) {
                lastError = reportStatus.GetValueOrDefault(0);
            } else {
                lastError = (int)status;
            }

            if (lastError > 0) {
                LOG.LogWarning("Recorded a device error (" + GetType() + "): " + (DeviceStatus)lastError);
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
        /// This method can be used if any device operations should use exceptions instead of return codes. If this is true, only <see cref="DeviceStatus.Ok"/> will be returned, or an exception.
        /// </remarks>
        /// <param name="enableExceptions">true to enable exceptions</param>
        public void SetUseExceptions(bool enableExceptions) {
            useExceptions = enableExceptions;
        }
    }

    /// <summary>
    /// The enum of device status codes.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum DeviceStatus {
        /**
         * The device request succeeded.
         */
        Ok = -2,

        /**
         * The device is reporting that it's busy. Retry with the same parameters again in a moment.
         */
        Busy = -1,

        /**
         * Begin of device status codes.
         */
        DeviceStatusCodesStart = 0,

        /**
         * End of device status codes.
         */
        DeviceStatusCodesEnd = 99999,

        /**
         * The device is not connected or the device was disconnected in the middle of the call.
         */
        ErrorNotConnected = 100000,

        /**
         * The device was not initialized (or never connected).
         */
        ErrorNotInitialized,

        /**
         * The device is not compatible with this call.
         */
        ErrorIncompatible,

        /**
         * The device reported a data checksum failure.
         */
        ErrorChecksum,

        /**
         * The payload that was attempted to be sent is too large to fit in device constraints (overflow of length field, etc.)
         */
        ErrorPayloadTooLarge,

        /**
         * The given buffer is too small to hold response data.
         */
        ErrorBufferTooSmall,

        /**
         * The device has reported an (undefined) error.
         */
        ErrorDevice,

        /**
         * The device has not responded to a query.
         */
        ErrorTimeout,

        /**
         * An error occurred trying to load a required library or assembly for this device.
         */
        ErrorLibrary,

        /**
         * An error occurred while trying to de- or encrypt data for this device.
         */
        ErrorCrypt,

        /**
         * An exception has occurred.
         */
        ErrorOther = 9999999,
    }
}