using MultiOpener.Entities;
using MultiOpener.ViewModels;
using System.Diagnostics;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsOpenPresetsFolderCommand : SettingsCommandBase
{
    public SettingsOpenPresetsFolderCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;

        LoadedGroupItem group = (LoadedGroupItem)parameter;
        Process.Start("explorer.exe", group.GetPath());
    }
}
