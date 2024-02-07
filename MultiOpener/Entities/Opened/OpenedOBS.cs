using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MultiOpener.Entities.Open;
using OBSStudioClient;
using System.Collections.Generic;
using System.Linq;

namespace MultiOpener.Entities.Opened;

public partial class OpenedOBS : OpenedProcess
{
    [GeneratedRegex("OBS (\\d+\\.\\d+\\.\\d+)", RegexOptions.IgnoreCase, 200)]
    public static partial Regex OBSPattern();


    public ObsClient? client;
    public OpenOBS OpenOBS { get; set; }

    public List<nint> openedProjectors = new();

    private bool _isConnectedToWebSocket;
    public bool IsConnectedToWebSocket
    {
        get => _isConnectedToWebSocket;
        set
        {
            _isConnectedToWebSocket = value;
            OnPropertyChanged(nameof(IsConnectedToWebSocket));
        }
    }


    public OpenedOBS(OpenOBS openObs)
    {
        OpenOBS = openObs;
    }

    public override async Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return false;

        if (Pid != -1)
        {
            await ConnectToWebSocket();
            return true;
        }

        Process? process;
        try
        {
            string root = System.IO.Path.GetPathRoot(ProcessStartInfo.WorkingDirectory)!.Replace("\\", "");
            string workingDirectory = System.IO.Path.GetDirectoryName(ProcessStartInfo.WorkingDirectory) ?? "";

            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = workingDirectory,
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process = new() { StartInfo = startInfo };

            process.Start();

            string[] commands = {
                root,
                $"cd \"{ProcessStartInfo.WorkingDirectory}\"",
                $"{ProcessStartInfo.FileName} {ProcessStartInfo.Arguments}"
            };

            foreach (string command in commands)
                process.StandardInput.WriteLine(command);

            process.StandardInput.Close();
        }
        catch (Exception ex)
        {
            StartViewModel.Log($"{Name}': {ex.Message}", ConsoleLineOption.Error);
            StartViewModel.Log($"StackTrace: {ex.StackTrace}", ConsoleLineOption.Error);
            return false;
        }

        if (process != null)
        {
            int count;
            nint hwnd = nint.Zero;

            if (!token.IsCancellationRequested)
            {
                StartViewModel.Instance!.SetDetailedLoadingText($"Waiting for obs to fully open");

                int errorCount = -1;
                var config = new TimeoutConfigurator(App.Config.TimeoutLookingForInstancesData, 30);
                do
                {
                    errorCount++;
                    count = Win32.GetWindowsCountByTitlePattern(OBSPattern());
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(config.Cooldown), token);
                    }
                    catch { break; }
                } while (count < 1 && errorCount < config.ErrorCount);

                if (count >= 1) hwnd = Win32.GetWindowsByTitlePattern(OBSPattern())[0];
            }

            if (hwnd == nint.Zero)
            {
                Clear();
                StartViewModel.Log($"An error occurred while opening '{Name}'. The Window Handle could not be found due to an incorrect process ID.", ConsoleLineOption.Error);
                return false;
            }

            SetHwnd(hwnd);
            SetPid();
        }

        if (Hwnd != nint.Zero && isMinimizeOnOpen)
            Win32.MinimizeWindowHwnd(Hwnd);

        UpdateUIPanel();

        StartViewModel.Log($"Succesfully opened '{Name}' at process ID: {Pid}");

        StartViewModel.Instance!.SetDetailedLoadingText($"Connecting to WebSocket");
        await ConnectToWebSocket();

        return true;
    }

    public async Task ConnectToWebSocket()
    {
        client = new();
        bool isConnected = await client.ConnectAsync(true, OpenOBS.Password, "localhost", OpenOBS.Port);
        client.ConnectionClosed += (x, args) => { StartViewModel.Log("Lost connection"); };
        if (isConnected)
        {
            try
            {
                await Task.Delay(500);
                await client.SetCurrentSceneCollection("JultiWall speedruns");
                await client.OpenSourceProjectorOnMonitor("Walling", 0);
                await client.OpenSourceProjectorWindow("magnifier", "-");
                await Task.Delay(500);

                string[] projectorName = { "Walling", "magnifier" };
                string variableNamesPattern = string.Join("|", projectorName.Select(Regex.Escape));
                string pattern = $@"^(Fullscreen|Windowed) Projector \(Source\) - ({variableNamesPattern})$";
                Regex regex = new(pattern);

                openedProjectors = Win32.GetWindowsByTitlePattern(regex);
            }
            catch (Exception ex)
            {
                StartViewModel.Log($"Error: {ex.Message} - {ex.StackTrace}", ConsoleLineOption.Error);
                client.Disconnect();
                client.Dispose();
            }
        }

        //TODO: 0 teraz zdecydowac jak zamykac websocket? czy bedzie potrzebny guzik ktory bedzie wywolywal funkcje do websocketu? itp itd, bo tak to juz wszystko pieknie dziala LETS GO
    }

    public override async Task<bool> Close()
    {
        client?.Disconnect();
        client?.Dispose();

        foreach (var projector in openedProjectors)
            await Win32.CloseProcessByHwnd(projector);

        bool output = true;
        return output;

        //TODO: 0 Czy tez prosciej bedzie wylaczac wszystko otwarte preview?
        //TODO: 1 Dac opcje ewentualnego wyboru czy na wylaczaniu uzytkownik chce zeby zatrzymywalo sie nagrywanie czy nie
        //return base.Close();
    }
}
