#if !LINUX

using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using System.Drawing;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Printer {
    internal class PrinterRunner {
        internal static DeviceStatus Run(Options opts) {
            Program.SetGlobalOptions(opts);

            DeviceStatus ret;

            if (opts.NoWait && opts.PrintCardId) {
                Log.WriteError("--no-wait and --print-card-id exclude each other.");
                return DeviceStatus.ERR_OTHER;
            }

            if (!File.Exists(opts.ImageFileName)) {
                Log.WriteError("Image file does not exist!");
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

            if (opts.HoloFileName != null && !File.Exists(opts.HoloFileName)) {
                Log.WriteError("Holo file does not exist: " + opts.HoloFileName);
                return DeviceStatus.ERR_OTHER;
            }

            if (opts.RFIDFileName != null && !File.Exists(opts.RFIDFileName)) {
                Log.WriteError("RFID file does not exist: " + opts.RFIDFileName);
                return DeviceStatus.ERR_OTHER;
            }

            List<CHCSeriesCardPrinter> printers = new List<CHCSeriesCardPrinter>();
            if (opts.Model == Options.PrinterModel.CHC310 || opts.Model == Options.PrinterModel.Any) {
                printers.Add(new CHC310Printer());
            }

            if (opts.Model == Options.PrinterModel.CHC310B || opts.Model == Options.PrinterModel.Any) {
                printers.Add(new CHC310BPrinter());
            }

            if (opts.Model == Options.PrinterModel.CHC330 || opts.Model == Options.PrinterModel.Any) {
                printers.Add(new CHC330Printer(opts.RFIDFileName != null ? new RFIDRWPrinter_837_15347(opts.Port) : null));
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

                CHCSeriesCardPrinter printer = printers[0]; // TODO: select on multiple?

                ushort rc = printer.GetPrinterStatusCode();
                if (rc != CHCSeriesCardPrinter.RESULT_NOERROR) {
                    Log.WriteError("Printer reports: " + CHCSeriesCardPrinter.RCToString(rc));
                    return DeviceStatus.ERR_NOT_INITIALIZED;
                }

                printer.SetIccTables(opts.ICC1FileName, opts.ICC2FileName);
                printer.SetMtfFile(opts.MtfFileName);

                Bitmap image, holo;
                try {
                    image = new Bitmap(Image.FromFile(opts.ImageFileName));
                    Log.Write("Image rotate: " + opts.ImageRotateFlip);
                    if (opts.ImageRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                        image.RotateFlip(opts.ImageRotateFlip);
                    }
                } catch (Exception ex) {
                    Log.WriteFault(ex, "Failed loading image from " + opts.ImageFileName);
                    return DeviceStatus.ERR_OTHER;
                }

                if (opts.HoloFileName != null) {
                    try {
                        holo = new Bitmap(Image.FromFile(opts.HoloFileName));
                        Log.Write("Holo rotate: " + opts.HoloRotateFlip);
                        if (opts.HoloRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                            holo.RotateFlip(opts.HoloRotateFlip);
                        }

                        if (opts.HoloSimplify) {
                            Log.Write("Holo simplify");
                            for (int w = 0; w < holo.Width; w++) {
                                for (int h = 0; h < holo.Height; h++) {
                                    Color p = holo.GetPixel(w, h);
                                    if (p.R != 0 || p.G != 0 || p.B != 0) {
                                        holo.SetPixel(w, h, Color.White);
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Log.WriteFault(ex, "Failed loading holo image from " + opts.HoloFileName);
                        return DeviceStatus.ERR_OTHER;
                    }
                } else {
                    holo = null;
                }

                byte[] rfid = null;
                if (opts.RFIDFileName != null) {
                    rfid = File.ReadAllBytes(opts.RFIDFileName);
                }

                printer.ImageStretchMode = opts.Stretch;

                ret = printer.StartPrinting(image, rfid, holo, !opts.NoWait, opts.RFIDOverrideCardId);

                if (opts.PrintCardId) {
                    ret = printer.GetWrittenRFIDCardId(out byte[] cardid);
                    if (ret == DeviceStatus.OK) {
                        Console.WriteLine(BitConverter.ToString(cardid).Replace("-", ""));
                    } else {
                        Log.WriteError("Could not obtain written RFID card ID: " + ret);
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