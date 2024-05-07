using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Haruka.Arcade.SEGA835Lib.Devices.Misc {

    /// <summary>
    /// A Futaba GP1232A02A VFD.
    /// </summary>
    public class VFD_GP1232A02A : Device {

        private SerialComm serial;

        /// <summary>
        /// The COM port this device is using.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The charset being used for this device.
        /// </summary>
        public VFDEncoding EncodingSetting { get; private set; }

        /// <summary>
        /// Creates a new VFD.
        /// </summary>
        /// <param name="port">The COM port to use.</param>
        public VFD_GP1232A02A(int port = 1) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Port = port;
            serial = new SerialComm(port, 115200, 1000, true, true);
            EncodingSetting = VFDEncoding.SHIFT_JIS;
        }

        /// <summary>
        /// Connects to this device.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> on success or if the device is already connected.<br />
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the device is not connected or does not respond.
        /// </returns>
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

        /// <summary>
        /// Disconnects from this device.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.OK"/>.</returns>
        public override DeviceStatus Disconnect() {
            Log.Write("Disconnected on Port " + Port);
            serial?.Disconnect();
            return DeviceStatus.OK;
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "GP1232A02";
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Resets the board.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus Reset() {
            Log.Write("Reset");
            return SetLastError(Write(new byte[] { 0x0B }));
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus ClearScreen() {
            Log.Write("Clear Screen");
            return SetLastError(Write(new byte[] { 0x0C }));
        }

        /// <summary>
        /// Sets the screen brightness.
        /// </summary>
        /// <param name="level">The screen brightness to set.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus SetBrightness(VFDBrightnessLevel level) {
            Log.Write("Set Brightness: " + level);
            return SetLastError(Write(new byte[] { 0x0C, (byte)level }));
        }

        /// <summary>
        /// Turns the display on or off.
        /// </summary>
        /// <param name="on">Whether the display should be on (true) or off (false).</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus SetOn(bool on) {
            Log.Write("Set On: " + on);
            return SetLastError(Write(new byte[] { 0x21, (byte)(on ? 0x01 : 0x00) }));
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        /// <param name="x"></param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        /// <exception cref="ArgumentException">If x is not between 0 (incl.) and 512 (excl.)</exception>
        public DeviceStatus SetWindowHScroll(ushort x) {
            Log.Write("Set Window H-Scroll: " + x);
            if (x >= 512) {
                throw new ArgumentException("x (" + x + ") must be within [0,512)");
            }
            return SetLastError(Write(new byte[] { 0x22, (byte)(x >> 8), (byte)x }));
        }

        /// <summary>
        /// Draws an image to the display.
        /// </summary>
        /// <param name="x">The starting x coordinate.</param>
        /// <param name="y">The starting y coordinate.</param>
        /// <param name="w">The image width.</param>
        /// <param name="h">The image height.</param>
        /// <param name="image">A column-major 1bpp bitmap with a length equal to w*h.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        /// <exception cref="ArgumentException">If x, x+w or w are outside the boundaries of [0,512), if y, h or y+h are outside [0,32], if y or h are not multiples of 8, if image.Length is not w*h.</exception>
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

        /// <summary>
        /// Unknown.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        /// <exception cref="ArgumentException">If x is not in [0,512), y not in [0,32) or not a multiple of 8.</exception>
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

        /// <summary>
        /// Sets the text encoding for writing text to the display.
        /// </summary>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus SetEncoding(VFDEncoding encoding) {
            Log.Write("Set Encoding: " + encoding);
            return SetLastError(Write(new byte[] { 0x32, (byte)encoding }));
        }

        /// <summary>
        /// Sets the coordinates on the display where text will be written to.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="w">Unknown. The text width??</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        /// <exception cref="ArgumentException">If x is not in [0,512), x+w is not in [0,512], y not in [0,32) or not a multiple of 8.</exception>
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

        /// <summary>
        /// Sets the text scroll speed.
        /// </summary>
        /// <param name="speed">The speed to set.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus SetTextScrollSpeed(VFDTextScrollSpeed speed) {
            Log.Write("Set Text Scroll Speed: " + speed);
            return SetLastError(Write(new byte[] { 0x41, (byte)speed }));
        }

        /// <summary>
        /// Writes text to the device. If the text should actually not scroll, use <see cref="SetTextScroll(bool)"/> with false.
        /// </summary>
        /// <param name="str">The string to write. (maximum 149 characters)</param>
        /// <param name="truncate">If the string should be truncated if it's too long, otherwise <see cref="DeviceStatus.ERR_PAYLOAD_TOO_LARGE"/> will be returned.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        /// <exception cref="InvalidOperationException">If the currently set encoding is invalid.</exception>
        /// <exception cref="ArgumentException">If the configured encoding is not supported on this computer.</exception>
        public DeviceStatus WriteScrollingText(string str, bool truncate = false) {
            Log.Write("Write Text: " + str);
            ArgumentNullException.ThrowIfNull(str);
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

        /// <summary>
        /// Enables or disables text scrolling.
        /// </summary>
        /// <param name="scroll">true if text scrolling should be enabled, false if the text should not scroll.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus SetTextScroll(bool scroll) {
            Log.Write("Set Text Scrolling: " + scroll);
            return SetLastError(Write(new byte[] { (byte)(scroll ? 0x51 : 0x52) }));
        }

        /// <summary>
        /// Reads the VFD's version from the board.
        /// </summary>
        /// <param name="version">The version information read from the device.</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
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

        /// <summary>
        /// Rotates the device's display by 180 degrees (upside-down).
        /// </summary>
        /// <param name="rotate">true if display should be rotated, false if not</param>
        /// <returns><see cref="DeviceStatus.OK"/> on success, any other status on failure.</returns>
        public DeviceStatus SetRotateBy180(bool rotate) {
            Log.Write("Set Rotation: " + rotate);
            return SetLastError(Write(new byte[] { 0x5D, (byte)(rotate ? 0x72 : 0x6E) }));
        }

        // TODO: 0x1a commands (character bitmap creation?)
        // TODO: feed actual B/W images into DrawBitmap? (bad apple on VFD when)
    }

    /// <summary>
    /// Enum of supported charsets for the VFD.
    /// </summary>
    public enum VFDEncoding : byte {
        /// <summary>
        /// Chinese (simplified).
        /// </summary>
        GB2312,
        /// <summary>
        /// Chinese (traditional).
        /// </summary>
        BIG5,
        /// <summary>
        /// Japanese.
        /// </summary>
        SHIFT_JIS,
        /// <summary>
        /// Korean.
        /// </summary>
        KSC5601
    }

    /// <summary>
    /// Enum of supported brightness levels for the VFD.
    /// </summary>
    public enum VFDBrightnessLevel : byte {
        /// <summary>
        /// Off.
        /// </summary>
        OFF,
        /// <summary>
        /// On.
        /// </summary>
        LEVEL1,
        /// <summary>
        /// Bright.
        /// </summary>
        LEVEL2,
        /// <summary>
        /// Brighter.
        /// </summary>
        LEVEL3,
        /// <summary>
        /// Brightest. (whoa)
        /// </summary>
        LEVEL4
    }

    /// <summary>
    /// Enum of supported text scrolling speeds for the VFD.
    /// </summary>
    public enum VFDTextScrollSpeed : byte {
        /// <summary>
        /// FAST.
        /// </summary>
        FAST,
        /// <summary>
        /// Slow.
        /// </summary>
        SLOW
    }
}
