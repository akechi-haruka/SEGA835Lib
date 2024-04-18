using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Reflection.PortableExecutable;

namespace _835TestsMaybeLess {
    public class _835_20004_RFIDDeckReaderTest {

        private _837_20004_RFIDDeckReader reader;

        [SetUp]
        public void Setup() {
            reader = new _837_20004_RFIDDeckReader(2);
            SerialComm.DUMP_BYTES = false;
            SerialComm.LOG_RW = false;
        }

        [TearDown]
        public void Cleanup() {
            reader.Disconnect();
        }

        [Test]
        public void T01_TestVersion() {
            Assert.That(reader.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.GetBootVersion(out byte version), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version, Is.Not.EqualTo(0));
            Log.Write("Boot: " + version);
            Assert.That(reader.GetAppVersion(out byte version2), Is.EqualTo(DeviceStatus.OK));
            Assert.That(version2, Is.Not.EqualTo(0));
            Log.Write("App: " + version2);
            Assert.That(reader.GetBoardInfo(out string board), Is.EqualTo(DeviceStatus.OK));
            Assert.That(board, Is.Not.Null);
            Log.Write("Board: " + board);
        }

        [Test]
        public void T02_TestRead() {
            Assert.That(reader.Connect(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.GetUnknown81(out byte _), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.SetUnknown4(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.SetUnknown5(), Is.EqualTo(DeviceStatus.OK));
            Assert.That(reader.Scan(out byte[][] cards), Is.EqualTo(DeviceStatus.OK));
            Assert.That(cards, Is.Not.Null);
            Log.Write("Card Count: " + cards.Length);
            if (cards.Length == 0) {
                Assert.Warn("No cards were in reader, can't verify!");
            }
            Assert.That(cards, Has.None.Null);
            Assert.That(cards, Has.None.Length.Not.EqualTo(reader.GetCardPayloadSize()));
            for (int i = 0; i < cards.Length; i++) {
                byte[] card = cards[i];
                Log.Dump(card, "cards["+i+"]");
            }
        }

    }
}