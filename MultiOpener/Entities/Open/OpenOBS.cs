using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System.Diagnostics;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace MultiOpener.Entities.Open;

public class OpenOBS : OpenItem
{
    public bool ConnectWebSocket { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }


    [JsonConstructor]
    public OpenOBS(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = OpenType.ResetTrackerMC, bool MinimizeOnOpen = false,
                   bool ConnectWebSocket = true, int Port = 4455, string Password = "")
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
    }
    public OpenOBS(string Name) : this(Name, "") { }
    public OpenOBS(OpenOBS item) : this(item.Name, item.PathExe, item.DelayBefore, item.DelayAfter, item.Type, item.MinimizeOnOpen, item.ConnectWebSocket, item.Port, item.Password) { }

    public override string Validate()
    {
        if (Port <= 0)
            return $"Wrong port in OBS open";

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
            {
                opened.FindProcess();

                if (!opened.IsOpenedFromStatus())
                    await opened.OpenProcess(token);
                else
                    await opened.ConnectToWebSocket();
            }

            Application.Current?.Dispatcher.Invoke(delegate { StartViewModel.Instance?.AddOpened(opened); });

            if (!isCancelled) await Task.Delay(DelayAfter);
        }
        catch (Exception e)
        {
            StartViewModel.Log(e.ToString(), ConsoleLineOption.Error);
        }
    }
}
