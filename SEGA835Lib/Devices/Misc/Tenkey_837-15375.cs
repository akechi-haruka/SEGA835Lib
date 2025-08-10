using Haruka.Arcade.SEGA835Lib.Devices.IO;
using Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01;
using Haruka.Arcade.SEGA835Lib.Misc;

namespace Haruka.Arcade.SEGA835Lib.Devices.Misc {
    /// <summary>
    /// A 835-15375 KEY SWITCH 4X3 BD that is connected to a IO4.
    /// </summary>
    public class Tenkey_837_15375 : Device {
        /// <summary>
        /// Definition of keys on this tenkey.
        /// </summary>
        public enum Key {
            /// <summary>
            /// The '1' key.
            /// </summary>
            K1,

            /// <summary>
            /// The '2' key.
            /// </summary>
            K2,

            /// <summary>
            /// The '3' key.
            /// </summary>
            K3,

            /// <summary>
            /// The '4' key.
            /// </summary>
            K4,

            /// <summary>
            /// The '5' key.
            /// </summary>
            K5,

            /// <summary>
            /// The '6' key.
            /// </summary>
            K6,

            /// <summary>
            /// The '7' key.
            /// </summary>
            K7,

            /// <summary>
            /// The '8' key.
            /// </summary>
            K8,

            /// <summary>
            /// The '9' key.
            /// </summary>
            K9,

            /// <summary>
            /// The '0' key.
            /// </summary>
            K0,

            /// <summary>
            /// The 'C' key.
            /// </summary>
            Clear,

            /// <summary>
            /// The 'ENTER' key.
            /// </summary>
            Enter,

            /// <summary>
            /// No key (is pressed).
            /// </summary>
            None
        };

        private readonly IO4USB_835_15257_01 io4;
        private readonly int r1, r2, r3, r4;
        private readonly int c1, c2, c3;

        /// <summary>
        /// Creates a new instance of a 835-15375 board with the specified buttons being wired to the board. The default bindings are equal to the wiring for a Sangokushi Taisen satellite cabinet. For a Eiketsu Taisen satellite cabinet, subtract 1 from all column pins.
        /// </summary>
        /// <param name="io4">The IO4 instance the board is connected to.</param>
        /// <param name="r1">The button pin that row 1 is wired to.</param>
        /// <param name="r2">The button pin that row 2 is wired to.</param>
        /// <param name="r3">The button pin that row 3 is wired to.</param>
        /// <param name="r4">The button pin that row 4 is wired to.</param>
        /// <param name="c1">The button pin that column 1 is wired to.</param>
        /// <param name="c2">The button pin that column 2 is wired to.</param>
        /// <param name="c3">The button pin that column 3 is wired to.</param>
        public Tenkey_837_15375(IO4USB_835_15257_01 io4, int r1 = 1, int r2 = 0, int r3 = 15, int r4 = 14, int c1 = 13, int c2 = 12, int c3 = 11) {
            NetStandardBackCompatExtensions.ThrowIfNull(io4, nameof(io4));
            this.io4 = io4;
            this.r1 = r1;
            this.r2 = r2;
            this.r3 = r3;
            this.r4 = r4;
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <returns>always <see cref="DeviceStatus.OK"/></returns>
        public override DeviceStatus Connect() {
            return DeviceStatus.OK;
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        /// <returns>always <see cref="DeviceStatus.OK"/></returns>
        public override DeviceStatus Disconnect() {
            return DeviceStatus.OK;
        }

        /// <inheritdoc />
        public override string GetDeviceModel() {
            return "837-15375";
        }

        /// <inheritdoc />
        public override string GetName() {
            return "KEY SWITCH 4X3 BD";
        }

        /// <summary>
        /// Returns the key that is currently being pressed on the tenkey (based on the last time <see cref="JVSUSBIO.Poll(out JVSUSBReportIn)" /> was called on the connected <see cref="IO4USB_835_15257_01"/>.
        /// </summary>
        /// <remarks>Multiple keys being pressed is undefined.</remarks>
        /// <returns>A <see cref="Key"/> of what key is currently pressed or <see cref="Key.None"/> if nothing is pressed.</returns>
        public Key GetPressedKey() {
            JVSUSBReportIn? report = io4.LastReport;
            if (!report.HasValue) {
                return Key.None;
            }

            JVSUSBReportIn val = report.Value;

            bool r1 = val.GetButton(this.r1);
            bool r2 = val.GetButton(this.r2);
            bool r3 = val.GetButton(this.r3);
            bool r4 = val.GetButton(this.r4);
            bool c1 = val.GetButton(this.c1);
            bool c2 = val.GetButton(this.c2);
            bool c3 = val.GetButton(this.c3);

            if (r1 && c1) {
                return Key.K1;
            } else if (r1 && c2) {
                return Key.K2;
            } else if (r1 && c3) {
                return Key.K3;
            } else if (r2 && c1) {
                return Key.K4;
            } else if (r2 && c2) {
                return Key.K5;
            } else if (r2 && c3) {
                return Key.K6;
            } else if (r3 && c1) {
                return Key.K7;
            } else if (r3 && c2) {
                return Key.K8;
            } else if (r3 && c3) {
                return Key.K9;
            } else if (r4 && c1) {
                return Key.Clear;
            } else if (r4 && c2) {
                return Key.K0;
            } else if (r4 && c3) {
                return Key.Enter;
            } else {
                return Key.None;
            }
        }

        /// <summary>
        /// Returns a numeric or control character based on the pressed key.
        /// </summary>
        /// <remarks>Multiple keys being pressed is undefined.</remarks>
        /// <returns>'0'-'9' if a numeric key is pressed, a newline character on Enter, a backspace character on Clear or null if nothing is pressed.</returns>
        public char? GetPressedChar() {
            switch (GetPressedKey()) {
                case Key.None: return null;
                case Key.K1: return '1';
                case Key.K2: return '2';
                case Key.K3: return '3';
                case Key.K4: return '4';
                case Key.K5: return '5';
                case Key.K6: return '6';
                case Key.K7: return '7';
                case Key.K8: return '8';
                case Key.K9: return '9';
                case Key.K0: return '0';
                case Key.Clear: return '\b';
                case Key.Enter: return '\n';
                default: return null;
            }
        }
    }
}