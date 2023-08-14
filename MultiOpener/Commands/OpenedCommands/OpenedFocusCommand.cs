using MultiOpener.Commands.StartCommands;
using MultiOpener.Entities.Opened;
using MultiOpener.Utils;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.OpenedCommands;

public class OpenedFocusCommand : StartCommandBase
{
    public OpenedProcess openedProcess;

    public OpenedFocusCommand(OpenedProcess openedProcess, StartViewModel? startViewModel) : base(startViewModel)
    {
        this.openedProcess = openedProcess;
    }

    public override void Execute(object? parameter)
    {
        if (openedProcess == null || Consts.IsStartPanelWorkingNow) return;

        openedProcess.Update();
        Win32.UnminimizeWindowHwnd(openedProcess.Hwnd);
        Win32.SetFocus(openedProcess.Hwnd);
    }
}
