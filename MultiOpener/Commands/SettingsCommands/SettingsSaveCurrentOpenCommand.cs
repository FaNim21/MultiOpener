using MultiOpener.ListView;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsSaveCurrentOpenCommand : SettingsCommandBase
    {
        public SettingsSaveCurrentOpenCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null || Settings.SelectedOpenTypeViewModel == null) return;

            int n = Settings.Opens.Count;
            for (int i = 0; i < n; i++)
            {
                var open = Settings.Opens[i];
                if (open.Name.Equals(Settings.currentChosen.Name))
                {
                    string appPath = Settings.SelectedOpenTypeViewModel.ApplicationPathField ?? "";
                    if(!string.IsNullOrEmpty(appPath))
                        while (appPath[0] == '\u202A')
                            appPath = appPath.Substring(1);

                    open.PathExe = appPath;
                    open.Type = Settings.ChooseTypeBox;
                    open.DelayBefore = int.Parse(Settings.SelectedOpenTypeViewModel.DelayBeforeTimeField ?? "0");
                    open.DelayAfter = int.Parse(Settings.SelectedOpenTypeViewModel.DelayAfterTimeField ?? "0");

                    if(open.GetType() == typeof(OpenInstance))
                    {
                        //((OpenInstance)open).Quantity = Settings.SelectedOpenTypeViewModel;
                    }

                    break;
                }
            }
        }
    }
}
