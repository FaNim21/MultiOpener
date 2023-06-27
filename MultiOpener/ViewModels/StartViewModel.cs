using MultiOpener.Commands.StartCommands;
using MultiOpener.Items;
using MultiOpener.ViewModels.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public ConsoleViewModel ConsoleViewModel { get; set; }

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
            ConsoleViewModel = new();

            OpenCommand = new StartOpenCommand(this, mainWindow);
            CloseCommand = new StartCloseCommand(this);
            RefreshOpenedCommand = new StartRefreshOpenedCommand(this);

            Opened = new ObservableCollection<OpenedProcess>();

            //SimpleOpenedTest();

            /*for (int i = 0; i < 100; i++)
            {
                ProcessCommandLine("Siema error tutaj", ConsoleLineOption.Error);
                ProcessCommandLine("Siema tutaj");
                ProcessCommandLine("Siema warning tutaj", ConsoleLineOption.Warning);
                ProcessCommandLine("Siema error tutaj", ConsoleLineOption.Error);
                ProcessCommandLine("Siema tutaj");
                ProcessCommandLine("Siema warning tutaj", ConsoleLineOption.Warning);
                ProcessCommandLine("Siema error tutaj", ConsoleLineOption.Error);
                ProcessCommandLine("Siema tutaj");
                ProcessCommandLine("Siema warning tutaj", ConsoleLineOption.Warning);
            }*/

            Task task = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(200);
                    ProcessCommandLine("Siema tutaj");
                }
            });

            UpdatePresetName();
        }

        public void ProcessCommandLine(string text, ConsoleLineOption option = ConsoleLineOption.Normal)
        {
            ConsoleViewModel.ProcessCommandLine(text, option);
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

        private void SimpleOpenedTest()
        {
            var opened = new OpenedProcess(this);
            for (int i = 0; i < 90; i++) //test
            {
                AddOpened(opened);
                opened.FastUpdate();
            }
            UpdateText("Tekst aktualizacji odpalania/zamykania czy odswiezania procesow w presecie");
        }
    }
}
