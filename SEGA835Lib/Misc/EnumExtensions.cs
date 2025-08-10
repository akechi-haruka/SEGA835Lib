﻿using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using System;
using System.Reflection;

namespace Haruka.Arcade.SEGA835Lib.Misc {
    internal static class EnumExtensions {
#if NET8_0_OR_GREATER
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
            var type = @enum.GetType();
            var name = Enum.GetName(type, @enum);
            return type.GetField(name).GetCustomAttribute<TAttribute>();
        }
#endif
    }
}