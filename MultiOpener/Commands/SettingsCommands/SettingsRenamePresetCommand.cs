using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRenamePresetCommand : SettingsCommandBase
{
    public SettingsRenamePresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;
        if (parameter is not LoadedPresetItem preset) return;

        string name = DialogBox.ShowInputField($"New name for '{preset.Name}' preset:", $"Renaming", Settings.IsPresetNameUnique);
        if (string.IsNullOrEmpty(name)) return;

        preset.ChangeName(name);
        Settings.SetPresetAsNotSaved();
    }
}
