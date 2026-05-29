using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;

namespace _835TestsMaybeLess;

class Io4Test {
    private Io4Usb15257 io4;

    [SetUp]
    public void Setup() {
        io4 = new Io4Usb15257();
    }

    [TearDown]
    public void Cleanup() {
        io4?.Disconnect();
    }

    [Test]
    public void T01_TestGetInfo() {
        if (!Util.CheckConnect(io4.Connect)) {
            return;
        }

        Assert.That(io4.GetProduct(out string product), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(product, Is.Not.Null);
        Assert.That(io4.GetManufacturer(out string manufacturer), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(manufacturer, Is.Not.Null);
    }

    [Test]
    public void T02_TestGPIO() {
        if (!Util.CheckConnect(io4.Connect)) {
            return;
        }

        for (int i = 0; i < 32; i++) {
            Assert.That(io4.SetGpio(i, true), Is.EqualTo(DeviceStatus.Ok));
            Thread.Sleep(250);
        }

        Assert.That(io4.ClearGpio(), Is.EqualTo(DeviceStatus.Ok));
    }

    [Test]
    public void T03_TestLEDs() {
        if (!Util.CheckConnect(io4.Connect)) {
            return;
        }

        for (int i = 0; i < 32; i++) {
            Assert.That(io4.SetLed(i, byte.MaxValue), Is.EqualTo(DeviceStatus.Ok));
            Thread.Sleep(250);
        }

        Assert.That(io4.ClearLed(), Is.EqualTo(DeviceStatus.Ok));
    }
}