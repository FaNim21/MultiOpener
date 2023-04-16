using MultiOpener.Items;
using MultiOpener.ListView;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenCommand : StartCommandBase
    {
        private MainWindow MainWindow { get; set; }
        private SettingsViewModel Settings { get; set; }

        private OpenningProcessLoadingWindow? loadingProcesses;

        public CancellationTokenSource source = new();
        private CancellationToken token;


        public StartOpenCommand(StartViewModel startViewModel) : base(startViewModel)
        {
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public override void Execute(object? parameter)
        {
            if (Start == null) return;

            Settings ??= MainWindow.MainViewModel.settings;

            if (Start.OpenButtonName.Equals("OPEN"))
            {
                if (!Start.OpenedIsEmpty() || Settings.OpenIsEmpty()) return;
                Start.OpenButtonName = "CLOSE";

                //TODO: 1 -- NAPRAWIC -- kwestie zatrzymywania czesto zawiesza to cale ladowanie, a jak zatrzymuje sie przy odpalaniu instancji to crashuje

                source = new();
                token = source.Token;
                Task task = Task.Run(OpenProgramsList, token);
            }
            else
                Start.CloseCommand.Execute(null);
        }

        private async Task OpenProgramsList()
        {
            if (Start == null) return;

            int length = Settings.Opens.Count;
            int progressLength = length;

            for (int i = 0; i < length; i++)
            {
                var current = Settings.Opens[i];
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
                    if (Start.MultiMC == null)
                    {
                        try
                        {
                            ProcessStartInfo startInfo = new(current.PathExe) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
                            Process? process = Process.Start(startInfo);
                            if (process != null)
                            {
                                process.WaitForInputIdle();

                                OpenedProcess open = new();
                                open.SetHandle(process.Handle);
                                open.SetPid();
                                Start.MultiMC = open;
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
                var current = Settings.Opens[i];

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

            if(Start.MultiMC != null)
            {
                await Start.MultiMC.Close();
                Start.MultiMC = null;
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses.Close();
                MainWindow.OnShow();
                loadingProcesses = null;

                if (Start.OpenedIsEmpty())
                    Start.OpenButtonName = "OPEN";
            });

            await Task.Delay(3500);
            Application.Current.Dispatcher.Invoke(delegate
            {
                Start.RefreshOpenedCommand.Execute(null);
            });
        }

        //trzeba na to uwazac, poniewaz nie jestem tego pewien
        [Obsolete]
        public void TryRunRefreshLoop()
        {
            if (Start == null) return;

            Start.loopSource = new();
            Start.loopToken = Start.loopSource.Token;
            Start.Loop = Task.Factory.StartNew(async () => await RefreshOpenedLoop(), Start.loopToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        [Obsolete]
        private async Task RefreshOpenedLoop()
        {
            if (Start == null) return;

            while (!Start.loopSource.IsCancellationRequested)
            {
                Trace.WriteLine("Loop");
                Start.RefreshOpenedCommand.Execute(null);
                await Task.Delay(2000);
            }
        }
    }
}
