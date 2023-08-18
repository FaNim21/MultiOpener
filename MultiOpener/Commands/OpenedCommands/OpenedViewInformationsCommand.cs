using MultiOpener.Components.Controls;
using MultiOpener.Entities.Opened;

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
            openedProcess.Update();
            DialogBox.ViewInformations(openedProcess);
        }
    }
}
