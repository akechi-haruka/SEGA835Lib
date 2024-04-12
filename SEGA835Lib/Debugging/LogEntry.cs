using System;

namespace Haruka.Arcade.SEGA835Lib.Debugging {
    public class LogEntry {
        public string section;
        public string message;
        public ConsoleColor color;

        public LogEntry(string section, string message, ConsoleColor c) {
            this.section = section;
            this.message = message;
            this.color = c;
        }
    }
}