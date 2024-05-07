using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Misc {
    internal static class NetStandardBackCompatExtensions {

        public static void ThrowIfNull(Object arg, String name) {
            if (arg == null) {
                throw new ArgumentNullException(name);
            }
        }

        public static void Populate<T>(this T[] arr, T value) {
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = value;
            }
        }

    }
}
