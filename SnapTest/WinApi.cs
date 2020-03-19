/////////////////////////////////////////////////////////////////////////
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is a “supplement” under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
// and is licensed under the terms of that license agreement. 
//
/////////////////////////////////////////////////////////////////////////

// get past MouseData not being initialized warning...it needs to be there for p/invoke
#pragma warning disable 0649

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SnapTest
{





	public static class NativeMethods
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


		

		
	}
}
