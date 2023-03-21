using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsClearCurrentOpenCommand : SettingsCommandBase
    {
        public SettingsClearCurrentOpenCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null || Settings.SelectedOpenTypeViewModel == null) return;

            Settings.SelectedOpenTypeViewModel.Clear();
        }
    }
}
