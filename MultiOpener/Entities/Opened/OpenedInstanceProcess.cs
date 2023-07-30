﻿using MultiOpener.Utils;
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
    public short number = -1;


    public override void FastUpdate()
    {
        base.FastUpdate();
    }
    public override void Update()
    {
        base.FastUpdate();
    }
    public override async Task UpdateAsync(CancellationToken token = default)
    {
        if (!IsOpenedFromStatus())
            await SearchForSingleMCInstance(token);
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
            WindowTitle = $"[{number}] " + System.IO.Path.GetFileName(Path);
            return;
        }

        string title = $"[{number}] " + Win32.GetWindowTitle(Hwnd);
        if (!Win32.IsProcessResponding(Pid))
            title = "(Not Reponding)" + title;

        if (!string.IsNullOrEmpty(title))
            WindowTitle = title;

        if (string.IsNullOrEmpty(WindowTitle))
            WindowTitle = "Unknown";
    }

    public override async Task OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return;

        Process? process = null;
        try
        {
            process = Process.Start(ProcessStartInfo);
        }
        catch (Exception)
        {
            StartViewModel.Log($"Cannot open MultiMC instance process {Name} from {ProcessStartInfo.WorkingDirectory}", ConsoleLineOption.Error);
        }

        if (process != null)
        {
            process.WaitForInputIdle();
            await SearchForSingleMCInstance(token);
        }

        UpdateStatus();
    }

    public async Task<bool> SearchForSingleMCInstance(CancellationToken token = default)
    {
        Regex mcPatternRegex = MCPattern();
        List<nint> instances;
        int errorCount = -1;
        var config = new TimeoutConfigurator(App.Config.TimeoutLookingForSingleInstanceData, 15);
        bool isHwndFound;

        do
        {
            if (token.IsCancellationRequested) return false;

            errorCount++;

            try
            {
                await Task.Delay(config.Cooldown, token);
            }
            catch (Exception) { }

            instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
            isHwndFound = FindInstance(instances, false);

        } while (!isHwndFound && errorCount < config.ErrorCount);

        return errorCount < config.ErrorCount;
    }

    public bool FindInstance(List<nint> instances, bool canRemove = true)
    {
        for (int i = instances.Count - 1; i >= 0; i--)
        {
            try
            {
                if (IsInstancePathEqual(instances[i]))
                {
                    if (canRemove) instances.RemoveAt(i);
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

    [GeneratedRegex("^Minecraft\\*\\s*(?:-\\s*Instance)?\\s*(\\d+(\\.\\d+)*)$", RegexOptions.NonBacktracking | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    public static partial Regex MCPattern();
}
