using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;

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
            if (!Util.CheckConnect(vfd.Connect)) {
                return;
            }

            Assert.That(vfd.GetVersion(out string version), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version, Is.Not.Null);
            Log.Write(version);
        }

        [Test]
        public void T02_TestWritingText() {
            if (!Util.CheckConnect(vfd.Connect)) {
                return;
            }

            Assert.That(vfd.SetEncoding(VFDEncoding.SHIFT_JIS), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetOn(true), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetScrollWindowPosition(0, 0, 120), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.WriteScrollingText("835Tests are running"), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetTextDrawing(true), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetBrightness(VFDBrightnessLevel.LEVEL2), Is.EqualTo(DeviceStatus.OK));
        }

        [Test]
        public void T03_TestDoubleText() {
            if (!Util.CheckConnect(vfd.Connect)) {
                return;
            }

            Assert.That(vfd.SetEncoding(VFDEncoding.SHIFT_JIS), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetOn(true), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetBrightness(VFDBrightnessLevel.LEVEL2), Is.EqualTo(DeviceStatus.OK));
            Assert.That(vfd.SetText("LINE 1 IS NOT SCROLL", "LINE 2 IS SCROLL", false, true), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(2000);
            Assert.That(vfd.SetText("LINE 1 IS SCROLL", "LINE 2 IS NOT SCROLL", true, false), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(2000);
            Assert.That(vfd.SetText("THAT'S A LOT OF SCROLLING", "SCROLLING SCROLLING SCROLLING SCROLLING", true, true), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(2000);
            Assert.That(vfd.SetText("NO MORE", "STOP", false, false), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(2000);
        }
    }
}