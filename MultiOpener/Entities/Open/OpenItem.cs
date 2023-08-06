using MultiOpener.Components.Controls;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
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
}

[JsonDerivedType(typeof(OpenItem), typeDiscriminator: "base")]
[JsonDerivedType(typeof(OpenInstance), typeDiscriminator: "instances")]
public class OpenItem
{
    public OpenType Type { get; set; }

    public string Name { get; set; }
    public string PathExe { get; set; }

    public int DelayBefore { get; set; }
    public int DelayAfter { get; set; }

    public bool MinimizeOnOpen { get; set; }


    [JsonConstructor]
    public OpenItem(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, bool MinimizeOnOpen = false)
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
    }
    public OpenItem(OpenItem item)
    {
        Name = item.Name;
        PathExe = item.PathExe;
        DelayBefore = item.DelayBefore;
        DelayAfter = item.DelayAfter;
        Type = item.Type;
        MinimizeOnOpen = item.MinimizeOnOpen;
    }

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

    public virtual async Task Open(StartViewModel startModel, CancellationToken token, string infoText = "")
    {
        try
        {
            if (!token.IsCancellationRequested)
                await Task.Delay(DelayBefore);

            string executable = Path.GetFileName(PathExe);
            string pathDir = Path.GetDirectoryName(PathExe) ?? "";

            OpenedProcess opened = new();
            ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
            string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);
            opened.Initialize(startInfo, name!, PathExe, MinimizeOnOpen);

            if (token.IsCancellationRequested)
                opened.Clear();
            else
                await opened.OpenProcess(token);

            Application.Current?.Dispatcher.Invoke(delegate { ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(opened); });

            if (!token.IsCancellationRequested)
                await Task.Delay(DelayAfter);
        }
        catch (Exception e)
        {
            StartViewModel.Log(e.ToString(), ConsoleLineOption.Error);
        }
    }

    public virtual ushort GetAdditionalProgressCount() => 0;
}
