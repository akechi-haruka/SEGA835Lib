using System.Runtime.CompilerServices;
using Haruka.Arcade.SEGA835Lib.Devices;

[assembly: InternalsVisibleTo("835TestsMaybeLess32")]

namespace _835TestsMaybeLess;

static class Util {
    internal static bool CheckConnect(Func<DeviceStatus> connect) {
        DeviceStatus ret = connect();
        if (ret != DeviceStatus.Ok) {
            Assert.Inconclusive("Device is not connected!");
            return false;
        }

        return true;
    }
}