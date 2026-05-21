using System;
using System.Collections.Generic;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;
#if NET35
using Haruka.Logging.MEXNet35;
#endif

namespace Haruka.Arcade.SEGA835Lib.Debugging {
    public static class LogManager {
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

        public static void Initialize(ILoggerFactory factory) {
            NetStandardBackCompatExtensions.ThrowIfNull(factory, nameof(factory));
            Factory = factory;
        }

#if NET35
        private static void DefaultConsoleLogging(string key, string formattedMessage, LogLevel logLevel, EventId eventId, Exception exception) {
            Console.WriteLine(logLevel + " (" + key + "): " + formattedMessage);
            if (exception != null) {
                Console.WriteLine(exception);
            }
        }
        
        public static void AddLegacyCallback(LoggerShim.LogDelegate onLogMessage) {
            LoggerShim.MessageLogged -= DefaultConsoleLogging;
            LoggerShim.MessageLogged += onLogMessage;
        }
#endif

        public static ILogger GetOrCreate(Type t) {
            return GetOrCreate(t.Name);
        }

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