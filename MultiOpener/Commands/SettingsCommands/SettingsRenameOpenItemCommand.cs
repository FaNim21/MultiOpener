using MultiOpener.Components.Controls;
using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRenameOpenItemCommand : SettingsCommandBase
{
    public SettingsRenameOpenItemCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;
        if (parameter is not OpenItem open) return;

        string name = DialogBox.ShowInputField($"New name for '{open.Name}' process in you preset:", $"Renaming", Settings.IsOpenNameUnique);
        if (string.IsNullOrEmpty(name)) return;

        open.Name = name;
        Settings.SetPresetAsNotSaved();
    }
}
