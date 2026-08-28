using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06;
using Haruka.Arcade.SEGA835Lib.Misc;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Cmd.Modules.Led;

static class LedRunner {
    private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(LedRunner));

    internal static DeviceStatus Run(Options opts) {
        Program.SetGlobalOptions(opts);

        LedMonkey06 led = new LedMonkey06(opts.Port, (byte)opts.SourceAddress, (byte)opts.DestinationAddress);

        DeviceStatus ret = led.Connect();
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Connecting to LED board failed");
            return ret;
        }

        ret = led.GetBoardInfo(out string boardNumber, out string chipNumber, out byte fwVer);
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Getting board info failed");
            return ret;
        }

        LOG.LogInformation("Board Number: " + boardNumber);
        LOG.LogInformation("Chip Number: " + chipNumber);
        LOG.LogInformation("FW version: " + fwVer);

        ret = led.GetFirmwareChecksum(out ushort chk);
        if (ret != DeviceStatus.Ok) {
            LOG.LogError("Getting board info failed");
            return ret;
        }

        LOG.LogInformation("Board Checksum: " + chk);

        if (opts.MonkeyReset) {
            ret = led.ResetMonkey();
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Reset failed");
                return ret;
            }
        }

        if (opts.MonkeyChecksum > 0) {
            ret = led.SetFirmwareChecksum(opts.MonkeyChecksum);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting checksum failed");
                return ret;
            }
        }

        if (opts.MonkeyVersion > 0) {
            ret = led.SetFirmwareVersion(opts.MonkeyVersion);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting version failed");
                return ret;
            }
        }

        if (opts.MonkeyBoardName != null) {
            ret = led.SetBoardName(opts.MonkeyBoardName);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting board name failed");
                return ret;
            }
        }

        if (opts.MonkeyChipNumber != null) {
            ret = led.SetChipNumber(opts.MonkeyChipNumber);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting chip number failed");
                return ret;
            }
        }

        if (opts.MonkeyAppliMode != null) {
            ret = led.SetAppliMode(opts.MonkeyAppliMode.Value);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting appli mode failed");
                return ret;
            }
        }

        if (opts.MonkeyTable != null) {
            List<byte> data = new List<byte>();
            foreach (String s in opts.MonkeyTable.Split(',')) {
                data.Add(Byte.Parse(s));
            }

            ret = led.SetLedTranslationTable(data);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting translation table failed");
                return ret;
            }
        }

        if (opts.MonkeyChannels != null) {
            string[] channels = opts.MonkeyChannels.Split(',');
            if (channels.Length != 3) {
                LOG.LogError("Invalid argument for channels");
                return ret;
            }

            led.SetChannels(Enum.Parse<LedMonkey06.Channel>(channels[0]), Enum.Parse<LedMonkey06.Channel>(channels[1]), Enum.Parse<LedMonkey06.Channel>(channels[2]));
        }

        if (opts.LedTable != null) {
            List<Color> data = new List<Color>();
            string[] array = opts.LedTable.Split(',');
            for (int i = 0; i < opts.Offset; i++) {
                data.Add(Color.Black);
            }

            for (int i = 0; i < array.Length; i += 3) {
                data.Add(Color.FromArgb(byte.Parse(array[i]), byte.Parse(array[i + 1]), byte.Parse(array[i + 2])));
            }

            ret = led.SetLeds(data);
            if (ret != DeviceStatus.Ok) {
                LOG.LogError("Setting LEDs failed");
            }
        }

        return ret;
    }
}