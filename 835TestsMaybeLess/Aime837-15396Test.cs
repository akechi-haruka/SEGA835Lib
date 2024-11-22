using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card;
using Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _835TestsMaybeLess {

    internal class Aime837_15396Test {

        private AimeCardReader_837_15396 reader;

        [SetUp]
        public void Setup() {
            reader = new AimeCardReader_837_15396(3, true);
            reader.serial.DumpRWCommandsToLog = true;
            reader.serial.DumpBytesToLog = true;
        }

        [TearDown]
        public void Cleanup() {
            reader?.Disconnect();
        }

        [Test]
        public void T01_TestGetInfo() {
            if (!Util.CheckConnect(reader.Connect)) {
                return;
            }
            Assert.That(reader.GetHWVersion(out string version), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version, Is.Not.Null);
            Assert.That(reader.GetFWVersion(out string version2, out byte version3), Is.EqualTo(DeviceStatus.OK));
            if (version3 == 0) {
                Assert.That(version2, Is.Not.Null);
            }
            Assert.That(reader.GetFWChecksum(out ushort checksum), Is.EqualTo(DeviceStatus.OK));
            Assert.That(checksum, Is.GreaterThan(0));
        }

        [Test]
        public void T02_TestOfflineRead() {
            if (!Util.CheckConnect(reader.Connect)) {
                return;
            }
            Assert.That(reader.RadioOn(RadioOnType.Both), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.StartPolling(), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(100);
            Assert.That(reader.StopPolling(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.RadioOff(), Is.EqualTo(DeviceStatus.OK));
        }

        [Test]
        public void T03_TestDisco() {
            if (!Util.CheckConnect(reader.Connect)) {
                return;
            }
            Assert.That(reader.LEDReset(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.LEDGetInfo(out string info), Is.EqualTo(DeviceStatus.OK));
            Log.Write(info);
            Assert.That(info, Is.Not.Null);
            Assert.That(reader.LEDGetHWVersion(out string info2), Is.EqualTo(DeviceStatus.OK));
            Log.Write(info2);
            Assert.That(info2, Is.Not.Null);
            Assert.That(reader.LEDSetColor(Color.Black), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++) {
                Assert.That(reader.LEDSetColor(Color.Red), Is.EqualTo(DeviceStatus.OK));
                Thread.Sleep(200);
                Assert.That(reader.LEDSetColor(Color.Green), Is.EqualTo(DeviceStatus.OK));
                Thread.Sleep(200);
                Assert.That(reader.LEDSetColor(Color.Blue), Is.EqualTo(DeviceStatus.OK));
                Thread.Sleep(200);
                Assert.That(reader.LEDSetColor(Color.White), Is.EqualTo(DeviceStatus.OK));
                Thread.Sleep(200);
            }
            Assert.That(reader.LEDSetColor(Color.Black), Is.EqualTo(DeviceStatus.OK));
        }

        [Test]
        public void T04_TestRead() {
            if (!Util.CheckConnect(reader.Connect)) {
                return;
            }
            Assert.That(reader.RadioOn(RadioOnType.Both), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.StartPolling(), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(100);
            int timeout = 10000;
            while (reader.IsPolling()) {
                if (reader.HasDetectedCard()) {
                    break;
                }
                Thread.Sleep(100);
                timeout -= 100;
                if (timeout <= 0) {
                    Assert.Fail("Card Read Timeout");
                }
            }
            Assert.That(reader.HasDetectedCard(), Is.True);
            Assert.That(reader.GetCardType(), Is.Not.Null);
            Assert.That(reader.GetCardUID(), Is.Not.Null);
            Assert.That(reader.StopPolling(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.RadioOff(), Is.EqualTo(DeviceStatus.OK));
        }

        [Test]
        public void T05_TestReadEMoney() {
            if (!Util.CheckConnect(reader.Connect)) {
                return;
            }
            Assert.That(reader.RadioOn(RadioOnType.Both), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.StartPolling(), Is.EqualTo(DeviceStatus.OK));
            Thread.Sleep(100);
            int timeout = 10000;
            while (reader.IsPolling()) {
                if (reader.HasDetectedCard()) {
                    break;
                }
                Thread.Sleep(100);
                timeout -= 100;
                if (timeout <= 0) {
                    Assert.Fail("Card Read Timeout");
                }
            }
            Assert.That(reader.HasDetectedCard(), Is.True);
            Assert.That(reader.GetCardType(), Is.Not.Null);
            Assert.That(reader.GetCardType(), Is.EqualTo(CardType.MIFARE));
            Assert.That(reader.GetCardUID(), Is.Not.Null);
            Assert.That(reader.GetMIFARECardLUID(), Is.Not.Null);

            reader.ReadMIFAREeMoneyAuthentication(reader.GetMIFARECardLUID().Value, Convert.FromHexString(File.ReadAllText("TestFiles/emoney.key")), out byte proxy_type, out byte _, out string store_card_id, out string merchant_code, out UInt128 store_branch_id, out string passphrase);

            Log.Write("Proxy type: " + proxy_type);
            Log.Write("Store Card ID: " + store_card_id);
            Log.Write("Merchant Code: " + merchant_code);
            Log.Write("Store branch ID: " + store_branch_id);
            Log.Write("Passphrase: " + passphrase);

            Assert.That(proxy_type, Is.EqualTo(2).Or.EqualTo(3));
            Assert.That(store_branch_id, Is.Not.EqualTo(new UInt128(0, 0)));

            Assert.That(reader.StopPolling(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.RadioOff(), Is.EqualTo(DeviceStatus.OK));
        }
    }
}
