using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Misc;
using System.Drawing;
using System.Drawing.Imaging;

namespace _835TestsMaybeLess {
    public class CHC330PrinterTest {
        private CHC330Printer printer;

        [SetUp]
        public void Setup() {
            printer = new CHC330Printer(new RFIDRWPrinter_837_15347(4));
        }

        [TearDown]
        public void Cleanup() {
            printer?.Disconnect();
        }

        [Test]
        public void T01_TestPrinterDllLoad() {
            Log.Write("CWD is " + Environment.CurrentDirectory);
            if (!File.Exists(Native.DLL)) {
                Assert.Inconclusive("DLL not found in CWD!");
            }

            printer.Disconnect();
        }

        [Test]
        public void T02_TestGetPrinterSerial() {
            if (!Util.CheckConnect(printer.Connect)) {
                return;
            }

            Assert.That(printer.GetPrinterSerial(out string serial), Is.EqualTo(DeviceStatus.OK));
            Assert.That(serial, Is.Not.Null);
            Log.Write(serial);
        }

        [Test]
        public void T03_TestRFIDBoardGetInfo() {
            if (!Util.CheckConnect(printer.Connect)) {
                return;
            }

            RFIDRWPrinter_837_15347 rfid = printer.GetRFIDBoard();
            Assert.That(rfid, Is.Not.Null);
            Assert.That(rfid.GetBootVersion(out byte version), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version, Is.Not.EqualTo(0));
            Log.Write("Boot: " + version);
            Assert.That(rfid.GetAppVersion(out byte version2), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version2, Is.Not.EqualTo(0));
            Log.Write("App: " + version2);
            Assert.That(rfid.GetBoardInfo(out string board), Is.EqualTo(DeviceStatus.OK));
            Assert.That(board, Is.Not.Null);
            Log.Write("Board: " + board);
        }

        [Test]
        public void T04_TestImageConversion() {
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage330.jpg"));
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
        public void T05_TestImageConversionOversizedCenter() {
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestOversized330.jpg"));
            image = image.CopyCentered(printer.ImageDimensions);
            byte[] data = image.GetRawPixelsRGBNoPadding();
            Log.Write("pixels total = " + data.Length);

            Assert.That(data, Has.Length.EqualTo(printer.ImageDimensions.Width * printer.ImageDimensions.Height * 3));
        }

        [Test]
        public void T06_TestImageConversionOversizedStretch() {
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestOversized330.jpg"));
            image = image.CopyStretched(printer.ImageDimensions);
            byte[] data = image.GetRawPixelsRGBNoPadding();
            Log.Write("pixels total = " + data.Length);

            Assert.That(data, Has.Length.EqualTo(printer.ImageDimensions.Width * printer.ImageDimensions.Height * 3));
        }

        [Test]
        public void T07_TestImageConversionHolo() {
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestHolo330.png"));
            byte[] data = image.GetRawPixelsMonochrome();
            Log.Write("pixels total = " + data.Length);
            int count_black = 0;
            foreach (byte b in data) {
                count_black++;
            }

            Assert.That(data, Has.Length.EqualTo(printer.ImageDimensions.Width * printer.ImageDimensions.Height));
            Assert.That(count_black, Is.GreaterThan(80_000), constraintExpression: "not enough black pixels found, conversion may be wrong");
        }

        [Test]
        public void T08_Print() {
            if (!Util.CheckConnect(printer.Connect)) {
                return;
            }

            ushort rc = printer.GetPrinterStatusCode();
            Log.Write(CHCSeriesCardPrinter.RCToString(rc));
            Assert.That(rc, Is.EqualTo(0));
            printer.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C330-01.icc");
            printer.SetMtfFile("TestFiles/Printer/mtf140.txt");
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage330.jpg"));
            Bitmap image2 = new Bitmap(Image.FromFile("TestFiles/Printer/TestHolo330.png"));
            Assert.That(printer.StartPrinting(image, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x72, 0x50, 0x5C, 0x70, 0x05, 0x52, 0x05, 0xCD, 0x61, 0x16, 0x62, 0xD0, 0xD6, 0x12, 0xC4, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, image2), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.BUSY).After(300_000, 1000));
            Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetWrittenRFIDCardId(out byte[] cardid), Is.EqualTo(DeviceStatus.OK));
            Assert.That(cardid, Is.Not.Null);
            Assert.That(cardid, Has.Length.EqualTo(CHCSeriesCardPrinter.CARD_ID_LEN));
        }

        [Test]
        public void T09_PrintOversizedNoHolo() {
            if (!Util.CheckConnect(printer.Connect)) {
                return;
            }

            ushort rc = printer.GetPrinterStatusCode();
            Log.Write(CHCSeriesCardPrinter.RCToString(rc));
            Assert.That(rc, Is.EqualTo(0));
            printer.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C330-01.icc");
            printer.SetMtfFile("TestFiles/Printer/mtf140.txt");
            printer.ImageStretchMode = StretchMode.Stretch;
            Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestOversized330.jpg"));
            Assert.That(printer.StartPrinting(image, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x72, 0x50, 0x5C, 0x70, 0x05, 0x52, 0x05, 0xCD, 0x61, 0x16, 0x62, 0xD0, 0xD6, 0x12, 0xC4, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.BUSY).After(300_000, 1000));
            Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetWrittenRFIDCardId(out byte[] cardid), Is.EqualTo(DeviceStatus.OK));
            Assert.That(cardid, Is.Not.Null);
            Assert.That(cardid, Has.Length.EqualTo(CHCSeriesCardPrinter.CARD_ID_LEN));
        }

        [Test]
        public void T10_SAOConvertAndPrint() {
            Bitmap img = new Bitmap(Image.FromFile("TestFiles/Printer/TestImageSAO.png"));
            img.RotateFlip(RotateFlipType.Rotate180FlipX);
            img.Save("TestFiles/Printer/TestImageSAO_c.png", ImageFormat.Png);
            Bitmap holo = new Bitmap(Image.FromFile("TestFiles/Printer/TestHoloSAO.png"));
            holo.RotateFlip(RotateFlipType.RotateNoneFlipX);
            /*for (int w = 0; w < holo.Width; w++) {
                for (int h = 0; h < holo.Height; h++) {
                    System.Drawing.Color p = holo.GetPixel(w, h);
                    if (p.R != 0 || p.G != 0 || p.B != 0) {
                        holo.SetPixel(w, h, System.Drawing.Color.White);
                    }
                }
            }*/
            holo.Save("TestFiles/Printer/TestHoloSAO_c.png", ImageFormat.Png);
            printer.InitTimeout = 5_000;
            if (!Util.CheckConnect(printer.Connect)) {
                return;
            }

            ushort rc = printer.GetPrinterStatusCode();
            Log.Write(CHCSeriesCardPrinter.RCToString(rc));
            Assert.That(rc, Is.EqualTo(0));
            printer.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C330-01.icc");
            printer.SetMtfFile("TestFiles/Printer/mtf140.txt");
            printer.ImageStretchMode = StretchMode.Stretch;
            Assert.That(printer.StartPrinting(img, null, holo), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.BUSY).After(300_000, 1000));
            Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.OK));
        }
    }
}