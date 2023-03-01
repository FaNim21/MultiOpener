using MultiOpener.ListView;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using MultiOpener.Commands.SettingsCommands;
using MultiOpener.ViewModels.Settings;

namespace MultiOpener.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ObservableCollection<OpenItem> Opens { get; set; }

        public OpenItem? currentChosen;

        private const string _saveFileName = "settings.json";
        //public readonly string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + _saveFileName ?? "C:\\" + _saveFileName;   //Tymczasowo
        public readonly string directoryPath = "C:\\Users\\Filip\\Desktop\\Test\\" + _saveFileName;

        private OpenTypeViewModelBase? _selectedOpenTypeViewModel;
        public OpenTypeViewModelBase? SelectedOpenTypeViewModel
        {
            get { return _selectedOpenTypeViewModel; }
            set
            {
                _selectedOpenTypeViewModel = value;
                OnPropertyChanged(nameof(SelectedOpenTypeViewModel));
            }
        }

        private OpenType _chooseTypeBox;
        public OpenType ChooseTypeBox
        {
            get { return _chooseTypeBox; }
            set
            {
                if (_chooseTypeBox != value && currentChosen != null)
                {
                    int index = -1;
                    for (int i = 0; i < Opens.Count; i++)
                    {
                        if (currentChosen.Name.Equals(Opens[i].Name))
                        {
                            index = i;
                            break;
                        }
                    }
                    var openItem = Opens[index];

                    switch (value)
                    {
                        //TODO: Naprawic problem ze zlym przezucaniem zmiennych itp itd
                        case OpenType.Normal:
                            Opens[index] = new OpenItem(openItem.Name, openItem.PathExe, openItem.DelayBefore, openItem.DelayAfter, openItem.Type);
                            SelectedOpenTypeViewModel = new SettingsOpenNormalModelView();
                            break;
                        case OpenType.InstancesMultiMC:
                            Opens[index] = new OpenInstance(openItem.Name, openItem.PathExe, openItem.DelayBefore, openItem.DelayAfter, openItem.Type);
                            SelectedOpenTypeViewModel = new SettingsOpenInstancesModelView();
                            break;
                    }
                    SelectedOpenTypeViewModel?.UpdatePanelField(currentChosen);

                    //TODO: Tu bedzie sie zmieniac view dla lewego panelu
                    //ZROBIC bazowy view model dla lewego panelu z wyborem typu Open i dac z niego typowe dane typu delay before i after i applicationPath
                    //i z niego dziedziczyc kolejne typy wspieranych Open
                    //a przelaczanie itp itd zrobic tak samo jak jest w MainViewModel tylko ze wtedy ten obecny view model bedzie bazowym dla typow edycji Open
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

            ChooseTypeBox = currentChosen.Type;
            OpenNameLabel = currentChosen.Name;

            //if (SelectedOpenTypeViewModel == null) return;

            //SelectedOpenTypeViewModel.UpdatePanelField(currentChosen);
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
