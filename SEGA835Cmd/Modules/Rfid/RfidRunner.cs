using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Rfid;

static class RfidRunner {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(RfidRunner));

    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        RfidDeckReader20004 rfid = new RfidDeckReader20004(opts.Port);

        DeviceStatus ret = rfid.Connect();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Connect failed");
            return ret;
        }

        ret = rfid.Reset();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Board reset failed");
            return ret;
        }

        ret = rfid.GetUnknown81(out byte _);
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Board initialization failed");
            return ret;
        }

        ret = rfid.SetUnknown4();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Board initialization failed");
            return ret;
        }

        ret = rfid.SetUnknown5();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Board initialization failed");
            return ret;
        }

        byte[][] cards;
        do {
            ret = rfid.Scan(out cards);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Read failed");
                return ret;
            }

            if (cards.Length < opts.WaitUntil) {
                Thread.Sleep(500);
            }
        } while (cards.Length < opts.WaitUntil);

        LOG.LogInformation("Found " + cards.Length + " card(s)");
        foreach (byte[] card in cards) {
            Console.WriteLine(card.ToHexString());
        }

        return ret;
    }
}