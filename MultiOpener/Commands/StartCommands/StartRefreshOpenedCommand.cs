using MultiOpener.ViewModels;
using System.Threading.Tasks;

namespace MultiOpener.Commands.StartCommands
{
    public class StartRefreshOpenedCommand : StartCommandBase
    {
        public StartRefreshOpenedCommand(StartViewModel startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Start == null || Start.OpenedIsEmpty() || Consts.IsStartPanelWorkingNow) return;

            Task task = Task.Run(Refresh);
        }

        public async Task Refresh()
        {
            if (Consts.IsStartPanelWorkingNow)
                return;

            Consts.IsStartPanelWorkingNow = true;

            for (int i = 0; i < Start.Opened.Count; i++)
            {
                var current = Start.Opened[i];
                if (current.isMCInstance && !current.IsOpened())
                {
                    await current.SearchForMCInstance();
                }

                current.Update();
            }

            Consts.IsStartPanelWorkingNow = false;
        }
    }
}
