using MultiOpener.ViewModels;

namespace MultiOpener.Commands.StartCommands
{
    internal class StartCancelCommand : StartCommandBase
    {
        public StartCancelCommand(StartViewModel? startViewModel) : base(startViewModel) { }

        public override void Execute(object? parameter)
        {
            if (Start == null) return;

            ((StartOpenCommand)Start.OpenCommand).source.Cancel();
            Start.SetStartButtonState(StartButtonState.close);
        }
    }
}
