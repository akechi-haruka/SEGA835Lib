using System.Text;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Microsoft.Extensions.Logging;

namespace _835TestsMaybeLess;

public class VfdTest {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(VfdTest));
    private VfdGp1232A02A vfd;

    [SetUp]
    public void Setup() {
        vfd = new VfdGp1232A02A();
    }

    [TearDown]
    public void Cleanup() {
        vfd.Disconnect();
    }

    [Test]
    public void T01_TestEncoding() {
        Encoding.GetEncoding("shift_jis");
    }

    [Test]
    public void T02_TestVersion() {
        if (!Util.CheckConnect(vfd.Connect)) {
            return;
        }

        Assert.That(vfd.GetVersion(out string version), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(version, Is.Not.Null);
        LOG.LogInformation(version);
    }

    [Test]
    public void T03_TestWritingText() {
        if (!Util.CheckConnect(vfd.Connect)) {
            return;
        }

        Assert.That(vfd.SetEncoding(VfdEncoding.ShiftJis), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetOn(true), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetScrollWindowPosition(0, 0, 120), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.WriteScrollingText("835Tests are running"), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetTextDrawing(true), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetBrightness(VfdBrightnessLevel.Level2), Is.EqualTo(DeviceStatus.Ok));
    }

    [Test]
    public void T04_TestDoubleText() {
        if (!Util.CheckConnect(vfd.Connect)) {
            return;
        }

        Assert.That(vfd.SetEncoding(VfdEncoding.ShiftJis), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetOn(true), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetBrightness(VfdBrightnessLevel.Level2), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(vfd.SetText("LINE 1 IS NOT SCROLL", "LINE 2 IS SCROLL", false, true), Is.EqualTo(DeviceStatus.Ok));
        Thread.Sleep(2000);
        Assert.That(vfd.SetText("LINE 1 IS SCROLL", "LINE 2 IS NOT SCROLL", true), Is.EqualTo(DeviceStatus.Ok));
        Thread.Sleep(2000);
        Assert.That(vfd.SetText("THAT'S A LOT OF SCROLLING", "SCROLLING SCROLLING SCROLLING SCROLLING", true, true), Is.EqualTo(DeviceStatus.Ok));
        Thread.Sleep(2000);
        Assert.That(vfd.SetText("NO MORE", "STOP"), Is.EqualTo(DeviceStatus.Ok));
        Thread.Sleep(2000);
    }
}