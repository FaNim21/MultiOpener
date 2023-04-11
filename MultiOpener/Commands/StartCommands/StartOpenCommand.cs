using MultiOpener.Items;
using MultiOpener.ListView;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenCommand : StartCommandBase
    {
        private MainWindow MainWindow { get; set; }

        private OpenningProcessLoadingWindow loadingProcesses;

        public CancellationTokenSource source = new();
        private CancellationToken token;

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
                Start.CloseCommand.Execute(null);
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

                if (string.IsNullOrEmpty(current.PathExe)) return;
                if (source.IsCancellationRequested) break;

                Application.Current.Dispatcher.Invoke(delegate
                {
                    infoText = $"({i + 1}/{length}) Openning {current.Name}...";
                    loadingProcesses.SetText(infoText);
                    loadingProcesses.progress.Value++;
                });

                await current.Open(loadingProcesses, source, infoText);
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses.Close();
                MainWindow.OnShow();

                if (MainWindow.MainViewModel.start.Opened == null || MainWindow.MainViewModel.start.Opened.Count == 0)
                    Start.OpenButtonName = "OPEN";
            });

            //TODO: 9 to jest tymczasowo na automatyczne odswiezenie informacji po chwili od ich odpalenia
            if (!source.IsCancellationRequested)
            {
                await Task.Delay(5000);
                MainWindow.MainViewModel.start.RefreshOpenedCommand.Execute(null);
            }
        }
    }
}
