using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.ViewModels;
using System.IO;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRemovePresetCommand : SettingsCommandBase
{
    public SettingsRemovePresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;
        if (parameter is not LoadedPresetItem preset) return;

        string text = File.ReadAllText(preset.GetPath()) ?? string.Empty;
        if (string.IsNullOrEmpty(text) || text.Equals("[]"))
        {
            Remove(preset);
            return;
        }

        if (DialogBox.Show($"Are you sure that you want to delete {preset.Name!}?\nThe changes will not be able to be restored.", "Deleting preset!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            Remove(preset);
    }

    private void Remove(LoadedPresetItem preset)
    {
        if (preset.Name.Equals(Settings!.PresetName, System.StringComparison.OrdinalIgnoreCase))
            Settings.ClearOpenedPreset();
        Settings.RemovePreset(preset.Name);
    }
}
