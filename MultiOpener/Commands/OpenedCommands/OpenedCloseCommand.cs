using MultiOpener.Items;
using System.Diagnostics;

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
            Trace.WriteLine("Zamykanie");
        }
    }
}
