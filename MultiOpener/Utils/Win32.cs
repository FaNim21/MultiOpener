using MultiOpener.Components.Controls;
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

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);


    private const int SW_SHOWMINIMIZED = 2;
    private const int WM_CLOSE = 0x0010;
    private const int SW_RESTORE = 0x09;
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

    public static int GetPidFromHwnd(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return -1;

        GetWindowThreadProcessId(hwnd, out uint processId);
        return (int)processId;
    }

    public static IntPtr GetHwndFromPid(int pid)
    {
        if (pid <= 0) return IntPtr.Zero;

        uint processId = (uint)pid;

        Process process;
        IntPtr hwnd = IntPtr.Zero;

        try
        {
            process = Process.GetProcessById((int)processId);
            hwnd = process.MainWindowHandle;
        }
        catch (ArgumentException)
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
        if (pid <= 0) return false;

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
            DialogBox.Show("Failed to get process by ID: " + ex.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            DialogBox.Show("Error: " + ex.Message, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        return "";
    }
    private static string? GetJavaExecutablePathFromProcess(Process process)
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
    public static void UnminimizeWindowHwnd(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) return;

        if (IsIconic(hWnd))
            ShowWindow(hWnd, SW_RESTORE);
    }

    public static bool WindowExist(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return false;

        return IsWindow(hwnd);
    }

    public static void SetFocus(IntPtr hWnd)
    {
        SetForegroundWindow(hWnd);
    }

    public static bool IsProcessResponding(int pid)
    {
        if (pid <= 0) return true;

        try
        {
            Process? process = Process.GetProcessById(pid);
            if (process != null && !process.HasExited)
            {
                return process.Responding;
            }
        }catch (Exception) { }

        return false;
    }
}
