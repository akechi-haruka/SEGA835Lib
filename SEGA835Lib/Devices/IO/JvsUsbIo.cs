using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using HidLibrary;
using Microsoft.Extensions.Logging;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO {
    /// <summary>
    /// Base class for a USB-based JVS input device.
    /// </summary>
    public abstract class JvsUsbIo : JvsIo {
        private const int OUTGOING_REPORT_ID = 0x10;
        private const int INCOMING_REPORT_ID = 0x01;

        private static readonly ILogger LOG = LogManager.GetOrCreate(typeof(JvsUsbIo));

        /// <summary>
        /// USB vendor ID of this board.
        /// </summary>
        public int UsbVendorID { get; }

        /// <summary>
        /// USB product ID of this board.
        /// </summary>
        public int UsbProductID { get; }

        /// <summary>
        /// Communication timeout in ms to the board.
        /// </summary>
        public int Timeout => 1000;

        /// <summary>
        /// Last received JVS poll report (from <see cref="Poll(out JvsUsbReportIn)"/>
        /// </summary>
        public JvsUsbReportIn? LastReport { get; private set; }

        private HidDevice device;

        /// <summary>
        /// Creates a new JVSUSBIO.
        /// </summary>
        /// <param name="vid">The vendor id of the board.</param>
        /// <param name="pid">The product id of the board.</param>
        protected JvsUsbIo(int vid, int pid) {
            UsbVendorID = vid;
            UsbProductID = pid;
        }

        /// <summary>
        /// Connects to the USB device.
        /// </summary>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if connection was successful.
        /// <see cref="DeviceStatus.ErrorNotConnected"/> if the board is not attached or if opening the device fails.<br />
        /// </returns>
        public sealed override DeviceStatus Connect() {
            LOG.LogInformation("Open JVS USB: VID:" + UsbVendorID + ", PID: " + UsbProductID);
            device = HidDevices.Enumerate(UsbVendorID, UsbProductID).FirstOrDefault();

            if (device == null) {
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            LOG.LogInformation("Found JVS USB at " + device.DevicePath);

            try {
                device.OpenDevice();
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Opening USB device failed (" + GetName() + ")");
                device = null;
                return SetLastError(DeviceStatus.ErrorNotConnected);
            }

            return SetLastError(DeviceStatus.Ok);
        }

        /// <summary>
        /// Disconnects from the USB device.
        /// </summary>
        /// <returns>Always returns <see cref="DeviceStatus.Ok"/>.</returns>
        public override DeviceStatus Disconnect() {
            if (device != null) {
                device.CloseDevice();
                device = null;
            }

            return SetLastError(DeviceStatus.Ok);
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
        /// <see cref="DeviceStatus.Ok"/> if the data was successfully read.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ErrorOther"/> if the USB library threw an exception.
        /// </returns>
        public DeviceStatus GetManufacturer(out string manufacturer) {
            LOG.LogInformation("GetManufacturer");
            manufacturer = null;
            if (device == null) {
                return SetLastError(DeviceStatus.ErrorNotInitialized);
            }

            try {
                bool success = device.ReadManufacturer(out byte[] data);
                if (!success) {
                    return SetLastError(DeviceStatus.ErrorDevice);
                }

                manufacturer = Encoding.ASCII.GetString(data);
                return SetLastError(DeviceStatus.Ok);
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Failed reading USB Device Manufacturer of " + GetName());
                return SetLastError(DeviceStatus.ErrorOther);
            }
        }

        /// <summary>
        /// Returns the USB HID product name of the board.
        /// </summary>
        /// <param name="product">The product string that was read from the board.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the data was successfully read.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ErrorOther"/> if the USB library threw an exception.
        /// </returns>
        public DeviceStatus GetProduct(out string product) {
            LOG.LogInformation("GetProduct");
            product = null;
            if (device == null) {
                return SetLastError(DeviceStatus.ErrorNotInitialized);
            }

            try {
                bool success = device.ReadProduct(out byte[] data);
                if (!success) {
                    return SetLastError(DeviceStatus.ErrorDevice);
                }

                product = Encoding.ASCII.GetString(data);
                return SetLastError(DeviceStatus.Ok);
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Failed reading USB Device Product of " + GetName());
                return SetLastError(DeviceStatus.ErrorOther);
            }
        }

        /// <summary>
        /// Reads a report from the USB device.
        /// </summary>
        /// <param name="report">The report that was read from the device.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the data was successfully read.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorIncompatible"/> if an unexpected report type was read.<br />
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ErrorOther"/> if the USB library threw an exception.
        /// </returns>
        public DeviceStatus Poll(out JvsUsbReportIn report) {
            if (device == null) {
                report = default;
                return SetLastError(DeviceStatus.ErrorNotInitialized);
            }

            try {
                HidReport data = device.ReadReport(Timeout);
                if (data == null) {
                    report = default;
                    return SetLastError(DeviceStatus.ErrorDevice);
                }

                if (data.ReportId != INCOMING_REPORT_ID) {
                    report = default;
                    LOG.LogError("Read unknown report id " + data.ReportId);
                    return SetLastError(DeviceStatus.ErrorIncompatible);
                }

                report = StructUtils.FromBytes<JvsUsbReportIn>(data.Data);
                LastReport = report;
                return SetLastError(DeviceStatus.Ok);
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Failed reading data from " + GetName());
                report = default;
                return SetLastError(DeviceStatus.ErrorOther);
            }
        }

        /// <summary>
        /// Writes a report to the USB device.
        /// </summary>
        /// <param name="report">The report that should be written to the device.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the data was successfully written.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ErrorOther"/> if the USB library threw an exception.
        /// </returns>
        protected DeviceStatus Write(JvsUsbReportOut report) {
            if (device == null) {
                return SetLastError(DeviceStatus.ErrorNotInitialized);
            }

            if (report.cmd == JvsUsbReports.Unset) {
                throw new ArgumentException("JVS Report command must be set");
            }

            try {
                bool success = device.WriteReport(new HidReport(0x3F) {
                    ReportId = OUTGOING_REPORT_ID,
                    Data = StructUtils.GetBytes(report)
                }, Timeout);
                return SetLastError(success ? DeviceStatus.Ok : DeviceStatus.ErrorDevice, Marshal.GetLastWin32Error());
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Failed writing data to " + GetName());
                return SetLastError(DeviceStatus.ErrorOther);
            }
        }

        /// <summary>
        /// Writes an arbitary struct to the USB device.
        /// </summary>
        /// <param name="command">The command being sent.</param>
        /// <param name="struc">The payload data.</param>
        /// <returns>
        /// <see cref="DeviceStatus.Ok"/> if the data was successfully written.<br />
        /// <see cref="DeviceStatus.ErrorNotInitialized"/> if <see cref="Connect"/> was never called.<br />
        /// <see cref="DeviceStatus.ErrorDevice"/> if there was a communication error with the device.<br />
        /// <see cref="DeviceStatus.ErrorOther"/> if the USB library threw an exception.
        /// </returns>
        protected DeviceStatus Write<TStruct>(JvsUsbReports command, TStruct struc) where TStruct : struct {
            if (device == null) {
                return SetLastError(DeviceStatus.ErrorNotInitialized);
            }

            if (command == JvsUsbReports.Unset) {
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
                    LOG.LogError("HID Write failed");
                }

                return SetLastError(success ? DeviceStatus.Ok : DeviceStatus.ErrorDevice, Marshal.GetLastWin32Error());
            } catch (Exception ex) {
                LOG.LogCritical(ex, "Failed writing data to " + GetName());
                return SetLastError(DeviceStatus.ErrorOther);
            }
        }
    }
}