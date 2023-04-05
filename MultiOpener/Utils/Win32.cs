using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
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
        /*private delegate bool CallBackPtr(int hwnd, int lParam);
        private static CallBackPtr callBackPtr = Callback;
        private static List<WinStruct> _WinStructList = new();*/

        #region Extern methods
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern int GetProcessId(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        #endregion

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        /*private static bool Callback(int hWnd, int lparam)
        {
            StringBuilder sb = new(256);
            int res = GetWindowText(hWnd, sb, 256);
            _WinStructList.Add(new WinStruct { MainWindowHandle = hWnd, WinTitle = sb.ToString() });
            return true;
        }

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);
        public static List<WinStruct> GetWindows()
        {
            _WinStructList = new List<WinStruct>();
            EnumWindows(callBackPtr, IntPtr.Zero);
            return _WinStructList;
        }*/

        public static IntPtr GetWindowHandle(string windowName)
        {
            return FindWindow(null, windowName);
        }

        public static string GetWindowTitle(IntPtr hwnd)
        {
            StringBuilder sb = new(256);

            int length = GetWindowText(hwnd, sb, sb.Capacity);

            if (length > 0)
                return sb.ToString();

            return "";
        }

        public static IntPtr GetHwndFromHandle(IntPtr handle)
        {
            // Get the process ID associated with the handle
            int processId = GetProcessId(handle);

            // Enumerate all top-level windows and find the one with the matching process ID
            IntPtr hwnd = IntPtr.Zero;
            string windowTitle = null;
            EnumWindows((IntPtr wnd, IntPtr param) =>
            {
                uint thisProcessId;
                GetWindowThreadProcessId(wnd, out thisProcessId);
                if (thisProcessId == processId)
                {
                    // Found a window associated with the same process ID
                    // Get the window title to verify that it is the main window
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(wnd, sb, sb.Capacity);
                    if (sb.ToString().Contains(Process.GetProcessById(processId).ProcessName))
                    {
                        // The window title contains the process name, so this is the main window
                        hwnd = wnd;
                        windowTitle = sb.ToString();
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return hwnd;
        }

        public static Process GetProcessByHandle(IntPtr handle)
        {
            int id = GetProcessId(handle);
            return Process.GetProcessById(id);
        }
    }
}
