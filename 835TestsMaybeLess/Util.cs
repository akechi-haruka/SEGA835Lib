using Haruka.Arcade.SEGA835Lib.Devices;

namespace _835TestsMaybeLess {
    internal class Util {
        internal static bool CheckConnect(Func<DeviceStatus> connect) {
            DeviceStatus ret = connect();
            if (ret != DeviceStatus.OK) {
                Assert.Inconclusive("Device is not connected!");
                return false;
            }

            return true;
        }
    }
}