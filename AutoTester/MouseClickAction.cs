using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTester
{
	public class MouseClickAction : IAction
	{
		public int X { get; set; }
		public int Y { get; set; }

		public Task Execute(IntPtr windowHandle)
		{
			if (!User32.SetForegroundWindow(windowHandle))
			{
				throw new TestException(TestError.ForegroundFailed);
			}

			POINT point;
			point.X = (int)(X);
			point.Y = (int)(Y);

			int screenX = User32.GetSystemMetrics(User32.SM_CXSCREEN);
			int screenY = User32.GetSystemMetrics(User32.SM_CYSCREEN);

			bool rc = User32.ClientToScreen(windowHandle, ref point);

			if (!rc)
				throw new ArgumentException();

			User32.SendMouseInput(point.X, point.Y, screenX, screenY, leftDown: true);
			User32.SendMouseInput(point.X, point.Y, screenX, screenY, leftDown: false);

			return Task.CompletedTask;
		}
	}
}
