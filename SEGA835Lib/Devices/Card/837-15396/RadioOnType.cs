using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {

    /// <summary>
    /// The type for which card(s) the radio should be turned on.
    /// </summary>
    public enum RadioOnType {
        /// <summary>
        /// Turn on radio for MIFARE cards.
        /// </summary>
        MIFARE = 1,
        /// <summary>
        /// Turn on radio for FeliCa cards.
        /// </summary>
        FeliCa,
        /// <summary>
        /// Turn on radio for both cards.
        /// </summary>
        Both
    }
}
