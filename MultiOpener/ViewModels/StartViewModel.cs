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
        public ICommand OpenStructureCommand { get; set; }

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

        private string? _panelInteractionText;
        public string? PanelInteractionText
        {
            get { return _panelInteractionText; }
            set
            {
                _panelInteractionText = value;
                OnPropertyChanged(nameof(PanelInteractionText));
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


        public StartViewModel(MainWindow mainWindow)
        {
            OpenCommand = new StartOpenCommand(this, mainWindow);
            CloseCommand = new StartCloseCommand(this);
            RefreshOpenedCommand = new StartRefreshOpenedCommand(this);
            OpenStructureCommand = new StartOpenStructureCommand(this);

            Opened = new ObservableCollection<OpenedProcess>();

            UpdatePresetName();
        }

        public void UpdatePresetName(string name = "Empty preset")
        {
            PresetNameLabel = name;
        }

        public void UpdateText(string content)
        {
            PanelInteractionText = content;
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
        public bool OpenedIsEmpty()
        {
            return Opened == null || Opened.Count == 0;
        }
    }
}
