using System;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card {
    /// <summary>
    /// Base class for any Aime card reader board.
    /// A CardReader can read NFC cards of certain types with communication based on an <see cref="SProtDevice"/>.
    /// </summary>
    public abstract class CardReader : SProtDevice {
        /// <summary>
        /// Creates a new CardReader object.
        /// </summary>
        /// <param name="port">The serial COM port to use.</param>
        /// <param name="baudrate">The baudrate to use.</param>
        protected CardReader(int port, int baudrate) : base(port, baudrate) {
        }

        /// <summary>
        /// Starts scanning (polling) for cards.
        /// This method does not block until a card is read. Use <see cref="IsPolling"/> and <see cref="HasDetectedCard"/> to build your blocking loop.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if polling was successfully started or polling is already running.
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if polling failed to start.
        /// </returns>
        public abstract DeviceStatus StartPolling();

        /// <summary>
        /// Checks if polling is still in progress. This may return false if the backend has an error or the device is removed or stopped responding.
        /// </summary>
        /// <returns>true if polling is in progress</returns>
        public abstract bool IsPolling();

        /// <summary>
        /// Stops scanning (polling) for cards.
        /// This method will also implicitely turn off all radios.
        /// The last read card will still remain accessible by <see cref="GetCardType"/> and <see cref="GetCardUID"/>.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if polling was successfully stopped or polling was not running.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if "Connect" was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or "Disconnect" was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if polling could not be stopped.
        /// </returns>
        public abstract DeviceStatus StopPolling();

        /// <summary>
        /// Returns if a card was detected since the last call to <see cref="StartPolling"/>.
        /// </summary>
        /// <returns>true if a card was detected</returns>
        public abstract bool HasDetectedCard();

        /// <summary>
        /// Returns the last read card UID since the last call to <see cref="StartPolling"/>.
        /// </summary>
        /// <returns>the card UID</returns>
        public abstract byte[] GetCardUID();

        /// <summary>
        /// Returns the last read card UID since the last call to <see cref="StartPolling"/> as a string (ex. 010312345678...).
        /// </summary>
        /// <param name="felica_include_pmm">If the card is a FeliCa card, whether or not the PMm value should be retained. If this is false, this will only return the IDm part of a FeliCa. This has no effect for MIFARE cards.</param>
        /// <returns>the card UID</returns>
        public String GetCardUIDAsString(bool felica_include_pmm = false) {
            byte[] uid = GetCardUID();
            if (uid != null) {
                int len = uid.Length;
                if (GetCardType() == CardType.FeliCa && !felica_include_pmm) {
                    len = 8;
                }

                return BitConverter.ToString(uid, 0, len).Replace("-", "");
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns the last read card type since the last call to <see cref="StartPolling"/>.
        /// </summary>
        /// <returns></returns>
        public abstract CardType? GetCardType();

        /// <summary>
        /// Clears the last card values from the reader.
        /// </summary>
        /// <seealso cref="GetCardUID"/>
        public abstract void ClearCard();

        /// <summary>
        /// Sets the card reader's LED color.
        /// </summary>
        /// <param name="red">The R value of the color. [0-255]</param>
        /// <param name="green">The G value of the color. [0-255]</param>
        /// <param name="blue">The B value of the color. [0-255]</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public abstract DeviceStatus LEDSetColor(byte red, byte green, byte blue);

        /// <summary>
        /// Resets the card reader's LEDs to default and/or initializes the LED sub-board.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public abstract DeviceStatus LEDReset();
    }
}