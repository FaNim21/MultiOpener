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
                    Settings.Opens[i] = Settings.CreateNewOpen(selected, open.Name);
                    open = Settings.Opens[i];
                }

                Settings.SelectedOpenTypeViewModel.SetOpenProperties(ref open);
                Settings.CurrentChosen = open;
                break;
            }
        }
    }
}
