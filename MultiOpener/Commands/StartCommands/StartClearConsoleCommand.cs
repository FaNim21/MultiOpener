using MultiOpener.Components.Controls;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.StartCommands;

class StartClearConsoleCommand : StartCommandBase
{
    public StartClearConsoleCommand(StartViewModel? startViewModel) : base(startViewModel)
    {
    }

    public override void Execute(object? parameter)
    {
        if (Start == null) return;

        if(DialogBox.Show($"Are you certain about clearing console logs?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.None) == MessageBoxResult.Yes)
            Start.ConsoleViewModel.Clear();
    }
}
