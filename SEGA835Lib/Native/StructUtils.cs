using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Native {
    internal class StructUtils {

        public static byte[] GetBytes<T>(T str) {
            int size = Marshal.SizeOf(str);

            byte[] arr = new byte[size];

            GCHandle h = default(GCHandle);

            try {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                Marshal.StructureToPtr<T>(str, h.AddrOfPinnedObject(), false);
            } finally {
                if (h.IsAllocated) {
                    h.Free();
                }
            }

            return arr;
        }

        public static T FromBytes<T>(byte[] arr) where T : struct {
            T str = default(T);

            GCHandle h = default(GCHandle);

            try {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                str = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());

            } finally {
                if (h.IsAllocated) {
                    h.Free();
                }
            }

            return str;
        }

        public static unsafe byte[] ConvertToBytes<T>(T value) where T : unmanaged {
            byte* pointer = (byte*)&value;

            byte[] bytes = new byte[sizeof(T)];
            for (int i = 0; i < sizeof(T); i++) {
                bytes[i] = pointer[i];
            }

            return bytes;
        }
    }
}
