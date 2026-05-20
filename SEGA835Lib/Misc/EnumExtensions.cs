#if NET8_0_OR_GREATER
using System;
using System.Reflection;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;

namespace Haruka.Arcade.SEGA835Lib.Misc {
    static class EnumExtensions {
        /// <summary>
        /// Retrieves an attribute from an Enum, or null if no such attribute exists.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to retrieve</typeparam>
        /// <param name="enum">This enum.</param>
        /// <seealso cref="PrinterInfoTag"/>
        /// <seealso cref="PrinterTagBufferSize"/>
        /// <returns>The given attribute or null</returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum @enum)
            where TAttribute : Attribute {
            Type type = @enum.GetType();
            string name = Enum.GetName(type, @enum);
            if (name == null) {
                return null;
            }

            FieldInfo enumField = type.GetField(name);
            if (enumField == null) {
                return null;
            }

            return enumField.GetCustomAttribute<TAttribute>();
        }
    }
}

#endif