using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
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

    [Test]
    public void T04_TestProductParsing() {
        String product = "I/O CONTROL BD;15257;01;90;1831;6679A;00;GOUT=14_ADIN=8,E_ROTIN=4_COININ=2_SWIN=2,E_UQ1=41,6";

        Assert.That(JvsCapabilities.TryParse(product, out JvsCapabilities capabilities), Is.True);
        Assert.That(capabilities, Is.Not.Null);

        Assert.That(capabilities.Type, Is.EqualTo("I/O CONTROL BD"));
        Assert.That(capabilities.Outputs, Is.EqualTo(0x14));
        Assert.That(capabilities.AnalogInputs.Length, Is.EqualTo(8));
        Assert.That(capabilities.AnalogInputs[0], Is.EqualTo(0xE));
        Assert.That(capabilities.AnalogInputs[7], Is.EqualTo(0xE));
        Assert.That(capabilities.RotaryInputs, Is.EqualTo(4));
        Assert.That(capabilities.Chutes, Is.EqualTo(2));
        Assert.That(capabilities.SwitchInputs.Length, Is.EqualTo(2));
        Assert.That(capabilities.SwitchInputs[0], Is.EqualTo(0xE));
        Assert.That(capabilities.SwitchInputs[1], Is.EqualTo(0xE));
        Assert.That(capabilities.UniqueFunctions.Length, Is.EqualTo(1));
        Assert.That(capabilities.UniqueFunctions[0].Id, Is.EqualTo(0x41));
        Assert.That(capabilities.UniqueFunctions[0].Value, Is.EqualTo(6));
    }
}