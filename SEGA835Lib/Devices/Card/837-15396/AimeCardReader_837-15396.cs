using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {

    /// <summary>
    /// A Aime 837-15396 (Generation 3) card reader.
    /// </summary>
    public partial class AimeCardReader_837_15396 : CardReader {

        private const byte LED_BOARD_ADDRESS = 0x00;

        /// <summary>
        /// The COM port being used.
        /// </summary>
        public int Port { get; private set; }

        internal readonly SProtSerial serial;
        private byte[] lastReadCardUID;
        private CardType? lastReadCardType;
        private Thread pollingThread;

        /// <summary>
        /// Initializes a new card reader on the specified port.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        /// <param name="high_baudrate">Whether to use high baudrate (115200). This seems to depend on dipswitches on the reader?</param>
        public AimeCardReader_837_15396(int port, bool high_baudrate = true) {
            this.Port = port;
            this.serial = new SProtSerial(port, high_baudrate ? 115200 : 38400);
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            if (serial != null && serial.IsConnected()) {
                return DeviceStatus.OK;
            }
            lastReadCardUID = null;
            Log.Write("Connecting on Port " + Port);
            if (!serial.Connect()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }
            return Reset();
        }

        private DeviceStatus Write(byte addr, byte seq, byte cmd, byte[] payload) {
            byte[] packet = new byte[payload.Length + 5];
            if (packet.Length > 0xFF) {
                return DeviceStatus.ERR_PAYLOAD_TOO_LARGE;
            }
            packet[0] = (byte)packet.Length;
            packet[1] = addr;
            packet[2] = seq;
            packet[3] = cmd;
            packet[4] = (byte)payload.Length;
            Array.Copy(payload, 0, packet, 5, payload.Length);
            return serial.Write(packet);
        }

        private DeviceStatus Read(out byte addr, out byte seq, out byte cmd, out byte status, out byte[] payload) {
            DeviceStatus ret = serial.ReadLenByOffset(1, out byte[] data, false, true);
            if (ret != DeviceStatus.OK) {
                addr = 0;
                seq = 0;
                cmd = 0;
                status = 0;
                payload = null;
                return ret;
            }
            addr = data[2];
            seq = data[3];
            cmd = data[4];
            status = data[5];
            payload = new byte[data[6]];
            Array.Copy(data, 7, payload, 0, payload.Length);
            if (status != 0) {
                ret = DeviceStatus.ERR_DEVICE;
                SetLastError(ret, status);
            }
            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus Write(SProtFrame send) {
            return Write(send.Address, send.Sequence, send.Command, send.Payload);
        }

        /// <inheritdoc/>
        public override DeviceStatus Read(out SProtFrame recv) {
            DeviceStatus ret = Read(out byte addr, out byte seq, out byte cmd, out byte status, out byte[] payload);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            recv = new SProtFrame(seq, cmd, addr, status, payload);
            return ret;
        }

        /// <summary>
        /// Resets the device state. This is implicitely called on <see cref="Connect"/>.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, or if the reader was already reset (which will log a warning), or any other DeviceStatus on failure.</returns>
        public DeviceStatus Reset() {
            Log.Write("Reset");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out byte status);
            if (ret == DeviceStatus.ERR_DEVICE) { // error on double reset, ignore
                return SetLastError(DeviceStatus.OK, status);
            }
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's hardware version.
        /// </summary>
        /// <param name="version">The reader's hardware version (ex. "TN32MSEC003S H/W Ver3.0") or null on failure</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetHWVersion(out string version) {
            Log.Write("GetHWVersion");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetHWVersion(), out RespPacketGetHWVersion resp, out byte status);
            SetLastError(ret, status);
            if (ret == DeviceStatus.OK) {
                version = resp.version;
            } else {
                version = null;
            }
            return ret;
        }

        /// <summary>
        /// Queries the card reader's firmware version.
        /// </summary>
        /// <param name="version">The reader's firmware version (ex. "TN32MSEC003S F/W Ver1.2") or null on failure</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFWVersion(out string version) {
            Log.Write("GetFWVersion");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetFWVersion(), out RespPacketGetFWVersion resp, out byte status);
            SetLastError(ret, status);
            if (ret == DeviceStatus.OK) {
                version = resp.version;
            } else {
                version = null;
            }
            return ret;
        }

        /// <summary>
        /// Enables the reader's radio.
        /// </summary>
        /// <param name="type">The type of card that should be scanned for.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus RadioOn(RadioOnType type) {
            Log.Write("RadioOn("+type+")");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketRadioOn() { type = (byte)type }, out RespPacketRadioOn _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Disables the reader's radio.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus RadioOff() {
            Log.Write("RadioOff");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketRadioOff(), out RespPacketRadioOff _, out byte status);
            return SetLastError(ret, status);
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        /// <inheritdoc/>
        public override byte[] GetCardUID() {
            return lastReadCardUID;
        }

        /// <inheritdoc/>
        public override CardType? GetCardType() {
            return lastReadCardType;
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "837-15396";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "837-15396 Aime R/W Unit";
        }

        /// <inheritdoc/>
        public override bool HasDetectedCard() {
            return lastReadCardUID != null;
        }

        /// <inheritdoc/>
        public override DeviceStatus StartPolling() {
            Log.Write("Starting polling of Aime reader on port " + Port);
            if (IsPolling()) {
                return DeviceStatus.OK;
            }
            pollingThread = new Thread(PollT);
            pollingThread.Start();
            return DeviceStatus.OK;
        }

        /// <inheritdoc/>
        public override bool IsPolling() {
            return pollingThread != null;
        }

        private void PollT() {
            DeviceStatus ret = DeviceStatus.OK;
            do {
                try {
                    ret = Poll();
                    SetLastError(ret);
                    if (ret != DeviceStatus.OK) {
                        break;
                    }
                    Thread.Sleep(250);
                } catch (ThreadInterruptedException) {
                    break;
                }
            } while (pollingThread != null);
            Log.Write("Polling thread exited of Aime reader on port " + Port);
            if (ret != DeviceStatus.OK) {
                Log.WriteWarning("Last Error Code before polling was stopped: " + ret);
            }
            pollingThread = null;
        }

        private DeviceStatus Poll() {
            DeviceStatus ret = this.WriteAndRead(new ReqPacketPoll().ToFrame(), out SProtFrame resp);
            SetLastError(ret, resp?.Status);
            if (resp != null && resp.Payload != null) {
                byte[] data = resp.Payload;
                int offset = 0;

                byte count = data[offset++];
                for (int i = 0; i < count; i++) {
                    byte type = data[offset++];
                    byte size = data[offset++];

                    if (type == 0x10) { // MIFARE
                        byte[] id = new byte[size];
                        Array.Copy(data, offset, id, 0, size);
                        offset += size;
                        lastReadCardUID = id;
                        lastReadCardType = CardType.MIFARE;
                        Log.Write("Found a MIFARE card: \n" + Hex.Dump(id));
                    } else if (type == 0x20) { // FeliCa
                        if (size == 0x10) {
                            byte[] id = new byte[size];
                            Array.Copy(data, offset, id, 0, size);
                            offset += size;
                            lastReadCardUID = id;
                            lastReadCardType = CardType.FeliCa;
                            Log.Write("Found a FeliCa card: \n" + Hex.Dump(id));
                        } else {
                            ret = DeviceStatus.ERR_INCOMPATIBLE;
                        }
                    } else {
                        ret = DeviceStatus.ERR_INCOMPATIBLE;
                    }
                }
            }
            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus StopPolling() {
            DeviceStatus ret = DeviceStatus.OK;
            if (IsPolling()) {
                Log.Write("Stopping polling of Aime reader on port " + Port);
                try {
                    pollingThread.Interrupt();
                    pollingThread.Join();
                    pollingThread = null;
                } catch (Exception ex) {
                    Log.WriteFault(ex, "Failed to stop polling thread of card reader");
                    return DeviceStatus.ERR_OTHER;
                } finally {
                    ret = RadioOff();
                }
            }
            return ret;
        }

        /// <summary>
        /// Resets the card reader's LED sub-board.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LEDReset() {
            Log.Write("LEDReset");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketLEDReset(), out RespPacketLEDReset _, out byte status, LED_BOARD_ADDRESS);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's LED board hardware version.
        /// </summary>
        /// <param name="version">The reader's LED board hardware version (ex. TODO) or null on failure.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LEDGetHWVersion(out string version) {
            Log.Write("LEDGetHWVersion");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketLEDHWVersion(), out RespPacketLEDHWVersion resp, out byte status, LED_BOARD_ADDRESS);
            SetLastError(ret, status);
            if (ret == DeviceStatus.OK) {
                version = resp.version;
            } else {
                version = null;
            }
            return ret;
        }

        /// <summary>
        /// Queries the card reader's LED board information.
        /// </summary>
        /// <param name="info">The reader's LED board information (ex. TODO) or null on failure.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LEDGetInfo(out string info) {
            Log.Write("LEDGetInfo");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketLEDGetInfo(), out RespPacketLEDGetInfo resp, out byte status, LED_BOARD_ADDRESS);
            SetLastError(ret, status);
            if (ret == DeviceStatus.OK) {
                info = resp.info;
            } else {
                info = null;
            }
            return ret;
        }

        /// <summary>
        /// Sets the card reader's LED channels to the specified value.
        /// </summary>
        /// <param name="strength">The LED strength [0-255]</param>
        /// <param name="red">true if the strength should be applied to the red channel.</param>
        /// <param name="green">true if the strength should be applied to the green channel.</param>
        /// <param name="blue">true if the strength should be applied to the blue channel.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LEDSetChannels(byte strength, bool red, bool green, bool blue) {
            Log.Write("LEDSetChannels");
            DeviceStatus ret = Write(new ReqPacketLEDSetChannel() {
                rgb = (byte)((red ? 1 << 0 : 0) | (green ? 1 << 1 : 0) | (blue ? 1 << 2 : 0)),
                value = strength
            }.ToFrame(LED_BOARD_ADDRESS));
            return SetLastError(ret);
        }

        /// <summary>
        /// Sets the card reader's LED color.
        /// </summary>
        /// <param name="c">The color to set.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LEDSetColor(Color c) {
            return LEDSetColor(c.R, c.G, c.B);
        }

        /// <summary>
        /// Sets the card reader's LED color.
        /// </summary>
        /// <param name="red">The R value of the color. [0-255]</param>
        /// <param name="green">The G value of the color. [0-255]</param>
        /// <param name="blue">The B value of the color. [0-255]</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LEDSetColor(byte red, byte green, byte blue) {
            Log.Write("LEDSetColor");
            DeviceStatus ret = Write(new ReqPacketLEDSetColor() {
                red = red,
                green = green,
                blue = blue
            }.ToFrame(LED_BOARD_ADDRESS));
            return SetLastError(ret);
        }

        /// <summary>
        /// Clears the last card values from the reader.
        /// </summary>
        /// <seealso cref="GetCardUID"/>
        public void ClearCard() {
            lastReadCardType = null;
            lastReadCardUID = null;
        }
    }
}
