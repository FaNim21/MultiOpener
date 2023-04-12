using MultiOpener.Items;
using System.Diagnostics;
using System.Windows;

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
            openedProcess.Update();
            MessageBox.Show(openedProcess.ToString());
        }
    }
}
