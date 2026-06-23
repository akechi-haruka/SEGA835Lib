using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;
using Haruka.Arcade.SEGA835Lib.Serial;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396 {
    /// <summary>
    /// A Aime 837-15396 (Generation 3) card reader.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class AimeCardReader15396 : CardReader {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(AimeCardReader15396));

        /// <summary>
        /// Whether or not to include the PMM part when a FeliCa is read. If true, <see cref="GetCardUid"/> will return 16 bytes (8 bytes IDm + 8 bytes PMm), if false, only the 8 bytes IDm are returned.
        /// </summary>
        public bool FeliCaIncludePmm { get; set; } = false;

        /// <summary>
        /// Whether the card reader LEDs should flash while card scanning is in progress.
        /// </summary>
        public bool FlashLedsWhilePolling { get; set; }

        /// <summary>
        /// The result of the last execution of <see cref="Poll"/>. This will be set when <see cref="IsPolling"/> becomes false.
        /// </summary>
        public DeviceStatus PollingResult { get; private set; } = DeviceStatus.ErrorNotInitialized;

        private byte[] lastReadCardUid;
        private CardType? lastReadCardType;
        private Thread pollingThread;
        private RadioOnType? radioType;
        private uint lastMifareCardLuid;

        /// <summary>
        /// Initializes a new card reader on the specified port.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        /// <param name="highBaudrate">Whether to use high baudrate (115200). This seems to depend on dipswitches on the reader?</param>
        public AimeCardReader15396(int port, bool highBaudrate = true) : base(port, highBaudrate ? 115200 : 38400) {
        }

        /// <inheritdoc/>
        public override DeviceStatus Connect() {
            lock (SerialLocker) {
                if (Serial.IsConnected()) {
                    return DeviceStatus.Ok;
                }

                lastReadCardUid = null;
                radioType = null;
                lastMifareCardLuid = 0;
                LOG.LogInformation("Connecting on Port " + Port);
                if (!Serial.Connect()) {
                    return DeviceStatus.ErrorNotConnected;
                }
            }

            DeviceStatus ret = Reset();
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            return SetMifareParameters();
        }

        private DeviceStatus Write(byte addr, byte seq, byte cmd, byte[] payload) {
            lock (SerialLocker) {
                byte[] packet = new byte[payload.Length + 5];
                if (packet.Length > 0xFF) {
                    return DeviceStatus.ErrorPayloadTooLarge;
                }

                packet[0] = (byte)packet.Length;
                packet[1] = addr;
                packet[2] = seq;
                packet[3] = cmd;
                packet[4] = (byte)payload.Length;
                Array.Copy(payload, 0, packet, 5, payload.Length);
                return Serial.Write(packet);
            }
        }

        private DeviceStatus Read(out byte addr, out byte seq, out byte cmd, out byte status, out byte[] payload) {
            lock (SerialLocker) {
                DeviceStatus ret = Serial.ReadLenByOffset(1, out byte[] data, false, true);
                if (ret != DeviceStatus.Ok) {
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
                    ret = DeviceStatus.ErrorDevice;
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
            if (ret != DeviceStatus.Ok) {
                recv = null;
                return ret;
            }

            recv = new SProtFrame(seq, cmd, addr, status, payload);
            return ret;
        }

        /// <summary>
        /// Resets the device state. This is implicitely called on <see cref="Connect"/>.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, or if the reader was already reset (which will log a warning), or any other DeviceStatus on failure.</returns>
        public DeviceStatus Reset() {
            LOG.LogInformation("Reset");
            DeviceStatus ret;
            byte status;
            try {
                ret = WriteAndRead(new ReqPacketReset(), out RespPacketReset _, out status);
            } catch (ArgumentException) {
                LOG.LogError("There was an error reading from the reset response. You may have connected the TXD1/RXD2 lines incorrectly. (or there may be a different problem)");
                throw;
            }

            if (ret == DeviceStatus.ErrorDevice) { // error on double reset, ignore
                return SetLastError(DeviceStatus.Ok, status);
            }

            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's hardware version.
        /// </summary>
        /// <param name="version">The reader's hardware version (ex. "TN32MSEC003S H/W Ver3.0") or null on failure</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetHardwareVersion(out string version) {
            LOG.LogInformation("GetHWVersion");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetHardwareVersion(), out RespPacketGetHardwareVersion resp, out byte status);
            SetLastError(ret, status);
            version = ret == DeviceStatus.Ok ? resp.version : null;

            return ret;
        }

        /// <summary>
        /// Queries the card reader's firmware version.
        /// </summary>
        /// <param name="version">The reader's firmware version (ex. "TN32MSEC003S F/W Ver1.2"), null if the reader is a gen 2 or newer reader, or on failure</param>
        /// <param name="versionByte">The reader's firmware version (ex. 0x93), 0 if the reader is a gen 1 reader, or on failure</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFirmwareVersion(out string version, out byte versionByte) {
            LOG.LogInformation("GetFWVersion");
            DeviceStatus ret = Write(new SProtFrame(new ReqPacketGetFirmwareVersion())); // special handling here since data could be returned in two variants
            Read(out _, out _, out _, out byte status, out byte[] payload);
            SetLastError(ret, status);
            if (ret == DeviceStatus.Ok) {
                if (payload.Length == 1) {
                    RespPacketGetFirmwareVersion1Byte resp = StructUtils.FromBytes<RespPacketGetFirmwareVersion1Byte>(payload);
                    version = null;
                    versionByte = resp.version;
                } else {
                    RespPacketGetFirmwareVersion resp = StructUtils.FromBytes<RespPacketGetFirmwareVersion>(payload);
                    version = resp.version;
                    versionByte = 0;
                }
            } else {
                version = null;
                versionByte = 0;
            }

            return ret;
        }

        /// <summary>
        /// Queries the card reader's firmware checksum.
        /// </summary>
        /// <param name="checksum">The reader's firmware checksum</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus GetFirmwareChecksum(out ushort checksum) {
            LOG.LogInformation("GetFWChecksum");
            DeviceStatus ret = WriteAndRead(new ReqPacketGetFirmwareChecksum(), out RespPacketGetFirmwareChecksum resp, out byte status);
            SetLastError(ret, status);
            if (ret == DeviceStatus.Ok) {
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
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus RadioOn(RadioOnType type) {
            LOG.LogInformation("RadioOn(" + type + ")");
            radioType = type;
            DeviceStatus ret = WriteAndRead(new ReqPacketRadioOn() { type = (byte)type }, out RespPacketRadioOn _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Disables the reader's radio.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus RadioOff() {
            LOG.LogInformation("RadioOff");
            radioType = null;
            DeviceStatus ret = WriteAndRead(new ReqPacketRadioOff(), out RespPacketRadioOff _, out byte status);
            return SetLastError(ret, status);
        }

        /// <inheritdoc/>
        public override DeviceStatus Disconnect() {
            LOG.LogInformation("Disconnecting on Port " + Port);
            lock (SerialLocker) {
                Serial?.Disconnect();
            }

            LOG.LogInformation("Disconnected on Port " + Port);
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override byte[] GetCardUid() {
            return lastReadCardUid;
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
            return lastReadCardUid != null;
        }

        /// <inheritdoc/>
        public override DeviceStatus StartPolling() {
            LOG.LogInformation("Starting polling of Aime reader on port " + Port);
            if (IsPolling()) {
                return DeviceStatus.Ok;
            }

            if (radioType == null) {
                DeviceStatus ret = RadioOn(RadioOnType.Both);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }
            }

            pollingThread = new Thread(PollT);
            pollingThread.Start();
            return DeviceStatus.Ok;
        }

        /// <inheritdoc/>
        public override bool IsPolling() {
            return pollingThread != null;
        }

        private void PollT() {
            DeviceStatus ret = DeviceStatus.Ok;
            PollingResult = DeviceStatus.Ok;
            int count = 0;
            do {
                try {
                    ret = Poll();
                    SetLastError(ret);
                    if (ret != DeviceStatus.Ok) {
                        break;
                    }

                    count++;
                    if (FlashLedsWhilePolling && count % 4 == 0) {
                        ret = LedSetColor(count % 8 == 0 ? Color.White : Color.Black);
                        if (ret != DeviceStatus.Ok) {
                            break;
                        }
                    }

                    Thread.Sleep(250);
                } catch (ThreadInterruptedException) {
                    break;
                } catch (Exception ex) {
                    LOG.LogCritical(ex, "Internal error while polling");
                    ret = DeviceStatus.ErrorOther;
                }
            } while (pollingThread != null);

            LOG.LogInformation("Polling thread exited of Aime reader on port " + Port);
            if (ret != DeviceStatus.Ok) {
                LOG.LogWarning("Last Error Code before polling was stopped: " + ret);
            }

            PollingResult = ret;
            pollingThread = null;
        }

        private DeviceStatus Poll() {
            DeviceStatus ret = WriteAndRead(new ReqPacketPoll().ToFrame(), out SProtFrame resp);
            SetLastError(ret, resp?.Status);
            if (resp != null && resp.Payload != null && resp.Payload.Length >= 1) {
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
                        LOG.LogInformation("Found a MIFARE UID: \n" + Hex.Dump(id));

                        lastMifareCardLuid = BitConverter.ToUInt32(id, 0);

                        ret = ReadMifareCardID(lastMifareCardLuid, out byte[] cardid);
                        SetLastError(ret, resp.Status);
                        if (ret != DeviceStatus.Ok) {
                            return ret;
                        }

                        lastReadCardType = CardType.Mifare;
                        lastReadCardUid = cardid;
                        LOG.LogInformation("Found a MIFARE card: \n" + Hex.Dump(cardid));
                    } else if (type == 0x20) { // FeliCa
                        if (size == 0x10) {
                            if (!FeliCaIncludePmm) {
                                size = 0x8;
                            }

                            byte[] id = new byte[size];
                            Array.Copy(data, offset, id, 0, size);
                            offset += 0x10;
                            lastReadCardUid = id;
                            lastReadCardType = CardType.FeliCa;
                            LOG.LogInformation("Found a FeliCa card (PMm reading is " + FeliCaIncludePmm + "): \n" + Hex.Dump(id));
                        } else {
                            ret = DeviceStatus.ErrorIncompatible;
                        }
                    } else {
                        ret = DeviceStatus.ErrorIncompatible;
                    }
                }
            }

            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus StopPolling() {
            DeviceStatus ret = DeviceStatus.Ok;
            if (IsPolling()) {
                LOG.LogInformation("Stopping polling of Aime reader on port " + Port);
                try {
                    pollingThread.Interrupt();
                    pollingThread.Join();
                    pollingThread = null;
                } catch (Exception ex) {
                    LOG.LogCritical(ex, "Failed to stop polling thread of card reader");
                    return DeviceStatus.ErrorOther;
                } finally {
                    ret = RadioOff();
                }
            }

            return ret;
        }

        /// <inheritdoc/>
        public override DeviceStatus LedReset() {
            LOG.LogInformation("LEDReset");
            DeviceStatus ret = WriteAndRead(new ReqPacketLedReset(), out RespPacketLedReset _, out byte status);
            return SetLastError(ret, status);
        }

        /// <summary>
        /// Queries the card reader's LED board hardware version.
        /// </summary>
        /// <param name="version">The reader's LED board hardware version (ex. TODO) or null on failure.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LedGetHwVersion(out string version) {
            LOG.LogInformation("LEDGetHWVersion");
            DeviceStatus ret = WriteAndRead(new ReqPacketLedHardwareVersion(), out RespPacketLedHardwareVersion resp, out byte status);
            SetLastError(ret, status);
            version = ret == DeviceStatus.Ok ? resp.version : null;

            return ret;
        }

        /// <summary>
        /// Queries the card reader's LED board information.
        /// </summary>
        /// <param name="info">The reader's LED board information or null on failure.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LedGetInfo(out string info) {
            LOG.LogInformation("LEDGetInfo");
            DeviceStatus ret = WriteAndRead(new ReqPacketLedGetInfo(), out RespPacketLedGetInfo resp, out byte status);
            SetLastError(ret, status);
            info = ret == DeviceStatus.Ok ? resp.info : null;

            return ret;
        }

        /// <summary>
        /// Sets the card reader's LED channels to the specified value.
        /// </summary>
        /// <param name="strength">The LED strength [0-255]</param>
        /// <param name="red">true if the strength should be applied to the red channel.</param>
        /// <param name="green">true if the strength should be applied to the green channel.</param>
        /// <param name="blue">true if the strength should be applied to the blue channel.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LedSetChannels(byte strength, bool red, bool green, bool blue) {
            LOG.LogInformation("LEDSetChannels");
            DeviceStatus ret = Write(new ReqPacketLedSetChannel() {
                rgb = (byte)((red ? 1 << 0 : 0) | (green ? 1 << 1 : 0) | (blue ? 1 << 2 : 0)),
                value = strength
            }.ToFrame());
            return SetLastError(ret);
        }

        /// <summary>
        /// Sets the card reader's LED color.
        /// </summary>
        /// <param name="c">The color to set.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public DeviceStatus LedSetColor(Color c) {
            return LedSetColor(c.R, c.G, c.B);
        }

        /// <inheritdoc />
        public override DeviceStatus LedSetColor(byte red, byte green, byte blue) {
            LOG.LogInformation("LEDSetColor");
            DeviceStatus ret = Write(new ReqPacketLedSetColor() {
                red = red,
                green = green,
                blue = blue
            }.ToFrame());
            return SetLastError(ret);
        }

        /// <inheritdoc/>
        public override void ClearCard() {
            lastReadCardType = null;
            lastReadCardUid = null;
        }

        internal unsafe DeviceStatus SetMifareParameters() {
            byte[] k1 = { 0x57, 0x43, 0x43, 0x46, 0x76, 0x32 };
            byte[] k2 = { 0x60, 0x90, 0xd0, 0x06, 0x32, 0xf5 };

            LOG.LogInformation("Set Sega Key");
            ReqPacketMifareSetKeySega req = new ReqPacketMifareSetKeySega();
            StructUtils.Copy(k1, req.key, k1.Length);
            DeviceStatus ret = SetLastError(WriteAndRead(req, out RespPacketMifareSetKeySega _, out byte status), status);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            LOG.LogInformation("Set Namco Key");
            ReqPacketMifareSetKeyNamco req2 = new ReqPacketMifareSetKeyNamco();
            StructUtils.Copy(k2, req2.key, k2.Length);
            ret = SetLastError(WriteAndRead(req2, out RespPacketMifareSetKeyNamco _, out status), status);

            return ret;
        }

        internal unsafe DeviceStatus SetMifareParametersEMoney() {
            byte[] k = { 0x74, 0x68, 0x69, 0x6e, 0x63, 0x61 };

            LOG.LogInformation("Set E-Money Key");
            ReqPacketMifareSetKeySega req = new ReqPacketMifareSetKeySega();
            StructUtils.Copy(k, req.key, k.Length);
            return SetLastError(WriteAndRead(req, out RespPacketMifareSetKeySega _, out byte status), status);
        }

        private DeviceStatus PrepareMifareCommunication(uint uid) {
            LOG.LogInformation("Select Mifare (" + uid + ")");
            DeviceStatus ret = SetLastError(WriteAndRead(new ReqPacketSelectMifare() {
                uid = uid
            }, out RespPacketSelectMifare _, out byte status), status);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            LOG.LogInformation("Authenticate Mifare (" + uid + ")");
            ret = SetLastError(WriteAndRead(new ReqPacketAuthenticateMifare() {
                uid = uid,
                unk = 0x03
            }, out RespPacketAuthenticateMifare _, out status), status);
            return ret;
        }

        private unsafe DeviceStatus ReadMifareBlock(uint uid, byte blockNo, out byte* blockContent) {
            LOG.LogInformation("Read Mifare Block (" + uid + ", " + blockNo + ")");
            DeviceStatus ret = SetLastError(WriteAndRead(new ReqPacketReadMifare() {
                uid = uid,
                block = blockNo,
            }, out RespPacketAuthenticateMifare block, out byte status), status);
            blockContent = block.data;
            return ret;
        }

        /// <summary>
        /// Reads the card ID from a MIFARE tag.
        /// </summary>
        /// <param name="uid">The card UID to read from.</param>
        /// <param name="cardid">A 10 byte long array with the card ID.</param>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or any other DeviceStatus on failure.</returns>
        public unsafe DeviceStatus ReadMifareCardID(uint uid, out byte[] cardid) {
            cardid = null;

            DeviceStatus ret = PrepareMifareCommunication(uid);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            ret = ReadMifareBlock(uid, 2, out byte* block);
            if (ret != DeviceStatus.Ok) {
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
        /// <param name="decryptionKey">The 0x40 byte long decryption key for this card. Can be null to simply check if the scanned card is a e-money authentication card.</param>
        /// <param name="proxyType">The proxy_type stored on the card.</param>
        /// <param name="unk1">Unkown byte stored on the card.</param>
        /// <param name="storeCardID">Data stored on the card.</param>
        /// <param name="storeBranchID">Data stored on the card.</param>
        /// <param name="merchantCode">Data stored on the card.</param>
        /// <param name="passphrase">The password for the e-money authentication server server-supplied client certificate.</param>
        /// <exception cref="ArgumentException">If the given decryption key is not null and not 0x40 bytes long.</exception>
        /// <returns>
        /// * <see cref="DeviceStatus.Ok"/> on success.<br />
        /// * <see cref="DeviceStatus.ErrorIncompatible"/> if the scanned card is not an e-money auth card.<br />
        /// * <see cref="DeviceStatus.ErrorCrypt"/> if the decryption key is invalid. In this case, <paramref name="proxyType"/> and <paramref name="unk1"/> will however be set.<br />
        /// * Any other DeviceStatus on misc. reader failures.</returns>
        public unsafe DeviceStatus ReadMifarEeMoneyAuthentication(uint uid, byte[] decryptionKey, out byte proxyType, out byte unk1, out String storeCardID, out String merchantCode, out UInt128 storeBranchID, out String passphrase) {
            proxyType = 0;
            unk1 = 0;
            storeBranchID = 0;
            storeCardID = null;
            passphrase = null;
            merchantCode = null;

            if (decryptionKey != null && decryptionKey.Length != 0x40) {
                throw new ArgumentException("Decryption key has invalid length: " + decryptionKey.Length);
            }

            DeviceStatus ret = PrepareMifareCommunication(uid);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            ret = SetMifareParametersEMoney();
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            ret = ReadMifareBlock(uid, 3, out byte* header);
            if (ret != DeviceStatus.Ok) {
                return ret;
            }

            if (header[0] != 'T' || header[1] != 'C') {
                LOG.LogError("Scanned card is not a e-money authentication card!");
                return DeviceStatus.ErrorIncompatible;
            }

            proxyType = header[2];
            unk1 = header[3];

            if (decryptionKey == null) {
                return ret;
            }

            const int authBlockSize = 16;
            byte[] blocks = new byte[] { 5, 6, 8, 9, 10, 12, 13, 14 };
            byte[] encrypted = new byte[160];

            for (int i = 0; i < blocks.Length; i++) {
                ret = ReadMifareBlock(uid, blocks[i], out byte* content);
                if (ret != DeviceStatus.Ok) {
                    return ret;
                }

                int offset = i * 16;
                StructUtils.Copy(content, authBlockSize, encrypted, offset, authBlockSize);
            }

            byte[] authcardid = new byte[0x20];
            byte[] uidbytes = BitConverter.GetBytes(uid);
            for (int i = 0; i < authcardid.Length; i += uidbytes.Length) {
                Array.Copy(uidbytes, 0, authcardid, i, uidbytes.Length);
            }

            for (int i = 0; i < decryptionKey.Length; i++) {
                decryptionKey[i] ^= 0x1C;
            }

            try {
                HMACSHA256 hash = new HMACSHA256(decryptionKey);
                byte[] hmac = hash.ComputeHash(authcardid);
                byte[] iv = new byte[16];
                for (int i = 0; i < 16; i++) {
                    iv[i] = (byte)(hmac[i + 16] ^ hmac[i]);
                }

                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = decryptionKey;
                aes.IV = iv;
                ICryptoTransform dec = aes.CreateDecryptor();
                byte[] decrypted = dec.TransformFinalBlock(encrypted, 0, encrypted.Length);

                if (decrypted.Length != 0x41) {
                    throw new CryptographicException("Decrypted card data has invalid size: " + decrypted.Length);
                }

                if (decrypted[^1] != 0x00) {
                    throw new CryptographicException("Decrypted card data failed verification check");
                }

                storeCardID = Encoding.ASCII.GetString(decrypted, 0, 0x10);
                merchantCode = Encoding.ASCII.GetString(decrypted, 0x10, 0x14);
                storeBranchID = Unsafe.ReadUnaligned<UInt128>(ref decrypted[0x24]);
                passphrase = Encoding.ASCII.GetString(decrypted, 0x30, 0x10);
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Cryptographic error while decrypting data from card");
                return DeviceStatus.ErrorCrypt;
            }

            return ret;
        }
#endif

        /// <summary>
        /// Returns the last MIFARE card LUID.
        /// </summary>
        /// <returns>The last read MIFARE LUID or null if the last card was not a MIFARE card or no card was read.</returns>
        public uint? GetMifareCardLuid() {
            if (lastReadCardType != CardType.Mifare) {
                return null;
            }

            if (lastMifareCardLuid > 0) {
                return lastMifareCardLuid;
            }

            return null;
        }
    }
}