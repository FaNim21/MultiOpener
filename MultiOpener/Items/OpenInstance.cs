using MultiOpener.Utils;
using MultiOpener.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Windows;

namespace MultiOpener.Items;
public class OpenInstance : OpenItem
{
    public int DelayBetweenInstances { get; set; }

    public int Quantity { get; set; }
    public ObservableCollection<string> Names { get; set; }


    [JsonConstructor]
    public OpenInstance(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, int Quantity = 0, ObservableCollection<string>? Names = null, int DelayBetweenInstances = 0) : base(Name, PathExe, DelayBefore, DelayAfter, Type)
    {
        this.Quantity = Quantity;
        if (Names == null)
            this.Names = new ObservableCollection<string>();
        else
            this.Names = Names;
        this.DelayBetweenInstances = DelayBetweenInstances;
    }
    public OpenInstance(OpenInstance instance) : base(instance)
    {
        Quantity = instance.Quantity;
        if (Names == null)
            Names = new ObservableCollection<string>();
        else
            Names = instance.Names;
        DelayBetweenInstances = instance.DelayBetweenInstances;
    }

    public override string Validate()
    {
        string output = base.Validate();

        if (!Path.GetFileName(PathExe).Equals("MultiMC.exe"))
            return $"You set wrong path to MultiMC exe file in {Name}";

        if (!File.Exists(PathExe))
            return $"Your path to MultiMC doesn't exist in {Name}";

        if (DelayBetweenInstances > 99999 || DelayBetweenInstances < 0)
            return $"Delay between openning instances should be between 0 and 99999 in {Name}";

        if (Quantity > 32 || Quantity < 0)
            return $"Amount of instances should be between 0 and 32 in {Name}";

        int amount = 0;
        for (int i = 0; i < Names.Count; i++)
        {
            var current = Names[i];
            if (string.IsNullOrEmpty(current))
                amount++;
        }

        if (amount > 0)
            return $"You didn't set names for {amount} instances in {Name}";

        string instancePath = Path.GetDirectoryName(PathExe) + "\\instances\\";

        for (int i = 0; i < Quantity; i++)
        {
            var current = Names[i];

            if (!Directory.Exists(instancePath + current))
                return $"Instance \"{current}\" doesn't exist in multiMC instance folder";
        }

        return output;
    }

    public override async Task Open(OpenningProcessLoadingWindow? loading, CancellationTokenSource source, string infoText = "")
    {
        List<OpenedProcess> mcInstances = new();
        int openedCount = 0;
        try
        {
            if (!source.IsCancellationRequested)
            {
                await Task.Delay(DelayBefore);
            }

            for (int i = 0; i < Quantity; i++)
            {
                if (!source.IsCancellationRequested)
                {
                    await Task.Delay(i == 0 ? 0 : i == 1 ? 5000 : DelayBetweenInstances < 500 ? 500 : DelayBetweenInstances);
                }

                Application.Current?.Dispatcher.Invoke(delegate
                {
                    loading!.SetText($"{infoText} -- Instance ({i + 1}/{Quantity})");
                    loading.progress.Value++;
                });

                ProcessStartInfo startInfo = new(PathExe)
                {
                    UseShellExecute = false,
                    Arguments = $"--launch \"{Names[i]}\""
                };

                OpenedProcess opened = new();
                string path = (Path.GetDirectoryName(PathExe) + "\\instances\\" + Names[i]).Replace("\\", "/");
                opened.isMCInstance = true;

                if (source.IsCancellationRequested)
                    opened.Initialize(startInfo, Names[i], IntPtr.Zero, path);
                else
                {
                    openedCount++;
                    Process? process = Process.Start(startInfo);
                    if (process != null)
                    {
                        process.WaitForInputIdle();
                        opened.Initialize(startInfo, Names[i], process.Handle, path);
                    }
                }
                mcInstances.Add(opened);
            }

            Regex mcPatternRegex = new(OpenedProcess.MCPattern);
            List<IntPtr> instances = new();

            if (!source.IsCancellationRequested)
            {
                Application.Current?.Dispatcher.Invoke(delegate { loading!.SetText($"{infoText} (loading datas)"); });

                int errorCount = -1;
                var config = new TimeoutConfigurator(App.Config.TimeoutLookingForInstancesData, 30);
                do
                {
                    errorCount++;
                    instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
                    await Task.Delay(TimeSpan.FromMilliseconds(config.Cooldown));
                } while (instances.Count < openedCount && errorCount < config.ErrorCount);
            }

            for (int i = 0; i < Quantity; i++)
            {
                var current = mcInstances[i];
                bool isFound = false;

                for (int j = 0; j < instances.Count; j++)
                {
                    try
                    {
                        bool output = current.IsInstancePathEqual(instances[j]);
                        if (output)
                        {
                            isFound = true;
                            instances.Remove(instances[j]);
                            break;
                        }
                    }
                    catch { }
                }

                if (!isFound)
                    current.Clear();

                Application.Current?.Dispatcher.Invoke(delegate { ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(current); });
            }


            if (!source.IsCancellationRequested)
            {
                Application.Current?.Dispatcher.Invoke(delegate { loading!.SetText($"{infoText} (finalizing datas)"); });
                await Task.Delay(DelayAfter + App.Config.TimeoutInstanceFinalizingData);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}
