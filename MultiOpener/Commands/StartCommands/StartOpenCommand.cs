using MultiOpener.ListView;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenCommand : StartCommandBase
    {
        public MainWindow MainWindow { get; set; }

        public OpenningProcessLoadingWindow loadingProcesses;

        public CancellationTokenSource source = new();
        public CancellationToken token;

        public StartOpenCommand(StartViewModel startViewModel) : base(startViewModel)
        {
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public override void Execute(object? parameter)
        {
            if (Start == null) return;

            if (Start.OpenButtonName.Equals("OPEN"))
            {
                if (string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName)) return;

                if ((MainWindow.opened.Any() && MainWindow.opened.Count != 0) || string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName) || Start == null) return;
                if (MainWindow.MainViewModel.settings.Opens == null || !MainWindow.MainViewModel.settings.Opens.Any()) return;

                Start.OpenButtonName = "CLOSE";

                //TODO: Dac na starcie Panel z podgladem odpalonych aplikacji

                source = new();
                token = source.Token;
                Task task = Task.Run(OpenProgramsList, token);
            }
            else
            {
                Start.CloseCommand.Execute(null);
            }
        }

        public async Task OpenProgramsList()
        {
            int length = MainWindow.MainViewModel.settings.Opens.Count;
            int progressLength = length;

            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (current.Validate())
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        Start.OpenButtonName = "OPEN";
                    });
                    return;
                }

                if (current.GetType() == typeof(OpenInstance))
                {
                    if (MainWindow.MultiMC == null)
                    {
                        try
                        {
                            ProcessStartInfo startInfo = new(current.PathExe) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
                            Process? process = Process.Start(startInfo);
                            if (process != null)
                            {
                                OpenedProcess open = new();
                                open.SetHandle(process.Handle);
                                MainWindow.MultiMC = open;
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }

                    progressLength += ((OpenInstance)current).Quantity;
                }
            }
            string infoText = "";

            Application.Current.Dispatcher.Invoke(delegate
            {
                MainWindow.Hide();
                float windowPositionX = (float)(MainWindow.Left + (MainWindow.Width / 2));
                float windowPositionY = (float)(MainWindow.Top + (MainWindow.Height / 2));
                loadingProcesses = new(this, windowPositionX, windowPositionY) { Owner = MainWindow };
                loadingProcesses.Show();
                loadingProcesses.progress.Maximum = progressLength;
            });

            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (string.IsNullOrEmpty(current.PathExe))
                    continue;

                if (source.IsCancellationRequested)
                    break;

                Application.Current.Dispatcher.Invoke(delegate
                {
                    infoText = $"({i + 1}/{length}) Openning {current.Name}...";
                    loadingProcesses.SetText(infoText);
                    loadingProcesses.progress.Value++;
                });

                if (current.Type == OpenType.InstancesMultiMC)
                    await OpenMultiMcInstances((OpenInstance)current, infoText);
                else
                {
                    try
                    {
                        await Task.Delay(current.DelayBefore, token);
                        string executable = Path.GetFileName(current.PathExe);
                        string pathDir = Path.GetDirectoryName(current.PathExe) ?? "";

                        ProcessStartInfo processStartInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
                        Process? process = Process.Start(processStartInfo);

                        if (process != null)
                        {
                            process.EnableRaisingEvents = true;

                            OpenedProcess open = new();
                            open.SetHandle(process.Handle);

                            int errors = 0;
                            while (!open.SetHwnd() && errors < 10)
                            {
                                if (source.IsCancellationRequested)
                                    break;

                                await Task.Delay(200);
                                errors++;
                            }
                            MainWindow.opened.Add(open);
                        }
                        await Task.Delay(current.DelayAfter, token);
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }

            await Task.Delay(3000, token);

            for (int i = 0; i < MainWindow.opened.Count; i++)
            {
                var current = MainWindow.opened[i];
                current.UpdateTitle();
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses.Close();
                MainWindow.OnShow();

            });
        }

        private async Task OpenMultiMcInstances(OpenInstance open, string infoText = "")
        {
            List<OpenedProcess> mcInstances = new();
            try
            {
                await Task.Delay(open.DelayBefore, token);

                ProcessStartInfo startInfo = new(open.PathExe) { UseShellExecute = false };
                for (int i = 0; i < open.Quantity; i++)
                {
                    await Task.Delay(i == 0 ? 0 : i == 1 ? 5000 : open.DelayBetweenInstances, token);

                    if (source.IsCancellationRequested)
                        return;

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        loadingProcesses.SetText($"{infoText} -- Instance ({i + 1}/{open.Quantity})");
                        loadingProcesses.progress.Value++;
                    });

                    startInfo.Arguments = $"--launch \"{open.Names[i]}\"";
                    Process? process = Process.Start(startInfo);
                    if (process != null)
                    {
                        process.WaitForInputIdle();

                        OpenedProcess opened = new();
                        opened.SetHandle(process.Handle);
                        opened.IsMinecraftInstance = true;
                        mcInstances.Add(opened);
                    }
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    loadingProcesses.SetText($"{infoText} (loading datas)");
                });

                var instances = Win32.GetWindowsByTitlePattern("Minecraft");
                do
                {
                    instances = Win32.GetWindowsByTitlePattern("Minecraft");
                    await Task.Delay(750);
                }
                while (instances.Count != open.Quantity);

                for (int i = 0; i < mcInstances.Count; i++)
                {
                    var current = mcInstances[i];
                    current.SetHwnd(instances[i]);
                    current.UpdateTitle();
                    MainWindow.opened.Add(current);
                }

                await Task.Delay(open.DelayAfter, token);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
