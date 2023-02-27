using MultiOpener.ListView;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using System.Windows;
using MultiOpener.Commands.SettingsCommands;

namespace MultiOpener.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ObservableCollection<OpenItem> Opens { get; set; }

        public OpenItem? currentChosen;

        private const string _saveFileName = "settings.json";
        //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + _saveFileName ?? "C:\\" + _saveFileName;   //Tymczasowo
        public readonly string directoryPath = "C:\\Users\\Filip\\Desktop\\Test\\" + _saveFileName;

        private OpenType _chooseTypeBox;
        public OpenType ChooseTypeBox
        {
            get { return _chooseTypeBox; }
            set
            {
                if (_chooseTypeBox != value)
                {
                    //TODO: Tu bedzie sie zmieniac view dla lewego panelu
                    _chooseTypeBox = value;
                    OnPropertyChanged(nameof(ChooseTypeBox));
                }
            }
        }

        private bool _leftPanelGridVisibility;
        public bool LeftPanelGridVisibility
        {
            get { return _leftPanelGridVisibility; }
            set
            {
                _leftPanelGridVisibility = value;
                OnPropertyChanged(nameof(LeftPanelGridVisibility));
            }
        }

        private bool _isUsingDelayAfter;
        public bool IsUsingDelayAfter
        {
            get { return _isUsingDelayAfter; }
            set
            {
                _isUsingDelayAfter = value;
                OnPropertyChanged(nameof(IsUsingDelayAfter));
            }
        }

        private string? _addNameField;
        public string? AddNameField
        {
            get { return _addNameField; }
            set
            {
                _addNameField = value;
                OnPropertyChanged(nameof(AddNameField));
            }
        }

        private string? _applicationPathField;
        public string? ApplicationPathField
        {
            get { return _applicationPathField; }
            set
            {
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
                _delayBeforeTimeField = value;
                OnPropertyChanged(nameof(DelayBeforeTimeField));
            }
        }

        private string? _openNameLabel;
        public string? OpenNameLabel
        {
            get { return _openNameLabel; }
            set
            {
                _openNameLabel = value;
                OnPropertyChanged(nameof(OpenNameLabel));
            }
        }

        public ICommand AddNewOpenItemCommand { get; set; }
        public ICommand RemoveCurrentOpenCommand { get; set; }
        public ICommand SaveCurrentOpenCommand { get; set; }
        public ICommand SaveJsonCommand { get; set; }
        public ICommand InsertItemToOpenCommand { get; set; }
        public ICommand OnItemClickCommand { get; set; }


        public SettingsViewModel()
        {
            Opens = new ObservableCollection<OpenItem>();

            AddNewOpenItemCommand = new SettingsAddNewOpenItemCommand(this);
            RemoveCurrentOpenCommand = new SettingsRemoveCurrentOpenCommand(this);
            SaveCurrentOpenCommand = new SettingsSaveCurrentOpenCommand(this);
            SaveJsonCommand = new SaveJsonCommand(this);
            InsertItemToOpenCommand = new InsertItemToOpenCommand(this);
            OnItemClickCommand = new SettingsOnItemListClickCommand(this);

            LeftPanelGridVisibility = false;

            LoadOpenList();
        }

        //ICommand
        private void LoadOpenList()
        {
            //TODO: --FUTURE-- Wczytac wybrany json do listy
            if (Opens != null && !Opens.Any())
            {
                if (!File.Exists(directoryPath))
                    return;

                string text = File.ReadAllText(directoryPath) ?? "";
                if (string.IsNullOrEmpty(text))
                    return;

                var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
                Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());
            }
        }

        public void UpdateLeftPanelInfo()
        {
            if (currentChosen == null) return;

            if (!LeftPanelGridVisibility)
                LeftPanelGridVisibility = true;

            OpenNameLabel = currentChosen.Name;
            ChooseTypeBox = currentChosen.Type;
            ApplicationPathField = currentChosen.PathExe;
            DelayBeforeTimeField = currentChosen.DelayBefore.ToString();
            DelayAfterTimeField = currentChosen.DelayAfter.ToString();
        }

        public void AddItem(OpenItem item)
        {
            Opens.Add(item);
        }
        public void RemoveItem(OpenItem item)
        {
            Opens.Remove(item);
        }
    }
}
