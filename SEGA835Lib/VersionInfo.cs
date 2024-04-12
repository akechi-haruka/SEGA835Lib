using System.Reflection;

namespace Haruka.Arcade.SEGA835Lib {

    /**
     * Version information about the library.
     */
    public class VersionInfo {

        /**
         * The library name.
         */
        public static readonly String LIB_NAME = typeof(VersionInfo).Assembly.GetName().Name;

        /**
         * The library version.
         */
        public static readonly Version LIB_VERSION = typeof(VersionInfo).Assembly.GetName().Version;

        /**
         * The global API version. This will change only if base API is modified and incompatible with previous versions.
         */
        public const int LIB_API_VERSION = 1;

    }
}
