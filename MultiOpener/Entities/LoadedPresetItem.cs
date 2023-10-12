using MultiOpener.Entities.Interfaces;
using MultiOpener.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;

namespace MultiOpener.Entities;

public class LoadedPresetItem : BaseViewModel, IRenameItem
{
    private string _name = "";
    public string Name
    {
        get { return _name; }
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }

    public LoadedGroupItem? ParentGroup { get; set; }


    public LoadedPresetItem(string Name) => this.Name = Name;

    public void ChangeName(string name)
    {
        SettingsViewModel settings = ((MainWindow)Application.Current.MainWindow).MainViewModel.settings;
        string jsonName = name + ".json";
        string path = GetPath();

        string directoryName = Path.GetDirectoryName(path)!;
        string newPath = Path.Combine(directoryName, jsonName);

        File.Move(path, newPath);
        Name = name;

        settings.SetPresetAsNotSaved();
    }

    public string GetPath()
    {
        return Path.Combine(ParentGroup!.GetPath(), Name + ".json");
    }
}

public class LoadedGroupItem : BaseViewModel, IRenameItem
{
    private string _name = "";
    public string Name
    {
        get { return _name; }
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get { return _isExpanded; }
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
    }

    private int _order = -1;
    public int Order
    {
        get { return _order; }
        set
        {
            if (_order != value)
            {
                _order = value;
                OnPropertyChanged(nameof(Order));
            }
        }
    }

    [JsonIgnore]
    public ObservableCollection<LoadedPresetItem> Presets { get; set; } = new();


    public LoadedGroupItem(string Name) => this.Name = Name;

    public void AddPreset(LoadedPresetItem preset)
    {
        Presets ??= new();

        preset.ParentGroup = this;
        Presets.Add(preset);
    }

    public void ChangeName(string name)
    {
        SettingsViewModel settings = ((MainWindow)Application.Current.MainWindow).MainViewModel.settings;
        string path = GetPath();
        string directoryName = Path.GetDirectoryName(path)!;
        string newPath = Path.Combine(directoryName, name);

        Directory.Move(path, newPath);
        Name = name;
        settings.SetPresetAsNotSaved();
    }

    public string GetPath()
    {
        string path;
        if (Name.Equals("Groupless", System.StringComparison.OrdinalIgnoreCase))
            path = Path.Combine(Consts.AppdataPath, "Presets");
        else
            path = Path.Combine(Consts.AppdataPath, "Presets", Name);
        return path;
    }
    public bool IsEmpty() => Presets == null || Presets.Count == 0;
    public bool Contains(string name)
    {
        for (int i = 0; i < Presets.Count; i++)
        {
            var current = Presets[i];
            if (current.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}