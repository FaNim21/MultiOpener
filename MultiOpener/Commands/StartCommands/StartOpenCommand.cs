using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.Entities.Open;
using MultiOpener.Entities.Opened;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
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

        if (Start.OpenButtonName.Equals("START") && !Start.OpenedIsEmpty())
        {
            Start.OpenButtonName = "CLOSE";
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

        int length = Settings!.Opens.Count;

        Initialize(length);
        await OpenAll(length);
        await Finalize();
    }

    public void Initialize(int length)
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            if (!Application.Current.MainWindow.Topmost)
                Application.Current.MainWindow.Topmost = true;
        });
        isOpening = true;
        int progressLength = length;

        Validate(ref progressLength, length);

        Start!.LoadingPanelVisibility = true;
        Start.SetLoadingText("");

        //Getting shift to be pressed to make structured open
        if (InputController.Instance.GetKey(Key.LeftShift))
        {
            source.Cancel();
            isShiftPressed = true;
        }
    }
    private void Validate(ref int progressLength, int length)
    {
        for (int i = 0; i < length; i++)
        {
            var current = Settings!.Opens[i];

            progressLength += current.GetAdditionalProgressCount();

            string result = current.Validate();
            if (!string.IsNullOrEmpty(result))
            {
                DialogBox.Show(result, "", MessageBoxButton.OK, MessageBoxImage.Error);
                Start!.OpenButtonName = "OPEN";
                return;
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
    }

    public async Task Finalize()
    {
        //Destroying MultiMC if there was Instances as type in Opens
        if (Start?.MultiMC != null)
        {
            await Start.MultiMC.Close();
            Start.MultiMC = null;
        }

        Start!.LoadingPanelVisibility = false;
        if (Start!.OpenedIsEmpty())
            Start.OpenButtonName = "OPEN";

        if (!isShiftPressed)
        {
            StartViewModel.Log($"Opened Preset {Settings!.PresetName} in {Math.Round(Stopwatch.Elapsed.TotalSeconds * 100) / 100} seconds");
            StartViewModel.Log("Attempting to first Auto-Refresh", ConsoleLineOption.Warning);
            await Task.Delay(App.Config.TimeLateRefresh);
        }

        Consts.IsStartPanelWorkingNow = false;
        bool isItOpening = true;
        Start!.RefreshOpenedCommand.Execute(new object[] { isShiftPressed, isItOpening });

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
    }

    public async Task OpenAll(int length)
    {
        Stopwatch.Start();
        for (int i = 0; i < length; i++)
        {
            var current = Settings!.Opens[i];

            if (string.IsNullOrEmpty(current.PathExe)) return;

            int count = i + 1;
            string infoText = $"({count}/{length}) Openning {current.Name}...";
            Start!.SetLoadingText(infoText);
            Start!.LoadingBarPercentage = (count * 100) / length;

            await current.Open(Start, token, infoText);
        }
        Stopwatch.Stop();
    }
}
