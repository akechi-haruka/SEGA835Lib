using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Misc {

    /// <summary>
    /// Misc. methods to deal with structs and managed/unmanaged conversion.
    /// </summary>
    public class StructUtils {

        /// <summary>
        /// Checks if the given type has no fields. (= is an empty struct)
        /// </summary>
        /// <remarks>See https://stackoverflow.com/a/27851610 for more information.</remarks>
        /// <param name="t">The type to check.</param>
        /// <returns>true if the type has no fields, false if not.</returns>
        public static bool IsZeroSizeStruct(Type t) {
            return t.IsValueType && !t.IsPrimitive &&
                   t.GetFields((BindingFlags)0x34).All(fi => IsZeroSizeStruct(fi.FieldType));
        }

        /// <summary>
        /// Marshals the given struct to a raw byte array.
        /// </summary>
        /// <typeparam name="T">The type being converted.</typeparam>
        /// <param name="obj">The instance being converted.</param>
        /// <returns>A byte array which is a binary representation of the input object based on default C# marshalling, or an empty array if the given object is a zero-size struct.</returns>
        public static byte[] GetBytes<T>(T obj) {
            int size = Marshal.SizeOf(obj);

            if (size == 1 && IsZeroSizeStruct(obj.GetType())) {
                return new byte[0];
            }

            byte[] arr = new byte[size];

            GCHandle h = default;

            try {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                Marshal.StructureToPtr(obj, h.AddrOfPinnedObject(), false);
            } finally {
                if (h.IsAllocated) {
                    h.Free();
                }
            }

            return arr;
        }

        /// <summary>
        /// Unmarshals the given byte array to an object.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="arr">The object.</param>
        /// <returns>A struct based on the input array.</returns>
        public static T FromBytes<T>(byte[] arr) where T : struct {
            T str = default;

            GCHandle h = default;

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

        internal static unsafe void Copy(byte[] from, byte* to, int length) {
            NetStandardBackCompatExtensions.ThrowIfNull(from, nameof(from));
            fixed (byte* ptr = from) {
                Copy(ptr, 0, to, 0, length);
            }
        }

        internal static unsafe void Copy(byte* from, byte[] to, int length) {
            NetStandardBackCompatExtensions.ThrowIfNull(to, nameof(to));
            fixed (byte* ptr = to) {
                Copy(from, 0, ptr, 0, length);
            }
        }

        internal static unsafe void Copy(byte* from, int from_offset, byte[] to, int to_offset, int length) {
            NetStandardBackCompatExtensions.ThrowIfNull(to, nameof(to));
            fixed (byte* ptr = to) {
                Copy(from, from_offset, ptr, to_offset, length);
            }
        }

        internal static unsafe void Copy(byte* from, int from_offset, byte* to, int to_offset, int length) {
            for (int i = from_offset, j = to_offset; i < from_offset + length; i++, j++) {
                to[j] = from[i];
            }
        }
    }
}
