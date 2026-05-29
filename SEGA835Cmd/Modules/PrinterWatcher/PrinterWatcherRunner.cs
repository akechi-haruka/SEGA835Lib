#if !LINUX

using Haruka.Arcade.SEGA835Cmd.Modules.Printer;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterWatcher;

static class PrinterWatcherRunner {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(PrinterWatcherRunner));

    private static string pendingImageFile;
    private static string pendingHoloFile;
    private static string pendingRfidFile;
    private static bool running;
    private static Options options;

    private static readonly List<Tuple<string, string, string>> PENDING_IMAGES = new List<Tuple<string, string, string>>();

    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        options = opts;

        if (!Directory.Exists(opts.ImageDirectory)) {
            LOG.LogError("Image directory does not exist: " + opts.ImageDirectory);
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.Icc1FileName)) {
            LOG.LogError("ICC1 file does not exist: " + opts.Icc1FileName);
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.Icc2FileName)) {
            LOG.LogError("ICC2 file does not exist: " + opts.Icc2FileName);
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.MtfFileName)) {
            LOG.LogError("MTF file does not exist: " + opts.MtfFileName);
            return DeviceStatus.ErrorOther;
        }

        if (opts.HoloDirectory != null && !Directory.Exists(opts.HoloDirectory)) {
            LOG.LogError("Holo directory does not exist: " + opts.HoloDirectory);
            return DeviceStatus.ErrorOther;
        }

        if (opts.RfidDirectory != null && !Directory.Exists(opts.RfidDirectory)) {
            LOG.LogError("RFID directory does not exist: " + opts.RfidDirectory);
            return DeviceStatus.ErrorOther;
        }

        FileSystemWatcher watcher = new FileSystemWatcher {
            Path = opts.ImageDirectory,
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
            Filter = opts.ImageFilePattern
        };
        watcher.Changed += AddImageFile;
        watcher.Created += AddImageFile;
        watcher.EnableRaisingEvents = true;
        LOG.LogInformation("Monitoring: " + opts.ImageDirectory);

        if (opts.HoloDirectory != null) {
            FileSystemWatcher watcher2 = new FileSystemWatcher {
                Path = opts.HoloDirectory,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                Filter = opts.HoloFilePattern
            };
            watcher2.Changed += AddHoloFile;
            watcher2.Created += AddHoloFile;
            watcher2.EnableRaisingEvents = true;

            LOG.LogInformation("Monitoring: " + opts.HoloDirectory);
        }

        if (opts.RfidDirectory != null) {
            FileSystemWatcher watcher3 = new FileSystemWatcher {
                Path = opts.RfidDirectory,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
                Filter = opts.RfidFilePattern
            };
            watcher3.Changed += AddRfidFile;
            watcher3.Created += AddRfidFile;
            watcher3.EnableRaisingEvents = true;
            LOG.LogInformation("Monitoring: " + opts.RfidDirectory);
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

            while (pendingImageFile == null) {
                Thread.Sleep(50);
            }

            Thread.Sleep(1000); // wait if holo or RFID get set
            lock (PENDING_IMAGES) {
                PENDING_IMAGES.Add(new Tuple<string, string, string>(pendingImageFile, pendingHoloFile, pendingRfidFile));
                pendingRfidFile = null;
                pendingImageFile = null;
                pendingHoloFile = null;
            }
        }

        running = false;
        LOG.LogInformation("Waiting for queue thread to terminate...");
        queueExecutor.Join();

        return DeviceStatus.Ok;
    }

    private static void QueueExecutorT() {
        LOG.LogInformation("Queue thread started");
        while (running) {
            Tuple<string, string, string> image = null;
            lock (PENDING_IMAGES) {
                if (PENDING_IMAGES.Count > 0) {
                    image = PENDING_IMAGES[0];
                    PENDING_IMAGES.RemoveAt(0);
                }
            }

            if (image != null) {
                LOG.LogInformation("Starting print of: " + image.Item1);
                DeviceStatus ret = PrinterRunner.Run(new Printer.Options() {
                    HoloFileName = image.Item2,
                    Icc1FileName = options.Icc1FileName,
                    Icc2FileName = options.Icc2FileName,
                    ImageFileName = image.Item1,
                    LogFile = null,
                    Model = options.Model,
                    MtfFileName = options.MtfFileName,
                    NoWait = false,
                    Port = options.Port,
                    PrintCardId = false,
                    RfidFileName = image.Item3,
                    RfidOverrideCardId = true,
                    Silent = options.Silent,
                    Stretch = options.Stretch
                });
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("Printing of " + image.Item1 + " returned " + ret);
                    if (!options.ContinueOnFail) {
                        running = false;
                    }
                }

                if (options.DeleteAfterPrint) {
                    try {
                        if (image.Item1 != null) {
                            File.Delete(image.Item1);
                        }

                        if (image.Item2 != null) {
                            File.Delete(image.Item2);
                        }

                        if (image.Item3 != null) {
                            File.Delete(image.Item3);
                        }
                    } catch (Exception ex) {
                        LOG.LogCritical(ex, "Failed to delete file(s) after printing");
                    }
                }
            }

            Thread.Sleep(1000);
        }

        LOG.LogInformation("Queue thread stopped");
    }

    private static void AddRfidFile(object sender, FileSystemEventArgs e) {
        LOG.LogInformation("RFID File modification detected: " + e.FullPath);
        lock (PENDING_IMAGES) {
            pendingRfidFile = e.FullPath;
        }
    }

    private static void AddHoloFile(object sender, FileSystemEventArgs e) {
        LOG.LogInformation("Holo File modification detected: " + e.FullPath);
        lock (PENDING_IMAGES) {
            pendingHoloFile = e.FullPath;
        }
    }

    private static void AddImageFile(object sender, FileSystemEventArgs e) {
        LOG.LogInformation("Image File modification detected: " + e.FullPath);
        lock (PENDING_IMAGES) {
            pendingImageFile = e.FullPath;
        }
    }
}

#endif