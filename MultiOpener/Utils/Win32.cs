using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MultiOpener.Utils
{
    public class WinStruct
    {
        public string WinTitle { get; set; }
        public int MainWindowHandle { get; set; }
    }

    public class Win32
    {
        private delegate bool CallBackPtr(int hwnd, int lParam);
        private static CallBackPtr callBackPtr = Callback;
        private static List<WinStruct> _WinStructList = new();

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static bool Callback(int hWnd, int lparam)
        {
            StringBuilder sb = new(256);
            int res = GetWindowText(hWnd, sb, 256);
            _WinStructList.Add(new WinStruct { MainWindowHandle = hWnd, WinTitle = sb.ToString() });
            return true;
        }

        public static List<WinStruct> GetWindows()
        {
            _WinStructList = new List<WinStruct>();
            EnumWindows(callBackPtr, IntPtr.Zero);
            return _WinStructList;
        }

        public static IntPtr GetWindowHandle(string windowName)
        {
            return FindWindow(null, windowName);
        }
    }
}
