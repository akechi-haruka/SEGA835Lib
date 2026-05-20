using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card;
using Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396;
using Haruka.Arcade.SEGA835Lib.Misc;

namespace _835TestsMaybeLess;

class Aime15396Test {
    private AimeCardReader15396 reader;

    [SetUp]
    public void Setup() {
        reader = new AimeCardReader15396(3);
        reader.Serial.DumpReadWriteCommandsToLog = true;
        reader.Serial.DumpBytesToLog = true;
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

        Assert.That(reader.GetHardwareVersion(out string version), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(version, Is.Not.Null);
        Assert.That(reader.GetFirmwareVersion(out string version2, out byte version3), Is.EqualTo(DeviceStatus.Ok));
        if (version3 == 0) {
            Assert.That(version2, Is.Not.Null);
        }

        Assert.That(reader.GetFirmwareChecksum(out ushort checksum), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(checksum, Is.GreaterThan(0));
    }

    [Test]
    public void T02_TestOfflineRead() {
        if (!Util.CheckConnect(reader.Connect)) {
            return;
        }

        Assert.That(reader.RadioOn(RadioOnType.Both), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.StartPolling(), Is.EqualTo(DeviceStatus.Ok));
        Thread.Sleep(100);
        Assert.That(reader.StopPolling(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.RadioOff(), Is.EqualTo(DeviceStatus.Ok));
    }

    [Test]
    public void T03_TestDisco() {
        if (!Util.CheckConnect(reader.Connect)) {
            return;
        }

        Assert.That(reader.LedReset(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.LedGetInfo(out string info), Is.EqualTo(DeviceStatus.Ok));
        Log.Write(info);
        Assert.That(info, Is.Not.Null);
        Assert.That(reader.LedGetHwVersion(out string info2), Is.EqualTo(DeviceStatus.Ok));
        Log.Write(info2);
        Assert.That(info2, Is.Not.Null);
        Assert.That(reader.LedSetColor(Color.Black), Is.EqualTo(DeviceStatus.Ok));
        Thread.Sleep(200);
        for (int i = 0; i < 3; i++) {
            Assert.That(reader.LedSetColor(Color.Red), Is.EqualTo(DeviceStatus.Ok));
            Thread.Sleep(200);
            Assert.That(reader.LedSetColor(Color.Green), Is.EqualTo(DeviceStatus.Ok));
            Thread.Sleep(200);
            Assert.That(reader.LedSetColor(Color.Blue), Is.EqualTo(DeviceStatus.Ok));
            Thread.Sleep(200);
            Assert.That(reader.LedSetColor(Color.White), Is.EqualTo(DeviceStatus.Ok));
            Thread.Sleep(200);
        }

        Assert.That(reader.LedSetColor(Color.Black), Is.EqualTo(DeviceStatus.Ok));
    }

    [Test]
    public void T04_TestRead() {
        if (!Util.CheckConnect(reader.Connect)) {
            return;
        }

        Assert.That(reader.RadioOn(RadioOnType.Both), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.StartPolling(), Is.EqualTo(DeviceStatus.Ok));
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
        Assert.That(reader.GetCardUid(), Is.Not.Null);
        Assert.That(reader.StopPolling(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.RadioOff(), Is.EqualTo(DeviceStatus.Ok));
    }

    [Test]
    public void T05_TestReadEMoney() {
        if (!Util.CheckConnect(reader.Connect)) {
            return;
        }

        Assert.That(reader.RadioOn(RadioOnType.Both), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.StartPolling(), Is.EqualTo(DeviceStatus.Ok));
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
        Assert.That(reader.GetCardType(), Is.EqualTo(CardType.Mifare));
        Assert.That(reader.GetCardUid(), Is.Not.Null);
        Assert.That(reader.GetMifareCardLuid(), Is.Not.Null);

        reader.ReadMifarEeMoneyAuthentication(reader.GetMifareCardLuid() ?? 0, Convert.FromHexString(File.ReadAllText("TestFiles/emoney.key")), out byte proxyType, out byte _, out string storeCardID, out string merchantCode, out UInt128 storeBranchID, out string passphrase);

        Log.Write("Proxy type: " + proxyType);
        Log.Write("Store Card ID: " + storeCardID);
        Log.Write("Merchant Code: " + merchantCode);
        Log.Write("Store branch ID: " + storeBranchID);
        Log.Write("Passphrase: " + passphrase);

        Assert.That(proxyType, Is.EqualTo(2).Or.EqualTo(3));
        Assert.That(storeBranchID, Is.Not.EqualTo(new UInt128(0, 0)));

        Assert.That(reader.StopPolling(), Is.EqualTo(DeviceStatus.Ok));
        Assert.That(reader.RadioOff(), Is.EqualTo(DeviceStatus.Ok));
    }
}