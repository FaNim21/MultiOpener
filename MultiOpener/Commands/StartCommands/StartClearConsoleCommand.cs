using MultiOpener.ViewModels;

namespace MultiOpener.Commands.StartCommands;

class StartClearConsoleCommand : StartCommandBase
{
    public StartClearConsoleCommand(StartViewModel? startViewModel) : base(startViewModel)
    {
    }

    public override void Execute(object? parameter)
    {
        if (Start == null) return;

        Start.ConsoleViewModel.Clear();
    }
}
