using MultiOpener.Commands.StartCommands;
using MultiOpener.Entities.Opened;
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
            if (openedProcess == null || Consts.IsStartPanelWorkingNow) return;

            openedProcess.FastUpdate();
            Task task = Task.Run(CloseOpenOpened);
        }

        public async Task CloseOpenOpened()
        {
            Consts.IsStartPanelWorkingNow = true;

            if (openedProcess.IsOpenedFromStatus())
            {
                await openedProcess.Close();
                Start?.LogLine($"Closed {openedProcess.Name}");
            }
            else
            {
                await openedProcess.OpenProcess();
                Start?.LogLine($"Opened {openedProcess.Name}");
            }

            Consts.IsStartPanelWorkingNow = false;
        }
    }
}
