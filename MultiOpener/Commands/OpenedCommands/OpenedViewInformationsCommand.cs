using MultiOpener.Items;
using System.Diagnostics;

namespace MultiOpener.Commands.OpenedCommands
{
    public class OpenedViewInformationsCommand : BaseCommand
    {
        public OpenedProcess openedProcess;

        public OpenedViewInformationsCommand(OpenedProcess openedProcess)
        {
            this.openedProcess = openedProcess;
        }

        public override void Execute(object? parameter)
        {
            //TODO: 9 Panel/Okno z wyswietlanymi informacjami jak id 

            Trace.WriteLine($"{openedProcess.WindowTitle}");
        }
    }
}
