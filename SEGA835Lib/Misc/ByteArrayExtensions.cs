using System;

namespace Haruka.Arcade.SEGA835Lib.Misc {
    /// <summary>
    /// Helper methods for byte arrays.
    /// </summary>
    public static class ByteArrayExtensions {
        /// <summary>
        /// Converts this byte array into a hexadecimal string (ex. [0, 16, 32] -> "001020")
        /// </summary>
        /// <param name="bytes">This byte array.</param>
        /// <returns>a hexadecimal string representing this byte array.</returns>
        public static String ToHexString(this byte[] bytes) {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}