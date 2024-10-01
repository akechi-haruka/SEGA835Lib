using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Serial;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _835TestsMaybeLess {

    internal class IO4Test {

        private IO4USB_835_15257_01 io4;

        [SetUp]
        public void Setup() {
            io4 = new IO4USB_835_15257_01();
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
            Assert.That(io4.GetProduct(out string product), Is.EqualTo(DeviceStatus.OK));
            Assert.That(product, Is.Not.Null);
            Assert.That(io4.GetManufacturer(out string manufacturer), Is.EqualTo(DeviceStatus.OK));
            Assert.That(manufacturer, Is.Not.Null);
        }

        [Test]
        public void T02_TestGPIO() {
            if (!Util.CheckConnect(io4.Connect)) {
                return;
            }
            for (int i = 0; i < 32; i++) {
                Assert.That(io4.SetGPIO(i, true, true), Is.EqualTo(DeviceStatus.OK));
                Thread.Sleep(250);
            }
            Assert.That(io4.ClearGPIO(), Is.EqualTo(DeviceStatus.OK));
        }

        [Test]
        public void T03_TestLEDs() {
            if (!Util.CheckConnect(io4.Connect)) {
                return;
            }
            for (int i = 0; i < 32; i++) {
                Assert.That(io4.SetLED(i, byte.MaxValue, true), Is.EqualTo(DeviceStatus.OK));
                Thread.Sleep(250);
            }
            Assert.That(io4.ClearLED(), Is.EqualTo(DeviceStatus.OK));
        }
    }
}
