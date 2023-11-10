using MultiOpener.Entities.Interfaces;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Entities.Open;

public enum OpenType
{
    [Description("Normal")]
    Normal,
    [Description("Instances(MultiMC)")]
    InstancesMultiMC,
    [Description("Reset Tracker(MC)")]
    ResetTrackerMC,
}

[JsonDerivedType(typeof(OpenItem), typeDiscriminator: "base")]
[JsonDerivedType(typeof(OpenInstance), typeDiscriminator: "instances")]
[JsonDerivedType(typeof(OpenResetTracker), typeDiscriminator: "resetTracker")]
public class OpenItem : BaseViewModel, IRenameItem
{
    private string _name = "";
    public string Name
    {
        get { return _name; }
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public OpenType Type { get; set; }
    public string PathExe { get; set; }

    public int DelayBefore { get; set; }
    public int DelayAfter { get; set; }

    public bool MinimizeOnOpen { get; set; }


    [JsonConstructor]
    public OpenItem(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = OpenType.Normal, bool MinimizeOnOpen = false)
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
    }
    public OpenItem(string Name) : this(Name, "") { }
    public OpenItem(OpenItem item) : this(item.Name, item.PathExe, item.DelayBefore, item.DelayAfter, item.Type, item.MinimizeOnOpen) { }

    public virtual string Validate()
    {
        if (!File.Exists(PathExe))
            return $"You set a path to file that not exist in {Name}";

        if (DelayAfter < 0 || DelayBefore < 0)
            return $"You set delay lower than 0 in {Name}";

        if (DelayAfter > 999999 || DelayBefore > 99999)
            return $"Your delay can't be higher than 99999 in {Name}";

        return string.Empty;
    }

    public virtual async Task Open(StartViewModel startModel, CancellationToken token)
    {
        try
        {
            bool isCancelled = token.IsCancellationRequested;
            if (!isCancelled) await Task.Delay(DelayBefore);

            string executable = Path.GetFileName(PathExe);
            string pathDir = Path.GetDirectoryName(PathExe) ?? "";

            OpenedProcess opened = new();
            ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
            string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);
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

    public virtual ushort GetAdditionalProgressCount() => 0;

    public void ChangeName(string name)
    {
        Name = name;
    }
}
