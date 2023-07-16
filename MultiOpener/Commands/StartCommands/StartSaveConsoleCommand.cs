using MultiOpener.ViewModels;

namespace MultiOpener.Commands.StartCommands;

class StartSaveConsoleCommand : StartCommandBase
{
    public StartSaveConsoleCommand(StartViewModel? startViewModel) : base(startViewModel)
    {
    }

    public override void Execute(object? parameter)
    {
        //TODO: 0 zaimplementowac odswiezanie
    }
}
