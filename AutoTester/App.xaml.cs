using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AutoTester
{
	/// <summary>
	/// Win32 API Window Styles
	/// </summary>
	[Flags]
	enum WindowStyles : int
	{
		WS_CHILD = 0x40000000,
		WS_CLIPSIBLINGS = 0x04000000,
		WS_CLIPCHILDREN = 0x02000000,
		WS_VISIBLE = 0x10000000
	}

	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application
	{
		protected override async void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			await CreateNewTest();
		}

		private async Task CreateNewTest()
		{

			Process process = await AppTest.StartAsync("notepad", 1000);

			while (true)
			{
				Console.WriteLine("MainWindow: " + process.MainWindowHandle);
				var top = User32.GetTopWindow(process.MainWindowHandle);
				Console.WriteLine("Top window: " + top);
				/*User32.EnumWindows((ptr,arg) =>
				{
					int pId;
					User32.GetWindowThreadProcessId(ptr, out pId);
					if (pId == process.Id)
					{
						StringBuilder bld = new StringBuilder(200);
						User32.GetWindowText(ptr, bld, 200);
						Console.WriteLine($"{ptr} - {bld} - {User32.IsWindowEnabled(ptr)}");
					}
					return true;
				}, IntPtr.Zero);*/

				foreach (var ptr in User32.EnumerateProcessWindowHandles(process.Id))
				{
					StringBuilder bld = new StringBuilder(200);
					User32.GetWindowText(process.MainWindowHandle, bld, 200);
					if (User32.IsWindowVisible(ptr))
					{
						Console.WriteLine($"{ptr} - {bld} -  {User32.IsWindowEnabled(ptr)} - {User32.IsWindowVisible(ptr)} - {User32.GetWindowLongA(ptr, -16)& 0x00C00000}");
					}
				}
				
			
				Console.WriteLine();
				await Task.Delay(1000);
			}

			return;
			HwndSourceParameters parameters = new HwndSourceParameters();
			parameters.WindowStyle = (int)(WindowStyles.WS_CHILD | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_VISIBLE);
			parameters.ParentWindow = process.MainWindowHandle;
			Overlay frameworkElement = new Overlay();


			HwndSource hwndSource = new HwndSource(parameters)
			{
				SizeToContent = SizeToContent.WidthAndHeight,
				RootVisual = frameworkElement
			};
			hwndSource.CompositionTarget.BackgroundColor = Colors.LightSteelBlue;   // black background seems too dominant, use a different default


			frameworkElement.Width = 500;
			frameworkElement.Height = 500;
		}

		private static void RunExisting()
		{
			Model model = new Model
			{
				Program = "notepad",
				Actions =
				{
					new MouseClickAction { X = 10, Y = 0 }
				}
			};

			AppTest.Run(model, interactive: true);
		}
	}
}
