using System;
using System.Collections.Generic;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;
#if NET35
using Haruka.Logging.MEXNet35;
#endif

namespace Haruka.Arcade.SEGA835Lib.Debugging {
    /// <summary>
    /// Manager for logging inside Sega835Lib.
    /// By default, logging is done to the console in a simple format, and to a debugger.
    /// When running under .NET 3.5, Microsoft.Extensions.Logging is not available, and a shim is used.
    /// </summary>
    public static class LogManager {
        /// <summary>
        /// A logging function that is called on .NET 3.5 only.
        /// </summary>
        public delegate void LegacyLogDelegate(string key, string formattedMessage, LogLevel logLevel, EventId eventId, Exception exception);

        internal static Dictionary<string, ILogger> Loggers { get; } = new Dictionary<string, ILogger>();

        internal static ILoggerFactory Factory;

        static LogManager() {
            // default to console logging unless .NET 3.5

#if NET35
            Factory = LoggerFactory.Create(_ => { });
            LoggerShim.MessageLogged += DefaultConsoleLogging;
#else
            Factory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
#endif
        }

        /// <summary>
        /// Initializes the logging for the library.
        /// </summary>
        /// <param name="factory">The logger factory to use.</param>
        public static void Initialize(ILoggerFactory factory) {
            NetStandardBackCompatExtensions.ThrowIfNull(factory, nameof(factory));
            Factory = factory;
        }


        private static void DefaultConsoleLogging(string key, string formattedMessage, LogLevel logLevel, EventId eventId, Exception exception) {
            Console.WriteLine(logLevel + " (" + key + "): " + formattedMessage);
            if (exception != null) {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Adds a legacy callback function for log messages.
        /// This only has an effect on .NET 3.5
        /// </summary>
        /// <param name="onLogMessage">The callback to which log messages are sent to.</param>
        public static void AddLegacyCallback(LegacyLogDelegate onLogMessage) {
#if NET35
            LoggerShim.MessageLogged -= DefaultConsoleLogging;
            LoggerShim.MessageLogged += onLogMessage.Invoke;
#endif
        }

        /// <summary>
        /// Gets or creates a logger by a type name.
        /// </summary>
        /// <param name="t">The owning type for the logger.</param>
        /// <returns>The ILogger where log messages for that type can be written to.</returns>
        public static ILogger GetOrCreate(Type t) {
            return GetOrCreate(t.Name);
        }

        /// <summary>
        /// Gets or creates a logger by a name.
        /// </summary>
        /// <param name="key">The name for the logger.</param>
        /// <returns>The ILogger where log messages can be written to.</returns>
        public static ILogger GetOrCreate(string key) {
            if (Loggers.TryGetValue(key, out ILogger value)) {
                return value;
            }

            value = Factory.CreateLogger(key);
            Loggers[key] = value;

            return value;
        }
    }
}