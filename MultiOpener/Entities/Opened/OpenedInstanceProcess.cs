using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                if (!Win32.IsProcessResponding(Pid) || !IsInstanceProcessResponding())
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
        if (!Win32.IsProcessResponding(Pid) || !IsInstanceProcessResponding())
            title = "(Not Reponding)" + title;

        if (!string.IsNullOrEmpty(title))
            WindowTitle = title;

        if (string.IsNullOrEmpty(WindowTitle))
            WindowTitle = "Unknown";
    }

    public override async Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return false;

        Process? process = null;
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
            await SearchForSingleMCInstance(token);
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

            if(!Win32.IsProcessResponding(Pid))
                output = await Win32.CloseProcessByPid(Pid);

            //Experimental
            if (!IsInstanceProcessResponding() && !output)
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

    private bool IsInstanceProcessResponding()
    {
        string logsPath = Path + "\\.minecraft\\logs";
        string latestLogFilePath = System.IO.Path.Combine(logsPath, "latest.log");

        if (File.Exists(latestLogFilePath))
        {
            string lastLogLine = ReadLastLogLine(latestLogFilePath);
            if (lastLogLine.Contains("Saving the game") || lastLogLine.Contains("Saving worlds"))
            {
                StartViewModel.Log($"{Name} process is stuck in world saving", ConsoleLineOption.Warning);
                return false;
            }
            else return true;
        }

        return false;
    }
    private string ReadLastLogLine(string filePath)
    {
        string lastLine = string.Empty;

        using (FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader streamReader = new(fileStream))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
                lastLine = line;
        }

        return lastLine;
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

        if (!isHwndFound)
            StartViewModel.Log("An error occurred while searching for the MultiMC instance. Please use Shift+Refresh to try finding that instance again when the instance process is open in the background.", ConsoleLineOption.Error);

        return isHwndFound;
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

    [GeneratedRegex("^Minecraft\\*\\s*(?:-\\s*Instance)?\\s*((?:\\d+(?:\\.\\d+)*)?)(?:\\s*-\\s*(\\S+(?:\\s*\\S+)*))?$", RegexOptions.NonBacktracking | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    public static partial Regex MCPattern();
}
