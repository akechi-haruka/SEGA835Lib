using System;

namespace Haruka.Arcade.SEGA835Lib.Debugging {

    /// <summary>
    /// A log entry object sent to subscribers of <see cref="Log.LogMessageWritten"/>.
    /// </summary>
    public class LogEntry {

        /// <summary>
        /// The message of the log entry.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The color of the log entry.
        /// </summary>
        public ConsoleColor Color { get; private set; }

        internal LogEntry(string message, ConsoleColor c) {
            this.Message = message;
            this.Color = c;
        }
    }
}