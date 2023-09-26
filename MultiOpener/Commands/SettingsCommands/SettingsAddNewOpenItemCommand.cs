using MultiOpener.Components.Controls;
using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsAddNewOpenItemCommand : SettingsCommandBase
{
    public SettingsAddNewOpenItemCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;

        string name = DialogBox.ShowInputField($"Name for new 'Open' process in you preset:", $"Naming", Settings.IsOpenNameUnique);
        if (string.IsNullOrEmpty(name)) return;

        var newOpen = new OpenItem(name);
        Settings.AddItem(newOpen);
        Settings.SetPresetAsNotSaved();
    }
}
