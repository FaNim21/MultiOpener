﻿using MultiOpener.Components.Controls;
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

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

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

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsIconic(nint hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(nint hWnd);


    private const int SW_SHOWMINIMIZED = 2;
    private const int WM_CLOSE = 0x0010;
    private const int SW_RESTORE = 0x09;
    private const int SMTO_ABORTIFHUNG = 0x0002;
    #endregion


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
