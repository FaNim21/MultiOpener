using MultiOpener.ViewModels;

namespace MultiOpener.Commands.StartCommands
{
    public class StartRefreshOpenedCommand : StartCommandBase
    {
        public StartRefreshOpenedCommand(StartViewModel startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Start == null || Start.Opened == null || Start.Opened.Count == 0) return;

            for (int i = 0; i < Start.Opened.Count; i++)
            {
                var current = Start.Opened[i];

                current.UpdateTitle();
                current.UpdateStatus();
            }
        }
    }
}
