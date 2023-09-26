using MultiOpener.Components.Controls;
using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsClearCurrentOpenCommand : SettingsCommandBase
{
    public SettingsClearCurrentOpenCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;

        OpenItem item = (OpenItem)parameter;
        if (DialogBox.Show($"Are you sure you want to clear {item.Name}?", "", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
            if (Settings.SelectedOpenTypeViewModel != null && Settings.CurrentChosen == item)
                Settings.SelectedOpenTypeViewModel.Clear();
    }
}
