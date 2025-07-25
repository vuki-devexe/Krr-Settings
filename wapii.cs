using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Krr_Settings
{
    public class wapii
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string lpvParam, uint fuWinIni);

        // Constants for SystemParametersInfo
        private const uint SPI_SETDESKWALLPAPER = 0x14; // 20
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDWININICHANGE = 0x02;

        // Enum for wallpaper styles
        public enum Style : int
        {
            Tile = 0,
            Stretch = 2,
            Fit = 6,    // Windows 7 and later
            Fill = 10,  // Windows 7 and later
            Span = 22   // Windows 8 or newer only (for multi-monitor span)
        }
        public void ChangeBG(String imagePath, Style style) {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"The specified image file was not found: {imagePath}");
            }

            // Set wallpaper style in the registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            // Reset TileWallpaper and WallpaperStyle to defaults first
            key.SetValue(@"TileWallpaper", 0.ToString());
            key.SetValue(@"WallpaperStyle", 0.ToString());

            switch (style)
            {
                case Style.Tile:
                    key.SetValue(@"TileWallpaper", 1.ToString());
                    key.SetValue(@"WallpaperStyle", 0.ToString()); // 0 means centered if not tiled, tiled if tiled.
                    break;
                case Style.Stretch:
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    break;
                case Style.Fit:
                    key.SetValue(@"WallpaperStyle", 6.ToString());
                    break;
                case Style.Fill:
                    key.SetValue(@"WallpaperStyle", 10.ToString());
                    break;
                case Style.Span: // For multi-monitor span (Windows 8+)
                    key.SetValue(@"WallpaperStyle", 22.ToString());
                    break;
            }

            key.Close();

            // Set the wallpaper using SystemParametersInfo
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        public class deviceitem : INotifyPropertyChanged
        {
            private string _name;
            public string Name
            {
                get => _name;
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        OnPropertyChanged();
                    }
                }
            }

            private string _description;
            public string Description
            {
                get => _description;
                set
                {
                    if (_description != value)
                    {
                        _description = value;
                        OnPropertyChanged();
                    }
                }
            }

            private string _deviceID;
            public string DeviceID
            {
                get => _deviceID;
                set
                {
                    if (_deviceID != value)
                    {
                        _deviceID = value;
                        OnPropertyChanged();
                    }
                }
            }

            private bool _canEject;
            public bool CanEject
            {
                get => _canEject;
                set
                {
                    if (_canEject != value)
                    {
                        _canEject = value;
                        OnPropertyChanged();
                    }
                }
            }

            // To store the original WMI object if needed for advanced operations
            public ManagementObject WmiDeviceObject { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public static class SafeDeviceRemoval
    {

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            string Enumerator,
            IntPtr hwndParent,
            uint Flags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiGetDeviceInstanceId(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            StringBuilder DeviceInstanceId,
            uint DeviceInstanceIdSize,
            out uint RequiredSize
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint CM_Request_Device_Eject(
            uint DevInst,
            out PNPOBJECT_REMOVAL_VETO_TYPE VetoType, // <--- This must be 'out'
            StringBuilder VetoName,
            uint VetoNameLength,
            uint Flags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint CM_Get_Parent(
            out uint pdnDevInst,
            uint dnDevInst,
            uint ulFlags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint CM_Get_DevNode_Status(
            out uint pulStatus,
            out uint pulProblemNumber,
            uint dnDevInst,
            uint ulFlags
        );
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(
                Guid ClassGuid, // <--- ENSURE 'ref' IS GONE HERE
                string Enumerator,
                IntPtr hwndParent,
                uint Flags
            );
        // Constants
        private const uint DIGCF_PRESENT = 0x00000002;
        private const uint DIGCF_ALLCLASSES = 0x00000004;

        // Device removal flags (from cfgmgr32.h)
        private const uint CM_REENUMERATE_SYNCHRONOUS = 0x00000001;
        private const uint CM_DEVCAP_REMOVABLE = 0x00000004;
        private const uint CM_DEVCAP_EJECTCONTROLLABLE = 0x00000008;

        // GUID for USB devices (used as a general starting point for enumeration)
        // You might query more specifically if you know the exact device class
        private static readonly Guid GUID_DEVCLASS_USB = new Guid("{36FC9E60-C465-11CF-8056-444553540000}");
        // GUID for Disk Drives
        private static readonly Guid GUID_DEVCLASS_DISKDRIVE = new Guid("{53f56307-b6bf-11d0-94f2-00a0c91efb8b}");


        // Structure definitions (from setupapi.h and cfgmgr32.h)
        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        public enum PNPOBJECT_REMOVAL_VETO_TYPE : int // <-- Add ': int' to explicitly define underlying type
        {
            PNP_VetoTypeUnknown = 0,
            PNP_VetoTypeLegacyDevice = 1,
            PNP_VetoTypePendingInstall = 2,
            PNP_VetoTypeWindowsApp = 3,
            PNP_VetoTypeWindowsService = 4,
            PNP_VetoTypeLegacyDriver = 5,
            PNP_VetoTypeCloseAllHandles = 6,
            PNP_VetoTypeDevice = 7,
            PNP_VetoTypeDriver = 8,
            PNP_VetoTypeIllegalDeviceRequest = 9,
            PNP_VetoTypeInsufficientPower = 10,
            PNP_VetoTypeNonDisableable = 11,
            PNP_VetoTypeNegotiateDisable = 12,
            PNP_VetoTypeNoCompatibleDrivers = 13,
            PNP_VetoTypeNotDisableable = 14,
            PNP_VetoTypePluggedIn = 15,
            PNP_VetoTypeWindowsPolicy = 16,
            PNP_VetoTypeSystemCritical = 17,
            PNP_VetoTypeRemovableSystemDevice = 18,
            PNP_VetoTypeReserved = 19,
        }

        public static bool EjectDevice(string deviceInstanceId)
        {
            IntPtr deviceInfoSet = IntPtr.Zero;
            try
            {
                // Get a handle to the device information set for all present devices
                deviceInfoSet = SetupDiGetClassDevs(Guid.Empty, null, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
                if (deviceInfoSet == IntPtr.Zero)
                {
                    return false;
                }

                SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
                deviceInfoData.cbSize = (uint)Marshal.SizeOf(deviceInfoData);

                uint memberIndex = 0;
                while (SetupDiEnumDeviceInfo(deviceInfoSet, memberIndex, ref deviceInfoData))
                {
                    StringBuilder instanceIdBuilder = new StringBuilder(256);
                    uint requiredSize;
                    if (SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, instanceIdBuilder, (uint)instanceIdBuilder.Capacity, out requiredSize))
                    {
                        string currentDeviceInstanceId = instanceIdBuilder.ToString();

                        // Match the Device Instance ID (case-insensitive)
                        if (string.Equals(currentDeviceInstanceId, deviceInstanceId, StringComparison.OrdinalIgnoreCase))
                        {
                            // Found the device! Now try to eject it.
                            PNPOBJECT_REMOVAL_VETO_TYPE vetoType;
                            StringBuilder vetoName = new StringBuilder(256);
                            uint vetoNameLength = (uint)vetoName.Capacity;

                            // CM_Request_Device_Eject for the device itself
                            uint result = CM_Request_Device_Eject(deviceInfoData.DevInst, out vetoType, vetoName, vetoNameLength, 0);

                            if (result == 0) // CR_SUCCESS
                            {
                                return true;
                            }
                            else
                            {
                                // Attempt to find and eject the parent if direct eject failed and device is removable
                                // This is often necessary for USB drives where you eject the "USB Mass Storage Device" (parent)
                                // rather than the individual "USB Disk Drive" (child volume).
                                uint parentDevInst;
                                if (CM_Get_Parent(out parentDevInst, deviceInfoData.DevInst, 0) == 0) // CR_SUCCESS
                                {
                                    result = CM_Request_Device_Eject(parentDevInst, out vetoType, vetoName, vetoNameLength, 0);
                                    if (result == 0)
                                    {
                                        return true;
                                    }
                                }
                                Console.WriteLine($"Eject failed for {deviceInstanceId}. Veto type: {vetoType}, Veto name: {vetoName}. CM Result: {result}");
                                return false;
                            }
                        }
                    }
                    memberIndex++;
                }
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }
            return false; // Device not found or other error
        }
    }
}
