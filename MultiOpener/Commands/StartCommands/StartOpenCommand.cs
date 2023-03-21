using MultiOpener.ListView;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenCommand : StartCommandBase
    {
        public MainWindow MainWindow { get; set; }

        public OpenningProcessLoadingWindow loadingProcesses;
        public Vector2 windowPosition;

        public CancellationTokenSource source = new();
        public CancellationToken token;


        public StartOpenCommand(StartViewModel startViewModel) : base(startViewModel)
        {
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public override void Execute(object? parameter)
        {
            if ((MainWindow.openedProcess.Any() && MainWindow.openedProcess.Count != 0) || string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName) || Start == null) return;
            if (MainWindow.MainViewModel.settings.Opens == null || !MainWindow.MainViewModel.settings.Opens.Any()) return;

            Start.OpenButtonEnabled = false;

            //TODO: Odrazu zrobic na widoku modelu start rozpiske w kolumnie procesow uruchomionych z prawej sttrony
            //TODO: Wrzucic do watku cala liste i odpalajac ja sprawdzac czy dayn program juz nie istnieje najprosciej przez zapisywanie procesu do zmiennej czyli na przyszlosc pamietac zeby zabezpieczyc resetowanie programu czy cos albo przez zapamietywanie numeru procesu

            MainWindow.Hide();
            windowPosition.X = (float)(MainWindow.Left + (MainWindow.Width / 2));
            windowPosition.Y = (float)(MainWindow.Top + (MainWindow.Height / 2));

            source = new();
            token = source.Token;
            Task task = Task.Run(OpenProgramsList, token);
        }

        public async Task OpenProgramsList()
        {
            int length = MainWindow.MainViewModel.settings.Opens.Count;
            int progressLength = length;
            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (current.GetType() == typeof(OpenInstance))
                    progressLength += ((OpenInstance)current).Quantity;
            }
            string infoText = "";

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses = new(this, windowPosition.X, windowPosition.Y) { Owner = MainWindow };
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
                            process.Exited += Start.ProcessExited;
                            MainWindow.openedProcess.Add(process);
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
                MainWindow.Show();
            });
        }

        private async Task OpenMultiMcInstances(OpenInstance open, string infoText = "")
        {
            //TODO: Zrobic support na kontrole kazdej instancji oddzielnie, a nie przez tylko glowny proces multimc
            try
            {
                await Task.Delay(open.DelayBefore);
                ProcessStartInfo processStartInfo = new(open.PathExe, $"-l \"{open.Names[0]}\"") { UseShellExecute = true };
                Process? process = Process.Start(processStartInfo);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    loadingProcesses.SetText($"{infoText} -- Instance (1/{open.Names.Count})");
                });

                if (process != null)
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += Start.ProcessExited;
                    MainWindow.openedProcess.Add(process);
                }
                await Task.Delay(10000);

                for (int i = 1; i < open.Names.Count; i++)
                {
                    if (source.IsCancellationRequested)
                        return;

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        loadingProcesses.SetText($"{infoText} -- Instance ({i + 1}/{open.Names.Count})");
                        loadingProcesses.progress.Value++;
                    });

                    processStartInfo = new(open.PathExe, $"-l \"{open.Names[i]}\"") { UseShellExecute = true };
                    Process.Start(processStartInfo);
                    await Task.Delay(open.DelayBetweenInstances);
                }
                await Task.Delay(open.DelayAfter);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
