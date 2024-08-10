#if !RASPBERRY

using Haruka.Arcade.SEGA835Cmd.Modules.Printer;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoy.Wrapper;
using static System.Net.Mime.MediaTypeNames;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher {
    internal class PrinterWatcherRunner {

        private static string PendingImageFile;
        private static string PendingHoloFile;
        private static string PendingRFIDFile;
        private static bool running;
        private static Options options;

        private static List<Tuple<string, string, string>> pendingImages = new List<Tuple<string, string, string>>();

        internal static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            options = opts;

            if (!Directory.Exists(opts.ImageDirectory)) {
                Log.WriteError("Image directory does not exist: " + opts.ImageDirectory);
                return DeviceStatus.ERR_OTHER;
            }
            if (!File.Exists(opts.ICC1FileName)) {
                Log.WriteError("ICC1 file does not exist: " + opts.ICC1FileName);
                return DeviceStatus.ERR_OTHER;
            }
            if (!File.Exists(opts.ICC2FileName)) {
                Log.WriteError("ICC2 file does not exist: " + opts.ICC2FileName);
                return DeviceStatus.ERR_OTHER;
            }
            if (!File.Exists(opts.MtfFileName)) {
                Log.WriteError("MTF file does not exist: " + opts.MtfFileName);
                return DeviceStatus.ERR_OTHER;
            }
            if (opts.HoloDirectory != null && !Directory.Exists(opts.HoloDirectory)) {
                Log.WriteError("Holo directory does not exist: " + opts.HoloDirectory);
                return DeviceStatus.ERR_OTHER;
            }
            if (opts.RFIDDirectory != null && !Directory.Exists(opts.RFIDDirectory)) {
                Log.WriteError("RFID directory does not exist: " + opts.RFIDDirectory);
                return DeviceStatus.ERR_OTHER;
            }

            FileSystemWatcher watcher = new FileSystemWatcher {
                Path = opts.ImageDirectory,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                Filter = opts.ImageFilePattern
            };
            watcher.Changed += new FileSystemEventHandler(AddImageFile);
            watcher.Created += new FileSystemEventHandler(AddImageFile);
            watcher.EnableRaisingEvents = true;
            Log.Write("Monitoring: " + opts.ImageDirectory);

            if (opts.HoloDirectory != null) {
                FileSystemWatcher watcher2 = new FileSystemWatcher {
                    Path = opts.HoloDirectory,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                    Filter = opts.HoloFilePattern
                };
                watcher2.Changed += new FileSystemEventHandler(AddHoloFile);
                watcher2.Created += new FileSystemEventHandler(AddHoloFile);
                watcher2.EnableRaisingEvents = true;

                Log.Write("Monitoring: " + opts.HoloDirectory);
            }

            if (opts.RFIDDirectory != null) {
                FileSystemWatcher watcher3 = new FileSystemWatcher {
                    Path = opts.RFIDDirectory,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                    Filter = opts.RFIDFilePattern
                };
                watcher3.Changed += new FileSystemEventHandler(AddRFIDFile);
                watcher3.Created += new FileSystemEventHandler(AddRFIDFile);
                watcher3.EnableRaisingEvents = true;
                Log.Write("Monitoring: " + opts.RFIDDirectory);
            }

            running = true;
            Thread queueExecutor = new Thread(QueueExecutorT);
            queueExecutor.Start();

            Console.WriteLine("Press ESC to quit.");

            while (running) {
                if (Console.KeyAvailable) {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) {
                        running = false;
                    }
                }

                while (PendingImageFile == null) {
                    Thread.Sleep(50);
                }
                Thread.Sleep(1000); // wait if holo or RFID get set
                lock (pendingImages) {
                    pendingImages.Add(new Tuple<string, string, string>(PendingImageFile, PendingHoloFile, PendingRFIDFile));
                    PendingRFIDFile = null;
                    PendingImageFile = null;
                    PendingHoloFile = null;
                }
            }

            running = false;
            Log.Write("Waiting for queue thread to terminate...");
            queueExecutor.Join();

            return DeviceStatus.OK;
        }

        private static void QueueExecutorT() {
            Log.Write("Queue thread started");
            while (running) {
                Tuple<string, string, string> image = null;
                lock (pendingImages) {
                    if (pendingImages.Count > 0) {
                        image = pendingImages[0];
                        pendingImages.RemoveAt(0);
                    }
                }
                if (image != null) {
                    Log.Write("Starting print of: " + image.Item1);
                    DeviceStatus ret = PrinterRunner.Run(new Printer.Options() {
                        HoloFileName = image.Item2,
                        ICC1FileName = options.ICC1FileName,
                        ICC2FileName = options.ICC2FileName,
                        ImageFileName = image.Item1,
                        LogFile = null,
                        Model = options.Model,
                        MtfFileName = options.MtfFileName,
                        NoWait = false,
                        Port = options.Port,
                        PrintCardId = false,
                        RFIDFileName = image.Item3,
                        RFIDOverrideCardId = true,
                        Silent = options.Silent,
                        Stretch = options.Stretch
                    });
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Printing of " + image.Item1 + " returned " + ret);
                        if (!options.ContinueOnFail) {
                            running = false;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            Log.Write("Queue thread stopped");
        }

        private static void AddRFIDFile(object sender, FileSystemEventArgs e) {
            Log.Write("RFID File modification detected: " + e.FullPath);
            lock (pendingImages) {
                PendingRFIDFile = e.FullPath;
            }
        }

        private static void AddHoloFile(object sender, FileSystemEventArgs e) {
            Log.Write("Holo File modification detected: " + e.FullPath);
            lock (pendingImages) {
                PendingHoloFile = e.FullPath;
            }
        }

        private static void AddImageFile(object sender, FileSystemEventArgs e) {
            Log.Write("Image File modification detected: " + e.FullPath);
            lock (pendingImages) {
                PendingImageFile = e.FullPath;
            }
        }
    }
}

#endif