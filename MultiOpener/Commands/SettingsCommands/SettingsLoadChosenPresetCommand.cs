using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsLoadChosenPresetCommand : SettingsCommandBase
{
    public SettingsLoadChosenPresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;
        if (!Settings.IsCurrentPresetSaved) return;

        if (!StartViewModel.Instance!.OpenedIsEmpty()) return;
        if (parameter is not LoadedPresetItem presetItem) return;

        string path = presetItem.GetPath();
        if (string.IsNullOrEmpty(Settings.PresetName))
            Settings.LoadPreset(path);
        else
        {
            if (Settings.IsCurrentPresetSaved || DialogBox.Show($"Are you sure you want to load this preset?\nYou didn't save the previous one!", "Loading Preset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                Settings.LoadPreset(path);
        }
    }
}