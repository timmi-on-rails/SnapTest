using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTester
{
	public class ScreenshotVerifyAction : IAction
	{
		public string Name { get; set; }
		public int Timeout { get; set; }

		public async Task Execute(IntPtr windowHandle)
		{
			Bitmap bitmapFile = (Bitmap)Bitmap.FromFile(Name + ".png",true);
			bitmapFile.Save("cmp1.png");

			bool match = false;

			for (int i = 0; i < 10; i++)
			{
				User32.PrintScreen();

				await Task.Delay(500);

				System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();
				System.Drawing.Bitmap screenShot = (System.Drawing.Bitmap)clipboardData.GetData(System.Windows.Forms.DataFormats.Bitmap);
				screenShot.MakeTransparent();
				screenShot.Save("cmp2.png");
				if (CompareBitmapsFast(screenShot, bitmapFile))
				{
					match = true;
					break;
				}
				Thread.Sleep(100);
			}
			
			if (!match)
			{
				throw new TestException(TestError.ScreenshotMismatch);
			}
		}

		public static bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
		{
			if (bmp1 == null || bmp2 == null)
				return false;
			if (object.Equals(bmp1, bmp2))
				return true;
			if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
				return false;

			int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

			bool result = true;
			byte[] b1bytes = new byte[bytes];
			byte[] b2bytes = new byte[bytes];

			BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
			BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

			Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
			Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

			for (int n = 0; n <= bytes - 1; n++)
			{
				if ((b1bytes[n] - b2bytes[n]) > 10)
				{
					result = false;
					break;
				}
			}

			bmp1.UnlockBits(bitmapData1);
			bmp2.UnlockBits(bitmapData2);

			return result;
		}
	}
}
