using MultiOpener.Commands;
using MultiOpener.Entities.Open;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MultiOpener.ViewModels.Settings;

public class SettingsOpenOBSModelView : OpenTypeViewModelBase
{
    public override Type ItemType { get; set; } = typeof(OpenOBS);


    private bool _connectWebSocket = true;
    public bool ConnectWebSocket
    {
        get => _connectWebSocket;
        set
        {
            PresetIsNotSaved();
            _connectWebSocket = value;
            OnPropertyChanged(nameof(ConnectWebSocket));
        }
    }

    private bool _closeOBSOnCloseMOProcess;
    public bool CloseOBSOnCloseMOProcess
    {
        get => _closeOBSOnCloseMOProcess;
        set
        {
            PresetIsNotSaved();
            _closeOBSOnCloseMOProcess = value;
            OnPropertyChanged(nameof(CloseOBSOnCloseMOProcess));
        }
    }

    private int _port = 4455;
    public int Port
    {
        get => _port;
        set
        {
            PresetIsNotSaved();
            _port = value;
            OnPropertyChanged(nameof(Port));
        }
    }

    private string? _password;
    public string? Password
    {
        get => _password;
        set
        {
            PresetIsNotSaved();
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    private string? _sceneCollection;
    public string? SceneCollection
    {
        get => _sceneCollection;
        set
        {
            PresetIsNotSaved();
            _sceneCollection = value;
            OnPropertyChanged(nameof(SceneCollection));
        }
    }

    private bool _startRecordingOnOpen;
    public bool StartRecordingOnOpen
    {
        get => _startRecordingOnOpen;
        set
        {
            PresetIsNotSaved();
            _startRecordingOnOpen = value;
            OnPropertyChanged(nameof(StartRecordingOnOpen));
        }
    }

    private bool _stopRecordingOnClose;
    public bool StopRecordingOnClose
    {
        get => _stopRecordingOnClose;
        set
        {
            PresetIsNotSaved();
            _stopRecordingOnClose = value;
            OnPropertyChanged(nameof(StopRecordingOnClose));
        }
    }

    public ObservableCollection<ProjectorOpen> Projectors { get; set; } = new();

    public ICommand AddProjectorCommand { get; set; }
    public ICommand RemoveProjectorCommand { get; set; }


    public SettingsOpenOBSModelView(SettingsViewModel settingsViewModel) : base(settingsViewModel)
    {
        AddProjectorCommand = new RelayCommand(AddProjector);
        RemoveProjectorCommand = new RelayCommand<ProjectorOpen>(RemoveProjector);
    }

    public void AddProjector()
    {
        Projectors.Add(new());
    }

    public void RemoveProjector(ProjectorOpen projector)
    {
        Projectors.Remove(projector);
    }

    public override void UpdatePanelField(OpenItem currentChosen)
    {
        if (currentChosen is OpenOBS obs)
        {
            ConnectWebSocket = obs.ConnectWebSocket;
            CloseOBSOnCloseMOProcess = obs.CloseOBSOnCloseMOProcess;
            Port = obs.Port;
            Password = obs.Password;
            SceneCollection = obs.SceneCollection;
            Projectors = new(obs.Projectors ?? new());
            StartRecordingOnOpen = obs.StartRecordingOnOpen;
            StopRecordingOnClose = obs.StopRecordingOnClose;
        }

        base.UpdatePanelField(currentChosen);
    }

    public override void SetOpenProperties(ref OpenItem open)
    {
        base.SetOpenProperties(ref open);
        open.Type = OpenType.OBS;

        if (open is OpenOBS obs)
        {
            obs.ConnectWebSocket = ConnectWebSocket;
            obs.CloseOBSOnCloseMOProcess = CloseOBSOnCloseMOProcess;
            obs.Port = Port;
            obs.Password = Password!;
            obs.SceneCollection = SceneCollection!;
            obs.Projectors = new(Projectors ?? new());
            obs.StartRecordingOnOpen = StartRecordingOnOpen;
            obs.StopRecordingOnClose = StopRecordingOnClose;
        }
    }

    public override void Clear()
    {
        base.Clear();

        ConnectWebSocket = true;
        CloseOBSOnCloseMOProcess = false;
        Port = 4455;
        Password = "";
        SceneCollection = "";
        Projectors.Clear();
        StartRecordingOnOpen = false;
        StopRecordingOnClose = false;
    }
}
