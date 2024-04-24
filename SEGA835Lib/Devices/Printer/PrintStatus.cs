using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Printer {
    public enum PrintStatus {
        None,
        Started,
        RFIDRead,
        RFIDWrite,
        SetPrinterProperties,
        SetImage,
        Printing,
        Ending,
        Ejecting,
        Finished,
        Errored
    }
}
