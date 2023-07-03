using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class InsertItemToOpenCommand : SettingsCommandBase
    {
        public InsertItemToOpenCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (parameter == null || Settings == null) return;

            var elements = (object[])parameter;

            OpenItem insertedItem = (OpenItem)elements[0];
            OpenItem targetedItem = (OpenItem)elements[1];

            if (insertedItem == targetedItem)
                return;

            int oldIndex = Settings.Opens.IndexOf(insertedItem);
            int nextIndex = Settings.Opens.IndexOf(targetedItem);

            if (oldIndex != -1 && nextIndex != -1)
                Settings.Opens.Move(oldIndex, nextIndex);
        }
    }
}
