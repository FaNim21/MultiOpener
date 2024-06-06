using MultiOpener.Entities;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.IO;

namespace MultiOpener.Commands.SettingsCommands;

class SettingsDuplicatePresetCommand : SettingsCommandBase
{
    public SettingsDuplicatePresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;

        if (parameter is not LoadedPresetItem preset) return;

        string oldPath = preset.GetPath();
        LoadedGroupItem group = preset.ParentGroup!;
        LoadedPresetItem newPreset = new(preset.Name);

        newPreset.Name = Helper.GetUniqueName(preset.Name, newPreset.Name, Settings.IsPresetNameUnique);
        group.AddPreset(newPreset);
        string newPath = newPreset.GetPath();

        File.Copy(oldPath, newPath);
    }
}
