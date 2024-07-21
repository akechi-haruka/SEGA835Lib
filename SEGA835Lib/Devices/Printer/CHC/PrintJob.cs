#if NET8_0_OR_GREATER

using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {

    /// <summary>
    /// The base class for a CHC-series card printer.
    /// </summary>
    public abstract partial class CHCSeriesCardPrinter : Device {

        /// <summary>
        /// A print job.
        /// </summary>
        public class PrintJob { // subclass so we can grab the constants from CHCSeriesCardPrinter

            private PrintStatus _jobStatus;
            internal PrintStatus JobStatus {
                get {
                    return _jobStatus;
                }
                set {
                    if (_jobStatus != value) {
                        Log.Write(_jobStatus + " => " + value);
                        _jobStatus = value;
                    }
                }
            }
            internal DeviceStatus JobResult { get; private set; }
            internal byte[] WrittenRFIDCardId { get; private set; }
            internal Exception JobException { get; private set; }

            private readonly CHCSeriesCardPrinter Printer;
            private readonly INativeTrampolineCHC Native;
            private readonly Bitmap Image;
            private readonly Bitmap Holo;
            private readonly byte[] RfidPayload;
            // these two are probably structs...
            private readonly byte[] paperInfo = new byte[10];
            private readonly int[] mtf = new int[9];
            private readonly byte[] InToneR = new byte[256];
            private readonly byte[] InToneG = new byte[256];
            private readonly byte[] InToneB = new byte[256];
            private readonly byte[] OutToneR = new byte[256];
            private readonly byte[] OutToneG = new byte[256];
            private readonly byte[] OutToneB = new byte[256];
            private readonly bool OverrideCardId;

            internal PrintJob(CHCSeriesCardPrinter printer, INativeTrampolineCHC native, Bitmap image, Bitmap holo, byte[] rfidPayload, bool overrideCardId) {
                ArgumentNullException.ThrowIfNull(printer);
                ArgumentNullException.ThrowIfNull(native);
                ArgumentNullException.ThrowIfNull(image);
                Printer = printer;
                Native = native;
                Image = image;
                Holo = holo;
                RfidPayload = rfidPayload;
                JobResult = DeviceStatus.ERR_NOT_INITIALIZED;
                JobStatus = PrintStatus.None;
                JobException = null;
                paperInfo[0] = 0x02; // ???
                paperInfo[1] = (byte)(Printer.ImageDimensions.Width % 0x100);
                paperInfo[2] = (byte)((Printer.ImageDimensions.Width >> 8) % 0x100);
                paperInfo[3] = (byte)(Printer.ImageDimensions.Height % 0x100);
                paperInfo[4] = (byte)((Printer.ImageDimensions.Height >> 8) % 0x100);
                OverrideCardId = overrideCardId;
            }
            internal DeviceStatus PrintExitThreadError(DeviceStatus ret, ushort rc, ushort? pageId = null) {
                if (JobStatus == PrintStatus.Errored) {
                    return ret;
                }
                Log.Write("Terminating print job with error");
                Log.Write("Last printer return code: " + RCToString(rc));
                if (pageId != null) {
                    Log.Write("Cancelling print job");
                    ushort _ = 0;
                    Native.CHC_cancelCopies(pageId.Value, ref _);
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
                JobResult = DeviceStatus.BUSY;

                DeviceStatus ret = DeviceStatus.ERR_OTHER;
                uint len = 0;
                ushort? pageId = 0;

                Log.Write("Started");

                try {
                    Log.Write("Checking status");
                    ret = Printer.PrintWaitFor(ref rc, Native.CHC_status, 10000);
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    Log.Write("Set printer to standby");
                    Printer.PrintWaitFor(ref rc, (ref ushort rc) => Native.CHC_setPrintStandby(Standby_RFID, ref rc), 30000);
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    JobStatus = PrintStatus.RFIDRead;

                    Log.Write("Get loaded card ID");
                    // 310 only
                    /*byte[] cardIdBuf = new byte[CARD_ID_LEN];
                    Printer.PrintWaitFor(ref rc, (ref ushort rc) => {
                        unsafe {
                            fixed (byte* ptr = cardIdBuf) {
                                return Native.CHC_getCardRfidTID(ptr, ref rc);
                            }
                        }
                    }, 20000, RESULT_STATUS_BUSY, RESULT_STATUS_Operation);
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }
                    if (rc != RESULT_CARDRFID_ReadA) {
                        Log.WriteError("Unexpected result: " + rc);
                        return PrintExitThreadError(Printer.SetLastErrorByRC(Native.CHC_RC_OK, rc), rc);
                    }*/

                    ret = Printer.WriteRFID(ref rc, RfidPayload, OverrideCardId, out byte[] writtenCardId);
                    if (ret != DeviceStatus.OK || JobStatus == PrintStatus.Errored) {
                        return PrintExitThreadError(ret, rc);
                    }
                    WrittenRFIDCardId = writtenCardId;

                    JobStatus = PrintStatus.SetPrinterProperties;

                    Log.Write("Setting paper info");
                    fixed (byte* ptr = paperInfo) {
                        len = (uint)paperInfo.Length;
                        ret = Printer.SetLastErrorByRC(Native.CHC_setPrinterInfo(PrinterInfoTag.PAPER, ptr, ref len, ref rc), rc);
                    }
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    Log.Write("Setting polish info");
                    byte[] polish = new byte[2];
                    polish[0] = Printer.GetPolishParameter(Holo != null);
                    fixed (byte* ptr = polish) {
                        len = (uint)polish.Length;
                        ret = Printer.SetLastErrorByRC(Native.CHC_setPrinterInfo(PrinterInfoTag.PRINTMODE, ptr, ref len, ref rc), rc);
                    }
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    Log.Write("Checking status");
                    ret = Printer.PrintWaitFor(ref rc, Native.CHC_status, 10000);
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    Log.Write("Setting page parameters");
                    ret = Printer.SetLastErrorByRC(Native.CHC_imageformat(FORMAT_PIXEL_RGB, COMPONENT_RGB, COLOR_DEPTH, (ushort)Printer.ImageDimensions.Width, (ushort)Printer.ImageDimensions.Height, (byte*)0, ref rc), rc); // TODO
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    fixed (byte* ptrR = InToneR, ptrG = InToneG, ptrB = InToneB, ptrOR = OutToneR, ptrOG = OutToneG, ptrOB = OutToneB) {
                        Log.Write("Building tone tables");
                        _ = Native.CHC_makeGamma(100, ptrR, ptrG, ptrB);
                        _ = Native.CHC_makeGamma(100, ptrOR, ptrOG, ptrOB);

                        Log.Write("Setting ICC tables");
                        ret = Printer.SetLastErrorByRC(Native.CHC_setIcctable(Printer.IccTable1FileName, Printer.IccTable2FileName, RENDERING_INTENTS_PERCEPTUAL, ptrR, ptrG, ptrB, ptrOR, ptrOG, ptrOB, ref rc), rc);
                        if (ret != DeviceStatus.OK) {
                            return PrintExitThreadError(ret, rc);
                        }
                    }

                    fixed (int* ptr = mtf) {
                        Log.Write("Reading MTF");
                        ret = Printer.SetLastErrorByRC(Native.CHC_getMtf(Printer.MtfFileName, ptr, ref rc), rc);
                        if (ret != DeviceStatus.OK) {
                            return PrintExitThreadError(ret, rc);
                        }

                        Log.Write("Setting MTF");
                        _ = Native.CHC_setmtf(ptr);
                    }

                    JobStatus = PrintStatus.SetImage;

                    Log.Write("Setting page count");
                    ret = Printer.SetLastErrorByRC(Native.CHC_copies(1, ref rc), rc);
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc);
                    }

                    Log.Write("Starting page");
                    ushort pageIdQ = 0;
                    ret = Printer.PrintWaitFor(ref rc, (ref ushort rc) => Native.CHC_startpage(Printer.GetStartPageParameter(), ref pageIdQ, ref rc), 3000, RESULT_STATUS_BUSY, RESULT_STATUS_Operation);
                    pageId = pageIdQ;
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    uint writtenBytes;
                    int imageSize;
                    byte[] imageBytes;

                    Log.Write("Uploading image data (" + Printer.ImageDimensions.Width + "x" + Printer.ImageDimensions.Height + ")");
                    imageSize = Printer.ImageDimensions.Width * Printer.ImageDimensions.Height * COMPONENT_RGB;
                    imageBytes = Image.GetRawPixelsRGBNoPadding();
                    if (imageBytes.Length != imageSize) {
                        throw new Exception("imageBytes (" + imageBytes.Length + ") != imageSize (" + imageSize + ")");
                    }

                    writtenBytes = 0;
                    fixed (byte* ptr = imageBytes) {
                        for (uint pos = 0; pos < imageBytes.Length; pos += writtenBytes) {
                            writtenBytes = (uint)imageBytes.Length - pos;
                            ret = Printer.SetLastErrorByRC(Native.CHC_write(ptr + pos, ref writtenBytes, ref rc), rc);
                            if (ret != DeviceStatus.OK) {
                                return PrintExitThreadError(ret, rc, pageId);
                            }
                        }
                    }
                    Log.Write(writtenBytes + " bytes written");
                    if (writtenBytes != imageBytes.Length) {
                        ret = DeviceStatus.ERR_DEVICE;
                        Log.WriteError("Failed writing entire image: " + writtenBytes + "/" + imageBytes.Length);
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    if (Holo != null) {
                        Log.Write("Uploading holo image");
                        imageSize = Printer.ImageDimensions.Width * Printer.ImageDimensions.Height;
                        imageBytes = Holo.GetRawPixelsMonochrome();
                        if (imageBytes.Length != imageSize) {
                            throw new Exception("holo: imageBytes (" + imageBytes.Length + ") != imageSize (" + imageSize + ")");
                        }

                        writtenBytes = 0;
                        fixed (byte* ptr = imageBytes) {
                            for (uint pos = 0; pos < imageBytes.Length; pos += writtenBytes) {
                                writtenBytes = (uint)imageBytes.Length - pos;
                                ret = Printer.SetLastErrorByRC(Native.CHC_writeHolo(ptr + pos, ref writtenBytes, ref rc), rc);
                                if (ret != DeviceStatus.OK) {
                                    return PrintExitThreadError(ret, rc, pageId);
                                }
                            }
                        }
                        Log.Write(writtenBytes + " bytes written");
                        if (writtenBytes != imageBytes.Length) {
                            ret = DeviceStatus.ERR_DEVICE;
                            Log.WriteError("Failed writing entire image: " + writtenBytes + "/" + imageBytes.Length);
                            return PrintExitThreadError(ret, rc, pageId);
                        }
                    } else {
                        Log.Write("No holo image set");
                    }

                    Log.Write("Ending page");

                    JobStatus = PrintStatus.Printing;

                    ret = Printer.SetLastErrorByRC(Native.CHC_endpage(ref rc), rc);
                    if (ret != DeviceStatus.OK) {
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    Log.Write("Now printing...");
                    ret = Printer.PrintWaitFor(ref rc, (ref ushort rc) => {
                        if (Native.CHC_status(ref rc) == CHCUSB_RC_BUSY) {
                            if (rc != RESULT_STATUS_BUSY && rc != RESULT_STATUS_Operation && rc != 1007) {
                                Log.WriteError("Status check failed: " + rc);
                                return rc;
                            }
                        }

                        byte[] buf = new byte[8];
                        fixed (byte* ptr = buf) {
                            ret = Printer.SetLastErrorByRC(Native.CHC_getPrintIDStatus(0, ptr, ref rc), rc);
                        }

                        if (ret != DeviceStatus.OK) {
                            Log.WriteError("GetPrintIDStatus check failed: " + rc);
                            return rc;
                        }

                        int printStatus = buf[7] << 8 | buf[6];
                        if (printStatus == RESULT_STATUS_PrinttingComplete || printStatus == RESULT_STATUS_NoPrintting) {
                            return CHCUSB_RC_OK;
                        }

                        return CHCUSB_RC_BUSY;
                    }, 180000, RESULT_STATUS_BUSY, RESULT_STATUS_Operation);
                    if (ret != DeviceStatus.OK) {
                        if (ret == DeviceStatus.ERR_TIMEOUT) {
                            rc = RESULT_PRINT_TIMEOUT;
                        }
                        return PrintExitThreadError(ret, rc, pageId);
                    }
                    Log.Write("Print complete");

                    JobStatus = PrintStatus.Ejecting;

                    Log.Write("Ejecting card");
                    ret = Printer.PrintWaitFor(ref rc, Native.CHC_exitCard, 180_000, RESULT_STATUS_BUSY, RESULT_STATUS_Operation);
                    if (ret != DeviceStatus.OK) {
                        Log.WriteError("ExitCard failed");
                        return PrintExitThreadError(ret, rc, pageId);
                    }

                    Log.Write("Print finished");
                    JobStatus = PrintStatus.Finished;
                    JobResult = DeviceStatus.OK;

                } catch (Exception ex) {
                    Log.WriteFault(ex, "Exception in print job");
                    JobException = ex;
                    PrintExitThreadError(DeviceStatus.ERR_OTHER, 0, pageId);
                } finally {
                    Log.Write("Print job finished");
                }

                return ret;
            }
        }

    }

}

#endif