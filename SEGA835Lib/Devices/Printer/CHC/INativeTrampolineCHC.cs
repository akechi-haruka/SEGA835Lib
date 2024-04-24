using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC {
    public unsafe interface INativeTrampolineCHC {

        public String GetDLLFileName();

        public int CHC_MakeThread(ushort maxCount);
        public int CHC_open(ref ushort rResult);
        public void CHC_close();
        public int CHC_ReleaseThread(ref ushort rResult);
        public int CHC_listupPrinter(byte* rIdArray);
        public int CHC_listupPrinterSN(ulong* rSerialArray);
        public int CHC_selectPrinter(byte printerId, ref ushort rResult);
        public int CHC_selectPrinterSN(ulong printerSN, ref ushort rResult);
        public int CHC_getPrinterInfo(ushort tagNumber, byte* rBuffer, ref uint rLen);
        public int CHC_imageformat(ushort format, ushort ncomp, ushort depth, ushort width, ushort height, byte* image, ref ushort rResult);
        public int CHC_setmtf(int* mtf);
        public int CHC_makeGamma(ushort k, byte* intoneR, byte* intoneG, byte* intoneB);
        public int CHC_setIcctable(string icc1, string icc2, ushort intents, byte* intoneR, byte* intoneG, byte* intoneB, byte* outtoneR, byte* outtoneG, byte* outtoneB, ref ushort rResult);
        public int CHC_copies(ushort copies, ref ushort rResult);
        public int CHC_status(ref ushort rResult);
        public int CHC_statusAll(byte* idArray, ref ushort rResultArray);
        public int CHC_startpage(ushort postCardState, ref ushort pageId, ref ushort rResult);
        public int CHC_endpage(ref ushort rResult);
        public int CHC_write(byte* data, ref uint writeSize, ref ushort rResult);
        public int CHC_writeLaminate(byte* data, ref uint writeSize, ref ushort rResult);
        public int CHC_writeHolo(byte* data, ref uint writeSize, ref ushort rResult);
        public int CHC_setPrinterInfo(PrinterInfoTag tagNumber, byte* rBuffer, ref uint rLen, ref ushort rResult);
        public int CHC_getGamma(string filename, byte* r, byte* g, byte* b, ref ushort rResult);
        public int CHC_getMtf(string filename, int* mtf, ref ushort rResult);
        public int CHC_cancelCopies(ushort pageId, ref ushort rResult);
        public int CHC_setPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);
        public int CHC_getPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);
        public int CHC_blinkLED(ref ushort rResult);
        public int CHC_resetPrinter(ref ushort rResult);
        public int CHC_AttachThreadCount(ref ushort rCount, ref ushort rMaxCount);
        public int CHC_getPrintIDStatus(ushort pageId, byte* rBuffer, ref ushort rResult);
        public int CHC_setPrintStandby(ushort position, ref ushort rResult);
        public int CHC_testCardFeed(ushort mode, ushort times, ref ushort rResult);
        public int CHC_exitCard(ref ushort rResult);
        public int CHC_getCardRfidTID(byte* rCardTID, ref ushort rResult);
        public int CHC_commCardRfidReader(byte* sendData, byte* rRecvData, uint sendSize, ref uint rRecvSize, ref ushort rResult);
        public int CHC_updateCardRfidReader(byte* data, uint size, ref ushort rResult);
        public int CHC_getErrorLog(ushort index, byte* rData, ref ushort rResult);
        public int CHC_getErrorStatus(ushort* rBuffer);
        public int CHC_setCutList(byte* rData, ref ushort rResult);
        public int CHC_setLaminatePattern(ushort index, byte* rData, ref ushort rResult);
        public int CHC_color_adjustment(string filename, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);
        public int CHC_color_adjustmentEx(int a1, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);
        public int CHC_getEEPROM(byte index, byte* rData, ref ushort rResult);
        public int CHC_setParameter(byte a1, uint a2, ref ushort rResult);
        public int CHC_getParameter(byte a1, byte* a2, ref ushort rResult);
        public int CHC_universal_command(int a1, byte a2, int a3, byte* a4, ref ushort rResult);
        public int CHC_writeIred(byte* a1, byte* a2, ref ushort rResult);


    }
}
