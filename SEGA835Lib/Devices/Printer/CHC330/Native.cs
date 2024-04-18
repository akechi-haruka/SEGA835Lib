using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC330 {

    public unsafe partial class Native {

        public const String DLL = "C330Ausb.dll";

        /// Return Type: int
        ///maxCount: unsigned short
        [LibraryImport(DLL, EntryPoint = "chcusb_MakeThread")]
        internal static partial int chcusb_MakeThread(ushort maxCount);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_open")]
        internal static partial int chcusb_open(ref ushort rResult);


        /// Return Type: void
        [LibraryImport(DLL, EntryPoint = "chcusb_close")]
        internal static partial void chcusb_close();


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_ReleaseThread")]
        internal static partial int chcusb_ReleaseThread(ref ushort rResult);


        /// Return Type: int
        ///rIdArray: unsigned byte*
        [LibraryImport(DLL, EntryPoint = "chcusb_listupPrinter")]
        internal static partial int chcusb_listupPrinter(byte* rIdArray);


        /// Return Type: int
        ///rSerialArray: unsigned int*
        [LibraryImport(DLL, EntryPoint = "chcusb_listupPrinterSN")]
        internal static partial int chcusb_listupPrinterSN(ulong* rSerialArray);


        /// Return Type: int
        ///printerId: unsigned byte
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_selectPrinter")]
        internal static partial int chcusb_selectPrinter(byte printerId, ref ushort rResult);


        /// Return Type: int
        ///printerSN: unsigned int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_selectPrinterSN")]
        internal static partial int chcusb_selectPrinterSN(ulong printerSN, ref ushort rResult);


        /// Return Type: int
        ///tagNumber: unsigned short
        ///rBuffer: unsigned byte*
        ///rLen: unsigned int*
        [LibraryImport(DLL, EntryPoint = "chcusb_getPrinterInfo")]
        internal static partial int chcusb_getPrinterInfo(ushort tagNumber, byte* rBuffer, ref uint rLen);


        /// Return Type: int
        ///format: unsigned short
        ///ncomp: unsigned short
        ///depth: unsigned short
        ///width: unsigned short
        ///height: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_imageformat")]
        internal static partial int chcusb_imageformat(ushort format, ushort ncomp, ushort depth, ushort width, ushort height, ref ushort rResult);


        /// Return Type: int
        ///mtf: int*
        [LibraryImport(DLL, EntryPoint = "chcusb_setmtf")]
        internal static partial int chcusb_setmtf(ref int mtf);


        /// Return Type: int
        ///k: unsigned short
        ///intoneR: unsigned byte*
        ///intoneG: unsigned byte*
        ///intoneB: unsigned byte*
        [LibraryImport(DLL, EntryPoint = "chcusb_makeGamma")]
        internal static partial int chcusb_makeGamma(ushort k, ref byte intoneR, ref byte intoneG, ref byte intoneB);


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
        internal static partial int chcusb_setIcctable([MarshalAs(UnmanagedType.LPStr)] string icc1, [MarshalAs(UnmanagedType.LPStr)] string icc2, ushort intents, ref byte intoneR, ref byte intoneG, ref byte intoneB, ref byte outtoneR, ref byte outtoneG, ref byte outtoneB, ref ushort rResult);


        /// Return Type: int
        ///copies: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_copies")]
        internal static partial int chcusb_copies(ushort copies, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_status")]
        internal static partial int chcusb_status(ref ushort rResult);


        /// Return Type: int
        ///idArray: unsigned byte*
        ///rResultArray: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_statusAll")]
        internal static partial int chcusb_statusAll(ref byte idArray, ref ushort rResultArray);


        /// Return Type: int
        ///postCardState: unsigned short
        ///pageId: unsigned short*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_startpage")]
        internal static partial int chcusb_startpage(ushort postCardState, ref ushort pageId, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_endpage")]
        internal static partial int chcusb_endpage(ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///writeSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_write")]
        internal static partial int chcusb_write(ref byte data, ref uint writeSize, ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///writeSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_writeLaminate")]
        internal static partial int chcusb_writeLaminate(ref byte data, ref uint writeSize, ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///writeSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_writeHolo")]
        internal static partial int chcusb_writeHolo(ref byte data, ref uint writeSize, ref ushort rResult);


        /// Return Type: int
        ///tagNumber: unsigned short
        ///rBuffer: unsigned byte*
        ///rLen: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setPrinterInfo")]
        internal static partial int chcusb_setPrinterInfo(ushort tagNumber, ref byte rBuffer, ref uint rLen, ref ushort rResult);


        /// Return Type: int
        ///filename: LPCSTR->CHAR*
        ///r: unsigned byte*
        ///g: unsigned byte*
        ///b: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getGamma")]
        internal static partial int chcusb_getGamma([MarshalAs(UnmanagedType.LPStr)] string filename, ref byte r, ref byte g, ref byte b, ref ushort rResult);


        /// Return Type: int
        ///filename: LPCSTR->CHAR*
        ///mtf: int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getMtf")]
        internal static partial int chcusb_getMtf([MarshalAs(UnmanagedType.LPStr)] string filename, ref int mtf, ref ushort rResult);


        /// Return Type: int
        ///pageId: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_cancelCopies")]
        internal static partial int chcusb_cancelCopies(ushort pageId, ref ushort rResult);


        /// Return Type: int
        ///type: unsigned short
        ///number: unsigned short
        ///data: unsigned short*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setPrinterToneCurve")]
        internal static partial int chcusb_setPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);


        /// Return Type: int
        ///type: unsigned short
        ///number: unsigned short
        ///data: unsigned short*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getPrinterToneCurve")]
        internal static partial int chcusb_getPrinterToneCurve(ushort type, ushort number, ref ushort data, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_blinkLED")]
        internal static partial int chcusb_blinkLED(ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_resetPrinter")]
        internal static partial int chcusb_resetPrinter(ref ushort rResult);


        /// Return Type: int
        ///rCount: unsigned short*
        ///rMaxCount: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_AttachThreadCount")]
        internal static partial int chcusb_AttachThreadCount(ref ushort rCount, ref ushort rMaxCount);


        /// Return Type: int
        ///pageId: unsigned short
        ///rBuffer: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getPrintIDStatus")]
        internal static partial int chcusb_getPrintIDStatus(ushort pageId, ref byte rBuffer, ref ushort rResult);


        /// Return Type: int
        ///position: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setPrintStandby")]
        internal static partial int chcusb_setPrintStandby(ushort position, ref ushort rResult);


        /// Return Type: int
        ///mode: unsigned short
        ///times: unsigned short
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_testCardFeed")]
        internal static partial int chcusb_testCardFeed(ushort mode, ushort times, ref ushort rResult);


        /// Return Type: int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_exitCard")]
        internal static partial int chcusb_exitCard(ref ushort rResult);


        /// Return Type: int
        ///rCardTID: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getCardRfidTID")]
        internal static partial int chcusb_getCardRfidTID(ref byte rCardTID, ref ushort rResult);


        /// Return Type: int
        ///sendData: unsigned byte*
        ///rRecvData: unsigned byte*
        ///sendSize: unsigned int
        ///rRecvSize: unsigned int*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_commCardRfidReader")]
        internal static partial int chcusb_commCardRfidReader(ref byte sendData, ref byte rRecvData, uint sendSize, ref uint rRecvSize, ref ushort rResult);


        /// Return Type: int
        ///data: unsigned byte*
        ///size: unsigned int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_updateCardRfidReader")]
        internal static partial int chcusb_updateCardRfidReader(ref byte data, uint size, ref ushort rResult);


        /// Return Type: int
        ///index: unsigned short
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getErrorLog")]
        internal static partial int chcusb_getErrorLog(ushort index, ref byte rData, ref ushort rResult);


        /// Return Type: int
        ///rBuffer: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getErrorStatus")]
        internal static partial int chcusb_getErrorStatus(ref ushort rBuffer);


        /// Return Type: int
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setCutList")]
        internal static partial int chcusb_setCutList(ref byte rData, ref ushort rResult);


        /// Return Type: int
        ///index: unsigned short
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setLaminatePattern")]
        internal static partial int chcusb_setLaminatePattern(ushort index, ref byte rData, ref ushort rResult);


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
        internal static partial int chcusb_color_adjustment([MarshalAs(UnmanagedType.LPStr)] string filename, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);


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
        internal static partial int chcusb_color_adjustmentEx(int a1, int a2, int a3, short a4, short a5, int a6, int a7, ref ushort rResult);


        /// Return Type: int
        ///index: unsigned byte
        ///rData: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getEEPROM")]
        internal static partial int chcusb_getEEPROM(byte index, ref byte rData, ref ushort rResult);


        /// Return Type: int
        ///a1: unsigned byte
        ///a2: unsigned int
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_setParameter")]
        internal static partial int chcusb_setParameter(byte a1, uint a2, ref ushort rResult);


        /// Return Type: int
        ///a1: unsigned byte
        ///a2: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_getParameter")]
        internal static partial int chcusb_getParameter(byte a1, ref byte a2, ref ushort rResult);


        /// Return Type: int
        ///a1: int
        ///a2: unsigned byte
        ///a3: int
        ///a4: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_universal_command")]
        internal static partial int chcusb_universal_command(int a1, byte a2, int a3, ref byte a4, ref ushort rResult);


        /// Return Type: int
        ///a1: unsigned byte*
        ///a2: unsigned byte*
        ///rResult: unsigned short*
        [LibraryImport(DLL, EntryPoint = "chcusb_writeIred")]
        internal static partial int chcusb_writeIred(ref byte a1, ref byte a2, ref ushort rResult);

    }

}
