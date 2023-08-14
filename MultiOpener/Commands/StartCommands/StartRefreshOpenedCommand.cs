using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MultiOpener.Commands.StartCommands
{
    public class StartRefreshOpenedCommand : StartCommandBase
    {
        private CancellationTokenSource source = new();
        private CancellationToken token;
        private bool isRunning = false;

        private bool isRefreshRunningFromOpen = false;

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

            if (parameter is { })
            {
                object[]? parameters = parameter as object[];
                isRefreshRunningFromOpen = (bool)parameters![0];
            }

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
            Start.LogLine("Refreshing started...");

            int length = Start.Opened.Count;
            for (int i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested) break;
                var current = Start.Opened[i];

                current.Update();
            }

            Consts.IsStartPanelWorkingNow = false;
            Start.LogLine("Refreshing finished!");
            source.Dispose();
            isRunning = false;
            Start.RefreshButtonName = "Refresh";

            if (App.Config.IsMinimizedAfterOpen && isRefreshRunningFromOpen)
                await MinimizeMainWindow();
        }

        private async Task MinimizeMainWindow()
        {
            isRefreshRunningFromOpen = false;
            await Task.Delay(250);
            Application.Current?.Dispatcher.Invoke(delegate { Win32.MinimizeWindowHwnd(new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle()); });
        }
    }
}
