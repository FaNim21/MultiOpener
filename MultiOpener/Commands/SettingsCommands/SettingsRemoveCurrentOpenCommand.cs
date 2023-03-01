﻿using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsRemoveCurrentOpenCommand : SettingsCommandBase
    {
        public SettingsRemoveCurrentOpenCommand(SettingsViewModel Settings) : base(Settings) { }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            Settings.LeftPanelGridVisibility = false;
            if (Settings.currentChosen != null)
                Settings.RemoveItem(Settings.currentChosen);
        }
    }
}
