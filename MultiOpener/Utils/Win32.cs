using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MultiOpener.Utils
{
    public class Win32
    {
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
            int processId = GetProcessId(handle);

            Process process = Process.GetProcessById(processId);
            IntPtr hwnd = process.MainWindowHandle;

            // If the process does not have a visible window, enumerate all top-level windows and find the one with the matching process ID
            if (hwnd == IntPtr.Zero)
            {
                EnumWindows((IntPtr wnd, IntPtr param) => {
                    uint thisProcessId;
                    GetWindowThreadProcessId(wnd, out thisProcessId);
                    if (thisProcessId == processId)
                    {
                        StringBuilder sb = new StringBuilder(256);
                        GetWindowText(wnd, sb, sb.Capacity);
                        if (sb.ToString().Contains(process.ProcessName))
                        {
                            hwnd = wnd;
                            return false;
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }

            return hwnd;
        }

        public static Process GetProcessByHandle(IntPtr handle)
        {
            int id = GetProcessId(handle);
            return Process.GetProcessById(id);
        }
    }
}
