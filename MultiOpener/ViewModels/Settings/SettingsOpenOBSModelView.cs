using MultiOpener.Entities.Open;
using System;

namespace MultiOpener.ViewModels.Settings;

public class SettingsOpenOBSModelView : OpenTypeViewModelBase
{
    public override Type ItemType { get; set; } = typeof(OpenOBS);


    private bool _connectWebSocket;
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

    private int _port;
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

    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            PresetIsNotSaved();
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }


    public SettingsOpenOBSModelView(SettingsViewModel settingsViewModel) : base(settingsViewModel) { }

    public override void UpdatePanelField(OpenItem currentChosen)
    {
        if (currentChosen is OpenOBS obs)
        {
            ConnectWebSocket = obs.ConnectWebSocket;
            Port = obs.Port;
            Password = obs.Password;
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
            obs.Port = Port;
            obs.Password = Password;
        }
    }

    public override void Clear()
    {
        base.Clear();

        ConnectWebSocket = true;
        Port = 4455;
        Password = "";
    }
}
