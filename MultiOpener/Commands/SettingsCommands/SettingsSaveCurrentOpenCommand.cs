using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsSaveCurrentOpenCommand : SettingsCommandBase
    {
        public SettingsSaveCurrentOpenCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            int n = Settings.Opens.Count;
            for (int i = 0; i < n; i++)
            {
                var open = Settings.Opens[i];
                if (open == Settings.currentChosen)
                {
                    string appPath = Settings.ApplicationPathField ?? "";
                    if(!string.IsNullOrEmpty(appPath))
                        while (appPath[0] == '\u202A')
                            appPath = appPath.Substring(1);

                    open.PathExe = appPath;
                    open.Type = Settings.ChooseTypeBox;
                    open.DelayBefore = int.Parse(Settings.DelayBeforeTimeField ?? "0");
                    open.DelayAfter = int.Parse(Settings.DelayAfterTimeField ?? "0");
                }
            }
        }
    }
}
