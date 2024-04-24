using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using System.Reflection.PortableExecutable;
using Haruka.Arcade.SEGA835Lib.Misc;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;

namespace _835TestsMaybeLess {
    public class CHC310PrinterTest {

        private CHC310Printer printer;

        [SetUp]
        public void Setup() {
            printer = new CHC310Printer();
        }

        [TearDown]
        public void Cleanup() {
            printer?.Disconnect();
        }

        [Test]
        public void T01_TestPrinterDllLoad() {
            Log.Write("CWD is " + Environment.CurrentDirectory);
            if (!File.Exists(Native.DLL)) {
                Assert.Warn("DLL not found in CWD!");
            }
            printer.Disconnect();
        }

        [Test]
        public void T02_TestGetPrinterSerial() {
            Assert.That(printer.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetPrinterSerial(out string serial), Is.EqualTo(DeviceStatus.OK));
            Assert.That(serial, Is.Not.Null);
            Log.Write(serial);
        }

        [Test]
        public void T03_TestRFIDBoardGetInfo() {
            Assert.That(printer.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetRFIDBootVersion(out byte version), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version, Is.Not.EqualTo(0));
            Log.Write("Boot: " + version);
            Assert.That(printer.GetRFIDAppVersion(out byte version2), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version2, Is.Not.EqualTo(0));
            Log.Write("App: " + version2);
            Assert.That(printer.GetRFIDBoardInfo(out string board), Is.EqualTo(DeviceStatus.OK));
            Assert.That(board, Is.Not.Null);
            Log.Write("Board: " + board);
        }

        [Test]
        public void T04_TestImageConversion() {
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage310.jpg"));
            byte[] data = image.GetRawPixelsRGBNoPadding();
            Log.Write("pixels total = " + data.Length);
            int count_black = 0;
            foreach (byte b in data) {
                count_black++;
            }

            Assert.That(data, Has.Length.EqualTo(printer.ImageDimensions.Width * printer.ImageDimensions.Height * 3));
            Assert.That(count_black, Is.GreaterThan(200_000), constraintExpression: "not enough black pixels found, conversion may be wrong");
        }

        [Test]
        public void T05_Print() {
            Assert.That(printer.Connect(), Is.EqualTo(DeviceStatus.OK));
            ushort rc = printer.GetPrinterStatusCode();
            Log.Write(CHCSeriesCardPrinter.RCToString(rc));
            Assert.That(rc, Is.EqualTo(0));
            printer.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C310-01.icc");
            printer.SetMtfFile("TestFiles/Printer/MTF220.txt");
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage310.jpg"));
            Bitmap image2 = new Bitmap(Image.FromFile("TestFiles/Printer/TestHolo310.png"));
            Assert.That(printer.StartPrinting(image, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x72, 0x50, 0x5C, 0x70, 0x05, 0x52, 0x05, 0xCD, 0x61, 0x16, 0x62, 0xD0, 0xD6, 0x12, 0xC4, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, image2), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.BUSY).After(300_000, 1000));
            Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetWrittenRFIDCardId(out byte[] cardid), Is.EqualTo(DeviceStatus.OK));
            Assert.That(cardid, Is.Not.Null);
            Assert.That(cardid, Has.Length.EqualTo(CHCSeriesCardPrinter.CARD_ID_LEN));
        }

    }
}
