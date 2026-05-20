#if !LINUX

using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Printer;

static class PrinterRunner {
    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        if (opts.NoWait && opts.PrintCardId) {
            Log.WriteError("--no-wait and --print-card-id exclude each other.");
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.ImageFileName)) {
            Log.WriteError("Image file does not exist!");
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.Icc1FileName)) {
            Log.WriteError("ICC1 file does not exist: " + opts.Icc1FileName);
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.Icc2FileName)) {
            Log.WriteError("ICC2 file does not exist: " + opts.Icc2FileName);
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.MtfFileName)) {
            Log.WriteError("MTF file does not exist: " + opts.MtfFileName);
            return DeviceStatus.ErrorOther;
        }

        if (opts.HoloFileName != null && !File.Exists(opts.HoloFileName)) {
            Log.WriteError("Holo file does not exist: " + opts.HoloFileName);
            return DeviceStatus.ErrorOther;
        }

        if (opts.RfidFileName != null && !File.Exists(opts.RfidFileName)) {
            Log.WriteError("RFID file does not exist: " + opts.RfidFileName);
            return DeviceStatus.ErrorOther;
        }

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
            printers.Add(new Chc330Printer(opts.RfidFileName != null ? new RfidRwPrinter15347(opts.Port) : null));
        }

        Log.Write("Available printers: " + printers.Count);

        try {
            DeviceStatus ret;
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

            ChcSeriesCardPrinter printer = printers[0]; // TODO: select on multiple?

            ushort rc = printer.GetPrinterStatusCode();
            if (rc != ChcSeriesCardPrinter.RESULT_NOERROR) {
                Log.WriteError("Printer reports: " + ChcSeriesCardPrinter.RcToString(rc));
                return DeviceStatus.ErrorNotInitialized;
            }

            printer.SetIccTables(opts.Icc1FileName, opts.Icc2FileName);
            printer.SetMtfFile(opts.MtfFileName);

            Bitmap imageFront, holo, imageBack;
            try {
                imageFront = new Bitmap(Image.FromFile(opts.ImageFileName));
                Log.Write("Image rotate: " + opts.ImageRotateFlip);
                if (opts.ImageRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                    imageFront.RotateFlip(opts.ImageRotateFlip);
                }
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed loading image from " + opts.ImageFileName);
                return DeviceStatus.ErrorOther;
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
                    return DeviceStatus.ErrorOther;
                }
            } else {
                holo = null;
            }

            if (opts.BackImageFileName != null) {
                try {
                    imageBack = new Bitmap(Image.FromFile(opts.BackImageFileName));
                    Log.Write("Image rotate: " + opts.ImageRotateFlip);
                    if (opts.ImageRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                        imageFront.RotateFlip(opts.ImageRotateFlip);
                    }
                } catch (Exception ex) {
                    Log.WriteFault(ex, "Failed loading back side image from " + opts.BackImageFileName);
                    return DeviceStatus.ErrorOther;
                }
            } else {
                imageBack = null;
            }

            byte[] rfid = null;
            if (opts.RfidFileName != null) {
                rfid = File.ReadAllBytes(opts.RfidFileName);
            }

            printer.ImageStretchMode = opts.Stretch;

            ret = printer.StartPrinting(imageFront, rfid, holo, !opts.NoWait, opts.RfidOverrideCardId, imageBack);

            if (opts.PrintCardId) {
                ret = printer.GetWrittenRfidCardId(out byte[] cardid);
                if (ret == DeviceStatus.Ok) {
                    Console.WriteLine(BitConverter.ToString(cardid).Replace("-", ""));
                } else {
                    Log.WriteError("Could not obtain written RFID card ID: " + ret);
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