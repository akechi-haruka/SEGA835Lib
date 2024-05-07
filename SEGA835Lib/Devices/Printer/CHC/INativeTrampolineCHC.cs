using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {

    /// <summary>
    /// This class proxies calls to specific CHC printer DLLS (C3XX[AB]Usb.dll) so that implementors don't need to handle differences between the DLLs themselves (fastcall vs stdcall, different signatures, etc.)
    /// </summary>
    public unsafe interface INativeTrampolineCHC {

        /// <summary>
        /// Returns the DLL file name that is being used.
        /// </summary>
        /// <returns>the DLL file name.</returns>
        String GetDLLFileName();

        // Disable documentation requirements for these, 90% of them would just say "Unknown".
#if RELEASE
#pragma warning disable CS1591
#endif
        int CHC_MakeThread(ushort maxCount);
        int CHC_open(ref ushort rResult);
        void CHC_close();
        int CHC_ReleaseThread(ref ushort rResult);
        int CHC_listupPrinter(byte* rIdArray);
        int CHC_listupPrinterSN(ulong* rSerialArray);
        int CHC_selectPrinter(byte printerId, ref ushort rResult);
        int CHC_selectPrinterSN(ulong printerSN, ref ushort rResult);
        int CHC_getPrinterInfo(ushort tagNumber, byte* rBuffer, ref uint rLen);
        int CHC_imageformat(ushort format, ushort ncomp, ushort depth, ushort width, ushort height, byte* image, ref ushort rResult);
        int CHC_setmtf(int* mtf);
        int CHC_makeGamma(ushort k, byte* intoneR, byte* intoneG, byte* intoneB);
        int CHC_setIcctable(string icc1, string icc2, ushort intents, byte* intoneR, byte* intoneG, byte* intoneB, byte* outtoneR, byte* outtoneG, byte* outtoneB, ref ushort rResult);
        int CHC_copies(ushort copies, ref ushort rResult);
        int CHC_status(ref ushort rResult);
        int CHC_statusAll(byte* idArray, ref ushort rResultArray);
        int CHC_startpage(ushort postCardState, ref ushort pageId, ref ushort rResult);
        int CHC_endpage(ref ushort rResult);
        int CHC_write(byte* data, ref uint writeSize, ref ushort rResult);
        int CHC_writeLaminate(byte* data, ref uint writeSize, ref ushort rResult);
        int CHC_writeHolo(byte* data, ref uint writeSize, ref ushort rResult);
        int CHC_setPrinterInfo(PrinterInfoTag tagNumber, byte* rBuffer, ref uint rLen, ref ushort rResult);
        int CHC_getGamma(string filename, byte* r, byte* g, byte* b, ref ushort rResult);
        int CHC_getMtf(string filename, int* mtf, ref ushort rResult);
        int CHC_cancelCopies(ushort pageId, ref ushort rResult);
        int CHC_setPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);
        int CHC_getPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);
        int CHC_blinkLED(ref ushort rResult);
        int CHC_resetPrinter(ref ushort rResult);
        int CHC_AttachThreadCount(ref ushort rCount, ref ushort rMaxCount);
        int CHC_getPrintIDStatus(ushort pageId, byte* rBuffer, ref ushort rResult);
        int CHC_setPrintStandby(ushort position, ref ushort rResult);
        int CHC_testCardFeed(ushort mode, ushort times, ref ushort rResult);
        int CHC_exitCard(ref ushort rResult);
        int CHC_getCardRfidTID(byte* rCardTID, ref ushort rResult);
        int CHC_commCardRfidReader(byte* sendData, byte* rRecvData, uint sendSize, ref uint rRecvSize, ref ushort rResult);
        int CHC_updateCardRfidReader(byte* data, uint size, ref ushort rResult);
        int CHC_getErrorLog(ushort index, byte* rData, ref ushort rResult);
        int CHC_getErrorStatus(ushort* rBuffer);
        int CHC_setCutList(byte* rData, ref ushort rResult);
        int CHC_setLaminatePattern(ushort index, byte* rData, ref ushort rResult);
        int CHC_color_adjustment(string filename, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);
        int CHC_color_adjustmentEx(int a1, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);
        int CHC_getEEPROM(byte index, byte* rData, ref ushort rResult);
        int CHC_setParameter(byte a1, uint a2, ref ushort rResult);
        int CHC_getParameter(byte a1, byte* a2, ref ushort rResult);
        int CHC_universal_command(int a1, byte a2, int a3, byte* a4, ref ushort rResult);
        int CHC_writeIred(byte* a1, byte* a2, ref ushort rResult);

#if RELEASE
#pragma warning restore CS1591
#endif

    }
}
