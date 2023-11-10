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

        var text = File.ReadAllText(preset.GetPath()) ?? string.Empty;
        if (string.IsNullOrEmpty(text) || text.Equals("[]") || DialogBox.Show($"Are you sure that you want to delete {preset.Name!}?\nThe changes will not be able to be restored.", "Deleting preset!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            SettingsViewModel.RemovePreset(preset);
    }
}
