using Haruka.Arcade.SEGA835Lib.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01 {

    /// <summary>
    /// A 835-15257-01 SEGA I/O CONTROL BD ("IO4").
    /// </summary>
    public class IO4USB_835_15257_01 : JVSUSBIO {

        /// <summary>
        /// Creates a new IO4 board.
        /// </summary>
        public IO4USB_835_15257_01() : base(0x0ca3, 0x0021) {
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "835-15257-01";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "SEGA I/O CONTROL BD";
        }

        /// <summary>
        /// Resets the board status. Unknown what this <i>actually</i> does.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus ResetBoardStatus() {
            Log.Write("ResetBoardStatus");
            return Write(new JVSUSBReportOut() {
                cmd = JVSUSBReports.ClearBoardStatus
            });
        }
    }
}
