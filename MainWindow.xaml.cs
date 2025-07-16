using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
                    MessageBox.Show(ex.ToString());
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
                MessageBox.Show($"An Error Occurred: {ex.Message}","Error");
                ConnectedToInternet = 3;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            IsConnectedToInternetViaPing();
            
        }
        public void ChangePage(int PageId, int SubPageId)
        {
            HomePage.Visibility = Visibility.Collapsed;
            SystemPage.Visibility = Visibility.Collapsed;

            AboutOSSubPage.Visibility = Visibility.Collapsed;
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
                    AboutOSSubPage.Visibility = Visibility.Visible;
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
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(0,0);
        }

        private void SysRadio_Checked(object sender, RoutedEventArgs e)
        {
            ChangePage(1, 0);
        }

        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConnectedToInternet == 1)
            {
                WifiConHome.Content = "Connected";
            }
            else if (ConnectedToInternet == 2)
            {
                WifiConHome.Content = "SocketError";
            }
            else if (ConnectedToInternet == 3)
            {
                WifiConHome.Content = "OtherError";
            }
            else {
                WifiConHome.Content = "Disconnected";
            }
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
    }
}
