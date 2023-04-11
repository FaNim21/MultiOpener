using MultiOpener.Items;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MultiOpener.Commands.OpenedCommands
{
    public class OpenedCloseOpenCommand : BaseCommand
    {
        public OpenedProcess openedProcess;

        public OpenedCloseOpenCommand(OpenedProcess openedProcess)
        {
            this.openedProcess = openedProcess;
        }

        public override void Execute(object? parameter)
        {
            openedProcess.Update();
            Task task = Task.Run(CloseOpenOpened);
        }

        public async Task CloseOpenOpened()
        {
            if (openedProcess.IsOpened())
            {
                bool result = await openedProcess.Close();

                if (result)
                {
                    openedProcess.ClearAfterClose();
                    openedProcess.UpdateStatus();
                }
            }
            else
            {
                await openedProcess.QuickOpen();
            }
        }
    }
}
