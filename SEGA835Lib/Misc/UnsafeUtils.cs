using System;

namespace Haruka.Arcade.SEGA835Lib.Misc {
    /// <summary>
    /// Misc. methods for dealing with unmanged data.
    /// </summary>
    internal class UnsafeUtils {
        /// <summary>
        /// Conerts a zero-terminated ASCII byte array to a managed string.
        /// </summary>
        /// <param name="buffer">The zero-terminated string bytes</param>
        /// <param name="offset">The starting offset</param>
        /// <returns>A managed string with the contents of buffer.</returns>
        public static string BytesToString(byte[] buffer, int offset = 0) {
            int len = Array.IndexOf<byte>(buffer, 0);
            if (len == -1) {
                len = buffer.Length;
            }

            unsafe {
                fixed (byte* pAscii = buffer) {
                    return new String((sbyte*)pAscii, offset, len);
                }
            }
        }
    }
}