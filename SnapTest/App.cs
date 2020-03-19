using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace SnapTest
{
	public class App
	{
		private const double dpi = 1;//1.25;
		public static bool Write = false;

		Process process;
		IntPtr windowHandle;

		App()
		{
		}

		void StartPrivate(string fileName)
		{
			var startInfo = new ProcessStartInfo(fileName);
			//startInfo.WorkingDirectory = ".."; // TODO generalize
			startInfo.UseShellExecute = false;
			process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForInputIdle();

			Thread.Sleep(3000);
			windowHandle = process.MainWindowHandle;

			if (IntPtr.Zero == windowHandle)
			{
				throw new ArgumentException("unable to get handle of mainwindow");
			}
		}

		public Bitmap WaitSnapshot(string name, int skipTop, int waitMilli)
		{
			if (Write)
			{
				Thread.Sleep(2000);
				Bitmap bmp = Snapshot(skipTop);
				bmp.Save(name + ".png", ImageFormat.Png);
				return bmp;
			}
			else
			{
				bool match = false;
				Bitmap bitmapFile = (Bitmap)Bitmap.FromFile(name + ".png");

				for (int i = 0; i < 10; i++)
				{
					Bitmap bmp = Snapshot(skipTop);
					if (CompareBitmapsFast(bmp, bitmapFile))
					{
						match = true;
						break;
					}
					Thread.Sleep(100);
					bmp.Dispose();
				}

				bitmapFile.Dispose();

				if (!match)
				{
					throw new ArgumentException();
				}

				return null;
			}
		}

		

		private Bitmap Snapshot(int skipTop)
		{
			if (!NativeMethods.SetForegroundWindow(windowHandle))
				throw new ArgumentException();

			RECT srcRect;
			if (NativeMethods.GetClientRect(windowHandle, out srcRect))
			{
				POINT topLeft;
				topLeft.X = srcRect.Left;
				topLeft.Y = srcRect.Top;

				POINT bottomRight;
				bottomRight.X = srcRect.Right;
				bottomRight.Y = srcRect.Bottom;

				NativeMethods.ClientToScreen(windowHandle, ref topLeft);
				NativeMethods.ClientToScreen(windowHandle, ref bottomRight);

				srcRect.Left = topLeft.X;
				srcRect.Right = bottomRight.X;
				srcRect.Top = topLeft.Y;
				srcRect.Bottom = bottomRight.Y;

				// TODO get dpi

				double scaleX = dpi;
				double scaleY = dpi;

				srcRect.Left = (int)(srcRect.Left * scaleX);
				srcRect.Right = (int)(srcRect.Right * scaleX);
				srcRect.Top = (int)(srcRect.Top * scaleY);
				srcRect.Bottom = (int)(srcRect.Bottom * scaleY);

				int width = srcRect.Right - srcRect.Left;
				int height = srcRect.Bottom - srcRect.Top;

				/*using */
				Bitmap bmp = new Bitmap(width, height - skipTop);
				using (Graphics screenG = Graphics.FromImage(bmp))
				{
					screenG.CopyFromScreen(srcRect.Left, srcRect.Top + skipTop,
							0, 0, new Size(width, height - skipTop),
							CopyPixelOperation.SourceCopy);

					return bmp;
				}
			}
			else
				throw new ArgumentException();
		}

		public void WaitForExit()
		{
			process.WaitForExit();
		}

		public void WaitForInputIdle()
		{
			process.WaitForInputIdle();
		}

		public void Kill()
		{
			process.Kill();
		}

		public static App Start(string fileName)
		{
			App app = new App();
			app.StartPrivate(fileName);

			return app;
		}
	}
}
