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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands;

public class StartOpenCommand : StartCommandBase
{
    private MainWindow MainWindow { get; set; }
    private SettingsViewModel? Settings { get; set; }

    private OpenningProcessLoadingWindow? loadingProcesses;

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
        isOpening = true;
        int progressLength = length;

        Validate(ref progressLength, length);

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
                Application.Current.Dispatcher.Invoke(delegate { Start!.OpenButtonName = "OPEN"; });
                return;
            }

            //TODO: 9 Czy to jakkolwiek jest potrzebne?????
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

        Application.Current?.Dispatcher.Invoke(delegate
        {
            loadingProcesses!.Close();
            MainWindow.OnShow();
            loadingProcesses = null;

            if (Start!.OpenedIsEmpty())
                Start.OpenButtonName = "OPEN";
        });

        //Late Refresh
        StartViewModel.Log($"Opened Preset {Settings!.PresetName} in {Math.Round(Stopwatch.Elapsed.TotalSeconds * 100) / 100} seconds");
        if (!isShiftPressed)
        {
            Application.Current?.Dispatcher.Invoke(delegate { StartViewModel.Log("Attempting to first Auto-Refresh", ConsoleLineOption.Warning); });
            await Task.Delay(App.Config.TimeLateRefresh);
        }

        Application.Current?.Dispatcher.Invoke(delegate
        {
            Consts.IsStartPanelWorkingNow = false;
            bool isItOpening = true;
            Start!.RefreshOpenedCommand.Execute(new object[] { isShiftPressed, isItOpening });
        });

        source.Dispose();
        isOpening = false;
        isShiftPressed = false;
        Stopwatch.Reset();

        SystemSounds.Beep.Play();
    }

    public async Task OpenAll(int length)
    {
        string infoText = "";

        Stopwatch.Start();
        for (int i = 0; i < length; i++)
        {
            var current = Settings!.Opens[i];

            if (string.IsNullOrEmpty(current.PathExe)) return;

            Application.Current?.Dispatcher.Invoke(delegate
            {
                infoText = $"({i + 1}/{length}) Openning {current.Name}...";
                loadingProcesses!.SetText(infoText);
                loadingProcesses!.progress.Value++;
            });

            await current.Open(loadingProcesses, token, infoText);
        }
        Stopwatch.Stop();
    }
}
