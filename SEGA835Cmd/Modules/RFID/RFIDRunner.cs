using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Devices.Misc;
using Haruka.Arcade.SEGA835Lib.Devices.RFID;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoy.Wrapper;

namespace Haruka.Arcade.SEGA835Cmd.Modules.RFID {
    internal class RFIDRunner {
        internal static DeviceStatus Main(Options opts) {
            Program.SetGlobalOptions(opts);

            RFIDDeckReader_837_20004 rfid = new RFIDDeckReader_837_20004(opts.Port);

            DeviceStatus ret = rfid.Connect();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Connect failed");
                return ret;
            }

            ret = rfid.Reset();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Board reset failed");
                return ret;
            }

            ret = rfid.GetUnknown81(out byte _);
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Board initialization failed");
                return ret;
            }

            ret = rfid.SetUnknown4();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Board initialization failed");
                return ret;
            }

            ret = rfid.SetUnknown5();
            if (ret != DeviceStatus.OK) {
                Log.WriteError("Board initialization failed");
                return ret;
            }

            byte[][] cards;
            do {
                ret = rfid.Scan(out cards);
                if (ret != DeviceStatus.OK) {
                    Log.WriteError("Read failed");
                    return ret;
                }

                if (cards.Length < opts.WaitUntil) {
                    Thread.Sleep(500);
                }
            } while (cards.Length < opts.WaitUntil);

            Log.Write("Found " + cards.Length + " card(s)");
            foreach (byte[] card in cards) {
                Console.WriteLine(card.ToHexString());
            }

            return ret;
        }
    }
}
