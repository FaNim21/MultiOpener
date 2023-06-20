using MultiOpener.Items;
using MultiOpener.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace MultiOpener.Commands.StartCommands
{
    public class StartRefreshOpenedCommand : StartCommandBase
    {
        private CancellationTokenSource source = new();
        private CancellationToken token;
        private bool isRunning = false;

        public StartRefreshOpenedCommand(StartViewModel startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Start == null) return;

            if (isRunning)
            {
                source.Cancel();
                return;
            }

            if (Start.OpenedIsEmpty() || Consts.IsStartPanelWorkingNow) return;

            source = new();
            token = source.Token;
            Task task = Task.Run(Refresh, token);
        }

        public async Task Refresh()
        {
            if (Start == null) return;

            isRunning = true;
            Start.RefreshButtonName = "Stop";
            Consts.IsStartPanelWorkingNow = true;

            int length = Start.Opened.Count;
            for (int i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                var current = Start.Opened[i];
                string textInfo = $"({i + 1}/{length} - {current.Name})";

                if (App.Input.IsShiftPressed)
                    await FastRefresh(current, textInfo);
                else
                    await NormalRefresh(current, textInfo);
            }

            Consts.IsStartPanelWorkingNow = false;
            Start.UpdateText("");
            source.Dispose();
            isRunning = false;
            Start.RefreshButtonName = "Refresh";
        }

        private async Task NormalRefresh(OpenedProcess current, string textInfo)
        {
            Start?.UpdateText($"Refreshing {textInfo}");

            if (current.isMCInstance && !current.IsOpened())
            {
                Start?.UpdateText($"Refreshing {textInfo} - looking for window");
                await current.SearchForMCInstance(source);
            }

            current.Update();
            await Task.Delay(100);
        }

        private async Task FastRefresh(OpenedProcess current, string textInfo)
        {
            Start?.UpdateText($"Fast Refreshing {textInfo}");
            current.FastUpdate();

            await Task.Delay(25);
        }
    }
}
