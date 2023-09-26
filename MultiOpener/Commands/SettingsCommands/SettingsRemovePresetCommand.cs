using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRemovePresetCommand : SettingsCommandBase
{
    public SettingsRemovePresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;
        if (Settings.OpenIsEmpty() && string.IsNullOrEmpty(Settings.PresetName)) return;

        if (parameter is LoadedPresetItem preset)
        {
            //TODO: 4 Zrobic zczytywanie czy preset jest pusty do tego zeby przy pustych nie wyswietlac popupy czy napewno chce usunac preset
            if (DialogBox.Show($"Are you sure that you want to delete {preset.Name!}?\nThe changes will not be able to be restored.", "Deleting preset!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if(preset.Name.Equals(Settings.PresetName, System.StringComparison.OrdinalIgnoreCase))
                    Settings.ClearOpenedPreset();

                Settings.RemovePreset(preset.Name);
            }

        }
    }
}
