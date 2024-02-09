using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System.Diagnostics;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Collections.Generic;

namespace MultiOpener.Entities.Open;

public class ProjectorOpen
{
    public string? ProjectorName { get; set; }
    public bool AsFullscreen { get; set; }
    public int MonitorIndex { get; set; }
}

public class OpenOBS : OpenItem
{
    public bool ConnectWebSocket { get; set; }
    public bool CloseOBSOnCloseMOProcess { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public string SceneCollection { get; set; }
    public List<ProjectorOpen> Projectors { get; set; } = new();
    public bool StartRecordingOnOpen { get; set; }
    public bool StopRecordingOnClose { get; set; }


    [JsonConstructor]
    public OpenOBS(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = OpenType.ResetTrackerMC, bool MinimizeOnOpen = false,
                   bool ConnectWebSocket = true, int Port = 4455, string Password = "", string SceneCollection = "", List<ProjectorOpen> Projectors = default!)
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
        //...
        this.ConnectWebSocket = ConnectWebSocket;
        this.Port = Port;
        this.Password = Password;
        this.SceneCollection = SceneCollection;
        this.Projectors = Projectors;
    }
    public OpenOBS(string Name) : this(Name, "") { }
    public OpenOBS(OpenOBS item) : this(item.Name, item.PathExe, item.DelayBefore, item.DelayAfter, item.Type, item.MinimizeOnOpen, item.ConnectWebSocket, item.Port, item.Password, item.SceneCollection, item.Projectors) { }

    public override string Validate()
    {
        if (Port <= 0)
            return $"Wrong port in OBS open";

        for (int i = 0; i < Projectors.Count; i++)
            if (string.IsNullOrEmpty(Projectors[i].ProjectorName))
                return $"You didn't set a projector name in obs configurator";

        return base.Validate();
    }

    public override async Task Open(StartViewModel startModel, CancellationToken token)
    {
        try
        {
            bool isCancelled = token.IsCancellationRequested;
            if (!isCancelled) await Task.Delay(DelayBefore);

            string executable = Path.GetFileName(PathExe);
            string pathDir = Path.GetDirectoryName(PathExe) ?? "";

            OpenedOBS opened = new(this);
            ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, Arguments = "--disable-shutdown-check", UseShellExecute = true };
            string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);
            opened.Initialize(startInfo, name!, PathExe, MinimizeOnOpen);

            if (isCancelled) opened.Clear();
            else
                await opened.OpenProcess(token);

            Application.Current?.Dispatcher.Invoke(delegate { StartViewModel.Instance?.AddOpened(opened); });

            if (!isCancelled) await Task.Delay(DelayAfter);
        }
        catch (Exception e)
        {
            StartViewModel.Log(e.ToString(), ConsoleLineOption.Error);
        }
    }
}
