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
            if (DialogBox.Show($"You sure you want to delete all presets {group.Name}", "Removing Groupless presets", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                int n = group.Presets.Count;
                for (int i = 0; i < n; i++)
                {
                    var current = group.Presets[i];
                    File.Delete(current.GetPath());
                }
            }
            Settings.SetTreeWithGroupsAndPresets();
            return;
        }

        if (group.IsEmpty())
        {
            string path = group.GetPath();
            if (!Path.GetDirectoryName(path)!.Equals("Presets", System.StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(path);
                Settings.RemoveGroup(group.Name);
            }
        }
        else
        {
            MessageBoxResult result = DialogBox.Show($"You trying to remove {group.Name} with option:\nYES - Removing only that folder and move all presets to Groupless\nNO - Removing whole folder with all presets in it", "Removing Group", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                LoadedGroupItem? groupless = Settings.GetGroupByName("Groupless");
                if (groupless == null) return;

                int length = group.Presets.Count;
                for (int i = 0; i < length; i++)
                {
                    var preset = group.Presets![i];

                    string oldPath = preset.GetPath();
                    group!.Presets?.Remove(preset);
                    groupless.AddPreset(preset);
                    string newPath = preset.GetPath();

                    File.Move(oldPath, newPath);
                }

                if (group.IsEmpty())
                    Directory.Delete(group.GetPath());

            }
            else if (result == MessageBoxResult.No)
            {
                Settings.RemoveGroup(group.Name);
                Directory.Delete(group.GetPath(), true);
            }
        }

        Settings.SetTreeWithGroupsAndPresets();
    }
}
