using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Krr_Settings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public string con_type;
        public wapii winapii = new wapii();
        string pcName = Environment.MachineName;
        string userName = Environment.UserName;
        private const uint SPI_GETDESKWALLPAPER = 0x0073;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uAction, uint uParam, string lpvParam, uint fuWinIni);

        public static string GetDesktopWallpaperPath()
        {
            string wallpaperPath = new string('\0', 260);
            int result = SystemParametersInfo(SPI_GETDESKWALLPAPER, 260, wallpaperPath,0);

            if (result != 0)
            {
                return wallpaperPath.Substring(0, wallpaperPath.IndexOf('\0'));
            }
            else {
                return null;
            }

        }

        public static BitmapImage GetDesktopWallpaperImage()
        {
            string wallpaperPath = GetDesktopWallpaperPath();
            if (!string.IsNullOrEmpty(wallpaperPath) && File.Exists(wallpaperPath))
            {
                try
                {
                    var fs = new FileStream(wallpaperPath, FileMode.Open, FileAccess.Read);
                    var ms = new MemoryStream();
                    
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                    return null;
                }
            }
            else {
                return null;
            }
        }
        public BitmapImage UserBackground = GetDesktopWallpaperImage();
        public int ConnectedToInternet = 0;
        public void IsConnectedToInternetViaPing()
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        ConnectedToInternet = 1;
                    }
                }
            }

            catch (PingException)
            {
                ConnectedToInternet = 0;
            }
            catch (SocketException)
            {
                ConnectedToInternet = 2;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"An Error Occurred: {ex.Message}","Error");
                ConnectedToInternet = 3;
            }

            OtherErrorSigh.Visibility = Visibility.Hidden;
            DisconectedSign.Visibility = Visibility.Hidden;
            SocketErrorSign.Visibility = Visibility.Hidden;
            if (ConnectedToInternet == 1)
            {
                con_type = "Connected";
                StatusTextWifi.Content = "Connected";
            }
            else if (ConnectedToInternet == 2)
            {
                con_type = "SocketError";
                SocketErrorSign.Visibility = Visibility.Visible;
                StatusTextWifi.Content = "Socket isuee problem with the socket";

            }
            else if (ConnectedToInternet == 3)
            {
                con_type = "OtherError";
                OtherErrorSigh.Visibility = Visibility.Visible;
                StatusTextWifi.Content = "Some other error is preventing showing the status";
            }
            else
            {
                con_type = "Disconnected";
                DisconectedSign.Visibility = Visibility.Visible;
                StatusTextWifi.Content = "Disconected";
            }

            WifiConHome.Content = con_type;
        }
        public ObservableCollection<string> CommandOutput { get; set; }
        public ObservableCollection<wapii.deviceitem> DeviceList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            IsConnectedToInternetViaPing();
            CommandOutput = new ObservableCollection<string>();
            SystemInfo.ItemsSource = CommandOutput;

            // Clear previous output
            CommandOutput.Clear();

            Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "systeminfo",
                        Arguments = "",
                        UseShellExecute = false,           // Do not use the OS shell to start the process
                        RedirectStandardOutput = true,     // Redirect standard output to our application
                        RedirectStandardError = true,      // Redirect standard error to our application
                        CreateNoWindow = true              // Do not create a new window for the process
                    };

                    using (Process process = new Process { StartInfo = startInfo })
                    {
                        process.OutputDataReceived += (sender, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                // Update UI on the Dispatcher thread
                                Dispatcher.Invoke(() => CommandOutput.Add(args.Data));
                            }
                        };
                        process.ErrorDataReceived += (sender, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                // Update UI on the Dispatcher thread
                                Dispatcher.Invoke(() => CommandOutput.Add($"ERROR: {args.Data}"));
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine(); // Begin asynchronous reading of standard output
                        process.BeginErrorReadLine();  // Begin asynchronous reading of standard error

                        process.WaitForExit(); // Wait for the process to exit
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => CommandOutput.Add($"Error executing command: {ex.Message}"));
                }
            });
            DeviceList = new ObservableCollection<wapii.deviceitem>();
            DeviceListBox.ItemsSource = DeviceList;
            DeviceList.Clear();
            
            try
            {
                // Query for all Plug and Play devices
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject device in searcher.Get())
                {
                    string deviceId = device["DeviceID"]?.ToString();

                    // Only process devices that have a DeviceID and are related to USB or disk drives
                    if (!string.IsNullOrEmpty(deviceId) &&
                        (deviceId.StartsWith("USB\\") || deviceId.Contains("DISK&VEN_") || deviceId.Contains("VOLUMEDEV")))
                    {
                        wapii.deviceitem item = new wapii.deviceitem
                        {
                             Name = device["Name"]?.ToString() ?? "N/A",
                             Description = device["Description"]?.ToString() ?? "N/A",
                             DeviceID = deviceId,
                             WmiDeviceObject = device // Store the WMI object for later use
                        };

                        if (deviceId.Contains("USB") && (device["ClassGuid"]?.ToString().ToLower() == "{53f56307-b6bf-11d0-94f2-00a0c91efb8b}" || // Disk Drive Class GUID
                                                        device["ClassGuid"]?.ToString().ToLower() == "{36fc9e60-c465-11cf-8056-444553540000}")) // USB Device Class GUID
                        {
                            // A more reliable way is to check the Capabilities property
                            // which is a bitmask. For removable, look for capability bit 4 (CM_DEVCAP_REMOVABLE)
                            // or 6 (CM_DEVCAP_EJECTCONTROLLABLE)
                            uint? capabilities = device["Capabilities"] as uint?;
                            if (capabilities.HasValue)
                            {
                                    // CM_DEVCAP_REMOVABLE = 0x4 (bit 2, value 4) - device is removable
                                    // CM_DEVCAP_EJECTCONTROLLABLE = 0x8 (bit 3, value 8) - device can be programmatically ejected
                                if ((capabilities.Value & 0x4) != 0 || (capabilities.Value & 0x8) != 0)
                                {
                                    item.CanEject = true;
                                }
                            }
                                // Fallback for some older devices or specific cases (less reliable)
                        else if (device["Service"]?.ToString() == "Ustor" || device["Caption"]?.ToString().Contains("USB") == true)
                        {
                             item.CanEject = true;
                        }
                    }

                            // Add other device types that might be ejectable, e.g., CD/DVD drives
                    if (device["Caption"]?.ToString().Contains("CD-ROM") == true || device["Caption"]?.ToString().Contains("DVD") == true)
                    {
                         item.CanEject = true;
                     }

                        DeviceList.Add(item);
                    }
                }

                if (DeviceList.Count == 0)
                {
                    DeviceList.Add(new wapii.deviceitem { Name = "No relevant devices found or access denied." });
                }
            }
            catch (ManagementException ex)
            {
                DeviceList.Add(new wapii.deviceitem { Name = $"WMI Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                DeviceList.Add(new wapii.deviceitem { Name = $"An error occurred: {ex.Message}" });
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ChangePage(0, 0);
        }
        private void ChangePage(int PageId, int SubPageId)
        {
            HomePage.Visibility = Visibility.Collapsed;
            SystemPage.Visibility = Visibility.Collapsed;
            DevicePage.Visibility = Visibility.Collapsed;
            InternetPage.Visibility = Visibility.Collapsed;
            PersonalizePage.Visibility = Visibility.Collapsed;

            WallpaperSubPage.Visibility = Visibility.Collapsed;
            CheckForUpdatesSubPage.Visibility = Visibility.Collapsed;
            SystemInfoSubPage.Visibility = Visibility.Collapsed;
            if (PageId == 0) {
                if (SubPageId == 0) {
                    HomePage.Visibility = Visibility.Visible;
                    IsConnectedToInternetViaPing();
                }
                
            }
            if (PageId == 1) {
                if (SubPageId == 0)
                {
                    SystemPage.Visibility = Visibility.Visible;
                }
                if (SubPageId == 1)
                {
                    SystemInfoSubPage.Visibility = Visibility.Visible;
                }
                if (SubPageId == 2)
                {
                    CheckForUpdatesSubPage.Visibility = Visibility.Visible;
                }
                if (SubPageId == 3)
                {
                    SystemInfoSubPage.Visibility = Visibility.Visible;
                }

            }
            if (PageId == 2)
            {
                if (SubPageId == 0)
                {
                    DevicePage.Visibility = Visibility.Visible;
                }

            }
            if (PageId == 3)
            {
                if (SubPageId == 0)
                {
                    InternetPage.Visibility = Visibility.Visible;
                    IsConnectedToInternetViaPing();
                }
            }
            if (PageId == 4)
            {
                if (SubPageId == 0)
                {
                    PersonalizePage.Visibility = Visibility.Visible;
                }
                if (SubPageId == 1) 
                {
                    WallpaperSubPage.Visibility = Visibility.Visible;
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(0, 0);
        }

        private void SysRadio_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(1, 0);
        }
        
        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBrush ib = new ImageBrush(UserBackground);

            DesktopHome.Background = ib;
        }

        private void PCNameHome_Loaded(object sender, RoutedEventArgs e)
        {
            PCNameHome.Content = pcName;
            UserHome.Content = userName;
            UserSidebar.Content = userName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1,1);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ChangePage(1,2);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ChangePage(1,3);
        }

        private void DeviceRadio_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(2, 0);
        }

        private void WifiRadio_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(3, 0);
        }
        private void EjectDevice_Click(object sender, RoutedEventArgs e)
        {
            // Get the MenuItem that was clicked
            //MenuItem menuItem = sender as MenuItem;
            //if (menuItem == null) return;

            // Get the DataContext of the MenuItem (which is the deviceitem it belongs to)
            //wapii.deviceitem selectedDevice = menuItem.DataContext as wapii.deviceitem;

            //if (selectedDevice != null && selectedDevice.CanEject)
            //{
            //    // Execute eject logic in a background thread to keep UI responsive
            //    Task.Run(() =>
            //    {
            //        bool ejected = SafeDeviceRemoval.EjectDevice(selectedDevice.DeviceID);
            //
            //        // Update UI on the Dispatcher thread
            //        Dispatcher.Invoke(() =>
            //        {
            //            if (ejected)
            //            {
            //                System.Windows.MessageBox.Show($"Successfully ejected: {selectedDevice.Name}", "Eject Device", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Optionally, refresh the device list after ejection
                            // ListUsbDevices_Click(null, null);
            //            }
            //            else
            //            {
            //                System.Windows.MessageBox.Show($"Failed to eject: {selectedDevice.Name}. It might be in use or not truly ejectable.", "Eject Device Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            //            }
            //        });
            //    });
            //}
        }

        private void Border_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            string wallpaperPath = GetDesktopWallpaperPath();
            if (!string.IsNullOrEmpty(wallpaperPath) && File.Exists(wallpaperPath))
            {
                try
                {
                    WallpaperTextbox.Text = wallpaperPath;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    WallpaperTextbox.Text = openFileDialog.FileName;
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ChangePage(4, 1);
        }

        private void CostomRadio_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(4, 0);
        }

        //using (OpenFileDialog openFileDialog = new OpenFileDialog())
        //{
        //    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*";
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        txtImagePath.Text = openFileDialog.FileName;
        //    }
        //}
    }
}


