using MultiOpener.Commands;
using MultiOpener.Commands.SettingsCommands;
using MultiOpener.Entities;
using MultiOpener.Entities.Open;
using MultiOpener.Utils;
using MultiOpener.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    public MainViewModel MainViewModel { get; set; }

    public ObservableCollection<OpenItem> Opens { get; set; }
    public ObservableCollection<LoadedGroupItem>? Groups { get; set; }

    public OpenItem? CurrentChosen { get; set; }
    public string? CurrentLoadedChosenPath { get; set; }

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
            OnPropertyChanged(nameof(PresetName));
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

                if (SelectedOpenTypeViewModel != null && !Consts.IsSwitchingBetweenOpensInSettings)
                    SetPresetAsNotSaved();

                switch (value)
                {
                    case OpenType.Normal: SelectedOpenTypeViewModel = new SettingsOpenNormalModelView(this); break;
                    case OpenType.InstancesMultiMC: SelectedOpenTypeViewModel = new SettingsOpenInstancesModelView(this); break;
                }
                _chooseTypeBox = value;
                Consts.IsSwitchingBetweenOpensInSettings = false;
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

    private bool _isCurrentPresetSaved = true;
    public bool IsCurrentPresetSaved
    {
        get { return _isCurrentPresetSaved; }
        set
        {
            if (_isCurrentPresetSaved && !value)
                ((MainWindow)Application.Current.MainWindow).ChangePresetTitle("Preset*");
            else if (!IsCurrentPresetSaved && value)
                ((MainWindow)Application.Current.MainWindow).ChangePresetTitle("Preset");
            _isCurrentPresetSaved = value;
        }
    }

    public ICommand SaveCurrentOpenCommand { get; set; }
    public ICommand AddNewOpenItemCommand { get; set; }
    public ICommand RemoveCurrentOpenCommand { get; set; }
    public ICommand ClearCurrentOpenCommand { get; set; }

    public ICommand AddNewGroupItemCommand { get; set; }
    public ICommand RemoveGroupCommand { get; set; }

    public ICommand CreateNewPresetCommand { get; set; }
    public ICommand RemovePresetCommand { get; set; }
    public ICommand LoadChosenPresetCommand { get; set; }
    public ICommand DuplicatePresetCommand { get; set; }

    public ICommand SaveJsonCommand { get; set; }
    public ICommand RenameItemCommand { get; set; }

    public ICommand InsertItemToOpenCommand { get; set; }
    public ICommand OnItemClickCommand { get; set; }
    public ICommand OpenPresetsFolderCommand { get; set; }

    public ICommand RefreshTreeViewCommand { get; set; }

    public readonly string directoryPath;


    private string? _lolName = "asdas";
    public string? LolName
    {
        get { return _lolName; }
        set
        {
            _lolName = value;
            StartViewModel.Log($"LolName updated: {_lolName}");
            OnPropertyChanged(nameof(LolName));
        }
    }


    public SettingsViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;

        directoryPath = Path.Combine(Consts.AppdataPath, "Presets");

        Opens = new ObservableCollection<OpenItem>();

        AddNewOpenItemCommand = new SettingsAddNewOpenItemCommand(this);
        RemoveCurrentOpenCommand = new SettingsRemoveCurrentOpenCommand(this);
        ClearCurrentOpenCommand = new SettingsClearCurrentOpenCommand(this);
        SaveCurrentOpenCommand = new SettingsSaveCurrentOpenCommand(this);

        AddNewGroupItemCommand = new SettingsAddNewGroupItemCommand(this);
        RemoveGroupCommand = new SettingsRemoveGroupCommand(this);

        CreateNewPresetCommand = new SettingsCreateNewPresetCommand(this);
        RemovePresetCommand = new SettingsRemovePresetCommand(this);
        LoadChosenPresetCommand = new SettingsLoadChosenPresetCommand(this);
        DuplicatePresetCommand = new SettingsDuplicatePresetCommand(this);

        SaveJsonCommand = new SaveJsonCommand(this);
        RenameItemCommand = new SettingsRenameItemCommand(this);

        InsertItemToOpenCommand = new InsertItemToOpenCommand(this);
        OnItemClickCommand = new SettingsOnItemListClickCommand(this);
        OpenPresetsFolderCommand = new SettingsOpenPresetsFolderCommand(this);

        RefreshTreeViewCommand = new RelayCommand(SetTreeWithGroupsAndPresets);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        SetLeftPanelVisibility(false);
        SetTreeWithGroupsAndPresets();
    }

    public void SetTreeWithGroupsAndPresets()
    {
        //TODO: 1 Zrobic zapisywania i ladowanie IsExpanded i indexu dla grup TreeView

        Groups?.Clear();
        Groups = new ObservableCollection<LoadedGroupItem>();

        var folders = Directory.GetDirectories(directoryPath).AsSpan();
        for (int i = 0; i < folders.Length; i++)
        {
            var folderName = Path.GetFileName(folders[i]);
            LoadedGroupItem? group = new(folderName);

            var files = Directory.GetFiles(Path.Combine(directoryPath, folderName), "*.json", SearchOption.TopDirectoryOnly).AsSpan();
            for (int j = 0; j < files.Length; j++)
            {
                var fileName = Path.GetFileNameWithoutExtension(files[j]);
                var loadedPresetitem = new LoadedPresetItem(fileName);
                group.AddPreset(loadedPresetitem);
            }

            Groups.Add(group);
        }

        //Sprawdzanie presetów poza bazową grupą
        var grouplessFiles = Directory.GetFiles(Path.Combine(directoryPath), "*.json", SearchOption.TopDirectoryOnly).AsSpan();
        LoadedGroupItem? groupless = new("Groupless");
        for (int i = 0; i < grouplessFiles.Length; i++)
        {
            var fileName = Path.GetFileNameWithoutExtension(grouplessFiles[i]);
            var loadedPresetitem = new LoadedPresetItem(fileName);
            groupless.AddPreset(loadedPresetitem);
        }

        if (!groupless.IsEmpty())
            Groups.Add(groupless);

        OnPropertyChanged(nameof(Groups));
    }

    public void LoadStartupPreset(string loadedPresetPath)
    {
        if (File.Exists(loadedPresetPath))
            LoadPreset(loadedPresetPath);
        else
            PresetName = string.Empty;
    }
    public void LoadPreset(string presetPath)
    {
        if (!string.IsNullOrEmpty(presetPath) && File.Exists(presetPath))
        {
            string text = File.ReadAllText(presetPath) ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return;

            var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
            Opens.Clear();
            Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());

            string loadedName = Helper.GetFileNameWithoutExtension(presetPath);
            StartViewModel.Log($"Loaded {loadedName}");
            PresetName = loadedName;

            CurrentLoadedChosenPath = presetPath;
            SetLeftPanelVisibility(false);

            OnPropertyChanged(nameof(CurrentLoadedChosenPath));
            OnPropertyChanged(nameof(Opens));
        }
    }

    public void CreateGroupFolder(string name)
    {
        Directory.CreateDirectory(Path.Combine(directoryPath, name));
    }

    public void SetLeftPanelVisibility(bool visibility)
    {
        if (LeftPanelGridVisibility == visibility) return;

        LeftPanelGridVisibility = visibility;
    }

    public void UpdateLeftPanelInfo()
    {
        if (CurrentChosen == null) return;

        SetLeftPanelVisibility(true);
        ChooseTypeBox = CurrentChosen.Type;
    }

    public void RemovePreset(string name)
    {
        int n = Groups!.Count;
        for (int i = 0; i < n; i++)
        {
            var currentGroup = Groups[i];
            int k = currentGroup.Presets.Count;
            for (int j = 0; j < k; j++)
            {
                var current = currentGroup.Presets![j];
                if (current.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    currentGroup.Presets.Remove(current);
                    string path = current.GetPath();
                    if (File.Exists(path))
                        File.Delete(path);
                    return;
                }
            }
        }
    }
    public void RemoveGroup(string name)
    {
        int n = Groups!.Count;
        for (int i = 0; i < n; i++)
        {
            var current = Groups[i];
            if (current.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                Groups.Remove(current);
                return;
            }
        }
    }

    public LoadedGroupItem? GetGroupByName(string name)
    {
        int n = Groups!.Count;
        for (int i = 0; i < n; i++)
        {
            var current = Groups[i];
            if (current.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return current;
        }

        return null;
    }

    public void ClearOpenedPreset()
    {
        Opens.Clear();
        OnPropertyChanged(nameof(Opens));

        PresetName = string.Empty;

        SetLeftPanelVisibility(false);
    }

    public void AddItem(OpenItem item)
    {
        Opens.Add(item);
    }
    public void RemoveItem(OpenItem item)
    {
        Opens.Remove(item);
    }

    public bool IsPresetNameUnique(string name)
    {
        for (int i = 0; i < Groups!.Count; i++)
        {
            LoadedGroupItem currentGroup = Groups![i];
            int n = currentGroup.Presets!.Count;
            for (int j = 0; j < n; j++)
            {
                LoadedPresetItem currentPreset = currentGroup.Presets[j];
                if (currentPreset.Name!.Equals(name, StringComparison.OrdinalIgnoreCase)) return false;
            }
        }

        return true;
    }
    public bool IsOpenNameUnique(string name)
    {
        for (int i = 0; i < Opens.Count; i++)
        {
            var current = Opens[i];
            if (current.Name!.Equals(name, StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }
    public bool IsGroupNameUnique(string name)
    {
        if (name.Equals("groupless", StringComparison.OrdinalIgnoreCase)) return false;

        for (int i = 0; i < Groups!.Count; i++)
        {
            var current = Groups[i];
            if (current.Name!.Equals(name, StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }

    public void SetPresetAsNotSaved()
    {
        IsCurrentPresetSaved = false;
    }

    public Type GetSelectedOpenType()
    {
        if (SelectedOpenTypeViewModel != null)
            return SelectedOpenTypeViewModel.ItemType;

        return typeof(OpenItem);
    }

    public OpenItem CreateNewOpen(Type selectedOpenType, string name)
    {
        return selectedOpenType.Name switch
        {
            nameof(OpenInstance) => new OpenInstance(name),
            //New classes here
            _ => new OpenItem(name),
        };
    }

    public bool OpenIsEmpty()
    {
        return Opens == null || Opens.Count == 0;
    }

    [GeneratedRegex("[<>:\"/\\|?*]")]
    public static partial Regex SpecialCharacterPattern();

}
