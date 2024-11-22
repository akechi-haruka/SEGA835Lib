using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.Card;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {

    /// <summary>
    /// A Aime 837-15396 (Generation 3) card reader.
    /// </summary>
    public partial class AimeCardReader_837_15396 : CardReader {

        private const byte LED_BOARD_ADDRESS = 0x00;

        /// <summary>
        /// Whether or not to include the PMM part when a FeliCa is read. If true, <see cref="GetCardUID"/> will return 16 bytes (8 bytes IDm + 8 bytes PMm), if false, only the 8 bytes IDm are returned.
        /// </summary>
        public bool FeliCaIncludePMM { get; set; } = false;

        /// <summary>
        /// Whether the card reader LEDs should flash while card scanning is in progress.
        /// </summary>
        public bool FlashLEDsWhilePolling { get; set; }

        private byte[] lastReadCardUID;
        private CardType? lastReadCardType;
        private Thread pollingThread;
        private RadioOnType? radioType;
        private uint lastMifareCardLUID;

        /// <summary>
        /// Initializes a new card reader on the specified port.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        /// <param name="high_baudrate">Whether to use high baudrate (115200). This seems to depend on dipswitches on the reader?</param>
        public AimeCardReader_837_15396(int port, bool high_baudrate = true) : base(port, high_baudrate ? 115200 : 38400) {
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            lock (SerialLocker) {
                if (serial != null && serial.IsConnected()) {
                    return DeviceStatus.OK;
                }
                lastReadCardUID = null;
                radioType = null;
                lastMifareCardLUID = 0;
                Log.Write("Connecting on Port " + Port);
                if (!serial.Connect()) {
                    return DeviceStatus.ERR_NOT_CONNECTED;
                }
            }
            
            DeviceStatus ret = Reset();
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            return SetMIFAREParameters();
        }

        private DeviceStatus Write(byte addr, byte seq, byte cmd, byte[] payload) {
            lock (SerialLocker) {
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
        }

        private DeviceStatus Read(out byte addr, out byte seq, out byte cmd, out byte status, out byte[] payload) {
            lock (SerialLocker) {
                DeviceStatus ret = serial.ReadLenByOffset(1, out byte[] data, false, true);
                if (ret != DeviceStatus.OK) {
                    addr = 0;
                    seq = 0;
                    cmd = 0;
                    status = 0;
                    payload = null;
                    return ret;
                }
                // data[0] = sync
                // data[1] = full packet length
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
            DeviceStatus ret;
            byte status;
            try {
                ret = this.WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out status);
            }catch (ArgumentException) {
                Log.WriteError("There was an error reading from the reset response. You may have connected the TXD1/RXD2 lines incorrectly. (or there may be a different problem)");
                throw;
            }
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
        /// <param name="version">The reader's firmware version (ex. "TN32MSEC003S F/W Ver1.2"), null if the reader is a gen 2 or newer reader, or on failure</param>
        /// <param name="version_byte">The reader's firmware version (ex. 0x93), 0 if the reader is a gen 1 reader, or on failure</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFWVersion(out string version, out byte version_byte) {
            Log.Write("GetFWVersion");
            DeviceStatus ret = Write(new SProtFrame(new ReqPacketGetFWVersion())); // special handling here since data could be returned in two variants
            Read(out _, out _, out _, out byte status, out byte[] payload);
            SetLastError(ret, status);
            if (ret == DeviceStatus.OK) {
                if (payload.Length == 1) {
                    RespPacketGetFWVersion1Byte resp = StructUtils.FromBytes<RespPacketGetFWVersion1Byte>(payload);
                    version = null;
                    version_byte = resp.version;
                } else {
                    RespPacketGetFWVersion resp = StructUtils.FromBytes<RespPacketGetFWVersion>(payload);
                    version = resp.version;
                    version_byte = 0;
                }
            } else {
                version = null;
                version_byte = 0;
            }
            return ret;
        }

        /// <summary>
        /// Queries the card reader's firmware checksum.
        /// </summary>
        /// <param name="checksum">The reader's firmware checksum</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFWChecksum(out ushort checksum) {
            Log.Write("GetFWChecksum");
            DeviceStatus ret = this.WriteAndRead(new ReqPacketGetFirmwareChecksum(), out RespPacketGetFirmwareChecksum resp, out byte status);
            SetLastError(ret, status);
            if (ret == DeviceStatus.OK) {
                checksum = (ushort)(resp.fw_checksum_b2 << 8 | resp.fw_checksum_b1);
            } else {
                checksum = 0;
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
            radioType = type;
            DeviceStatus ret = this.WriteAndRead(new ReqPacketRadioOn() { type = (byte)type }, out RespPacketRadioOn _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Disables the reader's radio.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus RadioOff() {
            Log.Write("RadioOff");
            radioType = null;
            DeviceStatus ret = this.WriteAndRead(new ReqPacketRadioOff(), out RespPacketRadioOff _, out byte status);
            return SetLastError(ret, status);
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            Log.Write("Disconnecting on Port " + Port);
            lock (SerialLocker) {
                serial?.Disconnect();
            }
            Log.Write("Disconnected on Port " + Port);
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
            if (radioType == null) {
                DeviceStatus ret = RadioOn(RadioOnType.Both);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }
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
            int count = 0;
            do {
                try {
                    ret = Poll();
                    SetLastError(ret);
                    if (ret != DeviceStatus.OK) {
                        break;
                    }
                    count++;
                    if (FlashLEDsWhilePolling && count % 4 == 0) {
                        ret = LEDSetColor(count % 8 == 0 ? Color.White : Color.Black);
                        if (ret != DeviceStatus.OK) {
                            break;
                        }
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

                    if (type == 0x10 && size == 4) { // MIFARE UID
                        byte[] id = new byte[size];
                        Array.Copy(data, offset, id, 0, size);
                        offset += size;
                        Log.Write("Found a MIFARE UID: \n" + Hex.Dump(id));

                        lastMifareCardLUID = BitConverter.ToUInt32(id, 0);

                        ret = ReadMIFARECardID(lastMifareCardLUID, out byte[] cardid);
                        SetLastError(ret, resp?.Status);
                        if (ret != DeviceStatus.OK) {
                            return ret;
                        }

                        lastReadCardType = CardType.MIFARE;
                        lastReadCardUID = cardid;
                        Log.Write("Found a MIFARE card: \n" + Hex.Dump(cardid));
                    } else if (type == 0x20) { // FeliCa
                        if (size == 0x10) {
                            if (!FeliCaIncludePMM) {
                                size = 0x8;
                            }
                            byte[] id = new byte[size];
                            Array.Copy(data, offset, id, 0, size);
                            offset += 0x10;
                            lastReadCardUID = id;
                            lastReadCardType = CardType.FeliCa;
                            Log.Write("Found a FeliCa card (PMm reading is " + FeliCaIncludePMM + "): \n" + Hex.Dump(id));
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

        /// <inheritdoc/>
        public override DeviceStatus LEDReset() {
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

        /// <inheritdoc />
        public override DeviceStatus LEDSetColor(byte red, byte green, byte blue) {
            Log.Write("LEDSetColor");
            DeviceStatus ret = Write(new ReqPacketLEDSetColor() {
                red = red,
                green = green,
                blue = blue
            }.ToFrame(LED_BOARD_ADDRESS));
            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public override void ClearCard() {
            lastReadCardType = null;
            lastReadCardUID = null;
        }

        internal unsafe DeviceStatus SetMIFAREParameters() {

            byte[] k1 = new byte[] { 0x57, 0x43, 0x43, 0x46, 0x76, 0x32 };
            byte[] k2 = new byte[] { 0x60, 0x90, 0xd0, 0x06, 0x32, 0xf5 };
            DeviceStatus ret;

            Log.Write("Set Sega Key");
            ReqPacketMIFARESetKeySega req = new ReqPacketMIFARESetKeySega();
            StructUtils.Copy(k1, req.key, k1.Length);
            ret = SetLastError(this.WriteAndRead(req, out RespPacketMIFARESetKeySega _, out byte status), status);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            Log.Write("Set Namco Key");
            ReqPacketMIFARESetKeyNamco req2 = new ReqPacketMIFARESetKeyNamco();
            StructUtils.Copy(k2, req2.key, k2.Length);
            ret = SetLastError(this.WriteAndRead(req2, out RespPacketMIFARESetKeyNamco _, out status), status);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            return ret;
        }

        private DeviceStatus PrepareMIFARECommunication(uint uid) {
            Log.Write("Select Mifare (" + uid + ")");
            DeviceStatus ret = SetLastError(this.WriteAndRead(new ReqPacketSelectMIFARE() {
                uid = uid
            }, out RespPacketSelectMIFARE _, out byte status), status);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            Log.Write("Authenticate Mifare (" + uid + ")");
            ret = SetLastError(this.WriteAndRead(new ReqPacketAuthenticateMIFARE() {
                uid = uid,
                unk = 0x03
            }, out RespPacketAuthenticateMIFARE _, out status), status);
            return ret;
        }

        private unsafe DeviceStatus ReadMIFAREBlock(uint uid, byte block_no, out byte* block_content) {
            Log.Write("Read Mifare Block (" + uid + ", "+block_no+")");
            DeviceStatus ret = SetLastError(this.WriteAndRead(new ReqPacketReadMIFARE() {
                uid = uid,
                block = block_no,
            }, out RespPacketAuthenticateMIFARE block, out byte status), status);
            block_content = block.data;
            return ret;
        }

        /// <summary>
        /// Reads the card ID from a MIFARE tag.
        /// </summary>
        /// <param name="uid">The card UID to read from.</param>
        /// <param name="cardid">A 10 byte long array with the card ID.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus ReadMIFARECardID(uint uid, out byte[] cardid) {
            cardid = null;

            DeviceStatus ret = PrepareMIFARECommunication(uid);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            ret = ReadMIFAREBlock(uid, 2, out byte* block);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            cardid = new byte[10];

            StructUtils.Copy(block, 6, cardid, 0, 10);
            return ret;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Reads the binary blob contained on a e-money/Thinca authentication card.
        /// </summary>
        /// <param name="uid">The card UID to read from.</param>
        /// <param name="decryption_key">The 0x40 byte long decryption key for this card. Can be null to simply check if the scanned card is a e-money authentication card.</param>
        /// <param name="proxy_type">The proxy_type stored on the card.</param>
        /// <param name="unk1">Unkown byte stored on the card.</param>
        /// <param name="store_card_id">Data stored on the card.</param>
        /// <param name="store_branch_id">Data stored on the card.</param>
        /// <param name="merchant_code">Data stored on the card.</param>
        /// <param name="passphrase">The password for the e-money authentication server server-supplied client certificate.</param>
        /// <exception cref="ArgumentException">If the given decryption key is not null and not 0x40 bytes long.</exception>
        /// <returns>
        /// * <see cref="DeviceStatus.OK"/> on success.<br />
        /// * <see cref="DeviceStatus.ERR_INCOMPATIBLE"/> if the scanned card is not an e-money auth card.<br />
        /// * <see cref="DeviceStatus.ERR_CRYPT"/> if the decryption key is invalid. In this case, <paramref name="proxy_type"/> and <paramref name="unk1"/> will however be set.<br />
        /// * Any other DeviceStatus on misc. reader failures.</returns>
        public unsafe DeviceStatus ReadMIFAREeMoneyAuthentication(uint uid, byte[] decryption_key, out byte proxy_type, out byte unk1, out String store_card_id, out String merchant_code, out UInt128 store_branch_id, out String passphrase) {
            proxy_type = 0;
            unk1 = 0;
            store_branch_id = 0;
            store_card_id = null;
            passphrase = null;
            merchant_code = null;

            if (decryption_key != null && decryption_key.Length != 0x40) {
                throw new ArgumentException("Decryption key has invalid length: " + decryption_key.Length);
            }

            DeviceStatus ret = PrepareMIFARECommunication(uid);
            if (ret != DeviceStatus.OK) {
                return ret;
            }

            ret = ReadMIFAREBlock(uid, 3, out byte* header);
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            if (header[0] != 'T' || header[1] != 'C') {
                Log.WriteError("Scanned card is not a e-money authentication card!");
                return DeviceStatus.ERR_INCOMPATIBLE;
            }

            proxy_type = header[2];
            unk1 = header[3];

            if (decryption_key == null) {
                return ret;
            }

            const int AUTH_BLOCK_SIZE = 16;
            byte[] blocks = new byte[] { 5, 6, 8, 9, 10, 12, 13, 14 };
            byte[] encrypted = new byte[160];

            for (int i = 0; i < blocks.Length; i++) {

                ret = ReadMIFAREBlock(uid, blocks[i], out byte* content);
                if (ret != DeviceStatus.OK) {
                    return ret;
                }

                int offset = i * 16;
                StructUtils.Copy(content, AUTH_BLOCK_SIZE, encrypted, offset, AUTH_BLOCK_SIZE);
            }

            byte[] authcardid = new byte[0x20];
            byte[] uidbytes = BitConverter.GetBytes(uid);
            for (int i = 0; i < authcardid.Length; i += uidbytes.Length) {
                Array.Copy(uidbytes, 0, authcardid, i, uidbytes.Length);
            }

            for (int i = 0; i < decryption_key.Length; i++) {
                decryption_key[i] ^= 0x1C;
            }

            try {
                HMACSHA256 hash = new HMACSHA256(decryption_key);
                byte[] hmac = hash.ComputeHash(authcardid);
                byte[] iv = new byte[16];
                for (int i = 0; i < 16; i++) {
                    iv[i] = (byte)(hmac[i + 16] ^ hmac[i]);
                }

                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = decryption_key;
                aes.IV = iv;
                ICryptoTransform dec = aes.CreateDecryptor();
                byte[] decrypted = dec.TransformFinalBlock(encrypted, 0, encrypted.Length);

                if (decrypted.Length != 0x41) {
                    throw new CryptographicException("Decrypted card data has invalid size: " + decrypted.Length);
                }
                if (decrypted[decrypted.Length - 1] != 0x00) {
                    throw new CryptographicException("Decrypted card data failed verification check");
                }

                store_card_id = Encoding.ASCII.GetString(decrypted, 0, 0x10);
                merchant_code = Encoding.ASCII.GetString(decrypted, 0x10, 0x14);
                store_branch_id = Unsafe.ReadUnaligned<UInt128>(ref decrypted[0x24]);
                passphrase = Encoding.ASCII.GetString(decrypted, 0x30, 0x10);

            } catch (Exception ex) {
                Log.WriteFault(ex, "Cryptographic error while decrypting data from card");
                return DeviceStatus.ERR_CRYPT;
            }

            return ret;
        }
#endif

        /// <summary>
        /// Returns the last MIFARE card LUID.
        /// </summary>
        /// <returns>The last read MIFARE LUID or null if the last card was not a MIFARE card or no card was read.</returns>
        public uint? GetMIFARECardLUID() {
            if (lastReadCardType != CardType.MIFARE) {
                return null;
            }
            if (lastMifareCardLUID > 0) {
                return lastMifareCardLUID;
            } else {
                return null;
            }
        }

    }
}
