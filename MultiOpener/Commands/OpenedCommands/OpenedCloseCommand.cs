using MultiOpener.Commands.StartCommands;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System.Threading.Tasks;

namespace MultiOpener.Commands.OpenedCommands
{
    public class OpenedCloseOpenCommand : StartCommandBase
    {
        public OpenedProcess openedProcess;

        public OpenedCloseOpenCommand(OpenedProcess openedProcess, StartViewModel? start) : base(start)
        {
            this.openedProcess = openedProcess;
        }

        public override void Execute(object? parameter)
        {
            if (openedProcess == null || Consts.IsStartPanelWorkingNow) return;

            openedProcess.Update();
            Task task = Task.Run(CloseOpenOpened);
        }

        public async Task CloseOpenOpened()
        {
            Consts.IsStartPanelWorkingNow = true;

            if (openedProcess.IsOpenedFromStatus())
            {
                bool output = await openedProcess.Close();
                if(output)
                    Start?.LogLine($"Closed {openedProcess.Name}");
                else
                    Start?.LogLine($"Can't close {openedProcess.Name}", Entities.ConsoleLineOption.Warning);

                openedProcess.InfoButtonOpenName = "Open";
            }
            else
            {
                bool output = await openedProcess.OpenProcess();
                if(output)
                    Start?.LogLine($"Opened {openedProcess.Name}");
                else
                    Start?.LogLine($"Can't open {openedProcess.Name}", Entities.ConsoleLineOption.Warning);

                openedProcess.InfoButtonOpenName = "Close";
            }

            Consts.IsStartPanelWorkingNow = false;
        }
    }
}
