using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsOnItemListClickCommand : SettingsCommandBase
{
    public SettingsOnItemListClickCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;
        if (parameter is not OpenItem item) return;
        if (item == Settings.CurrentChosen) return;

        Settings.SaveCurrentOpenCommand.Execute(null);
        Settings.CurrentChosen = item;
        Settings.UpdateLeftPanelInfo();
    }
}
