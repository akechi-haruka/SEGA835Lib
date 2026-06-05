using System.Drawing;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;
using Native = Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C320.Native;

namespace _835TestsMaybeLess;

public class Chc320PrinterTest {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Chc320PrinterTest));
    private Chc320Printer printer;
    private Y3 y3;
    private Chc320Printer printerWithY3;

    [SetUp]
    public void Setup() {
        printer = new Chc320Printer(null);
        y3 = new Y3(11);
        printerWithY3 = new Chc320Printer(y3);
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
        Bitmap image = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage320front.bmp"));
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
        printer.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C320-01.icc");
        printer.SetMtfFile("TestFiles/Printer/SmplMtf.txt");
        Bitmap front = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage320front.bmp"));
        Bitmap holo = null; //new Bitmap(Image.FromFile("TestFiles/Printer/TestHolo320.png"));
        Bitmap back = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage320back.bmp"));
        Bitmap ir = new Bitmap(Image.FromFile("TestFiles/Printer/TestInfrared320.bmp"));
        printer.ImageStretchMode = StretchMode.Center;
        Assert.That(printer.StartPrinting(front, null, holo, false, false, back, ir), Is.EqualTo(DeviceStatus.Ok));
        Assert.That((Func<DeviceStatus>)printer.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.Busy).After(300_000, 1000));
        Assert.That(printer.GetPrintJobResult(), Is.EqualTo(DeviceStatus.Ok));
    }


    [Test]
    public void T05_PrintWithPrinterCamera() {
        if (!Util.CheckConnect(y3.Connect)) {
            return;
        }

        if (!Util.CheckConnect(printerWithY3.Connect)) {
            return;
        }

        ushort rc = printerWithY3.GetPrinterStatusCode();
        LOG.LogInformation(ChcSeriesCardPrinter.RcToString(rc));
        Assert.That(rc, Is.Zero);

        Assert.That(y3.GetStatus(), Is.EqualTo(Y3.Status.Idle));
        Assert.That(y3.SetParamsForPrinter(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(y3.Start(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(y3.GetStatus(), Is.EqualTo(Y3.Status.Active));
        Assert.That(y3.DetectPrinterMarker(out bool detected), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(detected, Is.True);
        Assert.That(y3.GetStatus(), Is.EqualTo(Y3.Status.Active));

        printerWithY3.SetIccTables("TestFiles/Printer/sRGB_IEC61966-2-1_black_scaled.icc", "TestFiles/Printer/CHC-C320-01.icc");
        printerWithY3.SetMtfFile("TestFiles/Printer/SmplMtf.txt");
        Bitmap front = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage320front.bmp"));
        Bitmap holo = new Bitmap(Image.FromFile("TestFiles/Printer/TestHolo320.png"));
        Bitmap back = new Bitmap(Image.FromFile("TestFiles/Printer/TestImage320back.bmp"));
        Bitmap ir = new Bitmap(Image.FromFile("TestFiles/Printer/TestInfrared320.bmp"));

        Y3.CardInfo? detectedCard = null;

        printerWithY3.ImageStretchMode = StretchMode.Center;
        printerWithY3.CardDataRead += info => {
            LOG.LogInformation("Y3 detected card!");
            LOG.LogInformation("Detection:" + info.CardType);
            LOG.LogInformation("ID:" + info.ID);
            LOG.LogInformation("Type:" + info.UnknownType);
            LOG.LogInformation("IV:" + info.GetIvCode());
            LOG.LogInformation("Title Code:" + info.GetTitleCode());
            detectedCard = info;
        };

        Assert.That(printerWithY3.StartPrinting(front, null, holo, false, false, back, ir), Is.EqualTo(DeviceStatus.Ok));
        Assert.That((Func<DeviceStatus>)printerWithY3.GetPrintJobResult, Is.Not.EqualTo(DeviceStatus.Busy).After(300_000, 1000));
        Assert.That(printerWithY3.GetPrintJobResult(), Is.EqualTo(DeviceStatus.Ok));

        Assert.That(detectedCard, Is.Not.Null);
        Assert.That(detectedCard?.ID, Is.Not.Zero);
    }
}