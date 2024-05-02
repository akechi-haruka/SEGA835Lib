using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using System.Reflection.PortableExecutable;

namespace _835TestsMaybeLess {
    public class VFDTest {

        private VFD_GP1232A02A vfd;

        [SetUp]
        public void Setup() {
            vfd = new VFD_GP1232A02A();
        }

        [TearDown]
        public void Cleanup() {
            vfd.Disconnect();
        }

        [Test]
        public void T01_TestVersion() {
            Assert.That(vfd.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.GetVersion(out string version), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version, Is.Not.Null);
            Log.Write(version);
        }

        [Test]
        public void T02_TestWritingText() {
            Assert.That(vfd.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetEncoding(VFDEncoding.SHIFT_JIS), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetOn(true), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetTextPosition(20, 8, 120), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.WriteScrollingText("835Tests are running"), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetTextScroll(true), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetBrightness(VFDBrightnessLevel.LEVEL2), Is.EqualTo(DeviceStatus.OK));
        }

    }
}