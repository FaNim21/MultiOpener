using MultiOpener.ViewModels;
using System;
using System.Windows.Input;

namespace MultiOpener.Commands.OptionsCommands;

public abstract class OptionsCommandBase : ICommand
{
    public OptionsViewModel? Options { get; set; }

    public OptionsCommandBase(OptionsViewModel Options)
    {
        this.Options = Options;
    }

    public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

    public bool CanExecute(object? parameter) => true;
    public abstract void Execute(object? parameter);
}
