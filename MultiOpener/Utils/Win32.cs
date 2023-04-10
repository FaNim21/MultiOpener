using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiOpener.Utils
{
    public class Win32
    {
        #region Extern methods
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("kernel32.dll")]
        private static extern uint GetProcessId(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        private const int WM_CLOSE = 0x0010;
        private const int SMTO_ABORTIFHUNG = 0x0002;
        #endregion

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        public static string GetWindowTitle(IntPtr hwnd)
        {
            StringBuilder sb = new(256);
            int length = GetWindowText(hwnd, sb, sb.Capacity);
            if (length > 0)
                return sb.ToString();
            return "";
        }
        public static uint GetPidFromHwnd(IntPtr hwnd)
        {
            GetWindowThreadProcessId(hwnd, out uint processId);
            return processId;
        }
        public static IntPtr GetHwndFromHandle(IntPtr handle)
        {
            uint processId = GetProcessId(handle);

            Process process = Process.GetProcessById((int)processId);
            IntPtr hwnd = process.MainWindowHandle;

            // If the process does not have a visible window, enumerate all top-level windows and find the one with the matching process ID
            if (hwnd == IntPtr.Zero)
            {
                EnumWindows((IntPtr wnd, IntPtr param) =>
                {
                    GetWindowThreadProcessId(wnd, out uint thisProcessId);
                    if (thisProcessId == processId)
                    {
                        StringBuilder sb = new(256);
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

        public static List<IntPtr> GetWindowsByTitlePattern(string titlePattern)
        {
            List<IntPtr> windows = new();

            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder sb = new(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().Contains(titlePattern))
                {
                    windows.Add(hWnd);
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public static async Task<bool> CloseProcessByHwnd(IntPtr hwnd)
        {
            if (IsWindow(hwnd))
            {
                Task<bool> close = Task.Run(() =>
                {
                    return SendMessageTimeout(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, 4000, out var result) != IntPtr.Zero;
                });

                return await close;
            }

            return false;
        }
        public static async Task CloseProcessByPid(int pid)
        {
            Process process = Process.GetProcessById(pid);
            if (process != null)
            {
                await Task.Run(() =>
                {
                    process.Kill();
                    process.WaitForExit();
                });
            }
        }

    }
}
