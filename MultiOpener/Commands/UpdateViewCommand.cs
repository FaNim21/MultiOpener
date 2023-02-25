using MultiOpener.ViewModels;
using System;
using System.Windows.Input;

namespace MultiOpener.Commands
{
    class UpdateViewCommand : ICommand
    {
        private readonly MainViewModel viewModel;

        public UpdateViewCommand(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            //TODO: Zapisywac stan startu i settingsow

            string result = parameter?.ToString() ?? "";
            viewModel.MainWindow.EnableDisableChoosenHeadButton(result);

            if (result.Equals("Start"))
            {
                //TODO: Tu tez bedzie wazne zeby przy zmianie na start sprawdzac czy lsity sie roznia i zeby robic to w dwie strony
                viewModel.SelectedViewModel = viewModel.start;
            }
            else if (result.Equals("Settings"))
            {
                viewModel.SelectedViewModel = viewModel.settings;
            }
        }
    }
}
