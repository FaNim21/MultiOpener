﻿using System;
using System.Windows.Input;

namespace MultiOpener.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;


        public BaseCommand() { }

        public bool CanExecute(object? parameter) => true;
        public abstract void Execute(object? parameter);
    }
}
