using MultiOpener.ViewModels;
using System;
using System.Windows.Input;

namespace MultiOpener.Commands.SettingsCommands
{
    public abstract class SettingsCommandBase : ICommand
    {
        public SettingsViewModel? Settings { get; set; }

        public SettingsCommandBase(SettingsViewModel Settings)
        {
            this.Settings = Settings;
        }

        public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        public bool CanExecute(object? parameter) => true;
        public abstract void Execute(object? parameter);
    }
}
