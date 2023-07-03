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
            //TODO: 9 Panel/Okno z wyswietlanymi informacjami jak id i zeby byl w tym panelu guzik do odpalania folderu zrodla tego programu
            openedProcess.Update();

            //Tu narazie zostanie messagebox z racji ze tu i tak bedzie wywolywane specjalne okno
            DialogBox.Show(openedProcess.ToString(), $"{openedProcess.Name} Informations");
        }
    }
}
