using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsAddNewOpenItemCommand : SettingsCommandBase
    {
        public SettingsAddNewOpenItemCommand(SettingsViewModel Settings) : base(Settings) { }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;
            if (string.IsNullOrEmpty(Settings.AddNameField))
                return;

            for (int i = 0; i < Settings.Opens.Count; i++)
            {
                var item = Settings.Opens[i];
                if (item.Name == Settings.AddNameField)
                    return;
            }

            var newOpen = new OpenItem(Settings.AddNameField);

            Settings.AddItem(newOpen);
            Settings.AddNameField = "";
            Settings.IsCurrentPresetSaved = false;
        }
    }
}
