using MultiOpener.Items;
using MultiOpener.Utils;
using MultiOpener.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.ListView
{
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


        [JsonConstructor]
        public OpenItem(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default)
        {
            this.Name = Name;
            this.PathExe = PathExe;
            this.DelayBefore = DelayBefore;
            this.DelayAfter = DelayAfter;
            this.Type = Type;
        }
        public OpenItem(OpenItem item)
        {
            Name = item.Name;
            PathExe = item.PathExe;
            DelayBefore = item.DelayBefore;
            DelayAfter = item.DelayAfter;
            Type = item.Type;
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
                await Task.Delay(DelayBefore);
                string executable = Path.GetFileName(PathExe);
                string pathDir = Path.GetDirectoryName(PathExe) ?? "";

                ProcessStartInfo processStartInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
                Process? process = Process.Start(processStartInfo);

                if (process != null)
                {
                    process.EnableRaisingEvents = true;

                    OpenedProcess opened = new();
                    opened.SetStartInfo(processStartInfo);
                    opened.SetName();
                    opened.SetHandle(process.Handle);
                    opened.SetPath(PathExe);

                    int errors = 0;
                    while (!opened.SetHwnd() && errors < 15)
                    {
                        if (source.IsCancellationRequested)
                            break;

                        await Task.Delay(250);
                        errors++;
                    }

                    if (!process.HasExited)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(opened);
                        });
                    }
                }
                await Task.Delay(DelayAfter);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }

    public class OpenInstance : OpenItem
    {
        public int DelayBetweenInstances { get; set; }

        public int Quantity { get; set; }
        public ObservableCollection<string> Names { get; set; }


        [JsonConstructor]
        public OpenInstance(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, int Quantity = 0, ObservableCollection<string> Names = null, int DelayBetweenInstances = 0) : base(Name, PathExe, DelayBefore, DelayAfter, Type)
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
            try
            {
                await Task.Delay(DelayBefore);
                int count = 0;

                for (int i = 0; i < Quantity; i++)
                {
                    await Task.Delay(i == 0 ? 0 : i == 1 ? 5000 : DelayBetweenInstances < 500 ? 500 : DelayBetweenInstances);

                    if (source.IsCancellationRequested)
                        break;

                    count++;

                    if (loading != null)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            loading.SetText($"{infoText} -- Instance ({i + 1}/{Quantity})");
                            loading.progress.Value++;
                        });
                    }

                    ProcessStartInfo startInfo = new(PathExe)
                    {
                        UseShellExecute = false,
                        Arguments = $"--launch \"{Names[i]}\""
                    };

                    Process? process = Process.Start(startInfo);
                    if (process != null)
                    {
                        process.WaitForInputIdle();

                        OpenedProcess opened = new();
                        opened.SetHandle(process.Handle);
                        opened.SetStartInfo(startInfo);
                        opened.SetName(Names[i]);
                        opened.isMCInstance = true;

                        string path = Path.GetDirectoryName(PathExe) + "\\instances\\" + Names[i];
                        opened.SetPath(path.Replace("\\", "/"));

                        mcInstances.Add(opened);
                    }
                }

                if (loading != null)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        loading.SetText($"{infoText} (loading datas)");
                    });
                }

                Regex mcPatternRegex = new(OpenedProcess.MCPattern);
                int errorCount = -1;
                List<IntPtr> instances;
                do
                {
                    errorCount++;
                    instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
                    await Task.Delay(750);
                } while (instances.Count != count && errorCount < 50);

                //TODO: 9 OPTIMIZE IT?
                for (int i = 0; i < mcInstances.Count; i++)
                {
                    var current = mcInstances[i];

                    for (int j = 0; j < instances.Count; j++)
                    {
                        string currentPath = Win32.GetJavaFilePath((int)Win32.GetPidFromHwnd(instances[j]));
                        if (currentPath.Equals(current.Path))
                        {
                            current.SetHwnd(instances[j]);

                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(current);
                            });

                            instances.Remove(instances[j]);
                            break;
                        }
                    }
                }

                int loadingIntro = 3000;
                await Task.Delay(DelayAfter + loadingIntro);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
