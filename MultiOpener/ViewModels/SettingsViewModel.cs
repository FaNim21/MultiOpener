using MultiOpener.Commands.SettingsCommands;
using MultiOpener.Items;
using MultiOpener.Utils;
using MultiOpener.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public MainViewModel MainViewModel { get; set; }

        public ObservableCollection<OpenItem> Opens { get; set; }
        public ObservableCollection<LoadedPresetItem> Presets { get; set; }

        public OpenItem? CurrentChosen { get; set; }
        public LoadedPresetItem? CurrentLoadedChosen { get; set; }

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

        private string? _presetName;
        public string? PresetName
        {
            get { return _presetName; }
            set
            {
                _presetName = value;

                string output = "Empty preset";
                if (!string.IsNullOrEmpty(value))
                    output = "Current preset: " + value;

                MainViewModel.start.UpdatePresetName(output);
            }
        }

        private OpenType _chooseTypeBox;
        public OpenType ChooseTypeBox
        {
            get { return _chooseTypeBox; }
            set
            {
                if (CurrentChosen != null || SelectedOpenTypeViewModel == null)
                {
                    //Blocking to make more than one OpenInstance
                    if (value == OpenType.InstancesMultiMC)
                    {
                        for (int i = 0; i < Opens.Count; i++)
                        {
                            var current = Opens[i];
                            if (current.GetType().IsEquivalentTo(typeof(OpenInstance)) && CurrentChosen != current)
                                return;
                        }
                    }

                    switch (value)
                    {
                        case OpenType.Normal: SelectedOpenTypeViewModel = new SettingsOpenNormalModelView(); break;
                        case OpenType.InstancesMultiMC: SelectedOpenTypeViewModel = new SettingsOpenInstancesModelView(); break;
                    }
                    _chooseTypeBox = value;
                    OnPropertyChanged(nameof(ChooseTypeBox));
                }
                SelectedOpenTypeViewModel?.UpdatePanelField(CurrentChosen!);
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
        public ICommand CreateNewPresetCommand { get; set; }
        public ICommand ClearCurrentOpenCommand { get; set; }

        public readonly string directoryPath;


        public SettingsViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;

            directoryPath = Path.Combine(Consts.AppdataPath, "Presets");

            Opens = new ObservableCollection<OpenItem>();
            Presets = new ObservableCollection<LoadedPresetItem>();

            AddNewOpenItemCommand = new SettingsAddNewOpenItemCommand(this);
            RemoveCurrentOpenCommand = new SettingsRemoveCurrentOpenCommand(this);
            SaveCurrentOpenCommand = new SettingsSaveCurrentOpenCommand(this);
            SaveJsonCommand = new SaveJsonCommand(this);
            InsertItemToOpenCommand = new InsertItemToOpenCommand(this);
            OnItemClickCommand = new SettingsOnItemListClickCommand(this);
            LoadChosenPresetCommand = new SettingsLoadChosenPresetCommand(this);
            OpenPresetsFolderCommand = new SettingsOpenPresetsFolderCommand(this);
            RemovePresetCommand = new SettingsRemovePresetCommand(this);
            CreateNewPresetCommand = new SettingsCreateNewPresetCommand(this);
            ClearCurrentOpenCommand = new SettingsClearCurrentOpenCommand(this);

            LeftPanelGridVisibility = false;

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            UpdatePresetsComboBox();
        }

        public void LoadStartUPPreset(string loadedPresetName)
        {
            if (File.Exists(directoryPath + "\\" + loadedPresetName + ".json"))
                LoadOpenList(loadedPresetName + ".json");
            else
                PresetName = string.Empty;
        }

        public void UpdatePresetsComboBox(string selected = "")
        {
            Presets = new ObservableCollection<LoadedPresetItem>();
            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly).AsSpan();
            for (int i = 0; i < files.Length; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                var loadedPresetitem = new LoadedPresetItem(fileName);
                Presets.Add(loadedPresetitem);

                if (!string.IsNullOrEmpty(selected) && fileName.Equals(selected))
                {
                    CurrentLoadedChosen = loadedPresetitem;
                    OnPropertyChanged(nameof(CurrentLoadedChosen));
                }
            }

            OnPropertyChanged(nameof(Presets));
        }

        public void LoadOpenList(string presetName)
        {
            if (!string.IsNullOrEmpty(presetName))
            {
                string presetToLoad = directoryPath + "\\" + presetName;

                if (!File.Exists(presetToLoad))
                    return;

                string text = File.ReadAllText(presetToLoad) ?? "";
                if (string.IsNullOrEmpty(text))
                    return;

                var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
                Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());

                string loadedName = Helper.GetFileNameWithoutExtension(presetName);
                PresetName = loadedName;
                SaveNameField = loadedName;

                LeftPanelGridVisibility = false;
                OnPropertyChanged(nameof(Opens));
            }
        }

        public void RemoveCurrentOpenPreset()
        {
            if (!string.IsNullOrEmpty(PresetName))
            {
                var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    string name = Helper.GetFileNameWithoutExtension(files[i]);
                    if (name.ToLower().Equals(PresetName.ToLower()))
                        File.Delete(files[i]);
                }
            }

            CreateEmptyPreset();
            UpdatePresetsComboBox();
        }

        public void CreateEmptyPreset()
        {
            Opens = new ObservableCollection<OpenItem>();

            PresetName = string.Empty;
            SaveNameField = string.Empty;

            LeftPanelGridVisibility = false;
            OnPropertyChanged(nameof(Opens));
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
                return SelectedOpenTypeViewModel.ItemType;

            return typeof(OpenItem);
        }

        public bool OpenIsEmpty()
        {
            return Opens == null || Opens.Count == 0;
        }
    }
}
