using MultiOpener.Components.Controls;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsCreateNewPresetCommand : SettingsCommandBase
    {
        public SettingsCreateNewPresetCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            if (DialogBox.Show($"Are you sure you want to create empty preset?\nUnsaved changed will be lost!", $"Creating empty preset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Settings.CreateEmptyPreset();
        }
    }
}
