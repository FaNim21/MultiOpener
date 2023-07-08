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
    public override void FastUpdate()
    {
        base.FastUpdate();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void UpdateTitle()
    {
        if (Hwnd == nint.Zero)
        {
            WindowTitle = System.IO.Path.GetFileName(Path);
            return;
        }

        string title = Win32.GetWindowTitle(Hwnd);

        if (!string.IsNullOrEmpty(title))
            WindowTitle = title;

        if (string.IsNullOrEmpty(WindowTitle))
            WindowTitle = "Unknown";
    }

    public override async Task UpdateAsync(CancellationToken source = default)
    {
        if (!IsOpenedFromStatus())
            await SearchForSingleMCInstance(source);
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
            await Task.Delay(config.Cooldown, CancellationToken.None);

            instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
            isHwndFound = FindInstance(instances);

        } while (!isHwndFound && errorCount < config.ErrorCount);

        return errorCount < config.ErrorCount;
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
            catch
            {
                StartViewModel.Log("Erorr at FindInstance", ConsoleLineOption.Error);
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

    [GeneratedRegex("Minecraft\\*\\s+(\\s+-\\s+instance)?\\s*(?:\\d+(\\.\\d+)+|\\d+)", RegexOptions.NonBacktracking | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    public static partial Regex MCPattern();
}
