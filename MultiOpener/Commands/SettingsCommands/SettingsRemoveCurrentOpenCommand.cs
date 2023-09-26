using MultiOpener.Components.Controls;
using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRemoveCurrentOpenCommand : SettingsCommandBase
{
    public SettingsRemoveCurrentOpenCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;

        OpenItem item = (OpenItem)parameter;

        if (DialogBox.Show($"Are you sure that you want to delete {item.Name}?\nThe changes will not be able to be restored.", "Deleting process!", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        Settings.SetLeftPanelVisibility(false);
        Settings.RemoveItem(item);
        Settings.SetPresetAsNotSaved();
    }
}
