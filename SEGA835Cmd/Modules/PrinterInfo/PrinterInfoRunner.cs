#if !LINUX

using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.Tags;

namespace Haruka.Arcade.SEGA835Cmd.Modules.PrinterInfo {
    internal class PrinterInfoRunner {
        internal static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            DeviceStatus ret = DeviceStatus.ERR_NOT_CONNECTED;

            List<CHCSeriesCardPrinter> printers = new List<CHCSeriesCardPrinter>();
            if (opts.Model == Options.PrinterModel.CHC310 || opts.Model == Options.PrinterModel.Any) {
                printers.Add(new CHC310Printer());
            }

            if (opts.Model == Options.PrinterModel.CHC310B || opts.Model == Options.PrinterModel.Any) {
                printers.Add(new CHC310BPrinter());
            }

            if (opts.Model == Options.PrinterModel.CHC330 || opts.Model == Options.PrinterModel.Any) {
                printers.Add(new CHC330Printer(null));
            }

            Log.Write("Available printers: " + printers.Count);

            try {
                foreach (CHCSeriesCardPrinter possiblePrinter in printers.ToArray()) {
                    try {
                        ret = possiblePrinter.Connect();
                    } catch {
                        ret = DeviceStatus.ERR_NOT_INITIALIZED;
                    }

                    if (ret != DeviceStatus.OK) {
                        Log.WriteWarning(possiblePrinter + " not connected: " + ret);
                        possiblePrinter.Disconnect();
                        printers.Remove(possiblePrinter);
                    }
                }

                Log.Write("Connected printers: " + printers.Count);
                if (printers.Count == 0) {
                    Log.WriteError("No printers connected!");
                    return DeviceStatus.ERR_NOT_CONNECTED;
                }

                foreach (CHCSeriesCardPrinter printer in printers) {
                    Log.Write("Checking " + printer);
                    ushort rc = printer.GetPrinterStatusCode();
                    if (rc != CHCSeriesCardPrinter.RESULT_NOERROR) {
                        Log.WriteWarning("Printer reports: " + CHCSeriesCardPrinter.RCToString(rc));
                    }

                    ret = printer.GetPrinterSerial(out string serial);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Failed getting printer serial: " + ret);
                    }

                    ret = printer.GetPrintCnt2(out PrintCnt2? status);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Failed getting printer counts: " + ret);
                    }

                    ret = printer.GetPageStatus(out PageStatus? pageStatus);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("Failed getting page status: " + ret);
                    }

                    Console.WriteLine("Printer: " + printer.GetType().Name);
                    if (serial != null) {
                        Console.WriteLine(" - Serial: " + serial);
                    }

                    if (status != null) {
                        PrintCnt2 s = status.Value;
                        Console.WriteLine(" - Printer Counts:");
                        Console.WriteLine("   - Remaining Prints (Color): " + s.ribbonRemain);
                        Console.WriteLine("   - Print Counter 0: " + s.printCounter0);
                        Console.WriteLine("   - Print Counter 1: " + s.printCounter1);
                        Console.WriteLine("   - Print Counter 2: " + s.printCounter2);
                        Console.WriteLine("   - Cut Count: " + s.cutterCount);
                        Console.WriteLine("   - Feed Count: " + s.feedRoller);
                        Console.WriteLine("   - Head Count: " + s.headCount);
                        Console.WriteLine("   - Holo Head Count: " + s.holoCount);
                        Console.WriteLine("   - Paper Count: " + s.paperCount);
                        Console.WriteLine("   - Holo Print Counter: " + s.holoPrintCounter);
                    }

                    if (pageStatus != null) {
                        PageStatus s = pageStatus.Value;
                        Console.WriteLine(" - Page Status:");
                        Console.WriteLine("   - Remaining Prints (Holo): " + s.holoRemain);
                    }
                }

                return ret;
            } finally {
                foreach (CHCSeriesCardPrinter printer in printers) {
                    printer.Disconnect();
                }
            }
        }
    }
}

#endif