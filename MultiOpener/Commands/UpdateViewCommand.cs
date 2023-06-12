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

        public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            //TODO: 9 definitely need to rework it to make it better with adding new tabs in future etc?
            string result = parameter?.ToString() ?? "";
            viewModel.MainWindow.EnableDisableChoosenHeadButton(result);

            if (viewModel.SelectedViewModel == viewModel.options)
                viewModel.options.SaveOptions();
            else if (viewModel.SelectedViewModel == viewModel.settings)
                viewModel.settings?.SaveCurrentOpenCommand?.Execute(null);

            if (result.Equals("Start"))
                viewModel.SelectedViewModel = viewModel.start;
            else if (result.Equals("Options"))
                viewModel.SelectedViewModel = viewModel.options;
            else if (result.Equals("Settings"))
            {
                viewModel.settings?.UpdatePresetsComboBox();
                viewModel.SelectedViewModel = viewModel.settings;
            }
        }
    }
}
