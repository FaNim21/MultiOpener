using MultiOpener.ListView;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using MultiOpener.Commands.SettingsCommands;
using MultiOpener.ViewModels.Settings;
using System;
using System.Reflection;
using System.Windows;
using MultiOpener.Items;
using MultiOpener.Commands;

namespace MultiOpener.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ObservableCollection<OpenItem> Opens { get; set; }
        public ObservableCollection<LoadedPresetItem> Presets { get; set; } = new ObservableCollection<LoadedPresetItem>() { new("set"), new("elo"), new("xdd")};

        private string? _presetName;
        public string? PresetName
        {
            get { return _presetName; }
            set
            {
                _presetName = value;
                ((MainWindow)Application.Current.MainWindow).MainViewModel.start.PresetNameLabel = "Current preset: " + value;
            }
        }

        public OpenItem? CurrentChosen { get; set; }

        //public readonly string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Presets" ?? "C:\\Presets";   //Tymczasowo
        public readonly string directoryPath = "C:\\Users\\Filip\\Desktop\\Test\\Presets";

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
                if ((_chooseTypeBox != value && CurrentChosen != null) || SelectedOpenTypeViewModel == null)
                {
                    switch (value)
                    {
                        case OpenType.Normal: SelectedOpenTypeViewModel = new SettingsOpenNormalModelView(); break;
                        case OpenType.InstancesMultiMC: SelectedOpenTypeViewModel = new SettingsOpenInstancesModelView(); break;
                    }
                    _chooseTypeBox = value;
                    OnPropertyChanged(nameof(ChooseTypeBox));
                }
                SelectedOpenTypeViewModel?.UpdatePanelField(CurrentChosen);
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

        private string? _saveNameField;
        public string? SaveNameField
        {
            get { return _saveNameField; }
            set
            {
                _saveNameField = value;
                OnPropertyChanged(nameof(SaveNameField));
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
        public ICommand LoadChosenPresetCommand { get; set; }
        public ICommand OpenPresetsFolderCommand { get; set; }
        public ICommand RemovePresetCommand { get; set; }


        public SettingsViewModel()
        {
            Opens = new ObservableCollection<OpenItem>();

            AddNewOpenItemCommand = new SettingsAddNewOpenItemCommand(this);
            RemoveCurrentOpenCommand = new SettingsRemoveCurrentOpenCommand(this);
            SaveCurrentOpenCommand = new SettingsSaveCurrentOpenCommand(this);
            SaveJsonCommand = new SaveJsonCommand(this);
            InsertItemToOpenCommand = new InsertItemToOpenCommand(this);
            OnItemClickCommand = new SettingsOnItemListClickCommand(this);
            LoadChosenPresetCommand = new SettingsLoadChosenPresetCommand(this);
            OpenPresetsFolderCommand = new SettingsOpenPresetsFolderCommand(this);
            RemovePresetCommand = new SettingsRemovePresetCommand(this);

            LeftPanelGridVisibility = false;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            if (!string.IsNullOrEmpty(PresetName))
            {
                //TODO: ladowac zapisany ostatnio preset
            }
        }

        //ICommand
        private void LoadOpenList()
        {
            //TODO: Ladowac z ostatniej sesji odpalony preset
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
            if (CurrentChosen == null) return;

            if (!LeftPanelGridVisibility)
                LeftPanelGridVisibility = true;

            ChooseTypeBox = CurrentChosen.Type;
            OpenNameLabel = CurrentChosen.Name;
        }

        public void AddItem(OpenItem item)
        {
            Opens.Add(item);
        }
        public void RemoveItem(OpenItem item)
        {
            Opens.Remove(item);
        }

        public Type GetSelectedOpenType()
        {
            if (SelectedOpenTypeViewModel != null)
            {
                if (SelectedOpenTypeViewModel.GetType() == typeof(SettingsOpenInstancesModelView))
                    return typeof(OpenInstance);
            }

            return typeof(OpenItem);
        }
    }
}
