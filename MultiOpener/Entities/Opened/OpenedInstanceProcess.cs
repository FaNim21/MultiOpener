using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MultiOpener.Entities.Opened;

public partial class OpenedInstanceProcess : OpenedProcess
{
    public bool showNamesInsteadOfTitle = false;

    private short _number;
    public short Number
    {
        get { return _number; }
        set
        {
            _number = value;
            OnPropertyChanged(nameof(Number));
        }
    }


    public override void Update(bool lookForWindow = false)
    {
        base.Update(lookForWindow);
    }
    public override void FindProcess()
    {
        Win32.GetWindowByTitlePattern(MCPattern(), this);
    }

    public override void UpdateTitle()
    {
        if (showNamesInsteadOfTitle)
        {
            if (string.IsNullOrEmpty(WindowTitle))
            {
                string? titleName = System.IO.Path.GetFileName(Path);
                if (!Win32.IsProcessResponding(Pid))
                    titleName = "(Not Responding) " + titleName;
                WindowTitle = titleName;
            }
            return;
        }

        if (Hwnd == nint.Zero)
        {
            WindowTitle = $"[{Number}] " + System.IO.Path.GetFileName(Path);
            return;
        }

        string title = $"[{Number}] " + Win32.GetWindowTitle(Hwnd);
        if (!Win32.IsProcessResponding(Pid))
            title = "(Not Reponding)" + title;

        if (!string.IsNullOrEmpty(title))
            WindowTitle = title;

        if (string.IsNullOrEmpty(WindowTitle))
            WindowTitle = "Unknown";
    }

    public override async Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return false;

        Process? process;
        try
        {
            process = Process.Start(ProcessStartInfo);
        }
        catch (Exception)
        {
            StartViewModel.Log($"Cannot open MultiMC instance process '{Name}' from {ProcessStartInfo.WorkingDirectory}", ConsoleLineOption.Error);
            return false;
        }

        if (process != null)
        {
            process.WaitForInputIdle();
            await SearchForSingleMCInstance(App.Config.TimeoutWaitingForSingleInstanceToOpen, token);
        }

        UpdateStatus();

        StartViewModel.Log($"Succesfully opened Instance '{Name}' at process ID: {Pid}");
        return true;
    }

    public override async Task<bool> Close()
    {
        if (Pid == -1)
        {
            Clear();
            return true;
        }

        try
        {
            bool output = false;

            if (!Win32.IsProcessResponding(Pid))
                output = await Win32.CloseProcessByPid(Pid);

            if (!output)
            {
                output = await Win32.CloseProcessByHwnd(Hwnd);
                if (!output)
                    output = await Win32.CloseProcessByPid(Pid);
            }

            Clear();
            return output;
        }
        catch (Exception e)
        {
            StartViewModel.Log($"Cannot close MC instance named {Name}(Title: {WindowTitle}) \n{e}", ConsoleLineOption.Error);
            return false;
        }
    }

    public async Task<bool> SearchForSingleMCInstance(int timeout, CancellationToken token = default)
    {
        bool isHwndFound;
        Regex mcPatternRegex = MCPattern();
        int errorCount = -1;
        var config = new TimeoutConfigurator(timeout, 15);

        do
        {
            if (token.IsCancellationRequested) return false;
            errorCount++;

            try
            {
                await Task.Delay(config.Cooldown, token);
            }
            catch (Exception) { }

            isHwndFound = Win32.GetWindowByTitlePattern(mcPatternRegex, this);
        } while (!isHwndFound && errorCount < config.ErrorCount);

        if (!isHwndFound)
            StartViewModel.Log($"Could not found window for {Name} process", ConsoleLineOption.Warning);

        return isHwndFound;
    }

    public bool FindInstance(List<nint> instances)
    {
        for (int i = instances.Count - 1; i >= 0; i--)
        {
            try
            {
                if (IsInstancePathEqual(instances[i]))
                {
                    instances.RemoveAt(i);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StartViewModel.Log(ex.ToString(), ConsoleLineOption.Error);
            }
        }

        return false;
    }

    public bool IsInstancePathEqual(nint _hwnd)
    {
        if (_hwnd == nint.Zero) return false;

        int currentPid = Win32.GetPidFromHwnd(_hwnd);
        string? currentPath = Win32.GetJavaFilePath(currentPid);
        if (currentPath != null && currentPath.Equals(Path))
        {
            SetHwnd(_hwnd);
            SetPid(currentPid);
            return true;
        }
        return false;
    }

    public override void OpenOpenedPathFolder()
    {
        if (string.IsNullOrEmpty(Path)) return;

        string? argument = System.IO.Path.Combine(System.IO.Path.GetFullPath(Path), ".minecraft");
        Process.Start("explorer.exe", argument);

    }

    [GeneratedRegex("^Minecraft\\*\\s*(?:-\\s*Instance)?\\s*((?:\\d+(?:\\.\\d+)*)?)(?:\\s*-\\s*(\\S+(?:\\s*\\S+)*))?$", RegexOptions.NonBacktracking | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    public static partial Regex MCPattern();
}
