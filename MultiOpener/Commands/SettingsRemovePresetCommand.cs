using MultiOpener.Commands.SettingsCommands;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands
{
    public class SettingsRemovePresetCommand : SettingsCommandBase
    {
        public SettingsRemovePresetCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if(MessageBox.Show("Are you sure?", "Deleting currently opened preset!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Usuwanie obecnie otwartego presetu JESZCZE NIE GOTOWE");
            }
        }
    }
}
