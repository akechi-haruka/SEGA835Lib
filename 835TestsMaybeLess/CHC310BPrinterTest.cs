using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;

namespace _835TestsMaybeLess;

public class Chc310BPrinterTest {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc310BPrinterTest));
    private Chc310BPrinter printer;

    [SetUp]
    public void Setup() {
        printer = new Chc310BPrinter();
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
    public void T03_TestImageConversion() {
        Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage310.jpg"));
        byte[] data = image.GetRawPixelsRgbNoPadding();
        LOG.LogInformation("pixels total = " + data.Length);

        Assert.That(data, Has.Length.EqualTo(printer.ImageDimensions.Width * printer.ImageDimensions.Height * 3));
    }

    [Test]
    public void T04_Print() {
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
        Assert.That(printer.StartPrinting(image, null, image2), Is.EqualTo(DeviceStatus.Ok));
        Assert.That((Func<DeviceStatus>)printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.Busy).After(300_000, 1000));
        Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.Ok));
    }
}