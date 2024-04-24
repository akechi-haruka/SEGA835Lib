using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330 {

    public unsafe partial class Native : INativeTrampolineCHC {

        public const String DLL = "C330Ausb.dll";

        /// Return Type: int
        ///maxCount: unsigned short
        [LibraryImport(DLL, EntryPoint = "chcusb_MakeThread")]
        private static partial int chcusb_MakeThread(ushort maxCount);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_open")]
        private static partial int chcusb_open(ref ushort rResult);


        /// Return Type: void
        [LibraryImport(DLL, EntryPoint = "chcusb_close")]
        private static partial void chcusb_close();


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_ReleaseThread")]
        private static partial int chcusb_ReleaseThread(ref ushort rResult);


        /// Return Type: int
        ///rIdArray: unsigned byte*
        [LibraryImport(DLL, EntryPoint = "chcusb_listupPrinter")]
        private static partial int chcusb_listupPrinter(byte* rIdArray);


        /// Return Type: int
        ///rSerialArray: unsigned int*
        [LibraryImport(DLL, EntryPoint = "chcusb_listupPrinterSN")]
        private static partial int chcusb_listupPrinterSN(ulong* rSerialArray);


        /// Return Type: int
        ///printerId: unsigned byte
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_selectPrinter")]
        private static partial int chcusb_selectPrinter(byte printerId, ref ushort rResult);


        /// Return Type: int
        ///printerSN: unsigned int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_selectPrinterSN")]
        private static partial int chcusb_selectPrinterSN(ulong printerSN, ref ushort rResult);


        /// Return Type: int
        ///tagNumber: unsigned short
        ///rBuffer: unsigned byte*
        ///rLen: unsigned int*
        [LibraryImport(DLL, EntryPoint = "chcusb_getPrinterInfo")]
        private static partial int chcusb_getPrinterInfo(ushort tagNumber, byte* rBuffer, ref uint rLen);


        /// Return Type: int
        ///format: unsigned short
        ///ncomp: unsigned short
        ///depth: unsigned short
        ///width: unsigned short
        ///height: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_imageformat")]
        private static partial int chcusb_imageformat(ushort format, ushort ncomp, ushort depth, ushort width, ushort height, ref ushort rResult);


        /// Return Type: int
        ///mtf: int*
        [LibraryImport(DLL, EntryPoint = "chcusb_setmtf")]
        private static partial int chcusb_setmtf(int* mtf);


        /// Return Type: int
        ///k: unsigned short
        ///intoneR: unsigned byte*
        ///intoneG: unsigned byte*
        ///intoneB: unsigned byte*
        [LibraryImport(DLL, EntryPoint = "chcusb_makeGamma")]
        private static partial int chcusb_makeGamma(ushort k, byte* intoneR, byte* intoneG, byte* intoneB);


        /// Return Type: int
        ///icc1: LPCSTR->CHAR*
        ///icc2: LPCSTR->CHAR*
        ///intents: unsigned short
        ///intoneR: unsigned byte*
        ///intoneG: unsigned byte*
        ///intoneB: unsigned byte*
        ///outtoneR: unsigned byte*
        ///outtoneG: unsigned byte*
        ///outtoneB: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setIcctable")]
        private static partial int chcusb_setIcctable([MarshalAs(UnmanagedType.LPStr)] string icc1, [MarshalAs(UnmanagedType.LPStr)] string icc2, ushort intents, byte* intoneR, byte* intoneG, byte* intoneB, byte* outtoneR, byte* outtoneG, byte* outtoneB, ref ushort rResult);


        /// Return Type: int
        ///copies: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_copies")]
        private static partial int chcusb_copies(ushort copies, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_status")]
        private static partial int chcusb_status(ref ushort rResult);


        /// Return Type: int
        ///idArray: unsigned byte*
        ///rResultArray: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_statusAll")]
        private static partial int chcusb_statusAll(byte* idArray, ref ushort rResultArray);


        /// Return Type: int
        ///postCardState: unsigned short
        ///pageId: unsigned short*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_startpage")]
        private static partial int chcusb_startpage(ushort postCardState, ref ushort pageId, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_endpage")]
        private static partial int chcusb_endpage(ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///writeSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_write")]
        private static partial int chcusb_write(byte* data, ref uint writeSize, ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///writeSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_writeLaminate")]
        private static partial int chcusb_writeLaminate(byte* data, ref uint writeSize, ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///writeSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_writeHolo")]
        private static partial int chcusb_writeHolo(byte* data, ref uint writeSize, ref ushort rResult);


        /// Return Type: int
        ///tagNumber: unsigned short
        ///rBuffer: unsigned byte*
        ///rLen: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setPrinterInfo")]
        private static partial int chcusb_setPrinterInfo(PrinterInfoTag tagNumber, byte* rBuffer, ref uint rLen, ref ushort rResult);


        /// Return Type: int
        ///filename: LPCSTR->CHAR*
        ///r: unsigned byte*
        ///g: unsigned byte*
        ///b: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getGamma")]
        private static partial int chcusb_getGamma([MarshalAs(UnmanagedType.LPStr)] string filename, byte* r, byte* g, byte* b, ref ushort rResult);


        /// Return Type: int
        ///filename: LPCSTR->CHAR*
        ///mtf: int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getMtf")]
        private static partial int chcusb_getMtf([MarshalAs(UnmanagedType.LPStr)] string filename, int* mtf, ref ushort rResult);


        /// Return Type: int
        ///pageId: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_cancelCopies")]
        private static partial int chcusb_cancelCopies(ushort pageId, ref ushort rResult);


        /// Return Type: int
        ///type: unsigned short
        ///number: unsigned short
        ///data: unsigned short*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setPrinterToneCurve")]
        private static partial int chcusb_setPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);


        /// Return Type: int
        ///type: unsigned short
        ///number: unsigned short
        ///data: unsigned short*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getPrinterToneCurve")]
        private static partial int chcusb_getPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_blinkLED")]
        private static partial int chcusb_blinkLED(ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_resetPrinter")]
        private static partial int chcusb_resetPrinter(ref ushort rResult);


        /// Return Type: int
        ///rCount: unsigned short*
        ///rMaxCount: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_AttachThreadCount")]
        private static partial int chcusb_AttachThreadCount(ref ushort rCount, ref ushort rMaxCount);


        /// Return Type: int
        ///pageId: unsigned short
        ///rBuffer: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getPrintIDStatus")]
        private static partial int chcusb_getPrintIDStatus(ushort pageId, byte* rBuffer, ref ushort rResult);


        /// Return Type: int
        ///position: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setPrintStandby")]
        private static partial int chcusb_setPrintStandby(ushort position, ref ushort rResult);


        /// Return Type: int
        ///mode: unsigned short
        ///times: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_testCardFeed")]
        private static partial int chcusb_testCardFeed(ushort mode, ushort times, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_exitCard")]
        private static partial int chcusb_exitCard(ref ushort rResult);

        /// Return Type: int
        ///index: unsigned short
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getErrorLog")]
        private static partial int chcusb_getErrorLog(ushort index, byte* rData, ref ushort rResult);


        /// Return Type: int
        ///rBuffer: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getErrorStatus")]
        private static partial int chcusb_getErrorStatus(ushort* rBuffer);


        /// Return Type: int
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setCutList")]
        private static partial int chcusb_setCutList(byte* rData, ref ushort rResult);


        /// Return Type: int
        ///index: unsigned short
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setLaminatePattern")]
        private static partial int chcusb_setLaminatePattern(ushort index, byte* rData, ref ushort rResult);


        /// Return Type: int
        ///filename: LPCSTR->CHAR*
        ///a2: int
        ///a3: int
        ///a4: short
        ///a5: short
        ///a6: int
        ///a7: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_color_adjustment")]
        private static partial int chcusb_color_adjustment([MarshalAs(UnmanagedType.LPStr)] string filename, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);


        /// Return Type: int
        ///a1: int
        ///a2: int
        ///a3: int
        ///a4: short
        ///a5: short
        ///a6: int
        ///a7: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_color_adjustmentEx")]
        private static partial int chcusb_color_adjustmentEx(int a1, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);


        /// Return Type: int
        ///index: unsigned byte
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getEEPROM")]
        private static partial int chcusb_getEEPROM(byte index, byte* rData, ref ushort rResult);


        /// Return Type: int
        ///a1: unsigned byte
        ///a2: unsigned int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setParameter")]
        private static partial int chcusb_setParameter(byte a1, uint a2, ref ushort rResult);


        /// Return Type: int
        ///a1: unsigned byte
        ///a2: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getParameter")]
        private static partial int chcusb_getParameter(byte a1, byte* a2, ref ushort rResult);


        /// Return Type: int
        ///a1: int
        ///a2: unsigned byte
        ///a3: int
        ///a4: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_universal_command")]
        private static partial int chcusb_universal_command(int a1, byte a2, int a3, byte* a4, ref ushort rResult);


        /// Return Type: int
        ///a1: unsigned byte*
        ///a2: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_writeIred")]
        private static partial int chcusb_writeIred(byte* a1, byte* a2, ref ushort rResult);

        public string GetDLLFileName() {
            return DLL;
        }

        public int CHC_MakeThread(ushort maxCount) {
            Log.Write(Log.Args(maxCount));
            return chcusb_MakeThread(maxCount);
        }

        public int CHC_open(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_open(ref rResult);
        }

        public void CHC_close() {
            Log.Write(Log.Args());
            chcusb_close();
        }

        public int CHC_ReleaseThread(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_ReleaseThread(ref rResult);
        }

        public int CHC_listupPrinter(byte* rIdArray) {
            Log.Write(Log.Args());
            return chcusb_listupPrinter(rIdArray);
        }

        public int CHC_listupPrinterSN(ulong* rSerialArray) {
            Log.Write(Log.Args());
            return chcusb_listupPrinterSN(rSerialArray);
        }

        public int CHC_selectPrinter(byte printerId, ref ushort rResult) {
            Log.Write(Log.Args(printerId));
            return chcusb_selectPrinter(printerId, ref rResult);
        }

        public int CHC_selectPrinterSN(ulong printerSN, ref ushort rResult) {
            Log.Write(Log.Args(printerSN));
            return chcusb_selectPrinterSN(printerSN, ref rResult);
        }

        public int CHC_getPrinterInfo(ushort tagNumber, byte* rBuffer, ref uint rLen) {
            Log.Write(Log.Args(tagNumber, rLen));
            return chcusb_getPrinterInfo(tagNumber, rBuffer, ref rLen);

        }

        public int CHC_imageformat(ushort format, ushort ncomp, ushort depth, ushort width, ushort height, byte* _, ref ushort rResult) {
            Log.Write(Log.Args(format, ncomp, depth, width, height));
            return chcusb_imageformat(format, ncomp, depth, width, height, ref rResult);
        }

        public int CHC_setmtf(int* mtf) {
            Log.Write(Log.Args());
            return chcusb_setmtf(mtf);
        }

        public int CHC_makeGamma(ushort k, byte* intoneR, byte* intoneG, byte* intoneB) {
            Log.Write(Log.Args(k));
            return chcusb_makeGamma(k, intoneR, intoneG, intoneB);
        }

        public int CHC_setIcctable(string icc1, string icc2, ushort intents, byte* intoneR, byte* intoneG, byte* intoneB, byte* outtoneR, byte* outtoneG, byte* outtoneB, ref ushort rResult) {
            Log.Write(Log.Args(icc1, icc2, intents));
            return chcusb_setIcctable(icc1, icc2, intents, intoneR, intoneG, intoneB, outtoneR, outtoneG, outtoneB, ref rResult);
        }

        public int CHC_copies(ushort copies, ref ushort rResult) {
            Log.Write(Log.Args(copies));
            return chcusb_copies(copies, ref rResult);
        }

        public int CHC_status(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_status(ref rResult);
        }

        public int CHC_statusAll(byte* idArray, ref ushort rResultArray) {
            Log.Write(Log.Args());
            return chcusb_statusAll(idArray, ref rResultArray);
        }

        public int CHC_startpage(ushort postCardState, ref ushort pageId, ref ushort rResult) {
            Log.Write(Log.Args(postCardState, pageId));
            return chcusb_startpage(postCardState, ref pageId, ref rResult);
        }

        public int CHC_endpage(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_endpage(ref rResult);
        }

        public int CHC_write(byte* data, ref uint writeSize, ref ushort rResult) {
            Log.Write(Log.Args(writeSize));
            return chcusb_write(data, ref writeSize, ref rResult);
        }

        public int CHC_writeLaminate(byte* data, ref uint writeSize, ref ushort rResult) {
            Log.Write(Log.Args(writeSize));
            return chcusb_writeLaminate(data, ref writeSize, ref rResult);
        }

        public int CHC_writeHolo(byte* data, ref uint writeSize, ref ushort rResult) {
            Log.Write(Log.Args(writeSize));
            return chcusb_writeHolo(data, ref writeSize, ref rResult);
        }

        public int CHC_setPrinterInfo(PrinterInfoTag tagNumber, byte* rBuffer, ref uint rLen, ref ushort rResult) {
            Log.Write(Log.Args(tagNumber, rLen));
            return chcusb_setPrinterInfo(tagNumber, rBuffer, ref rLen, ref rResult);
        }

        public int CHC_getGamma(string filename, byte* r, byte* g, byte* b, ref ushort rResult) {
            Log.Write(Log.Args(filename));
            return chcusb_getGamma(filename, r, g, b, ref rResult);
        }

        public int CHC_getMtf(string filename, int* mtf, ref ushort rResult) {
            Log.Write(Log.Args(filename));
            return chcusb_getMtf(filename, mtf, ref rResult);
        }

        public int CHC_cancelCopies(ushort pageId, ref ushort rResult) {
            Log.Write(Log.Args(pageId));
            return chcusb_cancelCopies(pageId, ref rResult);
        }

        public int CHC_setPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult) {
            Log.Write(Log.Args(type, number, data));
            return chcusb_setPrinterToneCurve(type, number, ref data, ref rResult);
        }

        public int CHC_getPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult) {
            Log.Write(Log.Args(type, number, data));
            return chcusb_getPrinterToneCurve(type, number, ref data, ref rResult);
        }

        public int CHC_blinkLED(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_blinkLED(ref rResult);
        }

        public int CHC_resetPrinter(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_resetPrinter(ref rResult);
        }

        public int CHC_AttachThreadCount(ref ushort rCount, ref ushort rMaxCount) {
            Log.Write(Log.Args(rMaxCount));
            return chcusb_AttachThreadCount(ref rCount, ref rMaxCount);
        }

        public int CHC_getPrintIDStatus(ushort pageId, byte* rBuffer, ref ushort rResult) {
            Log.Write(Log.Args(pageId));
            return chcusb_getPrintIDStatus(pageId, rBuffer, ref rResult);
        }

        public int CHC_setPrintStandby(ushort position, ref ushort rResult) {
            Log.Write(Log.Args(position));
            return chcusb_setPrintStandby(position, ref rResult);
        }

        public int CHC_testCardFeed(ushort mode, ushort times, ref ushort rResult) {
            Log.Write(Log.Args(mode));
            return chcusb_testCardFeed(mode, times, ref rResult);
        }

        public int CHC_exitCard(ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_exitCard(ref rResult);
        }

        public int CHC_getCardRfidTID(byte* rCardTID, ref ushort rResult) {
            throw new NotImplementedException("CHC330");
        }

        public int CHC_commCardRfidReader(byte* sendData, byte* rRecvData, uint sendSize, ref uint rRecvSize, ref ushort rResult) {
            throw new NotImplementedException("CHC330");
        }

        public int CHC_updateCardRfidReader(byte* data, uint size, ref ushort rResult) {
            throw new NotImplementedException("CHC330");
        }

        public int CHC_getErrorLog(ushort index, byte* rData, ref ushort rResult) {
            Log.Write(Log.Args(index));
            return CHC_getErrorLog(index, rData, ref rResult);
        }

        public int CHC_getErrorStatus(ushort* rBuffer) {
            Log.Write(Log.Args());
            return chcusb_getErrorStatus(rBuffer);
        }

        public int CHC_setCutList(byte* rData, ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_setCutList(rData, ref rResult);
        }

        public int CHC_setLaminatePattern(ushort index, byte* rData, ref ushort rResult) {
            Log.Write(Log.Args(index));
            return chcusb_setLaminatePattern(index, rData, ref rResult);
        }

        public int CHC_color_adjustment(string filename, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult) {
            Log.Write(Log.Args(filename, a2, a3, a4, a5, a6, a7));
            return chcusb_color_adjustment(filename, a2, a3, a4, a5, a6, a7, ref rResult);
        }

        public int CHC_color_adjustmentEx(int a1, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult) {
            Log.Write(Log.Args(a1, a2, a3, a4, a5, a6, a7));
            return chcusb_color_adjustmentEx(a1, a2, a3, a4, a5, a6, a7, ref rResult);
        }

        public int CHC_getEEPROM(byte index, byte* rData, ref ushort rResult) {
            Log.Write(Log.Args(index));
            return chcusb_getEEPROM(index, rData, ref rResult);
        }

        public int CHC_setParameter(byte a1, uint a2, ref ushort rResult) {
            Log.Write(Log.Args(a1, a2));
            return chcusb_setParameter(a1, a2, ref rResult);
        }

        public int CHC_getParameter(byte a1, byte* a2, ref ushort rResult) {
            Log.Write(Log.Args(a1));
            return chcusb_getParameter(a1, a2, ref rResult);
        }

        public int CHC_universal_command(int a1, byte a2, int a3, byte* a4, ref ushort rResult) {
            Log.Write(Log.Args(a1, a2, a3));
            return chcusb_universal_command(a1, a2, a3, a4, ref rResult);
        }

        public int CHC_writeIred(byte* a1, byte* a2, ref ushort rResult) {
            Log.Write(Log.Args());
            return chcusb_writeIred(a2, a2, ref rResult);
        }
    }

}
