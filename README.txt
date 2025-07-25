SEGA835Lib / SEGA835Cmd
(c) 2024 Haruka and contributors

Licensed under the Server Side Public License.

API to interface with SEGA 835/837-series arcade hardware and companion command line application.

Nightly builds / downloads: https://nightly.link/akechi-haruka/SEGA835Lib/workflows/dotnet/master?preview

----------------
What can the application be used for?

* Use IO4 as a VJoy controller.
* Read MIFARE and FeliCa card UIDs via a real Aime reader.
* Display text on VFDs.
* LED shenanigans.
* Read data from KCA/FGO RFID cards.
* Retrieve information from CHC-series card printers.
* Print custom images to CHC-series card printers.
* Watch a directory for images and auto-print those.
* ... more to come

----------------
What can the library be used for?

* Implement Arcade hardware I/O in your own projects
* Replace/Mod an existing I/O handler in an existing game because it's causing issues (*cough* amdaemon)
* Convert between different boards or devices (ex. Game that uses a CHC-310 printer to a CHC-330 printer)

----------------
What devices are supported?

* Aime 837-15396 NFC Card Reader (Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396.AimeCardReader_837_15396)
* "IO4" 835-15257-01 JVS USB I/O Board (Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01.IO4USB_835_15257_01)
* GP1232A02A Futaba VFD (Haruka.Arcade.SEGA835Lib.Devices.Misc.VFD_GP1232A02A)
* SINFONIA CHC-310 card printer (Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310.CHC310Printer)
 - including embedded RFID
 - requires C310Ausb.dll
* SINFONIA CHC-310B card printer (Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C310.CHC310BPrinter)
 - requires C310Busb.dll
* SINFONIA CHC-330 card printer (Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC.C330.CHC330Printer)
 - requires C330Ausb.dll
* 837-15347 RFID Reader BD For Embedded (Haruka.Arcade.SEGA835Lib.Devices.RFID.RFIDRWPrinter_837_15347)
* 837-20004 RFID Deck Reader BD Half TKK (Haruka.Arcade.SEGA835Lib.Devices.RFID.RFIDDeckReader_837_20004)
* 835-15375 KEY SWITCH 4X3 BD Tenkey (Haruka.Arcade.SEGA835Lib.Devices.Misc.Tenkey_837_15375)
* 837-15093-06 IC BD I/O 7CH CONT RS232 12V (Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093.LED_837_15093_06)
* MONKEY06 837-15093-06 EMULATOR (Haruka.Arcade.SEGA835Lib.Devices.LED.MONKEY06.LED_MONKEY06
 - https://github.com/akechi-haruka/SuperMonkeyLEDs

TODOs:
* buy a IO3
* CHC320 (Sangokushi Taisen)
* buy a Y3

----------------
Implementation Notes:

* As this is a highly experimental API right now, consumer applications should check Haruka.Arcade.SEGA835Lib.VersionInfo.LIB_API_VERSION. This number will be incremented on any breaking changes for consumers.
* All device implementations operate by default on C-style error codes (enum DeviceStatus) as the devices themselves or dependent libaries do so. If you prefer exceptions, call SetUseExceptions on the device. (Invalid arguments, etc. will always throw exceptions regardless of this preference.)

Support Table:

+----------------+--------------------------+-------------+--------------------------------------------+
|    Version     | Compatible Unity Version |   Support   |                   Notes                    |
+----------------+--------------------------+-------------+--------------------------------------------+
| net35          | Before Unity 2018        | No Printing | or if netstandard.dll is not used in Unity |
| net48          | N/A                      | No Printing | Usable without .NET core                   |
| net6           | N/A                      | No Printing | Usable on Windows 7 if needed              |
| netstandard2.0 | Unity 2018 and later     | Full        |                                            |
| netstandard2.1 | Unity 2021.2 and later   | Full        |                                            |
| net8           | N/A                      | Full        | Default version used by Sega835Cmd         |
+----------------+--------------------------+-------------+--------------------------------------------+


----------------
Contributing:

If you own some of these obscure boards, feel free to add support and make PRs as I can't buy them all!

The only things to keep in mind are:
* Any actual board/device should inherit from Haruka.Arcade.SEGA835Lib.Devices.Device
* Any board that uses the JVS-like protocol (0xE0 as sync, 0xD0 as escape - I call it "SProt") should inherit from Haruka.Arcade.SEGA835Lib.Devices.SProtDevice
* Devices that share base functionality should have an appropriate superclass (ex. Haruka.Arcade.SEGA835Lib.Devices.Card.CardReader)
* Everything that's public should have appropriate documentation. (If you build in Release mode, missing documentation warnings will be generated)
* Unit tests should expect the device to be connected on its default port (usually defined by the game it's from). Devices that are not present should throw a warning and be skipped.
