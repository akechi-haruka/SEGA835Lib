using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Misc {
    public class GP1232A02A_VFD : Device {

        private SerialComm serial;
        public int Port { get; private set; }
        public VFDEncoding EncodingSetting { get; private set; }

        public GP1232A02A_VFD(int port = 1) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Port = port;
            serial = new SerialComm(port, 115200, 1000, true, true);
            EncodingSetting = VFDEncoding.SHIFT_JIS;
        }

        public override DeviceStatus Connect() {
            if (serial != null && serial.IsConnected()) {
                return DeviceStatus.OK;
            }
            Log.Write("Connecting on Port " + Port);
            if (!serial.Connect()) {
                return DeviceStatus.ERR_NOT_CONNECTED;
            }
            return Reset();
        }

        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        public override string GetDeviceModel() {
            return "GP1232A02";
        }

        public override string GetName() {
            return "Futaba GP1232A02A VFD";
        }

        private DeviceStatus Read(int len, out byte[] bytes) {
            return serial.Read(len, out bytes);
        }

        private DeviceStatus Write(byte[] payload) {
            DeviceStatus ret = serial.Write(new byte[] { 0x1B });
            if (ret != DeviceStatus.OK) {
                return SetLastError(ret);
            }
            return SetLastError(serial.Write(payload));
        }

        public DeviceStatus Reset() {
            Log.Write("Reset");
            return SetLastError(Write(new byte[] { 0x0B }));
        }

        public DeviceStatus ClearScreen() {
            Log.Write("Clear Screen");
            return SetLastError(Write(new byte[] { 0x0C }));
        }

        public DeviceStatus SetBrightness(VFDBrightnessLevel level) {
            Log.Write("Set Brightness: " + level);
            return SetLastError(Write(new byte[] { 0x0C, (byte)level }));
        }
        
        public DeviceStatus SetOn(bool b) {
            Log.Write("Set On: " + b);
            return SetLastError(Write(new byte[] { 0x21, (byte)(b ? 0x01 : 0x00) }));
        }

        public DeviceStatus SetWindowHScroll(ushort x) {
            Log.Write("Set Window H-Scroll: " + x);
            if (x >= 512) {
                throw new ArgumentException("x (" + x + ") must be within [0,512)");
            }
            return SetLastError(Write(new byte[] { 0x22, (byte)(x >> 8), (byte)x }));
        }

        public DeviceStatus DrawBitmap(ushort x, byte y, ushort w, ushort h, byte[] image) {
            Log.Write("Draw Bitmap " + w + "x" + h + "@" + x + "/" + y);
            if (w >= 512) {
                throw new ArgumentException("w (" + w + ") must be within [0,512)");
            }
            if (checked(x + w) > 512) {
                throw new ArgumentException("x + w (" + x + " + " + w + ") must be within [0,512]");
            }
            if (h >= 32) {
                throw new ArgumentException("h (" + h + ") must be within [0,32)");
            }
            if (checked(y + h) > 32) {
                throw new ArgumentException("y + h (" + y + " + " + h + ") must be within [0,32]");
            }
            if (y % 8 != 0) {
                throw new ArgumentException("y (" + y + ") must be a multiple of 8");
            }
            if (h % 8 != 0) {
                throw new ArgumentException("h (" + h + ") must be a multiple of 8");
            }
            if (image.Length != h * w) {
                throw new ArgumentException("h * w (" + h + " * " + w + ") is not equal to image byte array length (" + image.Length + ")");
            }
            byte yb = (byte)(y / 8);
            byte hb = (byte)(h / 8);
            DeviceStatus ret = SetLastError(Write(new byte[] { 0x2E, (byte)(x >> 8), (byte)x, yb, (byte)(w >> 8), (byte)w, (byte)(yb + hb - 1) }));
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            return SetLastError(Write(image));
        }

        public DeviceStatus SetCursorPosition(ushort x, byte y) {
            Log.Write("Set Cursor: " + x + "/" + y);
            if (x >= 512) {
                throw new ArgumentException("x ("+x+") must be within [0,512)");
            }
            if (y >= 32) {
                throw new ArgumentException("y (" + y + ") must be within [0,32)");
            }
            if (y % 8 != 0) {
                throw new ArgumentException("y (" + y + ") must be a multiple of 8");
            }
            return SetLastError(Write(new byte[] { 0x30, (byte)(x >> 8), (byte)x, (byte)(y / 8) }));
        }

        public DeviceStatus SetEncoding(VFDEncoding encoding) {
            Log.Write("Set Encoding: " + encoding);
            return SetLastError(Write(new byte[] { 0x32, (byte)encoding }));
        }

        public DeviceStatus SetTextPosition(ushort x, byte y, ushort w) {
            Log.Write("Set Text: " + x + "/" + y + "("+w+")");
            if (x >= 512) {
                throw new ArgumentException("x (" + x + ") must be within [0,512)");
            }
            if (checked(x + w) > 512) {
                throw new ArgumentException("x + w (" + x + " + " + w + ") must be within [0,512]");
            }
            if (y >= 32) {
                throw new ArgumentException("y (" + y + ") must be within [0,32)");
            }
            if (y % 8 != 0) {
                throw new ArgumentException("y (" + y + ") must be a multiple of 8");
            }
            return SetLastError(Write(new byte[] { 0x40, (byte)(x >> 8), (byte)x, (byte)(y / 8), (byte)(w >> 8), (byte)w, 0x00 }));
        }

        public DeviceStatus SetTextScrollSpeed(VFDTextScrollSpeed speed) {
            Log.Write("Set Text Scroll Speed: " + speed);
            return SetLastError(Write(new byte[] { 0x41, (byte)speed }));
        }

        public DeviceStatus WriteScrollingText(string str, bool truncate = false) {
            Log.Write("Write Text: " + str);
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }
            const int max_str_len = 0x95;
            if (str.Length >= max_str_len) {
                if (truncate) {
                    str = str.Substring(0, max_str_len);
                } else {
                    return SetLastError(DeviceStatus.ERR_PAYLOAD_TOO_LARGE);
                }
            }

            Encoding enc;
            switch (EncodingSetting) {
                case VFDEncoding.GB2312:
                    enc = Encoding.GetEncoding("gb2312");
                    break;
                case VFDEncoding.KSC5601:
                    enc = Encoding.GetEncoding("ks_c_5601-1987");
                    break;
                case VFDEncoding.BIG5:
                    enc = Encoding.GetEncoding("big5");
                    break;
                case VFDEncoding.SHIFT_JIS:
                    enc = Encoding.GetEncoding("shift_jis");
                    break;
                default:
                    throw new InvalidOperationException("Can't encode string for VFD with encoding: " + EncodingSetting);
            }

            byte[] strbytes = enc.GetBytes(str);
            byte[] packet = new byte[2 + str.Length];
            packet[0] = 0x50;
            packet[1] = (byte)str.Length;
            Array.Copy(strbytes, 0, packet, 2, str.Length);
            return SetLastError(Write(packet));
        }

        public DeviceStatus SetTextScroll(bool scroll) {
            Log.Write("Set Text Scrolling: " + scroll);
            return SetLastError(Write(new byte[] { (byte)(scroll ? 0x51 : 0x52) }));
        }

        public DeviceStatus GetVersion(out string version) {
            Log.Write("Get Version");
            version = null;
            DeviceStatus ret = SetLastError(Write(new byte[] { 0x5B, 0x63 }));
            if (ret != DeviceStatus.OK) {
                return ret;
            }
            ret = SetLastError(Read(7, out byte[] data));
            if (data != null) {
                version = Encoding.ASCII.GetString(data);
            }
            return ret;
        }

        public DeviceStatus SetRotateBy180(bool rotate) {
            Log.Write("Set Rotation: " + rotate);
            return SetLastError(Write(new byte[] { 0x5D, (byte)(rotate ? 0x72 : 0x6E) }));
        }

        // TODO: 0x1a commands (character bitmap creation?)
        // TODO: feed actual B/W images into DrawBitmap?
    }

    public enum VFDEncoding : byte {
        GB2312,
        BIG5,
        SHIFT_JIS,
        KSC5601
    }

    public enum VFDBrightnessLevel : byte {
        OFF,
        LEVEL_1,
        LEVEL_2,
        LEVEL_3,
        LEVEL_4
    }

    public enum VFDTextScrollSpeed : byte {
        FAST,
        SLOW
    }
}
