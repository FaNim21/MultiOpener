using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.ViewModels;
using System.IO;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRemoveGroupCommand : SettingsCommandBase
{
    public SettingsRemoveGroupCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;
        if (parameter is not LoadedGroupItem group) return;

        if (group.Name.Equals("Groupless", System.StringComparison.OrdinalIgnoreCase))
        {
            if (DialogBox.Show($"Are you sure you want to delete all presets {group.Name}", "Removing Groupless presets", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                group.RemoveAllPresets();
            return;
        }

        if (group.IsEmpty())
        {
            string path = group.GetPath();
            if (!Path.GetDirectoryName(path)!.Equals("Presets", System.StringComparison.OrdinalIgnoreCase))
                Settings.RemoveGroup(group.Name);
        }
        else
        {
            MessageBoxResult result = DialogBox.Show($"You are trying to remove {group.Name} with option:\nYES - Removing only that folder and move all presets to Groupless\nNO - Removing whole folder with all presets in it", "Removing Group", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                LoadedGroupItem? groupless = Settings.GetGroupByName("Groupless");
                if (groupless == null)
                {
                    groupless = new LoadedGroupItem("Groupless");
                    Settings.Groups!.Add(groupless);
                }

                for (int i = 0; i < group.Presets!.Count;)
                {
                    var preset = group.Presets![i];

                    string oldPath = preset.GetPath();
                    group!.Presets?.Remove(preset);
                    groupless.AddPreset(preset);
                    string newPath = preset.GetPath();

                    File.Move(oldPath, newPath);

                    if (!string.IsNullOrEmpty(Settings.PresetName) && Settings.PresetName.Equals(preset.Name, System.StringComparison.OrdinalIgnoreCase))
                        Settings.UpdateCurrentLoadedPreset(preset.GetPath());
                }

                if (group.IsEmpty()) Settings.RemoveGroup(group.Name);

            }
            else if (result == MessageBoxResult.No)
                Settings.RemoveGroup(group.Name, true);
        }
    }
}
