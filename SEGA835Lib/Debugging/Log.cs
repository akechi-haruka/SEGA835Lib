using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Haruka.Arcade.SEGA835Lib.Debugging {

    /// <summary>
    /// This class is used for logging to the console, and optionally a file.
    /// </summary>
    public class Log {

        private static string _logFileName = "Log\\Main.log";

        /// <summary>
        /// Sets the log file name.
        /// </summary>
        /// <exception cref="ArgumentException">If <see cref="Init(bool, int)"/> was already called</exception>
        public static string LogFileName {
            get { return _logFileName; }
            set {
                if (!open) {
                    _logFileName = value;
                } else {
                    throw new ArgumentException("Can't change log file after Log.Init().");
                }
            }
        }

        /// <summary>
        /// Flush the log file immediately after each call to <see cref="Write()"/>
        /// </summary>
        public static bool AutoFlush { get; set; } = true;

        /// <summary>
        /// Mutes the log from being written to the console.
        /// </summary>
        public static bool Mute { get; set; }

        /// <summary>
        /// Event handler that receives every log message that was written.
        /// </summary>
        public static event Action<LogEntry> LogMessageWritten;

        private static DateTime initTime = DateTime.Now;
        private static bool open;
        private static StreamWriter log;
        private static readonly object logLock = new object();

        /// <summary>
        /// Opens the log file.
        /// </summary>
        /// <remarks>
        /// This is not required to start logging to the console.
        /// </remarks>
        /// <param name="diagnosticInfo">If some simple system information (such as loaded assemblies, .NET version, etc.) should be logged</param>
        /// <param name="logrotateCount">The amount of log files to keep in rotation. If this is 0, files are not rotated. Rotated files will be suffixed with a number (ex. Main.log.3)</param>
        public static void Init(bool diagnosticInfo = true, int logrotateCount = 5) {
            initTime = DateTime.Now;
            open = true;

            if (_logFileName != null) {
                DirectoryInfo logfolder = Directory.GetParent(_logFileName);
                if (!logfolder.Exists) {
                    logfolder.Create();
                }

                try {
                    if (logrotateCount > 0) {
                        if (File.Exists(LogFileName + "." + logrotateCount)) {
                            File.Delete(LogFileName + "." + logrotateCount);
                        }
                        for (int i = logrotateCount - 1; i >= 1; i--) {
                            if (File.Exists(LogFileName + "." + i)) {
                                File.Move(LogFileName + "." + i, LogFileName + "." + (i + 1));
                            }
                        }
                        if (File.Exists(LogFileName)) {
                            File.Move(LogFileName, LogFileName + ".1");
                        }
                    }
                } catch {
                    WriteError("Log rotation failed (multiple instances?)");
                }

                try {
                    log = File.AppendText(LogFileName);
                } catch {
                    WriteError("Log file open failed (multiple instances?)");
                }
            }

            Write("Log Initialized");

            if (diagnosticInfo) {
                Write("It is currently " + DateTime.Now);
                Write("The Timezone is " + TimeZoneInfo.Local.StandardName);
                Write("App Information:");
                Write("    Name: " + Assembly.GetCallingAssembly().FullName);
                Write("    Path: " + Assembly.GetCallingAssembly().Location.Replace(Environment.UserName, "*****"));
                Write("    Version: " + Assembly.GetCallingAssembly().GetName().Version);
                Write("    Arguments: " + String.Join(" ", Environment.GetCommandLineArgs()));
                Write("    Loaded Libraries: ");
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
                    Write("      " + a.ToString());
                }
                Write("System Information:");
                Write("    CWD: " + Environment.CurrentDirectory.Replace(Environment.UserName, "*****"));
                Write("    OS Version: " + Environment.OSVersion);
                Write("    CPU Cores: " + Environment.ProcessorCount);
                Write("    .NET Version: " + Environment.Version);
                Write("    Language: " + CultureInfo.InstalledUICulture.EnglishName);
            }
        }

        private static void SetConsoleColor(ConsoleColor c) { // fix for unity error
            Console.ForegroundColor = c;
        }

        private static void WriteOut(string message, ConsoleColor c, string section) {
            lock (logLock) {
                if (!Mute) {
                    SetConsoleColor(c);
                }
                string o;
                if (message != null) {
                    string fullSection;
                    if (section.StartsWith("<")) {
                        fullSection = section;
                    } else if (section != null) {
                        fullSection = "[" + section + "]";
                    } else {
                        fullSection = "";
                    }
                    o = "[" + (DateTime.Now - initTime).TotalMilliseconds.ToString("N").PadLeft(14) + "]" + fullSection + " " + message;
                } else {
                    o = "";
                }
                o = o.Replace("\a", ""); // HACK: fixes random beeping caused by outputting Japanese to non-Japanese terminal
                if (!Mute) {
                    Console.WriteLine(o);
                }
                if (Debugger.IsAttached) {
                    Debugger.Log(0, section, message + "\r\n");
                }
                if (open) {
                    log?.WriteLine(o);
                    if (AutoFlush) {
                        Flush();
                    }
                }
            }
            LogMessageWritten?.Invoke(new LogEntry(message, c));
        }

        /// <summary>
        /// Converts an array of object arguments to a comma-seperated string.
        /// </summary>
        /// <remarks>This exists as a workaround for not being able to use params in addition to [CallerMemberName]</remarks>
        /// <param name="objects">The objects to stringify</param>
        /// <returns>A string in the format "object1, object2, object3, ..."</returns>
        public static String Args(params object[] objects) {
#if NET40_OR_GREATER
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

        /// <summary>
        /// Logs an empty line.
        /// </summary>
        public static void Write() {
            WriteOut(null, ConsoleColor.White, null);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="callerFilePath">Auto-generated caller file name</param>
        /// <param name="callerFunc">Auto-generated calling function</param>
        public static void Write(string message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut(message, ConsoleColor.White, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + ">");
        }

        /// <summary>
        /// Logs a warning. In the console, this will show up as yellow.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="callerFilePath">Auto-generated caller file name</param>
        /// <param name="callerFunc">Auto-generated calling function</param>
        public static void WriteWarning(string message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut(message, ConsoleColor.Yellow, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + "> WARN:");
        }

        /// <summary>
        /// Logs an error. In the console, this will show up as red.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="callerFilePath">Auto-generated caller file name</param>
        /// <param name="callerFunc">Auto-generated calling function</param>
        public static void WriteError(string message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut(message, ConsoleColor.Red, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + "> ERROR:");
        }


        /// <summary>
        /// Logs an exception. In the console, this will show up as purple.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="callerFilePath">Auto-generated caller file name</param>
        /// <param name="callerFunc">Auto-generated calling function</param>
        public static void WriteFault(Exception ex, string message = null, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut("FATAL: " + message + "\n" + ex, ConsoleColor.Magenta, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + ">");
        }

        /// <summary>
        /// Calls Flush on the log file. Does nothing if <see cref="Init(bool, int)"/> was not called.
        /// </summary>
        public static void Flush() {
            if (!open) { return; }
            try {
                log?.Flush();
            } catch { }
        }

        /// <summary>
        /// Closes the log file. Does nothing if <see cref="Init(bool, int)"/> was not called. To start logging to a new file, call <see cref="Init(bool, int)"/> again.
        /// </summary>
        public static void Close() {
            lock (logLock) {
                open = false;
                try {
                    log?.Flush();
                } catch { }
                log?.Close();
                log = null;
            }
        }

        /// <summary>
        /// Logs a byte arrray dump with hexadecimal values and text representation.
        /// </summary>
        /// <param name="data">The byte array to log. May be null.</param>
        /// <param name="header">The message to log (which should describe the byte dump)</param>
        /// <param name="callerFilePath">Auto-generated caller file name</param>
        /// <param name="callerFunc">Auto-generated calling function</param>
        public static void Dump(byte[] data, string header = null, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            Write(header + "\n" + Hex.Dump(data), callerFilePath, callerFunc);
        }

        /// <summary>
        /// Logs the current stack.
        /// </summary>
        /// <param name="callerFilePath">Auto-generated caller file name</param>
        /// <param name="callerFunc">Auto-generated calling function</param>
        public static void DumpStack([CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            Write(new StackTrace().ToString(), callerFilePath, callerFunc);
        }
    }
}
