using MultiOpener.Components.Controls;
using MultiOpener.Items;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands;

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
        bool isShiftPressed = false;

        //Validating all Opens
        for (int i = 0; i < length; i++)
        {
            var current = Settings.Opens[i];

            string result = current.Validate();
            if (!string.IsNullOrEmpty(result))
            {
                DialogBox.Show(result, "", MessageBoxButton.OK, MessageBoxImage.Error);

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
                        DialogBox.Show(e.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
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

        //Getting shift to be pressed to make structured open
        if (App.Input.IsShiftPressed)
        {
            source.Cancel();
            isShiftPressed = true;
        }

        //Opening everything
        for (int i = 0; i < length; i++)
        {
            var current = Settings.Opens[i];

            if (string.IsNullOrEmpty(current.PathExe)) return;

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
        if (!isShiftPressed)
        {
            Application.Current?.Dispatcher.Invoke(delegate
            {
                ((MainWindow)Application.Current.MainWindow).MainViewModel.start.UpdateText("Attempting to first Auto-Refresh");
            });
            await Task.Delay(App.Config.TimeLateRefresh);
        }
        Application.Current?.Dispatcher.Invoke(delegate
        {
            Consts.IsStartPanelWorkingNow = false;
            bool isItOpening = true;
            Start.RefreshOpenedCommand.Execute(new object[] { isShiftPressed, isItOpening});
        });

        isOpening = false;
        source.Dispose();

        //TODO: 7 narazie wrzucilem dzwiek do testu, ale trzeba tu dac cos kreatywniejszego moze
        SystemSounds.Beep.Play();
    }
}
