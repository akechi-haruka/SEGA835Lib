using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;

namespace _835TestsMaybeLess;

public class LedTest {
    private Led15093 led;

    [SetUp]
    public void Setup() {
        led = new Led15093(9);
        led.Serial.DumpReadWriteCommandsToLog = true;
        led.Serial.DumpBytesToLog = true;
    }

    [TearDown]
    public void Cleanup() {
        led?.Disconnect();
    }

    [Test]
    public void T01_TestLEDCommands() {
        if (!Util.CheckConnect(led.Connect)) {
            return;
        }

        Assert.That(led.GetBoardInfo(out string boardNumber, out string chipNumber, out byte fv), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(boardNumber, Is.Not.Null);
        Assert.That(chipNumber, Is.Not.Null);
        Assert.That(fv, Is.GreaterThan(0));
        Assert.That(led.GetFirmwareChecksum(out ushort checksum), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(checksum, Is.GreaterThan(0));
        Assert.That(led.SetTimeout(3000), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(led.SetResponseDisabled(true), Is.EqualTo(DeviceStatus.Ok));
        for (int i = 0; i < 10; i++) {
            Assert.That(led.SetLeds(new Color[] { Color.Red, Color.Green, Color.Blue, Color.White }), Is.EqualTo(DeviceStatus.Ok));
        }

        Thread.Sleep(5000);
        Assert.That(led.SetLeds(Array.Empty<Color>()), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(led.SetResponseDisabled(false), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(led.GetFirmwareChecksum(out checksum), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(checksum, Is.GreaterThan(0));
    }
}