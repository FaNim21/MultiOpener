using MultiOpener.Utils;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.StartCommands
{
    public class StartRefreshOpenedCommand : StartCommandBase
    {
        public StartRefreshOpenedCommand(StartViewModel startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Start == null || Start.Opened == null || Start.Opened.Count == 0) return;

            for (int i = 0; i < Start.Opened.Count; i++)
            {
                var current = Start.Opened[i];

                int pid = (int)Win32.GetPidFromHwnd(current.Hwnd);

                //moze powodowac problemy samo ustawianie hwnd jezeli i tak dany process nie ma swojego okna
                current.SetHwnd();
                current.UpdateTitle();
                current.UpdateStatus();
                current.SetPid();
            }
        }
    }
}
