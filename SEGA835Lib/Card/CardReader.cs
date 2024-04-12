using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Card {
    public abstract class CardReader : IDevice, IFrameRW {
        private int lastError;
        public abstract DeviceStatus Connect();
        public abstract DeviceStatus Disconnect();
        public abstract string GetDeviceModel();
        public int GetLastError() {
            return lastError;
        }
        public abstract string GetName();
        public abstract DeviceStatus StartPolling();
        public abstract bool IsPolling();
        public abstract DeviceStatus StopPolling();
        public abstract bool HasDetectedCard();
        public abstract byte[] GetCardUID();
        public abstract CardType? GetCardType();
        public abstract DeviceStatus Write(JVSFrame send);
        public abstract DeviceStatus Read(out JVSFrame recv);
        public DeviceStatus SetLastError(DeviceStatus status, int? reportStatus = null) {
            if (status == DeviceStatus.OK || status == DeviceStatus.ERR_DEVICE) {
                lastError = reportStatus.GetValueOrDefault(0);
            } else {
                lastError = (int)status;
            }
            if (lastError > 0) {
                Log.WriteWarning("Error: " + lastError);
            }
            return status;
        }
    }
}
