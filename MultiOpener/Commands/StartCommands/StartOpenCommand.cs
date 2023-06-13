using MultiOpener.Items;
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
        private SettingsViewModel? Settings { get; set; }

        private OpenningProcessLoadingWindow? loadingProcesses;

        public CancellationTokenSource source = new();
        private CancellationToken token;

        public bool isOpening = false;


        public StartOpenCommand(StartViewModel? startViewModel, MainWindow mainWindow) : base(startViewModel)
        {
            MainWindow = mainWindow;
        }

        public override void Execute(object? parameter)
        {
            if (Start == null) return;

            Settings ??= MainWindow.MainViewModel.settings;

            if (Start.OpenButtonName.Equals("CLOSE"))
            {
                Start.CloseCommand.Execute(null);
                return;
            }

            if (!Start.OpenedIsEmpty() || Settings.OpenIsEmpty()) return;
            Start.OpenButtonName = "CLOSE";

            source = new();
            token = source.Token;
            Task task = Task.Run(OpenProgramsList, token);
        }

        private async Task OpenProgramsList()
        {
            if (Start == null || Settings == null) return;

            isOpening = true;
            int length = Settings.Opens.Count;
            int progressLength = length;
            string infoText = "";

            //Validating all Opens
            for (int i = 0; i < length; i++)
            {
                var current = Settings.Opens[i];

                string result = current.Validate();
                if (!string.IsNullOrEmpty(result))
                {
                    MessageBox.Show(result, "", MessageBoxButton.OK, MessageBoxImage.Error);

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
                            ProcessStartInfo startInfo = new(current.PathExe) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Minimized };
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

            //Initializing loading window
            Application.Current?.Dispatcher.Invoke(delegate
            {
                MainWindow.Hide();
                float windowPositionX = (float)(MainWindow.Left + (MainWindow.Width / 2));
                float windowPositionY = (float)(MainWindow.Top + (MainWindow.Height / 2));
                loadingProcesses = new(this, windowPositionX, windowPositionY) { Owner = MainWindow };
                loadingProcesses.Show();
                loadingProcesses.progress.Maximum = progressLength;
            });

            //Opening everything
            for (int i = 0; i < length; i++)
            {
                //TODO: 1 Zrobic mozliwosc zrobienia quick preset open czyli poprostu startujesz i odrazu zamykasz i robi szkielety wszystkie 'open' w panelu start po to zeby moc jest wystartowac kiedy tylko sie zechce
                //lub dac opcje na right click contextmenu zeby zrobic quick open tylko okienek z zamknietymi procesami
                var current = Settings.Opens[i];

                if (string.IsNullOrEmpty(current.PathExe)) return;
                if (source.IsCancellationRequested) break;

                Application.Current?.Dispatcher.Invoke(delegate
                {
                    infoText = $"({i + 1}/{length}) Openning {current.Name}...";
                    loadingProcesses!.SetText(infoText);
                    loadingProcesses!.progress.Value++;
                });

                await current.Open(loadingProcesses, source, infoText);
            }

            //Destroying MultiMC if there was Instances as type in Opens
            if (Start.MultiMC != null)
            {
                await Start.MultiMC.Close();
                Start.MultiMC = null;
            }

            //Ending loading etc
            Application.Current?.Dispatcher.Invoke(delegate
            {
                loadingProcesses!.Close();
                MainWindow.OnShow();
                loadingProcesses = null;

                if (Start.OpenedIsEmpty())
                    Start.OpenButtonName = "OPEN";
            });

            //Late Refresh
            Application.Current?.Dispatcher.Invoke(delegate
            {
                ((MainWindow)Application.Current.MainWindow).MainViewModel.start.UpdateText("Attempting to first Auto-Refresh");
            });
            await Task.Delay(App.config.TimeLateRefresh);
            Application.Current?.Dispatcher.Invoke(delegate
            {
                Consts.IsStartPanelWorkingNow = false;
                Start.RefreshOpenedCommand.Execute(null);
            });

            isOpening = false;
            source.Dispose();
        }
    }
}
