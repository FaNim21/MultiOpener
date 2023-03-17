using MultiOpener.ViewModels;
using System;
using System.Diagnostics;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsOpenPresetsFolderCommand : SettingsCommandBase
    {
        public SettingsOpenPresetsFolderCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            Process.Start("explorer.exe", Settings.directoryPath);
        }
    }
}
