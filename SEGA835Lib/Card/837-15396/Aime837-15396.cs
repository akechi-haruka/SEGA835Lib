﻿using Haruka.Arcade.SEGA835Lib.Card._837_15396;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace Haruka.Arcade.SEGA835Lib.Card._837_15396 {
    public partial class AimeCardReader_837_15396 : CardReader {

        public int Port { get; private set; }

        private JVSSerial serial;
        private byte[] lastReadCardUID;
        private CardType? lastReadCardType;
        private Thread pollingThread;

        public AimeCardReader_837_15396(int port, bool high_baudrate = true) {
            this.Port = port;
            this.serial = new JVSSerial(port, high_baudrate ? 115200 : 9600);
        }

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
            DeviceStatus ret = serial.ReadLenByOffset(1, out byte[] data);
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

        public override DeviceStatus Write(JVSFrame send) {
            return Write(send.Address, send.Sequence, send.Command, send.Payload);
        }

        public override DeviceStatus Read(out JVSFrame recv) {
            DeviceStatus ret = Read(out byte addr, out byte seq, out byte cmd, out byte status, out byte[] payload);
            if (ret != DeviceStatus.OK) {
                recv = null;
                return ret;
            }
            recv = new JVSFrame(seq, cmd, addr, status, payload);
            return ret;
        }

        public DeviceStatus Reset() {
            Log.Write("Reset");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out byte status);
            if (ret == DeviceStatus.ERR_DEVICE) { // error on double reset, ignore
                return SetLastError(DeviceStatus.OK, status);
            }
            return SetLastError(ret, status);
        }

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

        public DeviceStatus RadioOn(RadioOnType type) {
            Log.Write("RadioOn("+type+")");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketRadioOn() { type = (byte)type }, out RespPacketRadioOn _, out byte status);
            return SetLastError(ret, status);
        }

        public DeviceStatus RadioOff() {
            Log.Write("RadioOff");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketRadioOff(), out RespPacketRadioOff _, out byte status);
            return SetLastError(ret, status);
        }

        private DeviceStatus Poll() {
            DeviceStatus ret = this.WriteAndRead(new ReqPacketPoll().ToFrame(), out JVSFrame resp);
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
                    } else if (type == 0x20){ // FeliCa
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

        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        public override byte[] GetCardUID() {
            return lastReadCardUID;
        }

        public override CardType? GetCardType() {
            return lastReadCardType;
        }

        public override string GetDeviceModel() {
            return "837-15396";
        }

        public override string GetName() {
            return "837-15396 Aime R/W Unit";
        }

        public override bool HasDetectedCard() {
            return lastReadCardUID != null;
        }

        public override DeviceStatus StartPolling() {
            Log.Write("Starting polling of Aime reader on port " + Port);
            if (IsPolling()) {
                return DeviceStatus.OK;
            }
            pollingThread = new Thread(PollT);
            pollingThread.Start();
            return DeviceStatus.OK;
        }

        public override bool IsPolling() {
            return pollingThread != null;
        }

        private void PollT() {
            do {
                try {
                    DeviceStatus ret = Poll();
                    SetLastError(ret);
                    if (ret != DeviceStatus.OK) {
                        break;
                    }
                    Thread.Sleep(250);
                } catch (ThreadInterruptedException) {
                    break;
                }
            } while (pollingThread != null);
            pollingThread = null;
            Log.Write("Polling thread exited of Aime reader on port " + Port);
        }

        public override DeviceStatus StopPolling() {
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
                    RadioOff();
                }
            }
            return DeviceStatus.OK;
        }

    }
}