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

    }
}
