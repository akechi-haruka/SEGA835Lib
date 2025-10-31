using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Misc;
using System;
using System.IO.Ports;

namespace Haruka.Arcade.SEGA835Lib.Serial {
    /// <summary>
    /// A RS232 serial communication interface that connects on a specific COM port with certain handshake parameters.
    /// This object can be re-used after a call to <see cref="Disconnect"/>.
    /// </summary>
    /// <remarks>
    /// Note that if any of the Read/Write commands fail, the device may be in an inconsistent state, therefore a hard reset (<see cref="Disconnect"/> + <see cref="Connect"/> is highly recommended.
    /// </remarks>
    public class SerialComm {
        /// <summary>
        /// Whether R/W commands should be logged to <see cref="Log"/>. Includes command type and read/write byte count.
        /// </summary>
        public bool DumpRWCommandsToLog { get; set; }

        /// <summary>
        /// The COM port that is being used.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The baudrate that is being used.
        /// </summary>
        public int Baudrate { get; private set; }

        /// <summary>
        /// Whether or not DTR is being used.
        /// </summary>
        public bool DTR { get; private set; }

        /// <summary>
        /// Whether or not RTS is being used.
        /// </summary>
        public bool RTS { get; private set; }

        /// <summary>
        /// The parity that is being used.
        /// </summary>
        public Parity Parity { get; private set; }

        /// <summary>
        /// The amount of data bits that are being used.
        /// </summary>
        public int DataBits { get; private set; }

        /// <summary>
        /// The amount of stop bits that are being used.
        /// </summary>
        public StopBits StopBits { get; private set; }

        /// <summary>
        /// The type of handshake that is being used.
        /// </summary>
        public Handshake Handshake { get; private set; }

        /// <summary>
        /// The timeout (in ms) that is being used.
        /// </summary>
        public int Timeout { get; private set; }

        private SerialPort device;

        /// <summary>
        /// Creates a new SerialComm. This will not connect to the device yet (and neither check if it's already in use, etc.)
        /// </summary>
        /// <param name="portNumber">The COM port number to use.</param>
        /// <param name="baudrate">The baudrate to use.</param>
        /// <param name="timeout">The timeout (in ms) to use before any R/W call to this device will return <see cref="DeviceStatus.ERR_TIMEOUT"/>.</param>
        /// <param name="dtr">Whether or not to use the DTR (Data Terminal Ready) signal. This depends on the specific device being used.</param>
        /// <param name="rts">Whether or not to use the RTS (Request To Send) signal. This depends on the specific device being used.</param>
        /// <param name="parity">The parity bit(s) to use. This depends on the specific device being used.</param>
        /// <param name="dataBits">The amount of data bits to use. This depends on the specific device being used.</param>
        /// <param name="stopBits">The amount of stop bits to use. This depends on the specific device being used.</param>
        /// <param name="flowControl">The type of flow control being used. This depends on the specific device being used.</param>
        public SerialComm(int portNumber, int baudrate = 115_200, int timeout = 1000, bool dtr = false, bool rts = false, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake flowControl = Handshake.None) {
            Log.Write("Initializing Serial connection on port " + portNumber + ", baud=" + baudrate + ", dtr=" + dtr + ", rts=" + rts + ", handshake=" + flowControl);
            Port = portNumber;
            Baudrate = baudrate;
            DTR = dtr;
            RTS = rts;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
            Handshake = flowControl;
            Timeout = timeout;
        }

