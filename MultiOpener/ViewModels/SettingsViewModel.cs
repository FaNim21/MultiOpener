﻿using MultiOpener.ListView;
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
using Microsoft.Win32;

namespace MultiOpener.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
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
                ((MainWindow)Application.Current.MainWindow).MainViewModel.start.PresetNameLabel = output;
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

        public readonly string directoryPath;


        public SettingsViewModel()
        {
#if DEBUG
            directoryPath = "C:\\Users\\Filip\\Desktop\\Test\\Presets";
#else
            directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Presets" ?? "C:\\Presets";
#endif

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

            UpdatePresetsComboBox();
        }

        public void LoadStartUPPreset(string loadedPresetName)
        {
            if (File.Exists(directoryPath + "\\" + loadedPresetName + ".json"))
                LoadOpenList(loadedPresetName);
            else
                PresetName = string.Empty;
        }

        public void UpdatePresetsComboBox()
        {
            //TODO: Optimize it? by adding and removing or changing names in it
            Presets = new ObservableCollection<LoadedPresetItem>();
            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                Presets.Add(new LoadedPresetItem(fileName));
            }
        }

        public void LoadOpenList(string presetName)
        {
            if (Opens != null && !Opens.Any())
            {
                string presetToLoad = directoryPath + "\\" + presetName + ".json";

                if (!File.Exists(presetToLoad))
                    return;

                string text = File.ReadAllText(presetToLoad) ?? "";
                if (string.IsNullOrEmpty(text))
                    return;

                var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
                Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());

                PresetName = presetName;
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
