using System;

namespace MultiOpener.Commands;

public class RelayCommand : BaseCommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute)
    {
        _execute = execute;
    }

    public override void Execute(object? parameter)
    {
        _execute();
    }
}