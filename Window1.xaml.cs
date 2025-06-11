
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using System.Management;
using System.Web;
using System.Security.Principal;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Krr_Settings
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Form f;
		public System.Windows.Forms.TextBox textBox1;
		[DllImport("user32.dll")]
		private static extern bool SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);
		[DllImport("user32.dll")]
		private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
		[DllImport("user32.dll")]
		private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(String lpFileName);		
		private const int SPI_SETDESKWALLPAPER = 20;
		private const int SPIF_UPDATEINIFILE = 0x01;
		private const int SPIF_SENDWININICHANGE = 0x02;
		private const int SPI_GETDESKWALLPAPER = 0x0073;
		private const int MAX_PATH = 260;
		private const uint IMAGE_BITMAP = 0;
		private const uint LR_LOADFROMFILE = 0x0010;
		
		private bool AUHI;
		private bool LOTA;
		private bool HASB;
		private bool HIDB;
		private bool HIPA;
		private bool ONTO;
		private bool USSC;
		private bool ENDF;
		private byte R;
		private byte G;
		private byte B;
		private byte RR;
		private byte GG;
		private byte BB;
		private int a1;
		private int a2;
		private int a3;
		private int a4;
		private int a5;
		private int a6;
		private string a7;
		private string b1;
		private int b2;
		private int b3;
		private string bmp;
		private WindowsIdentity UserName = WindowsIdentity.GetCurrent();
		private bool Usertype;
		
		
		public void LoadRegistryIntoVar()
		{
			try {
				UserNameSidebar.Content = UserName.Name.Split('\\')[1];
				UserNameUserTab.Content = UserName.Name.Split('\\')[1];
				WindowsPrincipal p = new WindowsPrincipal(UserName);
				Usertype = p.IsInRole(WindowsBuiltInRole.Administrator);
				if (Usertype == true) {
					isa.Content = "Administrator";
				} else {
					isa.Content = "User";
				}
				
				RegistryKey AppBarKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Shell\Appbar");
				RegistryKey StartMenu = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Shell\StartMenu");
				a1 = (int)AppBarKey.GetValue("AutoHide");
				a2 = (int)AppBarKey.GetValue("Locked");
				a3 = (int)AppBarKey.GetValue("StartButton");
				a4 = (int)AppBarKey.GetValue("ShowDesktopButton");
				a5 = (int)AppBarKey.GetValue("PinnedBar");
				a6 = (int)AppBarKey.GetValue("OnTop");
				a7 = AppBarKey.GetValue("BackColor").ToString();
				b1 = StartMenu.GetValue("BackColor2").ToString();
				b2 = (int)StartMenu.GetValue("DefaultFolder");
				b3 = (int)StartMenu.GetValue("UseSystemColor");
				AppBarKey.Close();
				StartMenu.Close();
			} catch (Exception e) {
				System.Windows.Forms.MessageBox.Show("Error Registry not found " + e);
				return;
			}
			try {
				if (!string.IsNullOrWhiteSpace(a7) && a7.Length == 7) {
					RR = Convert.ToByte(a7.Substring(1, 2), 16);
					GG = Convert.ToByte(a7.Substring(3, 2), 16);
					BB = Convert.ToByte(a7.Substring(5, 2), 16);
				} else {
					System.Windows.Forms.MessageBox.Show("error Non Valid color " + b1.ToString());
				}
			} catch (Exception e) {
				System.Windows.Forms.MessageBox.Show("Cant convert hex to rgb " + e);
			}
			try {
				if (!string.IsNullOrWhiteSpace(b1) && b1.Length == 7) {
					R = Convert.ToByte(b1.Substring(1, 2), 16);
					G = Convert.ToByte(b1.Substring(3, 2), 16);
					B = Convert.ToByte(b1.Substring(5, 2), 16);
				} else {
					System.Windows.Forms.MessageBox.Show("error Non Valid color " + b1.ToString());
				}
			} catch (Exception e) {
				System.Windows.Forms.MessageBox.Show("Cant convert hex to rgb " + e);
			}
			AUHI = a1 != 0;
			LOTA = a2 != 0;
			HASB = a3 != 0;
			HIDB = a4 != 0;
			HIPA = a5 != 0;
			ONTO = a6 != 0;
			USSC = b3 != 0;
			ENDF = b2 != 0;
			
			ah.IsChecked = AUHI;
			lt.IsChecked = LOTA;
			hsb.IsChecked = HASB;
			hdb.IsChecked = HIDB;
			HPB.IsChecked = HIPA;
			OT.IsChecked = ONTO;
			USC.IsChecked = USSC;
			EDF.IsChecked = ENDF;
			TBCR.Text = RR.ToString();
			TBCG.Text = GG.ToString();
			TBCB.Text = BB.ToString();
			ccr.Text = R.ToString();
			ccg.Text = G.ToString();
			ccb.Text = B.ToString();
			
			return;
		}
		public static String GDW()
		{
			StringBuilder sb = new StringBuilder(MAX_PATH);
			if (SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, sb, 0)) {
				return sb.ToString();
			}
			return null;
		}
		public static Bitmap LWBL()
		{
			const string dllPath = @"C:\Windows\Branding\ShellBrd\shellbrd.dll";
			IntPtr hInst = LoadLibrary(dllPath);
			
			IntPtr hImage = LoadImage(hInst, "#1", IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
			
			if (hImage == IntPtr.Zero)
				return null;
			
			return System.Drawing.Image.FromHbitmap(hImage);
		}
		public Window1()
		{
			InitializeComponent();
		}
		void menuItem1_Click(object sender, RoutedEventArgs e)
		{
			//HKEY_CURRENT_USER\Software\NW
			//HKEY_CURRENT_USER\Software\Shell
		}
		public void ChangeContent(int id)
		{
			id1.Visibility = Visibility.Collapsed;
			id2.Visibility = Visibility.Collapsed;
			id3.Visibility = Visibility.Collapsed;
			id4.Visibility = Visibility.Collapsed;
			
			switch (id) {
				case 1:
					id1.Visibility = Visibility.Visible;
					break;
				case 2:
					id2.Visibility = Visibility.Visible;
					break;
				case 3:
					id3.Visibility = Visibility.Visible;
					break;
				case 4:
					id4.Visibility = Visibility.Visible;
					update();
					break;
			}
		}
		void Button1_checked(object sender, RoutedEventArgs e)
		{
			ChangeContent(1);
		}
		void Button2_checked(object sender, RoutedEventArgs e)
		{
			ChangeContent(2);
		}
		void image1_Loaded(object sender, EventArgs e)
		{
			update();
		}
		public void update()
		{
			var bg = GDW();
			
			ImageBrush b = new ImageBrush(new BitmapImage(new Uri(bg))) {
				Stretch = Stretch.Uniform,
				AlignmentX = AlignmentX.Center,
				AlignmentY = AlignmentY.Center
			};
			
			image1.Background = b;
		}
		void button1_Click(object sender, RoutedEventArgs e)
		{
			
		}
		void Button3_checked(object sender, RoutedEventArgs e)
		{
			ChangeContent(3);
		}
		void Button4_checked(object sender, RoutedEventArgs e)
		{
			ChangeContent(4);
		}
		private void about()
		{
			Form f1 = new Form();
			f1.Show();
			
			f1.Width = 400;
			f1.Height = 300;
			PictureBox pictureBox1 = new PictureBox();
			pictureBox1.Width = 400;
			pictureBox1.Height = 150;
			pictureBox1.Image = LWBL();
			pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
			System.Windows.Forms.Label txt = new System.Windows.Forms.Label();
			txt.Text = "Todo This window.";
			f1.Controls.Add(pictureBox1);
			
		}
		void ButtonA_checked(object sender, RoutedEventArgs e)
		{
			about();
		}
		void Button5_checked(object sender, RoutedEventArgs e)
		{
		}
		public void fbutton1_Click(object sender, EventArgs e)
		{
			f.Close();
		}
		public void fbutton2_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.MessageBox.Show(textBox1.Text);
			f.Close();
		}
		void button2_Click(object sender, RoutedEventArgs e)
		{
			f = new Form();
			System.Windows.Forms.Label label1 = new System.Windows.Forms.Label();
			System.Windows.Forms.Button fbutton1 = new System.Windows.Forms.Button();
			System.Windows.Forms.Button fbutton2 = new System.Windows.Forms.Button();
			f.Show();
			f.MinimizeBox = false;
			f.Width = 300;
			f.Height = 200;
			label1.Text = "Enter Password";
			label1.Top = 10;
			textBox1 = new System.Windows.Forms.TextBox();
			textBox1.Width = 300;
			textBox1.Top = 35;
			fbutton1.Text = "Cancel";
			fbutton1.Top = 130;
			fbutton2.Text = "Apply";
			fbutton2.Top = 130;
			fbutton2.Left = 75;
			fbutton1.Click += fbutton1_Click;
			fbutton2.Click += fbutton2_Click;
			f.SuspendLayout();
			
			f.Controls.Add(fbutton1);
			f.Controls.Add(fbutton2);
			f.Controls.Add(label1);
			f.Controls.Add(textBox1);
			f.ResumeLayout();
		}
		void grid1_Loaded(object sender, RoutedEventArgs e)
		{
			LoadRegistryIntoVar();
		}
		void button3_Click(object sender, RoutedEventArgs e)
		{
			RegistryKey AppBarKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Shell\Appbar");
			RegistryKey StartMenu = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Shell\StartMenu");
			
			int sa1;
			int sa2;
			int sa3;
			int sa4;
			int sa5;
			int sa6;
			string sa7;
			string sb1;
			int sb2;
			int sb3;
			
			if ((bool)ah.IsChecked == true) {
				sa1 = 1;
			} else {
				sa1 = 0;
			}
			if ((bool)lt.IsChecked == true) {
				sa2 = 1;
			} else {
				sa2 = 0;
			}
			if ((bool)hsb.IsChecked == true) {
				sa3 = 1;
			} else {
				sa3 = 0;
			}
			if ((bool)hdb.IsChecked == true) {
				sa4 = 1;
			} else {
				sa4 = 0;
			}
			if ((bool)HPB.IsChecked == true) {
				sa5 = 1;
			} else {
				sa5 = 0;
			}
			if ((bool)OT.IsChecked == true) {
				sa6 = 1;
			} else {
				sa6 = 0;
			}
			if ((bool)EDF.IsChecked == true) {
				sb2 = 1;
			} else {
				sb2 = 0;
			}
			if ((bool)USC.IsChecked == true) {
				sb3 = 1;
			} else {
				sb3 = 0;
			}
			sa7 = "#" + RR.ToString("x") + GG.ToString("x") + BB.ToString("x");
			sb1 = "#" + R.ToString("x") + G.ToString("x") + B.ToString("x");
			
			AppBarKey.SetValue("AutoHide", sa1);
			AppBarKey.SetValue("Locked", sa2);
			AppBarKey.SetValue("StartButton", sa3);
			AppBarKey.SetValue("ShowDesktopButton", sa4);
			AppBarKey.SetValue("PinnedBar", sa5);
			AppBarKey.SetValue("OnTop", sa6);
			AppBarKey.SetValue("BackColor", sa7);
			StartMenu.SetValue("BackColor2", sb1);
			StartMenu.SetValue("DefaultFolder", sb2);
			StartMenu.SetValue("UseSystemColor", sb3);
			AppBarKey.Close();
			StartMenu.Close();
		}
		private void PIASW()
		{
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog {
				Title = "Choose an Image for the background",
				Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png",
				CheckFileExists = true
			};
			
			if (ofd.ShowDialog() == true) {
				string sp = ofd.FileName;
				
				bmp = ctbmpin(sp);
				RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop");
				switch (cb.SelectedIndex.ToString()) {
					case "0":
						key.SetValue("WallpaperStyle", "10");
						key.SetValue("TileWallpaper", "0");
						break;
					case "1":
						key.SetValue("WallpaperStyle", "6");
						key.SetValue("TileWallpaper", "0");
						break;
					case "2":
						key.SetValue("WallpaperStyle", "2");
						key.SetValue("TileWallpaper", "0");
						break;
					case "3":
						key.SetValue("WallpaperStyle", "0");
						key.SetValue("TileWallpaper", "1");
						break;
					case "4":
						key.SetValue("WallpaperStyle", "0");
						key.SetValue("TileWallpaper", "0");
						break;
					case "5":
						key.SetValue("WallpaperStyle", "22");
						key.SetValue("TileWallpaper", "0");
						break;
				}
				key.Close();
				bool result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, bmp, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
			}
		}
		private string ctbmpin(string impa)
		{
			string ext = System.IO.Path.GetExtension(impa).ToLower();
			if (ext == ".bmp" && ext == ".png")
				return impa;
			
			string bmpp = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), ".bmp");
			using (var img = System.Drawing.Image.FromFile(impa)) {
				img.Save(bmpp, System.Drawing.Imaging.ImageFormat.Bmp);
			}
			return bmpp;
			
			
		}
		void button4_Click(object sender, RoutedEventArgs e)
		{
			PIASW();
			update();
		}
		void cb_Loaded(object sender, RoutedEventArgs e)
		{
		}
		void button4_Copy_Click(object sender, RoutedEventArgs e)
		{
			RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop");
			switch (cb.SelectedIndex.ToString()) {
				case "0":
					key.SetValue("WallpaperStyle", "10");
					key.SetValue("TileWallpaper", "0");
					break;
				case "1":
					key.SetValue("WallpaperStyle", "6");
					key.SetValue("TileWallpaper", "0");
					break;
				case "2":
					key.SetValue("WallpaperStyle", "2");
					key.SetValue("TileWallpaper", "0");
					break;
				case "3":
					key.SetValue("WallpaperStyle", "0");
					key.SetValue("TileWallpaper", "1");
					break;
				case "4":
					key.SetValue("WallpaperStyle", "0");
					key.SetValue("TileWallpaper", "0");
					break;
				case "5":
					key.SetValue("WallpaperStyle", "22");
					key.SetValue("TileWallpaper", "0");
					break;
			}
			key.Close();
			if (bmp == null) {
				bool result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, bmp, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
			} else {
				StringBuilder sb = new StringBuilder(MAX_PATH);
				if (SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, sb, 0)) {
					bool result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, sb.ToString(), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
				}
				
			}
		}
	}
}