using MultiOpener.Commands.StartCommands;
using MultiOpener.Items;
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
            Task task = Task.Run(ResetOpened);
        }

        public async Task ResetOpened()
        {
            Consts.IsStartPanelWorkingNow = true;

            Start?.UpdateText($"Reseting {openedProcess.Name}");
            if (openedProcess.IsOpened())
            {

                bool result = await openedProcess.Close();

                if (result)
                {
                    openedProcess.Clear();
                    openedProcess.UpdateStatus();

                    await Task.Delay(1000);
                    await openedProcess.QuickOpen();
                }
            }

            Consts.IsStartPanelWorkingNow = false;
            Start?.UpdateText($"");
        }
    }
}
