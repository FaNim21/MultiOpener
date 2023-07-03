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

            string output;
            if (openedProcess.IsOpenedFromStatus())
            {
                output = $"Closed {openedProcess.Name}";
                Start?.LogLine($"Closing {openedProcess.Name}");
                await openedProcess.Close();
            }
            else
            {
                output = $"Opened {openedProcess.Name}";
                Start?.LogLine($"Opening {openedProcess.Name}");
                await openedProcess.OpenProcess();
            }
            await Task.Delay(100);

            Consts.IsStartPanelWorkingNow = false;
            Start?.LogLine($"{output}");
        }
    }
}
