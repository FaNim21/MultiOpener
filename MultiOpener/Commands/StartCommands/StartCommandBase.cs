using MultiOpener.ViewModels;
using System;
using System.Windows.Input;

namespace MultiOpener.Commands.StartCommands
{
    public abstract class StartCommandBase : ICommand
    {
        public StartViewModel? Start { get; set; }

        public StartCommandBase(StartViewModel? startViewModel)
        {
            Start = startViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;
        public abstract void Execute(object? parameter);
    }
}
