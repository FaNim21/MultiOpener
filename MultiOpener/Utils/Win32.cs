using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiOpener.Utils;

public partial class Win32
{
    #region Extern methods
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc enumProc, nint lParam);
    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [LibraryImport("user32.dll")]
    private static partial uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutA")]
    private static partial nint SendMessageTimeout(nint hWnd, int Msg, nint wParam, nint lParam, uint fuFlags, uint uTimeout, out nint lpdwResult);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindow(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(nint hWnd);

    [LibraryImport("user32.dll")]
    private static partial void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);


    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int SW_SHOWMINIMIZED = 2;
    private const int SW_SHOWMAXIMIZED = 3;
    private const int WM_CLOSE = 0x0010;
    private const int SW_RESTORE = 0x09;
    private const int SMTO_ABORTIFHUNG = 0x0002;
    #endregion


    public static void PressButtonOnWindow(nint hwnd, char key)
    {
        keybd_event((byte)key, 0, 0, UIntPtr.Zero);
        keybd_event((byte)key, 0, 2, UIntPtr.Zero);
    }

    public static string GetWindowTitle(nint hwnd)
    {
        StringBuilder sb = new(256);
        int length = GetWindowText(hwnd, sb, sb.Capacity);
        return length > 0 ? sb.ToString() : "";
    }

    public static int GetPidFromHwnd(nint hwnd)
    {
        if (hwnd == nint.Zero) return -1;

        _ = GetWindowThreadProcessId(hwnd, out uint processId);
        return (int)processId;
    }

    public static nint GetHwndFromPid(int pid)
    {
        if (pid <= 0) return nint.Zero;

        uint processId = (uint)pid;

        Process process;
        nint hwnd = nint.Zero;

        try
        {
            process = Process.GetProcessById((int)processId);
            hwnd = process.MainWindowHandle;
        }
        catch (ArgumentException)
        {
            return nint.Zero;
        }

        // If the process does not have a visible window, enumerate all top-level windows and find the one with the matching process ID
        if (hwnd == nint.Zero)
        {
            EnumWindows((wnd, param) =>
            {
                _ = GetWindowThreadProcessId(wnd, out uint thisProcessId);
                if (thisProcessId != processId) return true;

                StringBuilder sb = new(256);
                _ = GetWindowText(wnd, sb, sb.Capacity);

                if (!sb.ToString().Contains(process.ProcessName)) return true;

                hwnd = wnd;
                return false;
            }, nint.Zero);
        }

        return IsWindow(hwnd) ? hwnd : nint.Zero;
    }

    public static byte GetWindowsCountByTitlePattern(Regex titlePattern)
    {
        byte count = 0;
        EnumWindows((hWnd, lParam) =>
        {
            StringBuilder sb = new(256);
            _ = GetWindowText(hWnd, sb, sb.Capacity);

            try
            {
                if (titlePattern.IsMatch(sb.ToString()))
                    count++;
            }
            catch (RegexMatchTimeoutException)
            {
                StartViewModel.Log("Error timeout while looking for window title match", Entities.ConsoleLineOption.Error);
            }
            return true;
        }, nint.Zero);

        return count;
    }
    public static List<nint> GetWindowsByTitlePattern(Regex titlePattern)
    {
        List<nint> windows = new();

        EnumWindows((hWnd, lParam) =>
        {
            StringBuilder sb = new(256);
            _ = GetWindowText(hWnd, sb, sb.Capacity);

            try
            {
                if (titlePattern.IsMatch(sb.ToString()))
                    windows.Add(hWnd);
            }
            catch (RegexMatchTimeoutException)
            {
                StartViewModel.Log("Error timeout while looking for window title match", Entities.ConsoleLineOption.Error);
            }
            return true;
        }, nint.Zero);

        return windows;
    }
    public static bool GetWindowByTitlePattern(Regex titlePattern, OpenedInstanceProcess openedInstance)
    {
        bool output = false;
        EnumWindows((hWnd, lParam) =>
        {
            StringBuilder sb = new(256);
            _ = GetWindowText(hWnd, sb, sb.Capacity);
            try
            {
                if (titlePattern.IsMatch(sb.ToString()))
                {
                    int currentPid = GetPidFromHwnd(hWnd);
                    string? currentPath = GetJavaFilePath(currentPid);
                    if (currentPath != null && currentPath.Equals(openedInstance.Path))
                    {
                        openedInstance.SetHwnd(hWnd);
                        openedInstance.SetPid(currentPid);
                        output = true;
                        return false;
                    }
                }
            }
            catch (RegexMatchTimeoutException)
            {
                StartViewModel.Log("Error timeout while looking for window title match", Entities.ConsoleLineOption.Error);
            }
            catch (Exception ex)
            {
                StartViewModel.Log(ex.ToString(), Entities.ConsoleLineOption.Error);
            }
            return true;
        }, nint.Zero);

        return output;
    }

    public static List<nint> GetChildWindows(nint parent)
    {
        List<IntPtr> result = new();
        EnumChildWindows(parent, (hwnd, lParam) =>
        {
            StringBuilder className = new(256);
            _ = GetClassName(hwnd, className, className.Capacity);

            StartViewModel.Log(className);

            //if (className.Equals("Qt5QWindowIcon"))
            //{
            result.Add(hwnd);
            //}
            return true;
        }, IntPtr.Zero);

        return result;
    }

    public static nint GetProjectorHwnd(Regex regex)
    {
        IntPtr mainWindowHandle = FindWindow("Qt5QWindowIcon", "Windowed Projector (Source) - magnifier");

        return mainWindowHandle;
    }

    public static async Task<bool> CloseProcessByHwnd(nint hwnd)
    {
        if (!IsWindow(hwnd)) return false;

        Task<bool> close = Task.Run(() =>
        {
            return SendMessageTimeout(hwnd, WM_CLOSE, nint.Zero, nint.Zero, SMTO_ABORTIFHUNG, 4000, out var result) != nint.Zero;
        });

        return await close;

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

        if (process == null) return false;
        if (process.HasExited)
            return true;

        await Task.Run(() =>
        {
            process.Kill();
            process.WaitForExit();
        });
        return true;
    }

    public static bool ProcessExist(int pid)
    {
        if (pid <= 0) return false;

        try
        {
            Process.GetProcessById(pid);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static string? GetJavaFilePath(int pid)
    {
        try
        {
            Process process = Process.GetProcessById(pid);
            string? jarPath = GetJavaExecutablePathFromProcess(process);
            return jarPath;
        }
        catch (ArgumentException ex)
        {
            StartViewModel.Log("Failed to get process by ID: " + ex.Message, Entities.ConsoleLineOption.Error);
        }
        catch (Exception ex)
        {
            StartViewModel.Log("Error: " + ex.Message, Entities.ConsoleLineOption.Error);
        }

        return "";
    }
    private static string? GetJavaExecutablePathFromProcess(Process process)
    {
        string? javaLibraryPath = null;
        Regex regex = JavaLibrary();

        using ManagementObjectSearcher searcher = new("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
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
                if (!string.IsNullOrEmpty(javaLibraryPath)) break;
            }
        }

        return javaLibraryPath;
    }

    public static string? GetCommandLine(int pid)
    {
        using Process process = Process.GetProcessById(pid);
        string? commandLine = string.Empty;

        try
        {
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");

            foreach (ManagementObject obj in searcher.Get())
            {
                commandLine = obj["CommandLine"]?.ToString();
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting command line: " + ex.Message);
        }

        return commandLine;
    }

    public static void MinimizeWindowHwnd(nint hwnd)
    {
        if (hwnd == nint.Zero) return;

        ShowWindow(hwnd, SW_SHOWMINIMIZED);
    }
    public static void UnminimizeWindowHwnd(nint hWnd)
    {
        if (hWnd == nint.Zero) return;

        if (IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);
    }

    public static bool WindowExist(nint hwnd)
    {
        if (hwnd == nint.Zero) return false;

        return IsWindow(hwnd);
    }

    public static void SetFocus(nint hwnd)
    {
        SetForegroundWindow(hwnd);
    }

    public static bool IsProcessResponding(int pid)
    {
        if (pid <= 0) return true;

        try
        {
            Process? process = Process.GetProcessById(pid);
            if (process != null && !process.HasExited)
                return process.Responding;
        }
        catch (ArgumentException) { }
        catch (Exception ex) { StartViewModel.Log(ex.Message); }

        return false;
    }

    [GeneratedRegex("-Djava\\.library\\.path\\s*=\\s*\"?([^\"]+)/natives\"?")]
    private static partial Regex JavaLibrary();
}

public enum KeyCode
{
    // Klawisze sterujące
    VK_BACK = 0x08,
    VK_TAB = 0x09,
    VK_RETURN = 0x0D,
    VK_SHIFT = 0x10,
    VK_CONTROL = 0x11,
    VK_MENU = 0x12,   // Alt
    VK_PAUSE = 0x13,
    VK_CAPITAL = 0x14,   // Caps Lock
    VK_ESCAPE = 0x1B,
    VK_SPACE = 0x20,
    VK_PRIOR = 0x21,   // Page Up
    VK_NEXT = 0x22,   // Page Down
    VK_END = 0x23,
    VK_HOME = 0x24,
    VK_LEFT = 0x25,
    VK_UP = 0x26,
    VK_RIGHT = 0x27,
    VK_DOWN = 0x28,
    VK_SELECT = 0x29,
    VK_PRINT = 0x2A,
    VK_EXECUTE = 0x2B,
    VK_SNAPSHOT = 0x2C,   // Print Screen
    VK_INSERT = 0x2D,
    VK_DELETE = 0x2E,
    VK_HELP = 0x2F,

    // Klawisze 0-9
    VK_0 = 0x30,
    VK_1 = 0x31,
    VK_2 = 0x32,
    VK_3 = 0x33,
    VK_4 = 0x34,
    VK_5 = 0x35,
    VK_6 = 0x36,
    VK_7 = 0x37,
    VK_8 = 0x38,
    VK_9 = 0x39,

    // Klawisze A-Z
    VK_A = 0x41,
    VK_B = 0x42,
    VK_C = 0x43,
    VK_D = 0x44,
    VK_E = 0x45,
    VK_F = 0x46,
    VK_G = 0x47,
    VK_H = 0x48,
    VK_I = 0x49,
    VK_J = 0x4A,
    VK_K = 0x4B,
    VK_L = 0x4C,
    VK_M = 0x4D,
    VK_N = 0x4E,
    VK_O = 0x4F,
    VK_P = 0x50,
    VK_Q = 0x51,
    VK_R = 0x52,
    VK_S = 0x53,
    VK_T = 0x54,
    VK_U = 0x55,
    VK_V = 0x56,
    VK_W = 0x57,
    VK_X = 0x58,
    VK_Y = 0x59,
    VK_Z = 0x5A,

    // Klawisze numerycznej klawiatury
    VK_NUMPAD0 = 0x60,
    VK_NUMPAD1 = 0x61,
    VK_NUMPAD2 = 0x62,
    VK_NUMPAD3 = 0x63,
    VK_NUMPAD4 = 0x64,
    VK_NUMPAD5 = 0x65,
    VK_NUMPAD6 = 0x66,
    VK_NUMPAD7 = 0x67,
    VK_NUMPAD8 = 0x68,
    VK_NUMPAD9 = 0x69,
    VK_MULTIPLY = 0x6A,
    VK_ADD = 0x6B,
    VK_SEPARATOR = 0x6C,
    VK_SUBTRACT = 0x6D,
    VK_DECIMAL = 0x6E,
    VK_DIVIDE = 0x6F,

    // Klawisze funkcyjne
    VK_F1 = 0x70,
    VK_F2 = 0x71,
    VK_F3 = 0x72,
    VK_F4 = 0x73,
    VK_F5 = 0x74,
    VK_F6 = 0x75,
    VK_F7 = 0x76,
    VK_F8 = 0x77,
    VK_F9 = 0x78,
    VK_F10 = 0x79,
    VK_F11 = 0x7A,
    VK_F12 = 0x7B,

    // Klawisze specjalne
    VK_OEM_1 = 0xBA,    // ';:' for US
    VK_OEM_PLUS = 0xBB,    // '+' any country
    VK_OEM_COMMA = 0xBC,    // ',' any country
    VK_OEM_MINUS = 0xBD,    // '-' any country
    VK_OEM_PERIOD = 0xBE,    // '.' any country
    VK_OEM_2 = 0xBF,    // '/?' for US
    VK_OEM_3 = 0xC0,    // '`~' for US
    VK_OEM_4 = 0xDB,    //  '[{' for US
    VK_OEM_5 = 0xDC,    //  '\|' for US
    VK_OEM_6 = 0xDD,    //  ']}' for US
    VK_OEM_7 = 0xDE,    //  ''"' for US
    VK_OEM_8 = 0xDF,    //  'AX' button on Japanese AX kbd
}
