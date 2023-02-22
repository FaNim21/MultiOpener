using MultiOpener.ViewModels;
using System;
using System.Windows.Input;

namespace MultiOpener.Commands
{
    class UpdateViewCommand : ICommand
    {
        private MainViewModel viewModel;
        private MainWindow mainWindow;

        public UpdateViewCommand(MainViewModel viewModel, MainWindow mainWindow)
        {
            this.viewModel = viewModel;
            this.mainWindow = mainWindow;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            //TODO: Zapisywac stan startu i settingsow
            if (parameter?.ToString() == "Start")
            {
                mainWindow.StartButton.IsEnabled = false;
                mainWindow.SettingsButton.IsEnabled = true;
                viewModel.SelectedViewModel = new StartViewModel();
            }
            else if (parameter?.ToString() == "Settings")
            {
                mainWindow.SettingsButton.IsEnabled = false;
                mainWindow.StartButton.IsEnabled = true;
                viewModel.SelectedViewModel = new SettingsViewModel();
            }
        }
    }
}
