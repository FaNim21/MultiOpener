using MultiOpener.Commands.StartCommands;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System.Threading.Tasks;

namespace MultiOpener.Commands.OpenedCommands
{
    public class OpenedResetCommand : StartCommandBase
    {
        public OpenedProcess openedProcess;

        public OpenedResetCommand(OpenedProcess openedProcess, StartViewModel? start) : base(start)
        {
            this.openedProcess = openedProcess;
        }

        public override void Execute(object? parameter)
        {
            if (openedProcess == null || Consts.IsStartPanelWorkingNow) return;

            openedProcess.Update();
            if (!openedProcess.IsOpenedFromStatus()) return;

            Task task = Task.Run(ResetOpened);
        }

        public async Task ResetOpened()
        {
            Consts.IsStartPanelWorkingNow = true;

            Start?.LogLine($"Reseting {openedProcess.Name}");
            if (openedProcess.IsOpenedFromStatus())
            {
                bool result = await openedProcess.Close();

                if (result)
                {
                    await Task.Delay(1000);
                    await openedProcess.OpenProcess();
                }
            }

            Consts.IsStartPanelWorkingNow = false;
            Start?.LogLine($"Finished reseting {openedProcess.Name}");
        }
    }
}
