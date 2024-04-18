using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card {
    public abstract class CardReader : Device, ISProtRW {
        public abstract DeviceStatus StartPolling();
        public abstract bool IsPolling();
        public abstract DeviceStatus StopPolling();
        public abstract bool HasDetectedCard();
        public abstract byte[] GetCardUID();
        public abstract CardType? GetCardType();
        public abstract DeviceStatus Write(SProtFrame send);
        public abstract DeviceStatus Read(out SProtFrame recv);
    }
}
