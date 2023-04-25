using MultiOpener.Commands.StartCommands;
using MultiOpener.Items;
using MultiOpener.ViewModels;
using System.Threading.Tasks;

namespace MultiOpener.Commands.OpenedCommands
{
    public class OpenedCloseOpenCommand : StartCommandBase
    {
        public OpenedProcess openedProcess;

        public OpenedCloseOpenCommand(OpenedProcess openedProcess, StartViewModel? start) : base(start)
        {
            this.openedProcess = openedProcess;
        }

        public override void Execute(object? parameter)
        {
            if (Start == null || openedProcess == null || Consts.IsStartPanelWorkingNow) return;

            openedProcess.Update();
            Task task = Task.Run(CloseOpenOpened);
        }

        public async Task CloseOpenOpened()
        {
            Consts.IsStartPanelWorkingNow = true;

            if (openedProcess.IsOpened())
            {
                Start.UpdateText($"Closing {openedProcess.WindowTitle}");
                bool result = await openedProcess.Close();

                if (result)
                {
                    openedProcess.ClearAfterClose();
                    openedProcess.UpdateStatus();
                }
            }
            else
            {
                Start.UpdateText($"Opening {openedProcess.WindowTitle}");
                await openedProcess.QuickOpen();
            }
            await Task.Delay(100);

            Consts.IsStartPanelWorkingNow = false;
            Start.UpdateText($"");
        }
    }
}
