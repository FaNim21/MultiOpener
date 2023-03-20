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
            string result = parameter?.ToString() ?? "";
            viewModel.MainWindow.EnableDisableChoosenHeadButton(result);

            if (result.Equals("Start"))
            {
                viewModel.settings?.SaveCurrentOpenCommand.Execute(null);
                viewModel.SelectedViewModel = viewModel.start;
            }
            else if (result.Equals("Settings"))
                viewModel.SelectedViewModel = viewModel.settings;
        }
    }
}
