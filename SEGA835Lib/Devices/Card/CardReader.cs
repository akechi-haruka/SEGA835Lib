using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card {

    /// <summary>
    /// Base class for any Aime card reader board.
    /// A CardReader can read NFC cards of certain types with communication based on <see cref="ISProtRW"/>.
    /// </summary>
    public abstract class CardReader : Device, ISProtRW {

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
        /// <returns>the card UID</returns>
        public String GetCardUIDAsString() {
            byte[] uid = GetCardUID();
            if (uid != null) {
                return BitConverter.ToString(uid).Replace("-", "");
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns the last read card type since the last call to <see cref="StartPolling"/>.
        /// </summary>
        /// <returns></returns>
        public abstract CardType? GetCardType();

        /// <inheritdoc/>
        public abstract DeviceStatus Write(SProtFrame send);

        /// <inheritdoc/>
        public abstract DeviceStatus Read(out SProtFrame recv);
    }
}
