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
using System.Windows;
using System.Windows.Input;
using MultiOpener.Utils.Attributes;

namespace MultiOpener.ViewModels;

/// <summary>
/// TODO: 10 Problemy z dodawaniem nowych typow procesu:
/// - Robienie oddzielnie: open, opened, modelView, openView, openedView wiec jest 5 roznych plikow na jeden OpenType
/// - Dodawanie kazdego view oddzielnie za kazdym razem w App.xaml na zasadzie podlaczania viewModelu do view
/// </summary>
public sealed class SettingsViewModel : BaseViewModel
{
    public MainViewModel MainViewModel { get; set; }

    public ObservableCollection<OpenItem> Opens { get; set; }
    public ObservableCollection<LoadedGroupItem> Groups { get; set; }

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
                var attribute = value.GetAttribute<OpenTypeAttribute>();
                if (attribute == null) return;

                if (!attribute.AllowMultiple)
                {
                    for (int i = 0; i < Opens.Count; i++)
                    {
                        var current = Opens[i];
                        if (current.GetType() == attribute.OpenType && CurrentChosen != current) return;
                    }
                }

                if (SelectedOpenTypeViewModel != null && !Consts.IsSwitchingBetweenOpensInSettings)
                    SetPresetAsNotSaved();

                var viewModelType = attribute.OpenTypeViewModel;
                var viewModelInstance = Activator.CreateInstance(viewModelType!, this) as OpenTypeViewModelBase;
                SelectedOpenTypeViewModel = viewModelInstance;

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

    private readonly string _directoryPath;


    public SettingsViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;

        _directoryPath = Path.Combine(Consts.AppdataPath, "Presets");
        CurrentLoadedChosenPath = string.Empty;

        Opens = new ObservableCollection<OpenItem>();
        Groups = new ObservableCollection<LoadedGroupItem>();

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

