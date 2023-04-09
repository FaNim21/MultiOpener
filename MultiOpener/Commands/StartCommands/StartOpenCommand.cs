using MultiOpener.Items;
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

                if ((MainWindow.MainViewModel.start.Opened.Any() && MainWindow.MainViewModel.start.Opened.Count != 0) || string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName) || Start == null) return;
                if (MainWindow.MainViewModel.settings.Opens == null || !MainWindow.MainViewModel.settings.Opens.Any()) return;

                Start.OpenButtonName = "CLOSE";

                //TODO: 1 -- NAPRAWIC -- kwestie zatrzymywania czesto zawiesza to cale ladowanie, a jak zatrzymuje sie przy odpalaniu instancji to crashuje

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
                    if (MainWindow.MainViewModel.start.MultiMC == null)
                    {
                        try
                        {
                            ProcessStartInfo startInfo = new(current.PathExe) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
                            Process? process = Process.Start(startInfo);
                            if (process != null)
                            {
                                OpenedProcess open = new();
                                open.SetHandle(process.Handle);
                                MainWindow.MainViewModel.start.MultiMC = open;
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
                        await Task.Delay(current.DelayBefore);
                        string executable = Path.GetFileName(current.PathExe);
                        string pathDir = Path.GetDirectoryName(current.PathExe) ?? "";

                        ProcessStartInfo processStartInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
                        Process? process = Process.Start(processStartInfo);

                        if (process != null)
                        {
                            process.EnableRaisingEvents = true;

                            OpenedProcess open = new();
                            open.SetStartInfo(processStartInfo);
                            open.SetHandle(process.Handle);

                            int errors = 0;
                            while (!open.SetHwnd() && errors < 10)
                            {
                                if (source.IsCancellationRequested)
                                    break;

                                await Task.Delay(250);
                                errors++;
                            }
                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                MainWindow.MainViewModel.start.AddOpened(open);
                            });
                        }
                        await Task.Delay(current.DelayAfter);
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses.Close();
                MainWindow.OnShow();
            });

            //to jest tymczasowo na automatyczne odswiezenie informacji po chwili od ich odpalenia
            await Task.Delay(5000);
            MainWindow.MainViewModel.start.RefreshOpenedCommand.Execute(null);
        }

        private async Task OpenMultiMcInstances(OpenInstance open, string infoText = "")
        {
            List<OpenedProcess> mcInstances = new();
            try
            {
                await Task.Delay(open.DelayBefore);
                int count = 0;

                ProcessStartInfo startInfo = new(open.PathExe) { UseShellExecute = false };
                for (int i = 0; i < open.Quantity; i++)
                {
                    await Task.Delay(i == 0 ? 0 : i == 1 ? 5000 : open.DelayBetweenInstances);

                    if (source.IsCancellationRequested)
                        break;

                    count++;
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
                        opened.SetStartInfo(startInfo);
                        opened.isMCInstance = true;
                        mcInstances.Add(opened);
                    }
                }

                Application.Current.Dispatcher.Invoke(delegate
                {
                    loadingProcesses.SetText($"{infoText} (loading datas)");
                });

                List<IntPtr> instances;
                do
                {
                    instances = Win32.GetWindowsByTitlePattern("Minecraft");
                    await Task.Delay(750);
                }
                while (instances.Count != count);

                for (int i = 0; i < mcInstances.Count; i++)
                {
                    var current = mcInstances[i];
                    current.SetHwnd(instances[i]);
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        MainWindow.MainViewModel.start.AddOpened(current);
                    });
                }

                //TODO: 9 CALY CZAS TRZEBA LEKKO OPOZNIC SEGMENT PO MINECRAFTACH DO TEGO ZEBY WYKRYWAC CZY MC JEST ODPALONY DO MAIN MENU ZEBY WALLE GO ZCZYTYWALY
                int loadingIntro = 3000;
                await Task.Delay(open.DelayAfter + loadingIntro);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
