using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using HidLibrary;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO {

    /// <summary>
    /// Base class for a USB-based JVS input device.
    /// </summary>
    public abstract class JVSUSBIO : JVSIO {

        private const int OUTGOING_REPORT_ID = 0x10;
        private const int INCOMING_REPORT_ID = 0x01;

        /// <summary>
        /// USB vendor ID of this board.
        /// </summary>
        public int USBVendorID { get; private set; }
        /// <summary>
        /// USB product ID of this board.
        /// </summary>
        public int USBProductID { get; private set; }
        /// <summary>
        /// Communication timeout in ms to the board.
        /// </summary>
        public int Timeout { get; private set; } = 1000;
        /// <summary>
        /// Last received JVS poll report (from <see cref="Poll(out JVSUSBReportIn)"/>
        /// </summary>
        public JVSUSBReportIn? LastReport { get; private set; }

        private HidDevice device;

        /// <summary>
        /// Creates a new JVSUSBIO.
        /// </summary>
        /// <param name="vid">The vendor id of the board.</param>
        /// <param name="pid">The product id of the board.</param>
        protected JVSUSBIO(int vid, int pid) {
            USBVendorID = vid;
            USBProductID = pid;
        }

        /// <summary>
        /// Connects to the USB device.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if connection was successful.
        /// <see cref="DeviceStatus.ERR_NOT_CONNECTED"/> if the board is not attached or if opening the device fails.<br />
        /// </returns>
        public override sealed DeviceStatus Connect() {
            Log.Write("Open JVS USB: VID:" + USBVendorID + ", PID: " + USBProductID);
            device = HidDevices.Enumerate(USBVendorID, USBProductID).FirstOrDefault();

            if (device == null) {
                return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
            }

            Log.Write("Found JVS USB at " + device.DevicePath);

            try {
                device.OpenDevice();
            }catch(Exception ex) {
                Log.WriteFault(ex, "Opening USB device failed (" + GetName() + ")");
                device = null;
                return SetLastError(DeviceStatus.ERR_NOT_CONNECTED);
            }

            return SetLastError(DeviceStatus.OK);
        }

        /// <summary>
        /// Disconnects from the USB device.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.OK"/>.</returns>
        public override DeviceStatus Disconnect() {
            if (device != null) {
                device.CloseDevice();
                device = null;
            }
            return SetLastError(DeviceStatus.OK);
        }

        /// <summary>
        /// Returns if the device is connected and was not unplugged since the last call to <see cref="Connect"/>.
        /// </summary>
        /// <returns>true if the device is connected</returns>
        public bool IsConnected() {
            return device?.IsConnected ?? false;
        }

        /// <summary>
        /// Returns the USB HID Manufacturer name of the board.
        /// </summary>
        /// <param name="manufacturer">The manufacturer string that was read from the board.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the data was successfully read.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if the USB library threw an exception.
        /// </returns>
        public DeviceStatus GetManufacturer(out string manufacturer) {
            Log.Write("GetManufacturer");
            manufacturer = null;
            if (device == null) {
                return SetLastError(DeviceStatus.ERR_NOT_INITIALIZED);
            }
            try {
                bool success = device.ReadManufacturer(out byte[] data);
                if (!success) {
                    return SetLastError(DeviceStatus.ERR_DEVICE);
                }
                manufacturer = Encoding.ASCII.GetString(data);
                return SetLastError(DeviceStatus.OK);
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading USB Device Manufacturer of " + GetName());
                return SetLastError(DeviceStatus.ERR_OTHER);
            }
        }

        /// <summary>
        /// Returns the USB HID product name of the board.
        /// </summary>
        /// <param name="product">The product string that was read from the board.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the data was successfully read.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if the USB library threw an exception.
        /// </returns>
        public DeviceStatus GetProduct(out string product) {
            Log.Write("GetProduct");
            product = null;
            if (device == null) {
                return SetLastError(DeviceStatus.ERR_NOT_INITIALIZED);
            }
            try {
                bool success = device.ReadProduct(out byte[] data);
                if (!success) {
                    return SetLastError(DeviceStatus.ERR_DEVICE);
                }
                product = Encoding.ASCII.GetString(data);
                return SetLastError(DeviceStatus.OK);
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading USB Device Product of " + GetName());
                return SetLastError(DeviceStatus.ERR_OTHER);
            }
        }

        /// <summary>
        /// Reads a report from the USB device.
        /// </summary>
        /// <param name="report">The report that was read from the device.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the data was successfully read.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ERR_INCOMPATIBLE"/> if an unexpected report type was read.<br />
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if the USB library threw an exception.
        /// </returns>
        public DeviceStatus Poll(out JVSUSBReportIn report) {
            if (device == null) {
                report = default;
                return SetLastError(DeviceStatus.ERR_NOT_INITIALIZED);
            }
            try {
                HidReport data = device.ReadReport(Timeout);
                if (data == null) {
                    report = default;
                    return SetLastError(DeviceStatus.ERR_DEVICE);
                }
                if (data.ReportId != INCOMING_REPORT_ID) {
                    report = default;
                    Log.WriteError("Read unknown report id " + data.ReportId);
                    return SetLastError(DeviceStatus.ERR_INCOMPATIBLE);
                }
                report = StructUtils.FromBytes<JVSUSBReportIn>(data.Data);
                LastReport = report;
                return SetLastError(DeviceStatus.OK);
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading data from " + GetName());
                report = default;
                return SetLastError(DeviceStatus.ERR_OTHER);
            }
        }

        /// <summary>
        /// Writes a report to the USB device.
        /// </summary>
        /// <param name="report">The report that should be written to the device.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the data was successfully written.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if the USB library threw an exception.
        /// </returns>
        protected DeviceStatus Write(JVSUSBReportOut report) {
            if (device == null) {
                return SetLastError(DeviceStatus.ERR_NOT_INITIALIZED);
            }
            if (report.cmd == JVSUSBReports.Unset) {
                throw new ArgumentException("JVS Report command must be set");
            }
            try {
                bool success = device.WriteReport(new HidReport(0x3F) {
                    ReportId = OUTGOING_REPORT_ID,
                    Data = StructUtils.GetBytes(report)
                }, Timeout);
                return SetLastError(success ? DeviceStatus.OK : DeviceStatus.ERR_DEVICE, Marshal.GetLastWin32Error());
            }catch (Exception ex) {
                Log.WriteFault(ex, "Failed writing data to " + GetName());
                return SetLastError(DeviceStatus.ERR_OTHER);
            }
        }

        /// <summary>
        /// Writes an arbitary struct to the USB device.
        /// </summary>
        /// <param name="command">The command being sent.</param>
        /// <param name="struc">The payload data.</param>
        /// <returns>
        /// <see cref="DeviceStatus.OK"/> if the data was successfully written.<br />
        /// <see cref="DeviceStatus.ERR_NOT_INITIALIZED"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ERR_DEVICE"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ERR_OTHER"/> if the USB library threw an exception.
        /// </returns>
        protected DeviceStatus Write<StructType>(JVSUSBReports command, StructType struc) where StructType : struct {
            if (device == null) {
                return SetLastError(DeviceStatus.ERR_NOT_INITIALIZED);
            }
            if (command == JVSUSBReports.Unset) {
                throw new ArgumentException("JVS Report command must be set");
            }
            try {
                byte[] data = StructUtils.GetBytes(struc);
                byte[] payload = new byte[data.Length + 1];
                payload[0] = (byte)command;
                Array.Copy(data, 0, payload, 1, data.Length);
                if (payload.Length != 63) {
                    throw new ArgumentException("invalid payload size: " + payload.Length);
                }
                bool success = device.WriteReport(new HidReport(payload.Length) {
                    ReportId = OUTGOING_REPORT_ID,
                    Data = payload
                }, Timeout);
                if (!success) {
                    Log.WriteError("HID Write failed");
                }
                return SetLastError(success ? DeviceStatus.OK : DeviceStatus.ERR_DEVICE, Marshal.GetLastWin32Error());
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed writing data to " + GetName());
                return SetLastError(DeviceStatus.ERR_OTHER);
            }
        }

    }

}
