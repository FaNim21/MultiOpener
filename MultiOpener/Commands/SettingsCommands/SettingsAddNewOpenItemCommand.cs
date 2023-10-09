using MultiOpener.Entities.Open;
using MultiOpener.Utils;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsAddNewOpenItemCommand : SettingsCommandBase
{
    public SettingsAddNewOpenItemCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;

        string name = "New Open";
        name = Helper.GetUniqueName(name, name, Settings.IsOpenNameUnique);

        var newOpen = new OpenItem(name);
        Settings.AddItem(newOpen);
        Settings.SetPresetAsNotSaved();
    }
}
