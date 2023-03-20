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

            //TODO: Ogarnac czemu nazwy sie zmieniaja, ale wczytuja sie zle Open jak sie save'uje ostatnio odpalony open i zarazem zmienia na nowy w liscie
            Settings.SaveCurrentOpenCommand.Execute(null);

            Settings.CurrentChosen = (OpenItem)parameter;
            Settings.UpdateLeftPanelInfo();
        }
    }
}
