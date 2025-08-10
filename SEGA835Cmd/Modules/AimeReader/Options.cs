using CommandLine;
using Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396;

namespace Haruka.Arcade.SEGA835Cmd.Modules.AimeReader {
    [Verb("aime", HelpText = "Interacts with an Aime card reader")]
    internal class Options : GlobalOptions {
        [Option('r', Required = false, HelpText = "Sets the red part of the LED.")]
        public byte LEDRed { get; set; }

        [Option('g', Required = false, HelpText = "Sets the green part of the LED.")]
        public byte LEDGreen { get; set; }

        [Option('b', Required = false, HelpText = "Sets the blue part of the LED.")]
        public byte LEDBlue { get; set; }

        [Option('p', "port", Required = false, HelpText = "Sets the reader's COM port.", Default = 3)]
        public int Port { get; set; }

        [Option("slow-baudrate", Required = false, HelpText = "Uses slow baudrate for older models.")]
        public bool SlowBaudrate { get; set; }

        [Option("reset-leds", Required = false, HelpText = "Resets the reader LEDs.")]
        public bool ResetLEDs { get; set; }

        [Option("scan-single", Required = false, HelpText = "Scans for a card, and on success, prints the card ID.", Group = "ScanType")]
        public bool Scan { get; set; }

        [Option("scan-continous", Required = false, HelpText = "Continiously scans for cards until ESC is pressed.", Group = "ScanType")]
        public bool Continous { get; set; }

        [Option("get-hardware", Required = false, HelpText = "Prints the Aime reader's hardware revision.", Group = "ScanType")]
        public bool GetHardware { get; set; }

        [Option("get-firmware", Required = false, HelpText = "Prints the Aime reader's firmware revision.", Group = "ScanType")]
        public bool GetFirmware { get; set; }

        [Option("get-firmware-checksum", Required = false, HelpText = "Prints the Aime reader's firmware checksum.", Group = "ScanType")]
        public bool GetFirmwareChecksum { get; set; }

        [Option("timeout", Required = false, HelpText = "(Default: Infinite) Maximum number of seconds to wait for a card.")]
        public int Timeout { get; set; }

        [Option("card-type", Required = false, HelpText = "Card type to scan for. (MIFARE,FeliCa,Both)", Default = RadioOnType.Both)]
        public RadioOnType CardType { get; set; }
    }
}