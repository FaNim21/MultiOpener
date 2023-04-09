using MultiOpener.Items;
using System.Diagnostics;

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
            Trace.WriteLine("Resetowanie");
        }
    }
}
