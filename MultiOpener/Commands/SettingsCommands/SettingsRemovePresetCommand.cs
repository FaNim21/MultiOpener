using MultiOpener.ViewModels;
using System.Linq;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsRemovePresetCommand : SettingsCommandBase
    {
        public SettingsRemovePresetCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            if ((Settings.Opens == null || !Settings.Opens.Any()) && string.IsNullOrEmpty(Settings.PresetName))
                return;

            if(MessageBox.Show($"Are you sure that you want to delete {Settings.PresetName}?", "Deleting currently opened preset!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Settings.RemoveCurrentOpenPreset();
        }
    }
}
