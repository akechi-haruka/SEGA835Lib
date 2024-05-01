using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Misc;
using HidLibrary;
using System.Runtime.InteropServices;
using System.Text;

namespace Haruka.Arcade.SEGA835Lib.Devices.IO {
    public abstract class JVSUSBIO : JVSIO {

        private const int OUTGOING_REPORT_ID = 0x10;
        private const int INCOMING_REPORT_ID = 0x01;

        public int USBVendorID { get; private set; }
        public int USBProductID { get; private set; }
        public int Timeout { get; private set; } = 1000;

        private HidDevice device;

        public JVSUSBIO(int vid, int pid) {
            USBVendorID = vid;
            USBProductID = pid;
        }

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

        public override DeviceStatus Disconnect() {
            if (device != null) {
                device.CloseDevice();
                device = null;
            }
            return SetLastError(DeviceStatus.OK);
        }

        public bool IsConnected() {
            return device?.IsConnected ?? false;
        }

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
                return SetLastError(DeviceStatus.OK);
            } catch (Exception ex) {
                Log.WriteFault(ex, "Failed reading data from " + GetName());
                report = default;
                return SetLastError(DeviceStatus.ERR_OTHER);
            }
        }

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

    }

}
