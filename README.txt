SEGA835Lib / SEGA835Cmd
(c) 2024 Haruka and contributors

Licensed under the Server Side Public License.

API to interface with SEGA 835/837-series arcade hardware and companion command line application.

----------------
What can this be used for?

* Implement Arcade hardware I/O in your own projects
* Replace/Mod an existing I/O handler in an existing game because it's causing issues (*cough* amdaemon)
* Convert between different boards or devices (ex. Game that uses a CHC-310 printer to a CHC-330 printer)
* (SEGA835Cmd) Interact with arcade hardware from command-line (ex. swipe a card to trigger a process, make an LED light show, print custom cards, ...)

----------------
What devices are supported?

* Aime 837-15396 NFC Card Reader (Haruka.Arcade.SEGA835Lib.Devices.Card._837_15396.AimeCardReader_837_15396)
* "IO4" 835-15257-01 JVS USB I/O Board (Haruka.Arcade.SEGA835Lib.Devices.IO._835_15257_01.IO4USB835_15257_01)
* GP1232A02A Futaba VFD display (Haruka.Arcade.SEGA835Lib.Devices.Misc.GP1232A02A_VFD)
* SINFONIA CHC-310 card printer (Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC310.CHC310_Printer)
 - including embedded RFID
 - requires C310Ausb.dll
* SINFONIA CHC-330 card printer (Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC330.CHC330_Printer)
 - requires C330Ausb.dll
* 837-15347 RFID Reader BD For Embedded (Haruka.Arcade.SEGA835Lib.Devices.RFID._837_15347_RFIDRWPrinter)
* 837-20004 RFID Deck Reader BD Half TKK (Haruka.Arcade.SEGA835Lib.Devices.RFID._837_20004_RFIDDeckReader)
* 837-15375 Tenkey (Haruka.Arcade.SEGA835Lib.Devices.Misc._837_15375_Tenkey)

----------------
Implementation Notes:

* As this is a highly experimental API right now, consumer applications should check Haruka.Arcade.SEGA835Lib.VersionInfo.LIB_API_VERSION. This number will be incremented on any breaking changes for consumers.
* All device implementations operate by default on C-style error codes (enum DeviceStatus) as the devices themselves or dependent libaries do so. If you prefer exceptions, call SetUseExceptions on the device. (Invalid arguments, etc. will always throw exceptions regardless of this preference.)