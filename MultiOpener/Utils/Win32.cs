using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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

        [DllImport("kernel32.dll")]
        static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);
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
        public static IntPtr GetHwndFromHandleDifferent(IntPtr handle)
        {
            GetWindowThreadProcessId(handle, out uint processId);

            int id = GetMinecraftProcessID(handle);

            IntPtr mainWindowHandle = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint currentProcessId);

                if (currentProcessId == id)
                {
                    mainWindowHandle = hWnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);

            return mainWindowHandle;
        }

        public static Process GetProcessFromHandle(IntPtr handle)
        {
            uint id = GetProcessId(handle);
            return Process.GetProcessById((int)id);
        }

        public static int GetMinecraftProcessID(IntPtr handle)
        {
            int pathLength = 1024;
            StringBuilder sb = new(pathLength);

            if (QueryFullProcessImageName(handle, 0, sb, out pathLength))
            {
                string exePath = sb.ToString();

                if (exePath.ToLower().EndsWith(".exe"))
                {
                    ProcessStartInfo psi = new("tasklist.exe", "/FI \"IMAGENAME eq " + Path.GetFileName(exePath) + "\" /NH /FO CSV");
                    psi.RedirectStandardOutput = true;
                    psi.UseShellExecute = false;

                    Process tasklistProcess = Process.Start(psi);
                    tasklistProcess.WaitForExit();

                    if (tasklistProcess.ExitCode == 0)
                    {
                        string output = tasklistProcess.StandardOutput.ReadToEnd();
                        string[] fields = output.Split(',');

                        if (fields.Length >= 2)
                        {
                            if (uint.TryParse(fields[1], out var processIdFromHandle))
                            {
                                return (int)processIdFromHandle;
                                // Successfully obtained the process ID from the tasklist output
                            }
                        }
                    }
                }
            }

            return 0;
        }

        private const int WM_CLOSE = 0x10;
        public static void CloseProcessByHwnd(IntPtr hWnd)
        {
            SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
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

        public static void CloseProcessByHandle(IntPtr handle)
        {
            uint id = GetProcessId(handle);
            Process process = Process.GetProcessById((int)id);
            if (process != null)
                if (!process.CloseMainWindow())
                    process.Kill();
        }
    }
}
