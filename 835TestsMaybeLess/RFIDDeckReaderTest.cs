using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Devices.RFID.Backends;
using Haruka.Arcade.SEGA835Lib.Serial;
using Microsoft.Extensions.Logging;

namespace _835TestsMaybeLess;

public class RfidDeckReaderTest {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(RfidDeckReaderTest));
    private RfidDeckReader20004 reader;

    [SetUp]
    public void Setup() {
        reader = new RfidDeckReader20004(2);
        SProtSerial serial = ((RfidBackendSerial)reader.Backend).Serial;
        serial.DumpReadWriteCommandsToLog = true;
        serial.DumpBytesToLog = true;
    }

    [TearDown]
    public void Cleanup() {
        reader.Disconnect();
    }

    [Test]
    public void T01_TestVersion() {
        if (!Util.CheckConnect(reader.Connect)) {
            return;
        }

        Assert.That(reader.GetBootVersion(out byte version), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(version, Is.Not.Zero);
        LOG.LogInformation("Boot: " + version);
        Assert.That(reader.GetAppVersion(out byte version2), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(version2, Is.Not.Zero);
        LOG.LogInformation("App: " + version2);
        Assert.That(reader.GetBoardInfo(out string board), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(board, Is.Not.Null);
        LOG.LogInformation("Board: " + board);
    }

    [Test]
    public void T02_TestRead() {
        if (!Util.CheckConnect(reader.Connect)) {
            return;
        }

        Assert.That(reader.GetUnknown81(out byte _), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.SetUnknown4(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.SetUnknown5(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.Scan(out byte[][] cards), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(cards, Is.Not.Null);
        LOG.LogInformation("Card Count: " + cards.Length);
        if (cards.Length == 0) {
            Assert.Inconclusive("No cards were in reader, can't verify!");
        }

        Assert.That(cards, Has.None.Null);
        Assert.That(cards, Has.None.Length.Not.EqualTo(reader.GetCardPayloadSize()));
        for (int i = 0; i < cards.Length; i++) {
            byte[] card = cards[i];
            LOG.LogInformation("cards[" + i + "]");
            LOG.LogInformation(Hex.Dump(card));
        }
    }
}