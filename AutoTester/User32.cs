using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoTester
{
	public struct POINT
	{
		public int X;
		public int Y;
	}

	public struct RECT
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}

	public static class User32
	{
		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int nIndex);

		public const int SM_CXSCREEN = 0;
		public const int SM_CYSCREEN = 1;

		public const int InputMouse = 0;

		public const int MouseEventMove = 0x01;
		public const int MouseEventLeftDown = 0x02;
		public const int MouseEventLeftUp = 0x04;
		public const int MouseEventRightDown = 0x08;
		public const int MouseEventRightUp = 0x10;
		public const int MouseEventAbsolute = 0x8000;

		private static bool lastLeftDown;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint SendInput(uint numInputs, Input[] inputs, int size);

		public static void SendMouseInput(int positionX, int positionY, int maxX, int maxY, bool leftDown)
		{
			if (positionX > int.MaxValue)
				throw new ArgumentOutOfRangeException("positionX");
			if (positionY > int.MaxValue)
				throw new ArgumentOutOfRangeException("positionY");

			Input[] i = new Input[2];

			// move the mouse to the position specified
			i[0] = new Input();
			i[0].Type = InputMouse;
			i[0].MouseInput.X = (positionX * 65535) / maxX;
			i[0].MouseInput.Y = (positionY * 65535) / maxY;
			i[0].MouseInput.Flags = MouseEventAbsolute | MouseEventMove;

			// determine if we need to send a mouse down or mouse up event
			if (!lastLeftDown && leftDown)
			{
				i[1] = new Input();
				i[1].Type = InputMouse;
				i[1].MouseInput.Flags = MouseEventLeftDown;
			}
			else if (lastLeftDown && !leftDown)
			{
				i[1] = new Input();
				i[1].Type = InputMouse;
				i[1].MouseInput.Flags = MouseEventLeftUp;
			}

			lastLeftDown = leftDown;

			// send it off
			uint result = SendInput(2, i, Marshal.SizeOf(i[0]));
			if (result == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		internal struct Input
		{
			public int Type;
			public MouseInput MouseInput;
		}

		internal struct MouseInput
		{
			public int X;
			public int Y;
			public uint MouseData;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

		[DllImport("user32.Dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);
	
		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumWindows(CallBackPtr callback, IntPtr extraData);

		public delegate bool CallBackPtr(IntPtr hwnd, int lParam);

		[DllImport("user32.dll")]
		static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
			IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern long GetWindowLongA(IntPtr hWnd, int nIndex);

		public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
		{
			var handles = new List<IntPtr>();

			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
				EnumThreadWindows(thread.Id,
					(hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

			return handles;
		}

		[DllImport("user32")]
		public static extern bool IsWindowEnabled(IntPtr hwnd);

		[DllImport("user32")]
		public static extern bool IsWindowVisible(IntPtr hwnd);

		[DllImport("user32")]
		public static extern IntPtr GetTopWindow(IntPtr hwnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("dwmapi.dll")]
		public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);
		
		[Flags]
		public enum DwmWindowAttribute : int
		{
			DWMWA_NCRENDERING_ENABLED = 1,
			DWMWA_NCRENDERING_POLICY,
			DWMWA_TRANSITIONS_FORCEDISABLED,
			DWMWA_ALLOW_NCPAINT,
			DWMWA_CAPTION_BUTTON_BOUNDS,
			DWMWA_NONCLIENT_RTL_LAYOUT,
			DWMWA_FORCE_ICONIC_REPRESENTATION,
			DWMWA_FLIP3D_POLICY,
			DWMWA_EXTENDED_FRAME_BOUNDS,
			DWMWA_HAS_ICONIC_BITMAP,
			DWMWA_DISALLOW_PEEK,
			DWMWA_EXCLUDED_FROM_PEEK,
			DWMWA_CLOAK,
			DWMWA_CLOAKED,
			DWMWA_FREEZE_REPRESENTATION,
			DWMWA_LAST
		}

		public static Bitmap CaptureWindow(IntPtr handle)
		{
			var rect = new RECT();
			GetWindowRect(handle, out rect);
			var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
			var result = new Bitmap(bounds.Width, bounds.Height);

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
			}

			return result;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern void keybd_event(byte vVK, byte bScan, int dwFlags, int dwExtraInfo);

		public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //key down
		public const int KEYEVENTF_KEYUP = 0x0002; //key up

		public const int VK_SNAPSHOT = 0x2C; //VirtualKey code for print key
		public const int VK_MENU = 0x12;

		public static void PrintScreen()
		{
			keybd_event(VK_SNAPSHOT, 1, 0, 0);
			//keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY, 0);
			//keybd_event(VK_SNAPSHOT, 0, KEYEVENTF_EXTENDEDKEY, 0);
			//keybd_event(VK_SNAPSHOT, 0, KEYEVENTF_KEYUP, 0);
			//keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);
		}
	}
}
