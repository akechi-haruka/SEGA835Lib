using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Native {
    internal class UnsafeUtils {

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
