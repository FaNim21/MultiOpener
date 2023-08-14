using MultiOpener.Commands.StartCommands;
using MultiOpener.Entities;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MultiOpener.ViewModels;

public enum StartButtonState
{
    open,
    close,
    cancel,
}

public class StartViewModel : BaseViewModel
{
    public static StartViewModel? Instance { get; private set; }

    public ConsoleViewModel ConsoleViewModel { get; set; }

    public OpenedProcess? MultiMC { get; set; }
    public ObservableCollection<OpenedProcess> Opened { get; set; }

    public ICommand OpenCommand { get; set; }
    public ICommand CloseCommand { get; set; }
    public ICommand RefreshOpenedCommand { get; set; }
    public ICommand SaveConsoleCommand { get; set; }
    public ICommand ClearConsoleCommand { get; set; }
    public ICommand CancelCommand { get; set; }


    private string _openButtonName = "OPEN";
    public string OpenButtonName
    {
        get { return _openButtonName; }
        set
        {
            _openButtonName = value;
            OnPropertyChanged(nameof(OpenButtonName));
        }
    }

    private bool _openButtonEnabled = true;
    public bool OpenButtonEnabled
    {
        get { return _openButtonEnabled; }
        set
        {
            _openButtonEnabled = value;
            OnPropertyChanged(nameof(OpenButtonEnabled));
        }
    }

    private string? _presetNameLabel;
    public string? PresetNameLabel
    {
        get { return _presetNameLabel; }
        set
        {
            _presetNameLabel = value;
            OnPropertyChanged(nameof(PresetNameLabel));
        }
    }

    private string _refreshButtonName = "Refresh";
    public string RefreshButtonName
    {
        get { return _refreshButtonName; }
        set
        {
            _refreshButtonName = value;
            OnPropertyChanged(nameof(RefreshButtonName));
        }
    }


    private bool _loadingPanelVisibility = false;
    public bool LoadingPanelVisibility
    {
        get { return _loadingPanelVisibility; }
        set
        {
            _loadingPanelVisibility = value;
            OnPropertyChanged(nameof(LoadingPanelVisibility));
        }
    }

    public float _loadingBarPercentage;
    public float LoadingBarPercentage
    {
        get { return _loadingBarPercentage; }
        set
        {
            _loadingBarPercentage = value;
            OnPropertyChanged(nameof(LoadingBarPercentage));
        }
    }

    public string _loadingInfoText = "Info co sie teraz odpala";
    public string LoadingInfoText
    {
        get { return _loadingInfoText; }
        set
        {
            _loadingInfoText = value;
            OnPropertyChanged(nameof(LoadingInfoText));
        }
    }

    public string _loadingDetailedInfoText = "Szczegółowe info co sie teraz odpala";
    public string LoadingDetailedInfoText
    {
        get { return _loadingDetailedInfoText; }
        set
        {
            _loadingDetailedInfoText = value;
            OnPropertyChanged(nameof(LoadingDetailedInfoText));
        }
    }


    public StartViewModel(MainWindow mainWindow)
    {
        Instance = this;

        ConsoleViewModel = new();

        OpenCommand = new StartOpenCommand(this, mainWindow);
        CloseCommand = new StartCloseCommand(this);
        RefreshOpenedCommand = new StartRefreshOpenedCommand(this);
        SaveConsoleCommand = new StartSaveConsoleCommand(this);
        ClearConsoleCommand = new StartClearConsoleCommand(this);
        CancelCommand = new StartCancelCommand(this);

        Opened = new ObservableCollection<OpenedProcess>();

        //SimpleOpenedTest();

        UpdatePresetName();
    }

    public void LogLine(string text, ConsoleLineOption option = ConsoleLineOption.Normal)
    {
        ConsoleViewModel.ProcessCommandLine(text, option);
    }
    public static void Log(string text, ConsoleLineOption option = ConsoleLineOption.Normal)
    {
        Instance?.ConsoleViewModel.ProcessCommandLine(text, option);
    }

    public void SetLoadingText(string text)
    {
        LoadingInfoText = text;
    }
    public void SetDetailedLoadingText(string text)
    {
        LoadingDetailedInfoText = text;
    }

    public void UpdatePresetName(string name = "Empty preset")
    {
        PresetNameLabel = name;
    }

    public void SetStartButtonState(StartButtonState state)
    {
        OpenButtonName = state.ToString().ToUpper();
    }
    public bool IsStartButtonState(StartButtonState state)
    {
        return state.ToString().Equals(OpenButtonName.ToLower());
    }

    public void AddOpened(OpenedProcess openedProcess)
    {
        Opened.Add(openedProcess);
    }
    public void AddOpened(List<OpenedInstanceProcess> openedProcesses)
    {
        for (int i = 0; i < openedProcesses.Count; i++)
        {
            var current = openedProcesses[i];
            Opened.Add(current);
        }
    }
    public void RemoveOpened(OpenedProcess openedProcess)
    {
        Opened.Remove(openedProcess);
    }
    public void ClearOpened()
    {
        Opened.Clear();
    }
    public bool OpenedIsEmpty()
    {
        return Opened == null || Opened.Count == 0;
    }

    private void SimpleOpenedTest()
    {
        var opened = new OpenedProcess(this);
        for (int i = 0; i < 90; i++) //test
        {
            AddOpened(opened);
            opened.FastUpdate();
        }
        LogLine("Tekst aktualizacji odpalania/zamykania czy odswiezania procesow w presecie");

        Task.Run(async () =>
        {
            while (true)
            {
                LogLine("Tekst aktualizacji odpalania/zamykania czy odswiezania procesow w presecie");
                await Task.Delay(200);
            }
        });
    }
}
