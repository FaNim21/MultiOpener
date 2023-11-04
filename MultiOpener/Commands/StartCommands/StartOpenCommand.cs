using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.Entities.Open;
using MultiOpener.Entities.Opened;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Diagnostics;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Commands.StartCommands;

public class StartOpenCommand : StartCommandBase
{
    private MainWindow MainWindow { get; set; }
    private SettingsViewModel? Settings { get; set; }

    private Stopwatch Stopwatch { get; set; } = new();

    public CancellationTokenSource source = new();
    private CancellationToken token;

    public bool isOpening = false;
    private bool isShiftPressed = false;

    private int progressLength;
    private int count;


    public StartOpenCommand(StartViewModel? startViewModel, MainWindow mainWindow) : base(startViewModel)
    {
        MainWindow = mainWindow;
    }

    public override void Execute(object? parameter)
    {
        if (Start == null) return;

        Settings ??= MainWindow.MainViewModel.settings;

        if (Start.IsStartButtonState(StartButtonState.cancel))
        {
            Start.CancelCommand.Execute(null);
            return;
        }

        if (Start.IsStartButtonState(StartButtonState.close))
        {
            Start.CloseCommand.Execute(null);
            return;
        }

        if (Start.IsStartButtonState(StartButtonState.open) && !Start.OpenedIsEmpty())
        {
            Start.SetStartButtonState(StartButtonState.close);
            return;
        }

        if (!Start.OpenedIsEmpty() || Settings.OpenIsEmpty()) return;
        Start.SetStartButtonState(StartButtonState.cancel);

        source = new();
        token = source.Token;
        Task task = Task.Run(OpenProgramsList, token);
    }

    private async Task OpenProgramsList()
    {
        if (Start == null || Settings == null) return;

        int length = Settings!.Opens.Count;

        if (!Initialize(length)) return;

        await OpenAll(length);
        await Finalize();
    }

    public bool Initialize(int length)
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            if (!Application.Current.MainWindow.Topmost)
                Application.Current.MainWindow.Topmost = true;
        });
        isOpening = true;
        progressLength = length;
        count = 0;

        if (!Validate(length)) return false;

        Start!.LoadingPanelVisibility = true;
        Start.SetLoadingText(string.Empty);
        Start.SetDetailedLoadingText(string.Empty);

        if (InputController.Instance.GetKey(Key.LeftShift))
        {
            source.Cancel();
            isShiftPressed = true;
            Start!.LoadingPanelVisibility = false;
        }
        return true;
    }
    private bool Validate(int length)
    {
        for (int i = 0; i < length; i++)
        {
            var current = Settings!.Opens[i];

            progressLength += current.GetAdditionalProgressCount();

            string result = current.Validate();
            if (!string.IsNullOrEmpty(result))
            {
                DialogBox.Show(result, "", MessageBoxButton.OK, MessageBoxImage.Error);
                Start!.SetStartButtonState(StartButtonState.open);
                return false;
            }

            if (current.GetType() == typeof(OpenInstance))
            {
                if (Start?.MultiMC == null)
                {
                    try
                    {
                        Process[] mc = Process.GetProcessesByName("MultiMC");
                        if (mc != null && mc.Length > 0)
                        {
                            mc[0].Kill();
                            mc[0].WaitForExit();
                        }

                        ProcessStartInfo startInfo = new(current.PathExe) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Minimized };
                        Process? process = Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForInputIdle();

                            OpenedProcess open = new();
                            open.SetPid(process.Id);
                            Start!.MultiMC = open;
                        }
                    }
                    catch (Exception e)
                    {
                        DialogBox.Show(e.ToString());
                    }
                }
            }
        }
        return true;
    }

    public async Task OpenAll(int length)
    {
        Stopwatch.Start();
        for (int i = 0; i < length; i++)
        {
            var current = Settings!.Opens[i];

            Start!.SetLoadingText($"({i + 1}/{length}) Opening {current.Name}...");
            UpdateProgressBar();

            await current.Open(Start, token);
            Start!.SetDetailedLoadingText(string.Empty);

            Application.Current.Dispatcher.Invoke(delegate { Application.Current.MainWindow?.Activate(); });
        }
        Stopwatch.Stop();
    }

    public async Task Finalize()
    {
        if (Start?.MultiMC != null)
        {
            await Start.MultiMC.Close();
            Start.MultiMC = null;
        }

        if (Start!.OpenedIsEmpty())
            Start.SetStartButtonState(StartButtonState.open);

        Consts.IsStartPanelWorkingNow = false;
        bool isItOpening = true;

        if (!isShiftPressed)
        {
            Start!.SetLoadingText("Auto-Refreshing");
            StartViewModel.Log($"Opened Preset {Settings!.PresetName} in {Math.Round(Stopwatch.Elapsed.TotalSeconds * 100) / 100} seconds");
            StartViewModel.Log("Attempting to first Auto-Refresh", ConsoleLineOption.Warning);
            await Task.Delay(App.Config.TimeLateRefresh);
            Start!.RefreshOpenedCommand.Execute(new object[] { isItOpening });
        }

        Start!.LoadingPanelVisibility = false;
        source.Dispose();
        isOpening = false;
        isShiftPressed = false;
        Stopwatch.Reset();
        Application.Current?.Dispatcher.Invoke(delegate
        {
            if (!App.Config.AlwaysOnTop && Application.Current.MainWindow.Topmost)
                Application.Current.MainWindow.Topmost = false;
        });

        SystemSounds.Beep.Play();
        Start!.SetStartButtonState(StartButtonState.close);
    }

    public void UpdateCountForInstances()
    {
        count--;
    }
    public void UpdateProgressBar()
    {
        count++;
        Start!.LoadingBarPercentage = (count * 100) / progressLength;
    }
}