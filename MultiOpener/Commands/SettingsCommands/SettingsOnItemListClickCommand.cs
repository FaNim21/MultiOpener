using MultiOpener.ListView;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsOnItemListClickCommand : SettingsCommandBase
    {
        public SettingsOnItemListClickCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null || parameter == null) return;

            Settings.currentChosen = (OpenItem)parameter;
            Settings.UpdateLeftPanelInfo();
        }
    }
}
