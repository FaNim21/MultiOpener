using MultiOpener.Components.Controls;
using MultiOpener.Entities;
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

        string name = DialogBox.ShowInputField($"Name for new preset in group '{group.Name}':", $"Naming", Settings.IsPresetNameUnique);
        if (string.IsNullOrEmpty(name)) return;

        LoadedPresetItem item = new(name);
        group.AddPreset(item);
        File.WriteAllText(item.GetPath(), "[]");
    }
}
