using MultiOpener.ViewModels.Settings;
using Ookii.Dialogs.Wpf;
using System.Windows;

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
            var dialog = new VistaOpenFileDialog();
            if (dialog.ShowDialog(Application.Current.MainWindow).GetValueOrDefault())
            {
                if (dialog.CheckFileExists && !dialog.Multiselect)
                    OpenTypeViewModel.ApplicationPathField = dialog.FileName;
            }
        }
    }
}
