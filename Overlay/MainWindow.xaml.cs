using AutoTester;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Overlay
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const double dpi = 1.25;
		Model model = new Model();
		Process process;
		public MainWindow()
		{
			InitializeComponent();
			InitAsync();

		}

		private async Task InitAsync()
		{
			//process = await AppTest.StartAsync(@"C:\Program Files\7-Zip\7zFM.exe", 1000);
			const string FileName = @"notepad";
			process = await AppTest.StartAsync(FileName, 1000);
			model.Program = FileName;

			RECT rect, frame, client;
			User32.GetClientRect(process.MainWindowHandle, out client);
			User32.GetWindowRect(process.MainWindowHandle, out rect);
			User32.DwmGetWindowAttribute(process.MainWindowHandle, (int)User32.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out rect, Marshal.SizeOf(typeof(RECT)));
			grid.Width = 1 / dpi * (rect.Right - rect.Left);
			grid.Height = 1 / dpi * (rect.Bottom - rect.Top);

			clientRect.Width = 1 / dpi * (client.Right - client.Left);
			clientRect.Height = 1 / dpi * (client.Bottom - client.Top);

			POINT p;
			p.X = rect.Left;
			p.Y = rect.Top;

			Point relativePoint = grid.TransformToAncestor(this)
							  .Transform(new Point(0, 0));

			POINT p2;
			p2.X = 0;
			p2.Y = 0;

			User32.ClientToScreen(process.MainWindowHandle, ref p2);
			clientRect.Margin = new Thickness((p2.X - p.X) /dpi, (p2.Y - p.Y)/dpi , 0, 0);
			this.Left = (1 / dpi * p.X) - relativePoint.X;
			this.Top = (1 / dpi * p.Y) - relativePoint.Y;
			//this.Activate();
		}

		private async void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(clientRect);
			MouseClickAction click = new MouseClickAction
			{
				X = (int)(p.X * dpi),
				Y = (int)(p.Y * dpi)
			};
			model.Actions.Add(click);
			double old = grid.Opacity;
			grid.Opacity = 0;
			await Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })); // ui refresh
			await Task.Delay(250);
			await click.Execute(process.MainWindowHandle);
			await Task.Delay(100);
			grid.Opacity = old;

			Console.WriteLine($"click: {click.X}, {click.Y}");
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			double old = grid.Opacity;
			grid.Opacity = 0;

			await Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })); // ui refresh
			await Task.Delay(250);

			User32.SetForegroundWindow(process.MainWindowHandle);
			User32.PrintScreen();
			await Task.Delay(500);
			// ImageUIElement.Source = Clipboard.GetImage(); // does not work
			System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();
			string name = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
			if (clipboardData.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
			{
				System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
				bitmap.Save(name + ".png", ImageFormat.Png);
				Console.WriteLine("Clipboard copied to UIElement");
			}
			grid.Opacity = old;

			model.Actions.Add(new ScreenshotVerifyAction
			{
				Name = name,
				Timeout = 5000
			});
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			process.CloseMainWindow();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private async void Button_Click_2(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
			await Task.Delay(500);
			await AppTest.Run(model, false);
			WindowState = WindowState.Normal;
		}
	}
}
