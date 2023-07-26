using MultiOpener.Components.Controls;
using MultiOpener.ViewModels;
using System.Linq;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands
{
    class SettingsLoadChosenPresetCommand : SettingsCommandBase
    {
        public SettingsLoadChosenPresetCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            if (((MainWindow)Application.Current.MainWindow).MainViewModel.start.Opened.Any())
                return;

            if (Settings.CurrentLoadedChosen != null && !string.IsNullOrEmpty(Settings.CurrentLoadedChosen.Name))
            {
                if (string.IsNullOrEmpty(Settings.PresetName))
                    Settings.LoadOpenList(Settings.CurrentLoadedChosen.Name);
                else
                {
                    if (Settings.IsCurrentPresetSaved || DialogBox.Show($"Are you sure you want to load this preset?\nYou didn't save the previous one!", "Loading Preset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        Settings.LoadOpenList(Settings.CurrentLoadedChosen.Name);
                }
            }
        }
    }
}
