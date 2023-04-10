using MultiOpener.Commands.StartCommands;
using MultiOpener.Items;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public OpenedProcess? MultiMC { get; set; }
        public ObservableCollection<OpenedProcess> Opened { get; set; }

        public ICommand OpenCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand RefreshOpenedCommand { get; set; }

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

        public StartViewModel()
        {
            OpenCommand = new StartOpenCommand(this);
            CloseCommand = new StartCloseCommand(this);
            RefreshOpenedCommand = new StartRefreshOpenedCommand(this);

            Opened = new ObservableCollection<OpenedProcess>();

            UpdatePresetName();
        }

        public void UpdatePresetName(string name = "Empty preset")
        {
            PresetNameLabel = name;
        }

        public void AddOpened(OpenedProcess openedProcess)
        {
            Opened.Add(openedProcess);
        }
        public void RemoveOpened(OpenedProcess openedProcess)
        {
            Opened.Remove(openedProcess);
        }
        public void ClearOpened()
        {
            Opened.Clear();
        }
    }
}
