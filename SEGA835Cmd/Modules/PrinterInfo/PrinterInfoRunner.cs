#if !LINUX

using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterInfo;

static class PrinterInfoRunner {
    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        DeviceStatus ret = DeviceStatus.ErrorNotConnected;

        List<ChcSeriesCardPrinter> printers = new List<ChcSeriesCardPrinter>();
        if (opts.Model == Options.PrinterModel.Chc310 || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc310Printer());
        }

        if (opts.Model == Options.PrinterModel.Chc310B || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc310BPrinter());
        }

        if (opts.Model == Options.PrinterModel.Chc320 || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc320Printer());
        }

        if (opts.Model == Options.PrinterModel.Chc330 || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc330Printer(null));
        }

        Log.Write("Available printers: " + printers.Count);

        try {
            foreach (ChcSeriesCardPrinter possiblePrinter in printers.ToArray()) {
                try {
                    ret = possiblePrinter.Connect();
                } catch {
                    ret = DeviceStatus.ErrorNotInitialized;
                }

                if (ret != DeviceStatus.Ok) {
                    Log.WriteWarning(possiblePrinter + " not connected: " + ret);
                    possiblePrinter.Disconnect();
                    printers.Remove(possiblePrinter);
                }
            }

            Log.Write("Connected printers: " + printers.Count);
            if (printers.Count == 0) {
                Log.WriteError("No printers connected!");
                return DeviceStatus.ErrorNotConnected;
            }

            foreach (ChcSeriesCardPrinter printer in printers) {
                Log.Write("Checking " + printer);
                ushort rc = printer.GetPrinterStatusCode();
                if (rc != ChcSeriesCardPrinter.RESULT_NOERROR) {
                    Log.WriteWarning("Printer reports: " + ChcSeriesCardPrinter.RcToString(rc));
                }

                ret = printer.GetPrinterSerial(out string serial);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Failed getting printer serial: " + ret);
                }

                ret = printer.GetPrintCnt2(out PrintCnt2? status);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Failed getting printer counts: " + ret);
                }

                ret = printer.GetPageStatus(out PageStatus? pageStatus);
                if (ret != DeviceStatus.Ok) {
                    Log.WriteError("Failed getting page status: " + ret);
                }

                Console.WriteLine("Printer: " + printer.GetType().Name);
                if (serial != null) {
                    Console.WriteLine(" - Serial: " + serial);
                }

                if (status != null) {
                    PrintCnt2 s = status.Value;
                    Console.WriteLine(" - Printer Counts:");
                    Console.WriteLine("   - Remaining Prints (Color): " + s.RibbonRemain);
                    Console.WriteLine("   - Print Counter 0: " + s.PrintCounter0);
                    Console.WriteLine("   - Print Counter 1: " + s.PrintCounter1);
                    Console.WriteLine("   - Print Counter 2: " + s.PrintCounter2);
                    Console.WriteLine("   - Cut Count: " + s.CutterCount);
                    Console.WriteLine("   - Feed Count: " + s.FeedRoller);
                    Console.WriteLine("   - Head Count: " + s.HeadCount);
                    Console.WriteLine("   - Holo Head Count: " + s.HoloCount);
                    Console.WriteLine("   - Paper Count: " + s.PaperCount);
                    Console.WriteLine("   - Holo Print Counter: " + s.HoloPrintCounter);
                }

                if (pageStatus != null) {
                    PageStatus s = pageStatus.Value;
                    Console.WriteLine(" - Page Status:");
                    Console.WriteLine("   - Remaining Prints (Holo): " + s.HoloRemain);
                }
            }

            return ret;
        } finally {
            foreach (ChcSeriesCardPrinter printer in printers) {
                printer.Disconnect();
            }
        }
    }
}

#endif