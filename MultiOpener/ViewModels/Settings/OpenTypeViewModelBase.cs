using MultiOpener.Commands.SettingsCommands;
using MultiOpener.Entities.Open;
using System;
using System.Windows.Input;

namespace MultiOpener.ViewModels.Settings;

public abstract class OpenTypeViewModelBase : BaseViewModel
{
    public abstract Type ItemType { get; set; }

    private string? _applicationPathField;
    public string? ApplicationPathField
    {
        get => _applicationPathField;
        set
        {
            PresetIsNotSaved();
            _applicationPathField = value;
            OnPropertyChanged(nameof(ApplicationPathField));
        }
    }

    private string? _delayAfterTimeField;
    public string? DelayAfterTimeField
    {
        get => _delayAfterTimeField;
        set
        {
            PresetIsNotSaved();
            _delayAfterTimeField = value;
            OnPropertyChanged(nameof(DelayAfterTimeField));
        }
    }

    private string? _delayBeforeTimeField;
    public string? DelayBeforeTimeField
    {
        get => _delayBeforeTimeField;
        set
        {
            PresetIsNotSaved();
            _delayBeforeTimeField = value;
            OnPropertyChanged(nameof(DelayBeforeTimeField));
        }
    }

    private bool? _minimizeOnOpen;
    public bool? MinimizeOnOpen
    {
        get => _minimizeOnOpen;
        set
        {
            PresetIsNotSaved();
            _minimizeOnOpen = value;
            OnPropertyChanged(nameof(MinimizeOnOpen));
        }
    }

    public ICommand SettingsSetDirectoryPathCommand { get; set; }

    private readonly SettingsViewModel _settingsViewModel;
    private bool _wasSavedPreviously;


    protected OpenTypeViewModelBase(SettingsViewModel settingsViewModel)
    {
        _settingsViewModel = settingsViewModel;
        _wasSavedPreviously = true;

        SettingsSetDirectoryPathCommand = new SettingsSetDirectoryPathCommand(this);
    }

    protected void PresetIsNotSaved()
    {
        if (_wasSavedPreviously)
            return;

        _settingsViewModel.SetPresetAsNotSaved();
    }

    public virtual void UpdatePanelField(OpenItem currentChosen)
    {
        ApplicationPathField = currentChosen.PathExe;
        DelayBeforeTimeField = currentChosen.DelayBefore.ToString();
        DelayAfterTimeField = currentChosen.DelayAfter.ToString();
        MinimizeOnOpen = currentChosen.MinimizeOnOpen;

        _wasSavedPreviously = false;
    }

    public virtual void SetOpenProperties(ref OpenItem open)
    {
        open.PathExe = ApplicationPathField ?? "";
        open.Type = OpenType.Normal;

        open.DelayBefore = string.IsNullOrEmpty(DelayBeforeTimeField) ? 0 : int.Parse(DelayBeforeTimeField ?? "0");
        open.DelayAfter = string.IsNullOrEmpty(DelayAfterTimeField) ? 0 : int.Parse(DelayAfterTimeField ?? "0");

        open.MinimizeOnOpen = MinimizeOnOpen ?? false;
    }

    public virtual void Clear()
    {
        ApplicationPathField = "";
        DelayBeforeTimeField = "0";
        DelayAfterTimeField = "0";
        MinimizeOnOpen = false;
    }
}