using MultiOpener.Entities.Interfaces;
using MultiOpener.ViewModels;
using System;
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
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged(nameof(IsExpanded));
        }
    }

    public LoadedGroupItem? ParentGroup { get; set; }


    public LoadedPresetItem(string Name) => this.Name = Name;

    public void ChangeName(string name)
    {
        var jsonName = name + ".json";
        var path = GetPath();

        var directoryName = Path.GetDirectoryName(path)!;
        var newPath = Path.Combine(directoryName, jsonName);

        File.Move(path, newPath);
        Name = name;
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
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged(nameof(IsExpanded));
        }
    }

    private int _order = -1;
    public int Order
    {
        get => _order;
        set
        {
            if (_order == value) return;
            _order = value;
            OnPropertyChanged(nameof(Order));
            OnPropertyChanged(nameof(Order));
        }
    }

    [JsonIgnore]
    public ObservableCollection<LoadedPresetItem> Presets { get; set; } = new();


    public LoadedGroupItem(string Name) => this.Name = Name;

    public void AddPreset(LoadedPresetItem preset)
    {
        preset.ParentGroup = this;
        Presets.Add(preset);
    }
    public void RemovePreset(LoadedPresetItem preset, SettingsViewModel settings)
    {
        if (preset.Name.Equals(settings.PresetName, StringComparison.OrdinalIgnoreCase))
            settings.ClearOpenedPreset();

        Presets.Remove(preset);

        try
        {
            File.Delete(preset.GetPath());
        }
        catch { }
    }
    public void RemoveAllPresets(SettingsViewModel settings)
    {
        for (int i = 0; i < Presets.Count; i++)
            RemovePreset(Presets[i], settings);
    }

    public void ChangeName(string name)
    {
        var settings = ((MainWindow)Application.Current?.MainWindow!).MainViewModel.settings;
        var path = GetPath();
        var directoryName = Path.GetDirectoryName(path)!;
        var newPath = Path.Combine(directoryName, name);

        Directory.Move(path, newPath);
        Name = name;

        var preset = GetPreset(settings.PresetName!);
        if (preset != null) settings.UpdateCurrentLoadedPreset(preset.GetPath());
    }

    public LoadedPresetItem? GetPreset(string name)
    {
        for (int i = 0; i < Presets.Count; i++)
        {
            var preset = Presets[i];
            if (preset.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return preset;
        }
        return null;
    }
    public string GetPath()
    {
        string path;
        if (Name.Equals("Groupless", StringComparison.OrdinalIgnoreCase))
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
            if (current.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}