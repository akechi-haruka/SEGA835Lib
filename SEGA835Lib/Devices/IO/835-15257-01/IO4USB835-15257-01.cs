using System;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01 {
    /// <summary>
    /// A 835-15257-01 SEGA I/O CONTROL BD ("IO4").
    /// </summary>
    public class Io4Usb15257 : JvsUsbIo {
        private readonly byte[] gpio = new byte[4];
        private readonly byte[] leds = new byte[32];

        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(Io4Usb15257));

        /// <summary>
        /// Creates a new IO4 board.
        /// </summary>
        public Io4Usb15257() : base(0x0ca3, 0x0021) {
        }

        /// <inheritdoc/>
        public override string GetDeviceModel() {
            return "835-15257-01";
        }

        /// <inheritdoc/>
        public override string GetName() {
            return "SEGA I/O CONTROL BD";
        }

        /// <summary>
        /// Resets the board status. Unknown what this <i>actually</i> does.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success, any other status on failure.</returns>
        public DeviceStatus ResetBoardStatus() {
            LOG.LogInformation("ResetBoardStatus");
            return Write(new JvsUsbReportOut() {
                cmd = JvsUsbReports.ClearBoardStatus
            });
        }

        /// <summary>
        /// Sets a GPIO on the board (usually older LEDs).
        /// </summary>
        /// <param name="index">The GPIO index to set.</param>
        /// <param name="state">Whether to turn the GPIO on or off</param>
        /// <param name="update">Whether to update the GPIOs on the board or not (use false for bulk operations)</param>
        /// <exception cref="ArgumentException">If index is not between 0 (incl.) and 32 (excl.)</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or if update is false, any other status on failure.</returns>
        public DeviceStatus SetGpio(int index, bool state, bool update = true) {
            if (index < 0 || index > gpio.Length * 8) {
                throw new ArgumentException("index must be within [0,32)");
            }

            LOG.LogInformation("SetGPIO: " + index + " -> " + state);
            gpio[index / 8] = (byte)((state ? 1 : 0) << (index % 8));

            if (update) {
                return WriteGpio();
            }

            return SetLastError(DeviceStatus.Ok);
        }

        private unsafe DeviceStatus WriteGpio() {
            JvsUsbPayloadOutGpio payload = new JvsUsbPayloadOutGpio();

            for (int i = 0; i < gpio.Length; i++) {
                payload.led[i] = gpio[i];
            }

            return Write(JvsUsbReports.SetGeneralOutput, payload);
        }

        /// <summary>
        /// Sets the value of a LED on the board.
        /// </summary>
        /// <param name="index">The LED index to set.</param>
        /// <param name="state">The LED brightness value from 0-255.</param>
        /// <param name="update">Whether to update the LEDs on the board or not (use false for bulk operations)</param>
        /// <exception cref="ArgumentException">If index is not between 0 (incl.) and 32 (excl.)</exception>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or if update is false, any other status on failure.</returns>
        public DeviceStatus SetLed(int index, byte state, bool update = true) {
            if (index < 0 || index > leds.Length) {
                throw new ArgumentException("index must be within [0,32)");
            }

            LOG.LogInformation("SetLED: " + index + " -> " + state);
            leds[index / 8] = state;

            if (update) {
                return WriteLed();
            }

            return SetLastError(DeviceStatus.Ok);
        }

        private unsafe DeviceStatus WriteLed() {
            JvsUsbPayloadOutLed payload = new JvsUsbPayloadOutLed();

            for (int i = 0; i < leds.Length; i++) {
                payload.led[i] = leds[i];
            }

            return Write(JvsUsbReports.SetGeneralOutput, payload);
        }

        /// <summary>
        /// Turns off all GPIOs.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or if update is false, any other status on failure.</returns>
        public DeviceStatus ClearGpio() {
            for (int i = 0; i < gpio.Length; i++) {
                gpio[i] = 0;
            }

            return WriteGpio();
        }

        /// <summary>
        /// Turns off all LEDs.
        /// </summary>
        /// <returns><see cref="DeviceStatus.Ok"/> on success or if update is false, any other status on failure.</returns>
        public DeviceStatus ClearLed() {
            for (int i = 0; i < gpio.Length; i++) {
                leds[i] = 0;
            }

            return WriteLed();
        }
    }
}