using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRenameGroupCommand : SettingsCommandBase
{
    public SettingsRenameGroupCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;
        if (parameter is not LoadedGroupItem group) return;
        if (group.Name.Equals("Groupless")) return;

        string name = DialogBox.ShowInputField($"New name for '{group.Name}' group:", $"Renaming", Settings.IsPresetNameUnique);
        if (string.IsNullOrEmpty(name)) return;

        group.ChangeName(name);
        Settings.SetPresetAsNotSaved();
    }
}
