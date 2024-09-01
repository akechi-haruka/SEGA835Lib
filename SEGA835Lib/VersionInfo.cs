using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("835TestsMaybeLess")]

namespace Haruka.Arcade.SEGA835Lib {

    /// <summary>
    /// Version information about the library.
    /// </summary>
    public class VersionInfo {

        /// <summary>
        /// The library name.
        /// </summary>
        public static readonly String LIB_NAME = typeof(VersionInfo).Assembly.GetName().Name;

        /// <summary>
        /// The library version.
        /// </summary>
        public static readonly Version LIB_VERSION = typeof(VersionInfo).Assembly.GetName().Version;

        /// <summary>
        /// The global API version. This will change only if base API is modified and incompatible with previous versions.
        /// </summary>
        public const int LIB_API_VERSION = 4;

    }
}
