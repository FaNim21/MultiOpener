using MultiOpener.Commands.SettingsCommands;
using MultiOpener.Entities.Open;
using System;
using System.Windows.Input;

namespace MultiOpener.ViewModels.Settings
{
    public abstract class OpenTypeViewModelBase : BaseViewModel
    {
        public abstract Type ItemType { get; set; }

        private string? _applicationPathField;
        public string? ApplicationPathField
        {
            get { return _applicationPathField; }
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
            get { return _delayAfterTimeField; }
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
            get { return _delayBeforeTimeField; }
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
            get { return _minimizeOnOpen; }
            set
            {
                PresetIsNotSaved();
                _minimizeOnOpen = value;
                OnPropertyChanged(nameof(MinimizeOnOpen));
            }
        }

        public ICommand SettingsSetDirectoryPathCommand { get; set; }

        protected SettingsViewModel settingsViewModel;

        private bool _wasSavedPreviously = false;

        protected OpenTypeViewModelBase(SettingsViewModel settingsViewModel)
        {
            this.settingsViewModel = settingsViewModel;
            _wasSavedPreviously = true;

            SettingsSetDirectoryPathCommand = new SettingsSetDirectoryPathCommand(this);
        }

        protected void PresetIsNotSaved()
        {
            if (_wasSavedPreviously)
                return;

            settingsViewModel.IsCurrentPresetSaved = false;
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

            if (string.IsNullOrEmpty(DelayBeforeTimeField))
                open.DelayBefore = 0;
            else
                open.DelayBefore = int.Parse(DelayBeforeTimeField ?? "0");

            if (string.IsNullOrEmpty(DelayAfterTimeField))
                open.DelayAfter = 0;
            else
                open.DelayAfter = int.Parse(DelayAfterTimeField ?? "0");

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
}
