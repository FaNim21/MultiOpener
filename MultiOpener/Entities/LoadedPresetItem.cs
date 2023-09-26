using MultiOpener.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace MultiOpener.Entities;

public class LoadedPresetItem : BaseViewModel
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

    public LoadedGroupItem? ParentGroup { get; set; }


    public LoadedPresetItem(string Name) => this.Name = Name;

    public void ChangeName(string name)
    {
        string jsonName = name + ".json";
        string path = GetPath();

        string directoryName = Path.GetDirectoryName(path)!;
        string newPath = Path.Combine(directoryName, jsonName);

        File.Move(path, newPath);
        Name = name;
    }

    public string GetPath()
    {
        return Path.Combine(ParentGroup!.GetPath(), Name + ".json");
    }
}

public class LoadedGroupItem : BaseViewModel
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
        string path = GetPath();

        string directoryName = Path.GetDirectoryName(path)!;
        string newPath = Path.Combine(directoryName, name);

        Directory.Move(path, newPath);
        Name = name;
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
}
