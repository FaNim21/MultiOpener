using MultiOpener.Items;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MultiOpener.Commands.OpenedCommands
{
    public class OpenedResetCommand : BaseCommand
    {
        public OpenedProcess openedProcess;

        public OpenedResetCommand(OpenedProcess openedProcess)
        {
            this.openedProcess = openedProcess;
        }

        public override void Execute(object? parameter)
        {
            openedProcess.Update();
            Task task = Task.Run(ResetOpened);
        }

        public async Task ResetOpened()
        {
            if (openedProcess.IsOpened())
            {
                bool result = await openedProcess.Close();

                if (result)
                {
                    openedProcess.ClearAfterClose();
                    openedProcess.UpdateStatus();

                    await openedProcess.QuickOpen();
                }
            }
        }
    }
}
