using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiOpener.Utils
{
    public class Win32
    {
        #region Extern methods
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        private const int SW_SHOWMINIMIZED = 2;
        private const int WM_CLOSE = 0x0010;
        private const int SMTO_ABORTIFHUNG = 0x0002;
        #endregion


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
        public static uint GetPidFromHandle(IntPtr handle)
        {
            uint id = GetProcessId(handle);
            return id;
        }

        public static IntPtr GetHandleFromPid(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                return process.Handle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        public static IntPtr GetHwndFromHandle(IntPtr handle)
        {
            uint processId = GetProcessId(handle);

            Process process;
            IntPtr hwnd = IntPtr.Zero;

            try
            {
                process = Process.GetProcessById((int)processId);
                hwnd = process.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }

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

            if (IsWindow(hwnd))
                return hwnd;
            else
                return IntPtr.Zero;
        }
        public static IntPtr GetHwndFromPid(int pid)
        {
            uint processId = (uint)pid;

            Process process;
            IntPtr hwnd = IntPtr.Zero;

            try
            {
                process = Process.GetProcessById((int)processId);
                hwnd = process.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }

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

            if (IsWindow(hwnd))
                return hwnd;
            else
                return IntPtr.Zero;
        }

        public static List<IntPtr> GetWindowsByTitlePattern(Regex titlePattern)
        {
            List<IntPtr> windows = new();

            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder sb = new(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (titlePattern.IsMatch(sb.ToString()))
                    windows.Add(hWnd);

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
        public static async Task<bool> CloseProcessByPid(int pid)
        {
            Process process;
            try
            {
                process = Process.GetProcessById(pid);
            }
            catch
            {
                return false;
            }

            if (process != null)
            {
                if (process.HasExited)
                    return true;

                await Task.Run(() =>
                {
                    process.Kill();
                    process.WaitForExit();
                });
                return true;
            }
            else
                return false;

        }

        public static bool ProcessExist(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static string GetJavaFilePath(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                string jarPath = GetJavaExecutablePathFromProcess(process);
                return jarPath;
            }
            catch (ArgumentException ex)
            {
                System.Windows.MessageBox.Show("Failed to get process by ID: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }

            return "";
        }
        private static string GetJavaExecutablePathFromProcess(Process process)
        {
            string? javaLibraryPath = null;
            Regex regex = new(@"-Djava\.library\.path\s*=\s*""?([^""]+)/natives""?");
            using (ManagementObjectSearcher searcher = new("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                foreach (ManagementObject obj in objects)
                {
                    string? commandLine = obj["CommandLine"] as string;
                    if (!string.IsNullOrEmpty(commandLine))
                    {
                        Match match = regex.Match(commandLine);
                        if (match.Success)
                        {
                            javaLibraryPath = match.Groups[1].Value;
                            break;
                        }
                    }
                    obj.Dispose();
                    if (!string.IsNullOrEmpty(javaLibraryPath))
                    {
                        break;
                    }
                }
            }

            return javaLibraryPath;
        }

        public static void MinimizeWindowHwnd(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return;

            ShowWindow(hwnd, SW_SHOWMINIMIZED);
        }
    }
}
