using MultiOpener.Entities;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.IO;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsCreateNewPresetCommand : SettingsCommandBase
{
    public SettingsCreateNewPresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;

        if (parameter is not LoadedGroupItem group) return;

        string name = "New Preset";
        name = Helper.GetUniqueName(name, name, Settings.IsPresetNameUnique);

        LoadedPresetItem item = new(name);
        group.AddPreset(item);
        group.IsExpanded = true;
        File.WriteAllText(item.GetPath(), "[]");
    }
}