        UpdateLeftPanelVisibility(false);
        SetTreeWithGroupsAndPresets();
    }

    public override void OnEnable()
    {
        if (Groups != null) return;
        SetTreeWithGroupsAndPresets();
    }
    public override void OnDisable()
    {
        SaveCurrentOpenCommand.Execute(null);
        SaveGroupTree();
    }

    public void SetTreeWithGroupsAndPresets()
    {
        Groups.Clear();

        try
        {
            if (!Directory.Exists(_directoryPath))
                Directory.CreateDirectory(_directoryPath);
        }
        catch { }

        var folders = Directory.GetDirectories(_directoryPath).AsSpan();
        for (int i = 0; i < folders.Length; i++)
        {
            var folderName = Path.GetFileName(folders[i]);
            LoadedGroupItem? group = new(folderName, this);

            var files = Directory.GetFiles(Path.Combine(_directoryPath, folderName), "*.json", SearchOption.TopDirectoryOnly).AsSpan();
            for (int j = 0; j < files.Length; j++)
            {
                var fileName = Path.GetFileNameWithoutExtension(files[j]);
                var loadedPresetitem = new LoadedPresetItem(fileName);
                group.AddPreset(loadedPresetitem);
            }
            Groups.Add(group);
        }

        //Sprawdzanie presetów poza bazową grupą
        var grouplessFiles = Directory.GetFiles(_directoryPath, "*.json", SearchOption.TopDirectoryOnly).AsSpan();
        LoadedGroupItem? groupless = new("Groupless", this);
        for (int i = 0; i < grouplessFiles.Length; i++)
        {
            var fileName = Path.GetFileNameWithoutExtension(grouplessFiles[i]);
            var loadedPresetitem = new LoadedPresetItem(fileName);
            groupless.AddPreset(loadedPresetitem);
        }

        if (!groupless.IsEmpty()) Groups.Add(groupless);

        OnPropertyChanged(nameof(Groups));
        LoadGroupTree();
    }

    private void LoadGroupTree()
    {
        if (!File.Exists(Path.Combine(Consts.AppdataPath, "Groups.json"))) return;

        string text = File.ReadAllText(Path.Combine(Consts.AppdataPath, "Groups.json")) ?? string.Empty;
        var data = JsonSerializer.Deserialize<ObservableCollection<LoadedGroupItem>?>(text);
        if (data == null) return;

        for (int i = 0; i < Groups!.Count; i++)
        {
            var current = Groups[i];
            for (int j = 0; j < data.Count; j++)
            {
                if (!current.Name.Equals(data[j].Name, StringComparison.OrdinalIgnoreCase)) continue;

                current.IsExpanded = data[j].IsExpanded;
                current.Order = data[j].Order;
            }
        }
    }
    public void SaveGroupTree()
    {
        JsonSerializerOptions options = new() { WriteIndented = true, };
        var data = JsonSerializer.Serialize<object>(Groups!, options);
        File.WriteAllText(Path.Combine(Consts.AppdataPath, "Groups.json"), data);
    }

    public void LoadStartupPreset(string loadedPresetPath)
    {
        if (File.Exists(loadedPresetPath))
            LoadPreset(loadedPresetPath);
        else
        {
            if (!string.IsNullOrEmpty(loadedPresetPath))
                StartViewModel.Log($"Could not load previously opened: {Path.GetRelativePath(_directoryPath, loadedPresetPath)}");
            PresetName = string.Empty;
        }
    }
    public void LoadPreset(string presetPath)
    {
        if (string.IsNullOrEmpty(presetPath) || !File.Exists(presetPath)) return;

        string text = File.ReadAllText(presetPath) ?? string.Empty;
        if (string.IsNullOrEmpty(text)) return;

        var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
        Opens.Clear();
        Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());

        UpdateCurrentLoadedPreset(presetPath);
        UpdateLeftPanelVisibility(false);

        OnPropertyChanged(nameof(Opens));

        Properties.Settings.Default.LastOpenedPresetName = MainViewModel.settings.CurrentLoadedChosenPath;
        Properties.Settings.Default.Save();

        StartViewModel.Log($"Loaded {Path.GetRelativePath(_directoryPath, presetPath)}");
    }

    public void UpdateCurrentLoadedPreset(string presetPath)
    {
        CurrentLoadedChosenPath = presetPath;
        string loadedName = Helper.GetFileNameWithoutExtension(presetPath);
        PresetName = loadedName;
    }
    public void UpdateLeftPanelInfo()
    {
        if (CurrentChosen == null) return;

        UpdateLeftPanelVisibility(true);
        ChooseTypeBox = CurrentChosen.Type;
    }
    public void UpdateLeftPanelVisibility(bool visibility)
    {
        if (LeftPanelGridVisibility == visibility) return;

        LeftPanelGridVisibility = visibility;
    }

    public void RemovePreset(LoadedPresetItem preset)
    {
        var group = preset.ParentGroup!;
        group.RemovePreset(preset);
    }
    public void RemoveGroup(string name, bool recursive = false)
    {
        int n = Groups!.Count;
        for (int i = 0; i < n; i++)
        {
            var current = Groups[i];
            if (current.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                Groups.Remove(current);
                string path = current.GetPath();

                if (current.Name.Equals("Groupless", StringComparison.OrdinalIgnoreCase)) return;
                if (recursive) current.RemoveAllPresets();

                try
                {
                    Directory.Delete(path);
                }
                catch (Exception ex) { StartViewModel.Log($"Error: {ex.Message}", ConsoleLineOption.Warning); }
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

        CurrentLoadedChosenPath = string.Empty;
        PresetName = string.Empty;
        UpdateLeftPanelVisibility(false);
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

    public void CreateGroupFolder(string name)
    {
        Directory.CreateDirectory(Path.Combine(_directoryPath, name));
    }

    public bool OpenIsEmpty()
    {
        return Opens == null || Opens.Count == 0;
    }
}
