#if NET8_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    /// <summary>
    /// The base class for a CHC-series card printer.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantLambdaParameterType", Justification = ".NET 10 feature")]
    public abstract partial class ChcSeriesCardPrinter {
        /// <summary>
        /// A print job.
        /// </summary>
        public class PrintJob { // subclass so we can grab the constants from CHCSeriesCardPrinter

            private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(PrintJob));

            private PrintStatus jobStatus;

            internal PrintStatus JobStatus {
                get => jobStatus;
                set {
                    if (jobStatus != value) {
                        LOG.LogInformation(jobStatus + " => " + value);
                        jobStatus = value;
                    }
                }
            }

            internal DeviceStatus JobResult { get; private set; }
            internal byte[] WrittenRfidCardId { get; private set; }
            internal Exception JobException { get; private set; }

            private readonly ChcSeriesCardPrinter printer;
            private readonly INativeTrampolineChc native;
            private readonly Bitmap imageFront;
            private readonly Bitmap imageBack;
            private readonly Bitmap holo;
            private readonly Bitmap infrared;

            private readonly byte[] rfidPayload;

            // these two are probably structs...
            private readonly byte[] paperInfo = new byte[10];
            private readonly int[] mtf = new int[9];
            private readonly byte[] inToneR = new byte[256];
            private readonly byte[] inToneG = new byte[256];
            private readonly byte[] inToneB = new byte[256];
            private readonly byte[] outToneR = new byte[256];
            private readonly byte[] outToneG = new byte[256];
            private readonly byte[] outToneB = new byte[256];
            private readonly bool overrideCardId;

            internal PrintJob(ChcSeriesCardPrinter printer, INativeTrampolineChc native, Bitmap imageFront, Bitmap imageBack, Bitmap infrared, Bitmap holo, byte[] rfidPayload, bool overrideCardId) {
                ArgumentNullException.ThrowIfNull(printer);
                ArgumentNullException.ThrowIfNull(native);
                ArgumentNullException.ThrowIfNull(imageFront);
                this.printer = printer;
                this.native = native;
                this.imageFront = imageFront;
                this.imageBack = imageBack;
                this.holo = holo;
                this.infrared = infrared;
                this.rfidPayload = rfidPayload;
                JobResult = DeviceStatus.ErrorNotInitialized;
                JobStatus = PrintStatus.None;
                JobException = null;
                paperInfo[0] = 0x02; // ???
                paperInfo[1] = (byte)(this.printer.ImageDimensions.Width % 0x100);
                paperInfo[2] = (byte)((this.printer.ImageDimensions.Width >> 8) % 0x100);
                paperInfo[3] = (byte)(this.printer.ImageDimensions.Height % 0x100);
                paperInfo[4] = (byte)((this.printer.ImageDimensions.Height >> 8) % 0x100);
                this.overrideCardId = overrideCardId;
            }

            internal DeviceStatus PrintExitThreadError(DeviceStatus ret, ushort rc, ushort? pageId = null) {
                if (JobStatus == PrintStatus.Errored) {
                    return ret;
                }

                LOG.LogInformation("Terminating print job with error");
                LOG.LogInformation("Last printer return code: " + RcToString(rc));
                if (pageId != null) {
                    LOG.LogInformation("Cancelling print job");
                    ushort _ = 0;
                    native.CHC_cancelCopies(pageId.Value, ref _);
                }

                JobResult = ret;
                JobStatus = PrintStatus.Errored;
                return ret;
            }

            internal unsafe DeviceStatus Run(ref ushort rc) {
                if (JobStatus != PrintStatus.None) {
                    throw new ThreadStateException("This print job was already started");
                }

                JobStatus = PrintStatus.Started;
                JobResult = DeviceStatus.Busy;

                DeviceStatus ret = DeviceStatus.ErrorOther;
                ushort? pageId = 0;

                LOG.LogInformation("Started");

                try {
                    LOG.LogInformation("Checking status");
                    ret = printer.PrintWaitFor(ref rc, native.CHC_status, 10000);
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    LOG.LogInformation("Set printer to standby");
                    ret = printer.PrintWaitFor(ref rc, (ref ushort rc) => native.CHC_setPrintStandby(printer.GetInitialCardPosition(), ref rc), 30000);
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    JobStatus = PrintStatus.CardDataRead;

                    ret = printer.ReadCardInformation(ref rc);
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    if (rfidPayload != null) {
                        ret = printer.WriteRfid(ref rc, rfidPayload, overrideCardId, out byte[] writtenCardId);
                        if (ret != DeviceStatus.Ok || JobStatus == PrintStatus.Errored) {
                            return PrintExitThreadError(ret, rc);
                        }

                        WrittenRfidCardId = writtenCardId;
                    } else {
                        LOG.LogWarning("No RFID data to write");
                    }

                    JobStatus = PrintStatus.SetPrinterProperties;

                    LOG.LogInformation("Setting paper info");
                    uint len;
                    fixed (byte* ptr = paperInfo) {
                        len = (uint)paperInfo.Length;
                        ret = printer.SetLastErrorByReturnCode(native.CHC_setPrinterInfo(PrinterInfoTag.Paper, ptr, ref len, ref rc), rc);
                    }

                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    byte? polishParameter = printer.GetPolishParameter(holo != null);
                    if (polishParameter != null) {
                        LOG.LogInformation("Setting polish info");
                        byte[] polish = new byte[2];
                        polish[0] = polishParameter.Value;
                        fixed (byte* ptr = polish) {
                            len = (uint)polish.Length;
                            ret = printer.SetLastErrorByReturnCode(native.CHC_setPrinterInfo(PrinterInfoTag.PrintMode, ptr, ref len, ref rc), rc);
                        }

                        if (ret != DeviceStatus.Ok) {
                            return PrintExitThreadError(ret, rc);
                        }
                    } else {
                        LOG.LogInformation("Polish info not needed for this printer model");
                    }

                    LOG.LogInformation("Checking status");
                    ret = printer.PrintWaitFor(ref rc, native.CHC_status, 10000);
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    LOG.LogInformation("Setting page parameters");
                    ret = printer.SetLastErrorByReturnCode(native.CHC_imageformat(FORMAT_PIXEL_RGB, COMPONENT_RGB, COLOR_DEPTH, (ushort)printer.ImageDimensions.Width, (ushort)printer.ImageDimensions.Height, (byte*)0, ref rc), rc); // TODO
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    fixed (byte* ptrR = inToneR, ptrG = inToneG, ptrB = inToneB, ptrOutR = outToneR, ptrOutG = outToneG, ptrOutB = outToneB) {
                        LOG.LogInformation("Building tone tables");
                        _ = native.CHC_makeGamma(100, ptrR, ptrG, ptrB);
                        _ = native.CHC_makeGamma(100, ptrOutR, ptrOutG, ptrOutB);

                        LOG.LogInformation("Setting ICC tables");
                        ret = printer.SetLastErrorByReturnCode(native.CHC_setIcctable(printer.IccTable1FileName, printer.IccTable2FileName, RENDERING_INTENTS_PERCEPTUAL, ptrR, ptrG, ptrB, ptrOutR, ptrOutG, ptrOutB, ref rc), rc);
                        if (ret != DeviceStatus.Ok) {
                            return PrintExitThreadError(ret, rc);
                        }
                    }

                    fixed (int* ptr = mtf) {
                        LOG.LogInformation("Reading MTF");
                        ret = printer.SetLastErrorByReturnCode(native.CHC_getMtf(printer.MtfFileName, ptr, ref rc), rc);
                        if (ret != DeviceStatus.Ok) {
                            return PrintExitThreadError(ret, rc);
                        }

                        LOG.LogInformation("Setting MTF");
                        _ = native.CHC_setmtf(ptr);
                    }

                    JobStatus = PrintStatus.SetImage;

                    LOG.LogInformation("Setting page count");
                    ret = printer.SetLastErrorByReturnCode(native.CHC_copies(1, ref rc), rc);
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc);
                    }

                    LOG.LogInformation("Starting page");
                    ushort pageIdQ = 0;
                    ret = printer.PrintWaitFor(ref rc, (ref ushort rc) => native.CHC_startpage(printer.GetStartPageParameter(), ref pageIdQ, ref rc), 3000, RESULT_STATUS_BUSY, RESULT_STATUS_OPERATION);
                    pageId = pageIdQ;
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    LOG.LogInformation("Uploading image data (" + printer.ImageDimensions.Width + "x" + printer.ImageDimensions.Height + ")");
                    int imageSize = printer.ImageDimensions.Width * printer.ImageDimensions.Height * COMPONENT_RGB;
                    byte[] imageBytes = imageFront.GetRawPixelsRgbNoPadding();
                    if (imageBytes.Length != imageSize) {
                        throw new Exception("imageBytes (" + imageBytes.Length + ") != imageSize (" + imageSize + ")");
                    }

                    uint writtenBytes = 0;
                    fixed (byte* ptr = imageBytes) {
                        for (uint pos = 0; pos < imageBytes.Length; pos += writtenBytes) {
                            writtenBytes = (uint)imageBytes.Length - pos;
                            ret = printer.SetLastErrorByReturnCode(native.CHC_write(ptr + pos, ref writtenBytes, ref rc), rc);
                            if (ret != DeviceStatus.Ok) {
                                return PrintExitThreadError(ret, rc, pageId);
                            }
                        }
                    }

                    LOG.LogInformation(writtenBytes + " bytes written");
                    if (writtenBytes != imageBytes.Length) {
                        ret = DeviceStatus.ErrorDevice;
                        LOG.LogError("Failed writing entire image: " + writtenBytes + "/" + imageBytes.Length);
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    if (holo != null) {
                        LOG.LogInformation("Uploading holo image");
                        imageSize = printer.ImageDimensions.Width * printer.ImageDimensions.Height;
                        imageBytes = holo.GetRawPixelsMonochrome();
                        if (imageBytes.Length != imageSize) {
                            throw new Exception("holo: imageBytes (" + imageBytes.Length + ") != imageSize (" + imageSize + ")");
                        }

                        writtenBytes = 0;
                        fixed (byte* ptr = imageBytes) {
                            for (uint pos = 0; pos < imageBytes.Length; pos += writtenBytes) {
                                writtenBytes = (uint)imageBytes.Length - pos;
                                ret = printer.SetLastErrorByReturnCode(native.CHC_writeHolo(ptr + pos, ref writtenBytes, ref rc), rc);
                                if (ret != DeviceStatus.Ok) {
                                    return PrintExitThreadError(ret, rc, pageId);
                                }
                            }
                        }

                        LOG.LogInformation(writtenBytes + " bytes written");
                        if (writtenBytes != imageBytes.Length) {
                            ret = DeviceStatus.ErrorDevice;
                            LOG.LogError("Failed writing entire image: " + writtenBytes + "/" + imageBytes.Length);
                            return PrintExitThreadError(ret, rc, pageId);
                        }
                    } else {
                        LOG.LogInformation("No holo image set");
                    }

                    if (imageBack != null) {
                        LOG.LogInformation("Uploading back side image");
                        imageSize = printer.ImageDimensions.Width * printer.ImageDimensions.Height * COMPONENT_RGB;
                        imageBytes = imageBack.GetRawPixelsRgbNoPadding();
                        if (imageBytes.Length != imageSize) {
                            throw new Exception("backImageBytes (" + imageBytes.Length + ") != imageSize (" + imageSize + ")");
                        }

                        writtenBytes = 0;
                        fixed (byte* ptr = imageBytes) {
                            for (uint pos = 0; pos < imageBytes.Length; pos += writtenBytes) {
                                writtenBytes = (uint)imageBytes.Length - pos;
                                ret = printer.SetLastErrorByReturnCode(native.CHC_write(ptr + pos, ref writtenBytes, ref rc), rc);
                                if (ret != DeviceStatus.Ok) {
                                    return PrintExitThreadError(ret, rc, pageId);
                                }
                            }
                        }

                        LOG.LogInformation(writtenBytes + " bytes written");
                        if (writtenBytes != imageBytes.Length) {
                            ret = DeviceStatus.ErrorDevice;
                            LOG.LogError("Failed writing entire image: " + writtenBytes + "/" + imageBytes.Length);
                            return PrintExitThreadError(ret, rc, pageId);
                        }
                    } else {
                        LOG.LogInformation("No back image set");
                    }

                    if (infrared != null) {
                        JobStatus = PrintStatus.SetImageIr;

                        LOG.LogInformation("Uploading infrared image");
                        imageSize = printer.ImageDimensions.Width * printer.ImageDimensions.Height;
                        imageBytes = infrared.GetRawPixelsMonochrome();
                        if (imageBytes.Length != imageSize) {
                            throw new Exception("infrared: imageBytes (" + imageBytes.Length + ") != imageSize (" + imageSize + ")");
                        }

                        writtenBytes = 0;
                        fixed (byte* ptr = imageBytes) {
                            for (uint pos = 0; pos < imageBytes.Length; pos += writtenBytes) {
                                writtenBytes = (uint)imageBytes.Length - pos;
                                ret = printer.SetLastErrorByReturnCode(native.CHC_writeIred(ptr + pos, ref writtenBytes, ref rc), rc);
                                if (ret != DeviceStatus.Ok) {
                                    return PrintExitThreadError(ret, rc, pageId);
                                }
                            }
                        }

                        LOG.LogInformation(writtenBytes + " bytes written");
                        if (writtenBytes != imageBytes.Length) {
                            ret = DeviceStatus.ErrorDevice;
                            LOG.LogError("Failed writing entire image: " + writtenBytes + "/" + imageBytes.Length);
                            return PrintExitThreadError(ret, rc, pageId);
                        }
                    } else {
                        LOG.LogInformation("No infrared image set");
                    }

                    LOG.LogInformation("Ending page");

                    JobStatus = PrintStatus.Printing;

                    ret = printer.SetLastErrorByReturnCode(native.CHC_endpage(ref rc), rc);
                    if (ret != DeviceStatus.Ok) {
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    LOG.LogInformation("Now printing...");
                    ret = printer.PrintWaitFor(ref rc, (ref ushort rc) => {
                        if (native.CHC_status(ref rc) == CHCUSB_RC_BUSY) {
                            if (rc != RESULT_STATUS_BUSY && rc != RESULT_STATUS_OPERATION && rc != 1007) {
                                LOG.LogError("Status check failed: " + rc);
                                return rc;
                            }
                        }

                        byte[] buf = new byte[8];
                        fixed (byte* ptr = buf) {
                            ret = printer.SetLastErrorByReturnCode(native.CHC_getPrintIDStatus(0, ptr, ref rc), rc);
                        }

                        if (ret != DeviceStatus.Ok) {
                            LOG.LogError("GetPrintIDStatus check failed: " + rc);
                            return rc;
                        }

                        int printStatus = buf[7] << 8 | buf[6];
                        if (printStatus == RESULT_STATUS_PRINTTING_COMPLETE || printStatus == RESULT_STATUS_NO_PRINTTING) {
                            return CHCUSB_RC_OK;
                        }

                        return CHCUSB_RC_BUSY;
                    }, 180000, RESULT_STATUS_BUSY, RESULT_STATUS_OPERATION);
                    if (ret != DeviceStatus.Ok) {
                        if (ret == DeviceStatus.ErrorTimeout) {
                            rc = RESULT_PRINT_TIMEOUT;
                        }

                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    LOG.LogInformation("Print complete");

                    JobStatus = PrintStatus.Ejecting;

                    LOG.LogInformation("Ejecting card");
                    ret = printer.PrintWaitFor(ref rc, native.CHC_exitCard, 180_000, RESULT_STATUS_BUSY, RESULT_STATUS_OPERATION);
                    if (ret != DeviceStatus.Ok) {
                        LOG.LogError("ExitCard failed");
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    LOG.LogInformation("Print finished");
                    JobStatus = PrintStatus.Finished;
                    JobResult = DeviceStatus.Ok;
                } catch (Exception ex) {
                    LOG.LogCritical(ex, "Exception in print job");
                    JobException = ex;
                    PrintExitThreadError(DeviceStatus.ErrorOther, 0, pageId);
                } finally {
                    LOG.LogInformation("Print job finished");
                }

                return ret;
            }
        }
    }
}

#endif