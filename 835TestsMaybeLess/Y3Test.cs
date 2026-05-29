using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;

namespace _835TestsMaybeLess;

public class Y3Test {
    private Y3 y3;

    [SetUp]
    public void Setup() {
        y3 = new Y3(11);
    }

    [TearDown]
    public void Cleanup() {
        y3?.Disconnect();
    }

    [Test]
    public void T01_TestPrinterCamera() {
        if (!Util.CheckConnect(y3.Connect)) {
            return;
        }

        Assert.That(y3.GetStatus(), Is.EqualTo(Y3.Status.Idle));
        Assert.That(y3.SetParamsForPrinter(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(y3.DetectPrinterMarker(out bool detected), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(detected, Is.True);
    }
}