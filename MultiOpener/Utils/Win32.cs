using MultiOpener.Components.Controls;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace MultiOpener.Utils;

public partial class Win32
{
    #region Extern methods
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, nint lParam);
    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("kernel32.dll")]
    private static extern uint GetProcessId(nint handle);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern int SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern nint SendMessageTimeout(nint hWnd, int Msg, nint wParam, nint lParam, uint fuFlags, uint uTimeout, out nint lpdwResult);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool QueryFullProcessImageName(nint hProcess, int dwFlags, StringBuilder lpExeName, out int lpdwSize);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(nint hWnd);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(nint hObject);

    [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern uint GetModuleFileNameEx(nint hProcess, nint hModule, StringBuilder lpFileName, int nSize);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(nint hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(nint hWnd);


    private const int SW_SHOWMINIMIZED = 2;
    private const int WM_CLOSE = 0x0010;
    private const int SW_RESTORE = 0x09;
    private const int SMTO_ABORTIFHUNG = 0x0002;
    #endregion


    public static string GetWindowTitle(nint hwnd)
    {
        StringBuilder sb = new(256);
        int length = GetWindowText(hwnd, sb, sb.Capacity);
        if (length > 0)
            return sb.ToString();
        return "";
    }

    public static int GetPidFromHwnd(nint hwnd)
    {
        if (hwnd == nint.Zero) return -1;

        GetWindowThreadProcessId(hwnd, out uint processId);
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
            }, nint.Zero);
        }

        if (IsWindow(hwnd))
            return hwnd;
        else
            return nint.Zero;
    }

    public static List<nint> GetWindowsByTitlePattern(Regex titlePattern)
    {
        List<nint> windows = new();

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
        }, nint.Zero);

        return windows;
    }

    public static async Task<bool> CloseProcessByHwnd(nint hwnd)
    {
        if (IsWindow(hwnd))
        {
            Task<bool> close = Task.Run(() =>
            {
                return SendMessageTimeout(hwnd, WM_CLOSE, nint.Zero, nint.Zero, SMTO_ABORTIFHUNG, 4000, out var result) != nint.Zero;
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
        Regex regex = JavaLibrary();

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
                if (!string.IsNullOrEmpty(javaLibraryPath)) break;
            }
        }

        return javaLibraryPath;
    }

    public static void MinimizeWindowHwnd(nint hwnd)
    {
        if (hwnd == nint.Zero) return;

        ShowWindow(hwnd, SW_SHOWMINIMIZED);
    }
    public static void UnminimizeWindowHwnd(nint hWnd)
    {
        if (hWnd == nint.Zero) return;

        if (IsIconic(hWnd))
            ShowWindow(hWnd, SW_RESTORE);
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
            if (process != null && !process.HasExited )
                return process.Responding;
        }
        catch (Exception) { }

        return false;
    }

    [GeneratedRegex("-Djava\\.library\\.path\\s*=\\s*\"?([^\"]+)/natives\"?")]
    private static partial Regex JavaLibrary();
}
