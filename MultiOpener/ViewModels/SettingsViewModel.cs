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
        public readonly string directoryPath = "C:\\Users\\Filip\\Desktop\\test\\" + _saveFileName;

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

        private string? _delatTimeField;
        public string? DelayTimeField
        {
            get { return _delatTimeField; }
            set
            {
                _delatTimeField = value;
                OnPropertyChanged(nameof(DelayTimeField));
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

            LoadButtonClick();
        }

        //ICommand
        private void LoadButtonClick()
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
            ApplicationPathField = currentChosen.PathExe;
            IsUsingDelayAfter = currentChosen.IsDelayAfter;
            DelayTimeField = currentChosen.DelayAfter.ToString();
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
