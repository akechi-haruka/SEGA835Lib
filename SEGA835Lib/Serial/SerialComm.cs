using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    public class SerialComm {

        public static bool DUMP_BYTES = false;
        public static bool LOG_RW = false;
        public int Port { get; private set; }
        public int Baudrate { get; private set; }
        public bool DTR { get; private set; }
        public bool RTS { get; private set; }
        public Parity Parity { get; private set; }
        public int DataBits { get; private set; }
        public StopBits StopBits { get; private set; }
        public Handshake Handshake { get; private set; }
        public int Timeout { get; private set; }

        private SerialPort device;

        public SerialComm(int port_no, int baudrate = 115_200, int timeout = 1000, bool dtr = false, bool rts = false, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake flowControl = Handshake.None) {
            Log.Write("Initializing Serial connection on port " + port_no + ", baud=" + baudrate + ", dtr=" + dtr + ", rts=" + rts);
            Port = port_no;
            Baudrate = baudrate;
            DTR = dtr;
            RTS = rts;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
            Handshake = flowControl;
            Timeout = timeout;
        }

        public bool Connect() {
            Log.Write("Connecting to port " + Port);
            device = new SerialPort("COM" + Port, Baudrate, Parity, DataBits, StopBits) {
                DtrEnable = DTR,
                RtsEnable = RTS,
                ReadTimeout = Timeout,
                WriteTimeout = Timeout
            };
            try {
                device.Open();
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed to connect to port " + Port);
                return false;
            }
            return true;
        }

        public bool IsConnected() {
            return device?.IsOpen ?? false;
        }

        public virtual DeviceStatus ReadByte(out byte data) {
            data = 0;
            if (device == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }
            if (!IsConnected()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }
            try {
                int ret = device.ReadByte();
                if (ret == -1) {
                    throw new TimeoutException();
                }
                data = (byte)ret;
                //Log.Write("byte=" + data);
            } catch (OperationCanceledException) {
                Log.WriteError("Failed reading from port " + Port + " (1): Interrupted");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (TimeoutException) {
                Log.WriteError("Failed reading from port " + Port + " (1): Timed out");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading from port " + Port);
                return DeviceStatus.ERR_OTHER;
            }
            return DeviceStatus.OK;
        }

        public virtual DeviceStatus Read(int len, out byte[] data) {
            data = new byte[len];
            if (device == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }
            if (!IsConnected()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }
            if (len == 0) {
                return DeviceStatus.OK;
            }
            if (LOG_RW) {
                Log.Write("Port " + Port + ", Read Len=" + data.Length);
            }
            int pos = 0;
            try {
                while (pos < data.Length) {
                    int read = device.Read(data, pos, len);
                    if (read <= 0) {
                        throw new TimeoutException();
                    }
                    pos += read;
                }
            } catch (OperationCanceledException) {
                Log.WriteError("Failed reading from port " + Port + " ("+pos+"/"+len+"): Interrupted");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (TimeoutException) {
                Log.WriteError("Failed reading from port " + Port + " ("+pos+"/"+len+"): Timed out");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading from port " + Port);
                return DeviceStatus.ERR_OTHER;
            }
            return DeviceStatus.OK;
        }

        public virtual DeviceStatus Write(byte[] data) {
            if (device == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }
            if (!IsConnected()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            if (LOG_RW) {
                Log.Write("Port " + Port + ", Write Len=" + data.Length);
            }
            if (data.Length == 0) {
                return DeviceStatus.OK;
            }
            try {
                device.Write(data, 0, data.Length);
            } catch (OperationCanceledException) {
                Log.WriteError("Failed reading from port " + Port + ": Interrupted");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (TimeoutException) {
                Log.WriteError("Failed writing to port " + Port + ": Timed out");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed writing to port " + Port);
                return DeviceStatus.ERR_OTHER;
            }
            return DeviceStatus.OK;
        }

        public void Disconnect() {
            device?.Close();
        }

    }
}
