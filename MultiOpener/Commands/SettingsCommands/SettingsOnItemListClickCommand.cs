using MultiOpener.ListView;
using MultiOpener.ViewModels;
using System.Windows.Input;

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
            if ((OpenItem)parameter == Settings.CurrentChosen) return;

            Settings.SaveCurrentOpenCommand.Execute(null);

            Settings.CurrentChosen = (OpenItem)parameter;
            Settings.UpdateLeftPanelInfo();
        }
    }
}
