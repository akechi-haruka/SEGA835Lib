using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;

namespace _835TestsMaybeLess;

public class Chc310PrinterTest {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc310PrinterTest));
    private Chc310Printer printer;

    [SetUp]
    public void Setup() {
        printer = new Chc310Printer();
    }

    [TearDown]
    public void Cleanup() {
        printer?.Disconnect();
    }

    [Test]
    public void T01_TestPrinterDllLoad() {
        LOG.LogInformation("CWD is " + Environment.CurrentDirectory);
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

        Assert.That(printer.GetPrinterSerial(out string serial), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(serial, Is.Not.Null);
        LOG.LogInformation(serial);
    }

    [Test]
    public void T03_TestRFIDBoardGetInfo() {
        if (!Util.CheckConnect(printer.Connect)) {
            return;
        }

        Assert.That(printer.GetRfidBootVersion(out byte version), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(version, Is.Not.Zero);
        LOG.LogInformation("Boot: " + version);
        Assert.That(printer.GetRfidAppVersion(out byte version2), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(version2, Is.Not.Zero);
        LOG.LogInformation("App: " + version2);
        Assert.That(printer.GetRfidBoardInfo(out string board), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(board, Is.Not.Null);
        LOG.LogInformation("Board: " + board);
    }

    [Test]
    public void T04_TestImageConversion() {
        Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage310.jpg"));
        byte[] data = image.GetRawPixelsRgbNoPadding();
        LOG.LogInformation("pixels total = " + data.Length);

        Assert.That(data, Has.Length.EqualTo(printer.ImageDimensions.Width * printer.ImageDimensions.Height * 3));
    }

    [Test]
    public void T05_RFID() {
        if (!Util.CheckConnect(printer.Connect)) {
            return;
        }

        ushort rc = printer.GetPrinterStatusCode();
        LOG.LogInformation(ChcSeriesCardPrinter.RcToString(rc));
        Assert.That(rc, Is.Zero);
        Assert.That(printer.WriteRfid(ref rc, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x72, 0x50, 0x5C, 0x70, 0x05, 0x52, 0x05, 0xCD, 0x61, 0x16, 0x62, 0xD0, 0xD6, 0x12, 0xC4, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, false, out byte[] writtenCardId), Is.EqualTo(DeviceStatus.Ok));
        LOG.LogInformation(Hex.Dump(writtenCardId));
        Assert.That(writtenCardId, Is.Not.Null);
        Assert.That(writtenCardId, Has.Length.EqualTo(ChcSeriesCardPrinter.CARD_ID_LEN));
    }

    [Test]
    public void T06_Print() {
        if (!Util.CheckConnect(printer.Connect)) {
            return;
        }

        ushort rc = printer.GetPrinterStatusCode();
        LOG.LogInformation(ChcSeriesCardPrinter.RcToString(rc));
        Assert.That(rc, Is.Zero);
        printer.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C310-01.icc");
        printer.SetMtfFile("TestFiles/Printer/MTF220.txt");
        Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage310.jpg"));
        Bitmap image2 = new Bitmap(Image.FromFile("TestFiles/Printer/TestHolo310.png"));
        Assert.That(printer.StartPrinting(image, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x72, 0x50, 0x5C, 0x70, 0x05, 0x52, 0x05, 0xCD, 0x61, 0x16, 0x62, 0xD0, 0xD6, 0x12, 0xC4, 0xAF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, image2), Is.EqualTo(DeviceStatus.Ok));
        Assert.That((Func<DeviceStatus>)printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.Busy).After(300_000, 1000));
        Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(printer.GetWrittenRfidCardId(out byte[] cardid), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(cardid, Is.Not.Null);
        Assert.That(cardid, Has.Length.EqualTo(ChcSeriesCardPrinter.CARD_ID_LEN));
    }
}