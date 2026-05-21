using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
#if !NET5_0_OR_GREATER
using System.Text;
#endif

namespace Haruka.Arcade.SEGA835Lib.Debugging {
    public static class LogExtensions {
        private static string FormatSource(string message, string callerFilePath, string callerFunc) {
            return "<" + callerFilePath + ":" + callerFunc + "> " + message;
        }

        internal static void LogDebugWithSource(this ILogger logger, String message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            logger.LogDebug(FormatSource(message, callerFilePath, callerFunc));
        }

        internal static void LogInformationWithSource(this ILogger logger, String message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            logger.LogInformation(FormatSource(message, callerFilePath, callerFunc));
        }

        internal static void LogWarningWithSource(this ILogger logger, String message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            logger.LogWarning(FormatSource(message, callerFilePath, callerFunc));
        }

        internal static void LogErrorWithSource(this ILogger logger, String message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            logger.LogError(FormatSource(message, callerFilePath, callerFunc));
        }

        internal static void LogCriticalWithSource(this ILogger logger, String message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            logger.LogCritical(FormatSource(message, callerFilePath, callerFunc));
        }

        /// <summary>
        /// Converts an array of object arguments to a comma-seperated string.
        /// </summary>
        /// <remarks>This exists as a workaround for not being able to use params in addition to [CallerMemberName]</remarks>
        /// <param name="objects">The objects to stringify</param>
        /// <returns>A string in the format "object1, object2, object3, ..."</returns>
        public static String Arguments(params object[] objects) {
#if NET5_0_OR_GREATER
            return String.Join(", ", objects);
#else
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < objects.Length; i++) {
                sb.Append(objects[i]);
                if (i + 1 < objects.Length) {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
#endif
        }
    }
}