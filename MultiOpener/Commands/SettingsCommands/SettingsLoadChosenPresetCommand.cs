using MultiOpener.ViewModels;
using System.IO;
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

            if(Settings.CurrentLoadedChosen != null && !string.IsNullOrEmpty(Settings.CurrentLoadedChosen.Name))
            {
                if (Path.GetFileNameWithoutExtension(Settings.CurrentLoadedChosen.Name).Equals(Settings.PresetName))
                    return;

                if (string.IsNullOrEmpty(Settings.PresetName))
                    Settings.LoadOpenList(Settings.CurrentLoadedChosen.Name);
                else
                {
                    if(MessageBox.Show($"Are you sure you want to that load preset?\nYou might not saved previous!","Loading Preset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Settings.LoadOpenList(Settings.CurrentLoadedChosen.Name);
                    }
                }
            }
        }
    }
}
