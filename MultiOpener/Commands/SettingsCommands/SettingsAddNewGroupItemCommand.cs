using MultiOpener.Entities;
using MultiOpener.Utils;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsAddNewGroupItemCommand : SettingsCommandBase
{
    public SettingsAddNewGroupItemCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;

        Settings.Groups ??= new();

        string name = "New Group";
        name = Helper.GetUniqueName(name, name, Settings.IsGroupNameUnique);

        Settings.Groups.Add(new LoadedGroupItem(name));
        Settings.CreateGroupFolder(name);
    }
}
