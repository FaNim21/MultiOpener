﻿using MultiOpener.ListView;
using MultiOpener.ViewModels;
using MultiOpener.ViewModels.Settings;
using System;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsSaveCurrentOpenCommand : SettingsCommandBase
    {
        public SettingsSaveCurrentOpenCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null || Settings.currentChosen == null || Settings.SelectedOpenTypeViewModel == null) return;

            int n = Settings.Opens.Count;
            for (int i = 0; i < n; i++)
            {
                var open = Settings.Opens[i];
                if (open.Name.Equals(Settings.currentChosen.Name))
                {
                    Type selected = Settings.GetSelectedOpenType();
                    if (open.GetType() != Settings.GetSelectedOpenType())
                    {
                        if (selected == typeof(OpenInstance))
                            Settings.Opens[i] = new OpenInstance(open.Name);
                        else
                            Settings.Opens[i] = new OpenItem(open.Name);

                        open = Settings.Opens[i];
                    }

                    if(open.GetType() == typeof(OpenInstance))
                    {
                        OpenInstance instance = (OpenInstance)Settings.Opens[i];
                        instance.Quantity = ((SettingsOpenInstancesModelView)Settings.SelectedOpenTypeViewModel).Quantity;
                    }

                    string appPath = Settings.SelectedOpenTypeViewModel.ApplicationPathField ?? "";
                    if (!string.IsNullOrEmpty(appPath))
                        while (appPath[0] == '\u202A')
                            appPath = appPath.Substring(1);

                    open.PathExe = appPath;
                    open.Type = Settings.ChooseTypeBox;
                    open.DelayBefore = int.Parse(Settings.SelectedOpenTypeViewModel.DelayBeforeTimeField ?? "0");
                    open.DelayAfter = int.Parse(Settings.SelectedOpenTypeViewModel.DelayAfterTimeField ?? "0");

                    Settings.currentChosen = open;
                    break;
                }
            }
        }
    }
}
