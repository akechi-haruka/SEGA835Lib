using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Haruka.Arcade.SEGA835Lib.Debugging {
    public class Log {

        private static string _logFileName = "Log\\Main.log";
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

        public static bool DrawOnScreen { get; set; }
        public static bool AutoFlush { get; set; }
        public static bool LogDebug { get; set; }

        public static event Action<LogEntry> LogMessageWritten;

        private static DateTime initTime = DateTime.Now;
        private static bool open;
        private static StreamWriter log;
        private static readonly object logLock = new object();

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

        private static void WriteOut(string message, ConsoleColor c, string section) {
            lock (logLock) {
                Console.ForegroundColor = c;
                string o;
                if (message != null) {
                    string fullSection;
                    if (section.StartsWith('<')) {
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
                Console.WriteLine(o);
                if (Debugger.IsAttached) {
                    Debugger.Log(0, section, message + "\r\n");
                }
                if (!open) { return; }
                log?.WriteLine(o);
                if (AutoFlush) {
                    Flush();
                }
            }
            LogMessageWritten?.Invoke(new LogEntry(section, message, c));
        }

        public static void Write() {
            WriteOut(null, ConsoleColor.White, null);
        }

        public static void Write(string message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut(message, ConsoleColor.White, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + ">");
        }

        public static void WriteWarning(string message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut(message, ConsoleColor.Yellow, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + "> WARN:");
        }

        public static void WriteError(string message, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut(message, ConsoleColor.Red, "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + "> ERROR:");
        }

        public static void WriteFault(Exception ex, string message = null, string section = null, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            WriteOut("FATAL: " + message + "\n" + ex, ConsoleColor.Magenta, section + "<" + Path.GetFileNameWithoutExtension(callerFilePath) + ":" + callerFunc + ">");
        }

        public static void Flush() {
            if (!open) { return; }
            try {
                log?.Flush();
            } catch { }
        }

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

        public static void Dump(byte[] data, string header = null, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            Write(header + "\n" + Hex.Dump(data), callerFilePath, callerFunc);
        }

        internal static void DumpStack([CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerFunc = null) {
            Write(new StackTrace().ToString(), callerFilePath, callerFunc);
        }
    }
}
