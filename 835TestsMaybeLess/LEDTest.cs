using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;
using System.Reflection.PortableExecutable;

namespace _835TestsMaybeLess {
    public class LEDTest {

        private LED_837_15093_06 led;

        [SetUp]
        public void Setup() {
            led = new LED_837_15093_06(9);
            led.serial.DumpRWCommandsToLog = true;
            led.serial.DumpBytesToLog = true;
        }

        [TearDown]
        public void Cleanup() {
            led?.Disconnect();
        }

        [Test]
        public void T01_TestLEDCommands() {
            Assert.That(led.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(led.GetBoardInfo(out string board_number, out string chip_number, out byte fv), Is.EqualTo(DeviceStatus.OK));
            Assert.That(board_number, Is.Not.Null);
            Assert.That(chip_number, Is.Not.Null);
            Assert.That(fv, Is.GreaterThan(0));
            Assert.That(led.GetFirmwareChecksum(out ushort checksum), Is.EqualTo(DeviceStatus.OK));
            Assert.That(checksum, Is.GreaterThan(0));
            Assert.That(led.SetTimeout(3000), Is.EqualTo(DeviceStatus.OK));
            Assert.That(led.SetResponseDisabled(true), Is.EqualTo(DeviceStatus.OK));
            for (int i = 0; i < 10; i++) {
                Assert.That(led.SetLEDs(new Color[] { Color.Red, Color.Green, Color.Blue, Color.White }), Is.EqualTo(DeviceStatus.OK));
            }
            Thread.Sleep(5000);
            Assert.That(led.SetLEDs(new Color[] { }), Is.EqualTo(DeviceStatus.OK));
            Assert.That(led.SetResponseDisabled(false), Is.EqualTo(DeviceStatus.OK));
            Assert.That(led.GetFirmwareChecksum(out checksum), Is.EqualTo(DeviceStatus.OK));
            Assert.That(checksum, Is.GreaterThan(0));
        }

    }
}