using MultiOpener.Entities;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsAddNewGroupItemCommand : SettingsCommandBase
    {
        public SettingsAddNewGroupItemCommand(SettingsViewModel Settings) : base(Settings) { }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            Settings.Groups ??= new();

            /*string name = DialogBox.ShowInputField($"Name for new group:", $"Naming", Settings.IsGroupNameUnique);
            if (string.IsNullOrEmpty(name)) return;*/

            string name = "New Group";
            if (!Settings.IsGroupNameUnique(name)) return;

            Settings.Groups.Add(new LoadedGroupItem(name));
            Settings.CreateGroupFolder(name);
        }
    }
}
