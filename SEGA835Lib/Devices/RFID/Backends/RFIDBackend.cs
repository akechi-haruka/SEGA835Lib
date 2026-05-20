namespace Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends {
    /// <summary>
    /// The backend that is used for communicating with the RFID board. (Serial, DLL, ...)
    /// </summary>
    public abstract class RfidBackend {
        /// <summary>
        /// Connects to the RFID board.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if connection was successful or the board was already connected.<br />
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board is not attached.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.
        /// </returns>
        public abstract DeviceStatus Connect();

        /// <summary>
        /// Disconnects from the RFID board.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the board was disconnected successfuly or was not connected.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.
        /// </returns>
        public abstract DeviceStatus Disconnect();

        /// <summary>
        /// Reads a packet from the RFID device.
        /// </summary>
        /// <param name="packet">The packet that was read.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the packet was read successfully.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board was disconnected during the operation.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.<br />
        /// any other <see cref="DeviceStatus.DeviceStatusCodesEnd"/> >= DeviceStatus >= <see cref="DeviceStatus.DeviceStatusCodesStart"/> to represent device error codes.
        /// </returns>
        public abstract DeviceStatus Read(out byte[] packet);

        /// <summary>
        /// Writes a packet to the RFID device.
        /// </summary>
        /// <param name="packet">The packet that was written.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the packet was written successfully.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board was disconnected during the operation.<br />
        /// <see cref="DeviceStatus.ErrorLibrary"/> if an error occurred with the native library.<br />
        /// any other <see cref="DeviceStatus.DeviceStatusCodesEnd"/> >= DeviceStatus >= <see cref="DeviceStatus.DeviceStatusCodesStart"/> to represent device error codes.
        /// </returns>
        public abstract DeviceStatus Write(byte[] packet);
    }
}