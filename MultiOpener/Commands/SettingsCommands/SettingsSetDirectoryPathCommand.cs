using MultiOpener.Components.Controls;
using MultiOpener.ViewModels.Settings;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SettingsSetDirectoryPathCommand : BaseCommand
    {
        public OpenTypeViewModelBase OpenTypeViewModel { get; set; }

        public SettingsSetDirectoryPathCommand(OpenTypeViewModelBase openTypeVM)
        {
            OpenTypeViewModel = openTypeVM;
        }

        public override void Execute(object? parameter)
        {
            string output = DialogBox.ShowOpenFile(true, false);
            if (!string.IsNullOrEmpty(output))
                OpenTypeViewModel.ApplicationPathField = output;
        }
    }
}
