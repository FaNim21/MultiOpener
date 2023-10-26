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

public class OpenResetTracker : OpenItem
{
    //public string 
    public bool UsingBuiltInTracker { get; set; }


    [JsonConstructor]
    public OpenResetTracker(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, bool MinimizeOnOpen = false, bool UsingBuiltInTracker = true)
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
        //...
        this.UsingBuiltInTracker = UsingBuiltInTracker;
    }
    public OpenResetTracker(OpenResetTracker item)
    {
        Name = item.Name;
        PathExe = item.PathExe;
        DelayBefore = item.DelayBefore;
        DelayAfter = item.DelayAfter;
        Type = item.Type;
        MinimizeOnOpen = item.MinimizeOnOpen;
        //...
        UsingBuiltInTracker = item.UsingBuiltInTracker;
    }

    public override string Validate()
    {
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

            OpenedProcess opened = new OpenedResetTrackerProcess();
            ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
            string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);
            //initializowanie innych zmiennych z tej klasy do OpenedResetTrackerProcess
            opened.isMinimizeOnOpen = MinimizeOnOpen;
            opened.Initialize(startInfo, name!, PathExe);

            if (isCancelled) opened.Clear();
            else
            {
                opened.FindProcess();

                if (!opened.IsOpenedFromStatus())
                    await opened.OpenProcess(token);
            }

            Application.Current?.Dispatcher.Invoke(delegate { ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(opened); });

            if (!isCancelled) await Task.Delay(DelayAfter);
        }
        catch (Exception e)
        {
            StartViewModel.Log(e.ToString(), ConsoleLineOption.Error);
        }
    }
}
