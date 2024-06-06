using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;
using System;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsSaveCurrentOpenCommand : SettingsCommandBase
{
    public SettingsSaveCurrentOpenCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || Settings.CurrentChosen == null || Settings.SelectedOpenTypeViewModel == null) return;

        var opensCount = Settings.Opens.Count;
        var selected = Settings.GetSelectedOpenType();
        var currentChosenName = Settings.CurrentChosen.Name;

        for (int i = 0; i < opensCount; i++)
        {
            var open = Settings.Opens[i];
            if (!open.Name.Equals(currentChosenName))
                continue;

            if (open.GetType() != selected)
            {
                OpenItem newItem = (OpenItem)Activator.CreateInstance(selected, open.Name)! ?? new OpenItem(open.Name);
                Settings.Opens[i] = newItem;
                open = Settings.Opens[i];
            }

            Settings.SelectedOpenTypeViewModel.SetOpenProperties(ref open);
            Settings.CurrentChosen = open;
            break;
        }
    }
}
