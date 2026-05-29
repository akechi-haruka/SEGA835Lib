#if !LINUX

using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Printer;

static class PrinterRunner {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(PrinterRunner));

    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        if (opts.NoWait && opts.PrintCardId) {
            LOG.LogError("--no-wait and --print-card-id exclude each other.");
            return DeviceStatus.ErrorOther;
        }

        if (!File.Exists(opts.ImageFileName)) {
            LOG.LogError("Image file does not exist!");
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

        if (opts.HoloFileName != null && !File.Exists(opts.HoloFileName)) {
            LOG.LogError("Holo file does not exist: " + opts.HoloFileName);
            return DeviceStatus.ErrorOther;
        }

        if (opts.RfidFileName != null && !File.Exists(opts.RfidFileName)) {
            LOG.LogError("RFID file does not exist: " + opts.RfidFileName);
            return DeviceStatus.ErrorOther;
        }

        List<ChcSeriesCardPrinter> printers = new List<ChcSeriesCardPrinter>();
        if (opts.Model == Options.PrinterModel.Chc310 || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc310Printer());
        }

        if (opts.Model == Options.PrinterModel.Chc310B || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc310BPrinter());
        }

        Y3 y3 = null;
        if (opts.Y3Port > 0) {
            y3 = new Y3(opts.Y3Port);
        }

        if (opts.Model == Options.PrinterModel.Chc320 || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc320Printer(y3));
        }

        if (opts.Model == Options.PrinterModel.Chc330 || opts.Model == Options.PrinterModel.Any) {
            printers.Add(new Chc330Printer(opts.RfidFileName != null ? new RfidRwPrinter15347(opts.Port) : null));
        }

        LOG.LogInformation("Available printers: " + printers.Count);

        try {
            DeviceStatus ret;
            foreach (ChcSeriesCardPrinter possiblePrinter in printers.ToArray()) {
                try {
                    ret = possiblePrinter.Connect();
                } catch {
                    ret = DeviceStatus.ErrorNotInitialized;
                }

                if (ret != DeviceStatus.Ok) {
                    LOG.LogWarning(possiblePrinter + " not connected: " + ret);
                    possiblePrinter.Disconnect();
                    printers.Remove(possiblePrinter);
                }
            }

            LOG.LogInformation("Connected printers: " + printers.Count);
            if (printers.Count == 0) {
                LOG.LogError("No printers connected!");
                return DeviceStatus.ErrorNotConnected;
            }

            ChcSeriesCardPrinter printer = printers[0]; // TODO: select on multiple?

            ushort rc = printer.GetPrinterStatusCode();
            if (rc != ChcSeriesCardPrinter.RESULT_NOERROR) {
                LOG.LogError("Printer reports: " + ChcSeriesCardPrinter.RcToString(rc));
                return DeviceStatus.ErrorNotInitialized;
            }

            printer.SetIccTables(opts.Icc1FileName, opts.Icc2FileName);
            printer.SetMtfFile(opts.MtfFileName);

            Bitmap imageFront, holo, imageBack;
            try {
                imageFront = new Bitmap(Image.FromFile(opts.ImageFileName));
                LOG.LogInformation("Image rotate: " + opts.ImageRotateFlip);
                if (opts.ImageRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                    imageFront.RotateFlip(opts.ImageRotateFlip);
                }
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Failed loading image from " + opts.ImageFileName);
                return DeviceStatus.ErrorOther;
            }

            if (opts.HoloFileName != null) {
                try {
                    holo = new Bitmap(Image.FromFile(opts.HoloFileName));
                    LOG.LogInformation("Holo rotate: " + opts.HoloRotateFlip);
                    if (opts.HoloRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                        holo.RotateFlip(opts.HoloRotateFlip);
                    }

                    if (opts.HoloSimplify) {
                        LOG.LogInformation("Holo simplify");
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
                    LOG.LogCritical(ex, "Failed loading holo image from " + opts.HoloFileName);
                    return DeviceStatus.ErrorOther;
                }
            } else {
                holo = null;
            }

            if (opts.BackImageFileName != null) {
                try {
                    imageBack = new Bitmap(Image.FromFile(opts.BackImageFileName));
                    LOG.LogInformation("Image rotate: " + opts.ImageRotateFlip);
                    if (opts.ImageRotateFlip != RotateFlipType.RotateNoneFlipNone) {
                        imageFront.RotateFlip(opts.ImageRotateFlip);
                    }
                } catch (Exception ex) {
                    LOG.LogCritical(ex, "Failed loading back side image from " + opts.BackImageFileName);
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

            if (y3 != null) {
                ret = y3.Connect();
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("Error connecting to Y3 board");
                    return ret;
                }

                ret = y3.SetParamsForPrinter();
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("Error setting parameters for printer");
                    return ret;
                }

                ret = y3.Start();
                if (ret != DeviceStatus.Ok) {
                    LOG.LogError("Error starting printer camera");
                    return ret;
                }

                Y3.Status status = y3.GetStatus();
                if (status != Y3.Status.Active) {
                    LOG.LogError("Unexpected printer camera status: " + status);
                    return DeviceStatus.ErrorOther;
                }

                ((Chc320Printer)printer).CardDataRead += (card) => OnCardDataRead(opts.Y3OutputFile, card);
            }

            ret = printer.StartPrinting(imageFront, rfid, holo, !opts.NoWait, opts.RfidOverrideCardId, imageBack);

            if (opts.PrintCardId) {
                ret = printer.GetWrittenRfidCardId(out byte[] cardid);
                if (ret == DeviceStatus.Ok) {
                    Console.WriteLine(BitConverter.ToString(cardid).Replace("-", ""));
                } else {
                    LOG.LogError("Could not obtain written RFID card ID: " + ret);
                }
            }

            return ret;
        } finally {
            foreach (ChcSeriesCardPrinter printer in printers) {
                printer.Disconnect();
            }

            y3?.Disconnect();
        }
    }

    private static void OnCardDataRead(string outputFile, Y3.CardInfo obj) {
        File.WriteAllText(outputFile, obj.ToCsv());
    }
}

#endif