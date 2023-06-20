using MultiOpener.Utils;
using MultiOpener.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Items;

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

    public virtual async Task Open(OpenningProcessLoadingWindow? loading, CancellationTokenSource source, string infoText = "")
    {
        try
        {
            if (!source.IsCancellationRequested)
                await Task.Delay(DelayBefore);

            string executable = Path.GetFileName(PathExe);
            string pathDir = Path.GetDirectoryName(PathExe) ?? "";

            OpenedProcess opened = new();
            ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true};
            string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);

            if (source.IsCancellationRequested)
            {
                opened.Initialize(startInfo, name!, IntPtr.Zero, PathExe);
                opened.Clear();
            }
            else
            {
                Process? process = Process.Start(startInfo!);
                if (process != null)
                {
                    opened.Initialize(startInfo, name!, process.Handle, PathExe);

                    int errors = 0;
                    var config = new TimeoutConfigurator(App.Config.TimeoutOpen, 15);

                    while (!opened.SetHwnd() && errors < config.ErrorCount)
                    {
                        if (source.IsCancellationRequested)
                            break;

                        await Task.Delay(config.Cooldown);
                        errors++;
                    }

                }
            }

            if(MinimizeOnOpen)
                Win32.MinimizeWindowHwnd(opened.Hwnd);

            Application.Current?.Dispatcher.Invoke(delegate { ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(opened); });

            if (!source.IsCancellationRequested)
                await Task.Delay(DelayAfter);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}