        /// <summary>
        /// Connects the specified device.
        /// </summary>
        /// <returns>true if successful, false if not</returns>
        public bool Connect() {
            Log.Write("Connecting to port " + Port + " (" + Baudrate + ") DTR=" + DTR + ",RTS=" + RTS);
#if LINUX
            string portPrefix = "/dev/ttySC";
#else
            // Device namespace is necessary for COM10 and above to work
            // https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file#win32-device-namespaces
            string portPrefix = @"\\.\COM";
#endif
            device = new SerialPort(portPrefix + Port, Baudrate, Parity, DataBits, StopBits) {
                DtrEnable = DTR,
                RtsEnable = RTS,
                ReadTimeout = Timeout,
                WriteTimeout = Timeout,
                Handshake = Handshake
            };
            try {
                device.Open();
#if !LINUX
            } catch (ArgumentException ex) {
                Log.WriteWarning("Failed to open port with backslash path, trying regular... (Error was: "+ex.Message+")");
                device.PortName = "COM" + Port;
                try {
                    device.Open();
                } catch (Exception ex2) {
                    Log.WriteFault(ex2, "Failed to connect to port " + Port);
                    return false;
                }
#endif
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed to connect to port " + Port);
                return false;
            }

            Log.Write("Connected");
            return true;
        }

        /// <summary>
        /// Returns true if the device is connected (and the port was not externally closed)
        /// </summary>
        /// <returns>true if the device is connected</returns>
        public bool IsConnected() {
            return device?.IsOpen ?? false;
        }

        /// <summary>
        /// Reads a single byte from the device. This call may block up to <see cref="Timeout"/> ms.
        /// </summary>
        /// <param name="data">The byte that was read or 0 if any error occurred.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or <see cref="Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="Timeout"/> ms.
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
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
                return DeviceStatus.ERR_TIMEOUT;
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading from port " + Port);
                return DeviceStatus.ERR_OTHER;
            }

            return DeviceStatus.OK;
        }

        /// <summary>
        /// Reads a number of bytes from the device. This call may block up to <see cref="Timeout"/> ms.
        /// If len is zero, this call will immediately return success.
        /// </summary>
        /// <param name="len">The number of bytes to read</param>
        /// <param name="data">The bytes that were read or null any error occurred.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the requested number of bytes was read.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or <see cref="Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="Timeout"/> ms.
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
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

            if (DumpRWCommandsToLog) {
                Log.Write("Port " + Port + ", Read Len=" + data.Length);
            }

            int pos = 0;
            try {
                while (pos < data.Length) {
                    int read = device.Read(data, pos, len - pos);
                    if (read <= 0) {
                        throw new TimeoutException();
                    }

                    pos += read;
                }
            } catch (OperationCanceledException) {
                Log.WriteError("Failed reading from port " + Port + " (" + pos + "/" + len + "): Interrupted");
                return DeviceStatus.ERR_NOT_CONNECTED;
            } catch (TimeoutException) {
                Log.WriteError("Failed reading from port " + Port + " (" + pos + "/" + len + "): Timed out");
                return DeviceStatus.ERR_TIMEOUT;
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading from port " + Port);
                return DeviceStatus.ERR_OTHER;
            }

            return DeviceStatus.OK;
        }

        /// <summary>
        /// Writes the given bytes to the device.
        /// </summary>
        /// <param name="data">The bytes to write to the device.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the bytes were successfully written.
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not/no longer connected, the thread was interrupted or <see cref="Disconnect"/> was called while this call was waiting.
        /// <see cref="DeviceStatus.ERR_TIMEOUT"/> if no byte(s) were read for <see cref="Timeout"/> ms.
        /// <see cref="DeviceStatus.ERR_OTHER"/> if an exception occurred.
        /// </returns>
        public virtual DeviceStatus Write(byte[] data) {
            if (device == null) {
                return DeviceStatus.ERR_NOT_INITIALIZED;
            }

            if (!IsConnected()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }

            NetStandardBackCompatExtensions.ThrowIfNull(data, nameof(data));
            if (DumpRWCommandsToLog) {
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
                return DeviceStatus.ERR_TIMEOUT;
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed writing to port " + Port);
                return DeviceStatus.ERR_OTHER;
            }

            return DeviceStatus.OK;
        }

        /// <summary>
        /// Disconnects the device and frees the COM port. Has no effect if called when called multiple times or the port is not initialized.
        /// </summary>
        public void Disconnect() {
            device?.Close();
        }
    }
}