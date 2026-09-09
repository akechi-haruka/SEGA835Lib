using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO {
    /// <summary>
    /// Enum of possible outgoing report types for USB-based JVS devices. These have not yet been documented.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum JvsUsbReports : byte {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unset = 0x00,

        /// <summary>
        /// Unknown.
        /// </summary>
        SetCommTimeout = 0x01,

        /// <summary>
        /// Unknown.
        /// </summary>
        SetSamplingCount = 0x02,

        /// <summary>
        /// Unknown.
        /// </summary>
        ClearBoardStatus = 0x03,

        /// <summary>
        /// Unknown.
        /// </summary>
        SetGeneralOutput = 0x04,

        /// <summary>
        /// Unknown.
        /// </summary>
        SetPwmOutput = 0x05,

        /// <summary>
        /// Unknown.
        /// </summary>
        SetLeds = 0x41,

        /// <summary>
        /// Unknown.
        /// </summary>
        UpdateFirmware = 0x85
    }

    /// <summary>
    /// The structure that is sent to JVS-based USB I/O boards. This is not yet documented.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JvsUsbReportOut {
        /// <summary>
        /// The report type being sent.
        /// </summary>
        public JvsUsbReports cmd;

        /// <summary>
        /// Unknown.
        /// </summary>
        public fixed byte payload[62];
    }

    /// <summary>
    /// The payload for the JVS packet <see cref="JvsUsbReports.SetGeneralOutput"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JvsUsbPayloadOutGpio {
        /// <summary>
        /// LED data, up to 32 LEDs, where every bit indicates LED on/off
        /// </summary>
        public fixed byte led[4];

        /// <summary>
        /// Unknown.
        /// </summary>
        public fixed byte unknown[58];
    }

    /// <summary>
    /// The payload for the JVS packet <see cref="JvsUsbReports.SetLeds"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JvsUsbPayloadOutLed {
        /// <summary>
        /// LED data, up to 32 LEDs, where every byte indicated LED state from 0 to 255.
        /// </summary>
        public fixed byte led[32];

        /// <summary>
        /// Unknown.
        /// </summary>
        public fixed byte unknown[30];
    }

    /// <summary>
    /// The structure that is received from JVS-based USB I/O boards.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct JvsUsbReportIn {
        /// <summary>
        /// The maximum number of analog devices for this board.
        /// </summary>
        public const int ADC_COUNT = 8;

        /// <summary>
        /// The maximum number of spinner devices for this board.
        /// </summary>
        public const int SPINNER_COUNT = 4;

        /// <summary>
        /// The maximum number of (coin) chutes for this board.
        /// </summary>
        public const int CHUTE_COUNT = 2;

        /// <summary>
        /// The maximum number of players for this board.
        /// </summary>
        public const int BUTTON_COUNT = 2;

        /// <summary>
        /// The current values of the board's analog devices.
        /// </summary>
        public fixed ushort adcs[ADC_COUNT];

        /// <summary>
        /// The current values of the board's spinners.
        /// </summary>
        public fixed ushort spinners[SPINNER_COUNT];

        /// <summary>
        /// The current values of the board's chutes.
        /// </summary>
        public fixed ushort chutes[CHUTE_COUNT];

        /// <summary>
        /// The current values of the board's buttons. Individual buttons are encoded as bits, so ex. the 4th button of player 2 would be tested with (buttons[1] >> 3 &amp; 1) != 0
        /// </summary>
        public fixed ushort buttons[BUTTON_COUNT];

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte system_status;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte usb_status;

        /// <summary>
        /// Unknown.
        /// </summary>
        public fixed byte unknown[29];

        /// <summary>
        /// Checks if the given button is pressed.
        /// </summary>
        /// <param name="b">The button to check (across all player indicies)</param>
        /// <returns>true if the button is pressed, false if not.</returns>
        /// <exception cref="ArgumentException">if the given button is invalid</exception>
        public bool GetButton(int b) {
            const int max = BUTTON_COUNT * sizeof(ushort) * 8;
            if (b < 0 || b > max) {
                throw new ArgumentException("button must be within [0," + max + ")");
            }

            int p = b / 8;
            int o = b % 8;
            return (buttons[p] >> o & 1) != 0;
        }
    }

    /// <summary>
    /// Capabilities describind a JVS board.
    /// </summary>
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public class JvsCapabilities {
        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(JvsCapabilities));

        /// <summary>
        /// Usually "I/O CONTROL BD".
        /// </summary>
        public String Type { get; private set; }

        /// <summary>
        /// The board number, after 83X, for example "15257".
        /// </summary>
        public int BoardNumber { get; private set; }

        /// <summary>
        /// Unknown. 1 is observed.
        /// </summary>
        public int Mode { get; private set; }

        /// <summary>
        /// The board's firmware revision.
        /// </summary>
        public byte FirmwareRevision { get; private set; }

        /// <summary>
        /// The board's firmware revision.
        /// </summary>
        public short FirmwareChecksum { get; private set; }

        /// <summary>
        /// The board's chip number.
        /// </summary>
        public String ChipNumber { get; private set; }

        /// <summary>
        /// Unknown. 0 is observed.
        /// </summary>
        public byte Config { get; private set; }

        /// <summary>
        /// The number of general purpose outputs this board has. Usually refers to LEDs and coin blockers, sometimes to door locks.
        /// </summary>
        public int Outputs { get; private set; }

        /// <summary>
        /// Unknown. Not observed in the wild.
        /// </summary>
        public int PwmOutputs { get; private set; }

        /// <summary>
        /// The number of analog inputs per player. Array length is equal to player count, value equal to inputs for this player.
        /// </summary>
        public int[] AnalogInputs { get; private set; }

        /// <summary>
        /// The number of rotary inputs.
        /// </summary>
        public int RotaryInputs { get; private set; }

        /// <summary>
        /// The number of coin chutes/slots.
        /// </summary>
        public int Chutes { get; private set; }

        /// <summary>
        /// The number of digital inputs per player. Array length is equal to player count, value equal to inputs for this player.
        /// </summary>
        public int[] SwitchInputs { get; private set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public UniqueFunction[] UniqueFunctions { get; private set; }

        /// <summary>
        /// Attempts to parse the given I/O board's product string for the board's capabilities.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="capabilities">The parsed capabilities, or null on failure.</param>
        /// <returns>true on success, false otherwise.</returns>
        public static bool TryParse(String str, out JvsCapabilities capabilities) {
            if (String.IsNullOrEmpty(str)) {
                LOG.LogWarning("Failed to parse IO4 board parameters: null");
                capabilities = null;
                return false;
            }

            String[] parts = str.Split(';');
            if (parts.Length != 8) {
                LOG.LogWarning("Failed to parse IO4 board parameters: " + str);
                capabilities = null;
                return false;
            }

            try {
                capabilities = new JvsCapabilities {
                    Type = parts[0],
                    BoardNumber = Int32.Parse(parts[1]),
                    Mode = Int32.Parse(parts[2], NumberStyles.HexNumber),
                    FirmwareRevision = Byte.Parse(parts[3], NumberStyles.HexNumber),
                    FirmwareChecksum = Int16.Parse(parts[4], NumberStyles.HexNumber),
                    ChipNumber = parts[5],
                    Config = Byte.Parse(parts[6], NumberStyles.HexNumber)
                };

                // SEGA calls this string an "amyItemList", which has a function
                // amyItemListChangeConfig(context, "_=?")
                // underscore equal to question mark is what I'm literally thinking about all this.

                List<UniqueFunction> uniqueFunctions = new List<UniqueFunction>();

                String[] functions = parts[7].Split('_');
                foreach (String function in functions) {
                    String[] part = function.Split('=');
                    if (part.Length != 2) {
                        LOG.LogWarning("Failed to parse IO4 board parameters: " + str);
                        capabilities = null;
                        return false;
                    }

                    String type = part[0];
                    String[] values = part[1].Replace("_", "").Split(',');

                    if (type == "GOUT") {
                        capabilities.Outputs = Int32.Parse(values[0], NumberStyles.HexNumber);
                    } else if (type == "PWMOUT") {
                        capabilities.PwmOutputs = Int32.Parse(values[0], NumberStyles.HexNumber);
                    } else if (type == "ADIN") {
                        // players,bits
                        capabilities.AnalogInputs = new int[Int32.Parse(values[0], NumberStyles.HexNumber)];
                        for (int i = 0; i < capabilities.AnalogInputs.Length; i++) {
                            capabilities.AnalogInputs[i] = Int32.Parse(values[1], NumberStyles.HexNumber);
                        }
                    } else if (type == "ROTIN") {
                        capabilities.RotaryInputs = Int32.Parse(values[0], NumberStyles.HexNumber);
                    } else if (type == "COININ") {
                        capabilities.Chutes = Int32.Parse(values[0], NumberStyles.HexNumber);
                    } else if (type == "SWIN") {
                        // players,bits
                        capabilities.SwitchInputs = new int[Int32.Parse(values[0], NumberStyles.HexNumber)];
                        for (int i = 0; i < capabilities.SwitchInputs.Length; i++) {
                            capabilities.SwitchInputs[i] = Int32.Parse(values[1], NumberStyles.HexNumber);
                        }
                    } else if (type.StartsWith("UQ")) {
                        int functionId = Int32.Parse(values[0], NumberStyles.HexNumber);
                        int functionParam = Int32.Parse(values[1], NumberStyles.HexNumber);

                        uniqueFunctions.Add(new UniqueFunction(functionId, functionParam));
                    } else {
                        LOG.LogWarning("Encountered unknown IO4 function: " + type);
                    }
                }

                capabilities.UniqueFunctions = uniqueFunctions.ToArray();

                return true;
            } catch (Exception ex) {
                LOG.LogWarning(ex, "Failed to parse IO4 board parameters: " + str);
                capabilities = null;
                return false;
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class UniqueFunction {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Id { get; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Value { get; }

            internal UniqueFunction(int id, int value) {
                Id = id;
                Value = value;
            }
        }
    }
}