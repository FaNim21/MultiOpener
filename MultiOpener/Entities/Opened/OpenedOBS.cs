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
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using OBSStudioClient.Enums;

namespace MultiOpener.Entities.Opened;

public class OpenedProjector : BaseViewModel
{
    private nint _hwnd;
    public nint Hwnd
    {
        get => _hwnd;
        set
        {
            _hwnd = value;
            OnPropertyChanged(nameof(IsOpened));
        }
    }

    private string? _name;
    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public bool IsOpened { get => Hwnd != nint.Zero; }


    public OpenedProjector(string Name, nint Hwnd)
    {
        this.Name = Name;
        this.Hwnd = Hwnd;
    }
}

public partial class OpenedOBS : OpenedProcess
{
    [GeneratedRegex("OBS (\\d+\\.\\d+\\.\\d+)", RegexOptions.IgnoreCase, 200)]
    public static partial Regex OBSPattern();


    public ObsClient? client;
    public OpenOBS OpenOBS { get; set; }

    public ObservableCollection<OpenedProjector> OpenedProjectors { get; set; } = new();

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

    private int timeoutCount = 0;
    private int timeoutChecks = 35;


    public OpenedOBS(OpenOBS openObs)
    {
        OpenOBS = openObs;
    }

    public override void Update(bool lookForWindow = false)
    {
        foreach (var projector in OpenedProjectors)
        {
            if (!Win32.WindowExist(projector.Hwnd))
            {
                nint foundHwnd = Win32.GetWindowByTitlePattern(projector.Name!);
                projector.Hwnd = foundHwnd;
            }
        }

        base.Update(lookForWindow);
    }

    public override void UpdateStatus()
    {
        if (Pid != -1 && IsConnectedToWebSocket)
        {
            Application.Current?.Dispatcher.Invoke(delegate { StatusLabelColor = new SolidColorBrush(Color.FromRgb(51, 204, 51)); });
            Status = "OPENED";
        }
        else
        {
            Application.Current?.Dispatcher.Invoke(delegate { StatusLabelColor = new SolidColorBrush(Color.FromRgb(125, 38, 37)); });
            Status = "CLOSED";
        }
    }

    public override async Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (ProcessStartInfo == null) return false;

        if (Pid == -1)
            FindProcess();

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
    public override async Task<bool> Close()
    {
        Update();

        bool currentlyStreaming = true;

        if (client != null && client.ConnectionState == OBSStudioClient.Enums.ConnectionState.Connected)
        {
            if (OpenOBS.StopRecordingOnClose)
            {
                string recordingName = string.Empty;
                try
                {
                    recordingName = await client.StopRecord();
                }
                catch { }
                if (!string.IsNullOrEmpty(recordingName)) StartViewModel.Log($"Save recording: {recordingName}");
            }

            var streamStatus = await client.GetStreamStatus()!;
            currentlyStreaming = streamStatus.OutputActive;

            await Disconnect();
        }

        foreach (var projector in OpenedProjectors)
            await Win32.CloseProcessByHwnd(projector.Hwnd);
        Application.Current?.Dispatcher.Invoke(delegate
        {
            OpenedProjectors.Clear();
        });

        if (OpenOBS.CloseOBSOnCloseMOProcess && !currentlyStreaming)
            return await base.Close();
        return true;
    }

    public async Task ConnectToWebSocket()
    {
        if (!OpenOBS.ConnectWebSocket) return;

        client = new();
        client.RequestTimeout = 2000;
        bool isConnected = await client.ConnectAsync(true, OpenOBS.Password, "localhost", OpenOBS.Port);
        client.ConnectionClosed += (x, args) => { StartViewModel.Log("Lost connection"); IsConnectedToWebSocket = false; UpdateStatus(); };
        if (isConnected)
        {
            try
            {
                while (client.ConnectionState != ConnectionState.Connected && timeoutCount <= timeoutChecks)
                {
                    await Task.Delay(250);
                    timeoutCount++;
                }
                if (timeoutCount > timeoutChecks && client.ConnectionState != ConnectionState.Connected) return;

                IsConnectedToWebSocket = true;
                UpdateStatus();
                await client.SetCurrentSceneCollection(OpenOBS.SceneCollection);
                if (OpenOBS.StartRecordingOnOpen)
                {
                    try
                    {
                        await client.StartRecord();
                    }
                    catch { }
                }

                for (int i = 0; i < OpenOBS.Projectors.Count; i++)
                {
                    var current = OpenOBS.Projectors[i];

                    if (current.AsFullscreen)
                        await client.OpenSourceProjectorOnMonitor(current.ProjectorName!, current.MonitorIndex);
                    else
                        await client.OpenSourceProjectorWindow(current.ProjectorName!, "-");
                }

                List<string> projectorNames = new();
                foreach (var projector in OpenOBS.Projectors)
                    projectorNames.Add(projector.ProjectorName!);

                string variableNamesPattern = string.Join("|", projectorNames.Select(Regex.Escape));
                string pattern = $@"^(Fullscreen|Windowed) Projector \(Source\) - ({variableNamesPattern})$";
                Regex regex = new(pattern);

                var projectorsHwnd = Win32.GetWindowsByTitlePattern(regex);
                foreach (var hwnd in projectorsHwnd)
                {
                    string currentTitle = Win32.GetWindowTitle(hwnd);
                    currentTitle = currentTitle.Replace("(Source)", "(Scene)");
                    Application.Current?.Dispatcher.Invoke(delegate
                    {
                        OpenedProjectors.Add(new(currentTitle, hwnd));
                    });
                    Win32.SetWindowTitle(hwnd, currentTitle);
                }
            }
            catch (Exception ex)
            {
                StartViewModel.Log($"Error: {ex.Message} - {ex.StackTrace}", ConsoleLineOption.Error);
                await Disconnect();
            }
        }
    }

    public async Task Disconnect()
    {
        if (client == null) return;

        client.Disconnect();
        client.Dispose();

        while (client.ConnectionState != ConnectionState.Disconnected)
            await Task.Delay(100);

        client.ConnectionClosed -= OnConnectionClosed;
    }

    public void OnConnectionClosed(object? parametr, EventArgs args)
    {
        MessageBox.Show("Lost connection");
        IsConnectedToWebSocket = false;
    }
}
