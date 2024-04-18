using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC330;
using Haruka.Arcade.SEGA835Lib.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _835TestsMaybeLess {
    public class CHC330PrinterTest {

        private CHC330_Printer printer;

        [SetUp]
        public void Setup() {
            printer = new CHC330_Printer(null);
        }

        [TearDown]
        public void Cleanup() {
            printer?.Disconnect();
        }

        [Test]
        public void T01_TestPrinterDllLoad() {
            Log.Write("CWD is " + Environment.CurrentDirectory);
            if (!File.Exists(Native.DLL)) {
                Assert.Warn("DLL not found in CWD!");
            }
            printer.Disconnect();
        }

        [Test]
        public void T02_TestGetPrinterSerial() {
            Assert.That(printer.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(printer.GetPrinterSerial(out string serial), Is.EqualTo(DeviceStatus.OK));
            Assert.That(serial, Is.Not.Null);
            Log.Write(serial);
        }

    }
}
