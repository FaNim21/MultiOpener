using MultiOpener.Utils.Interfaces;

namespace MultiOpener.Commands.UtilCommands;

public class CopyTextToClipboardCommand : BaseCommand
{
    public IClipboardService ClipboardService { get; set; }

    public CopyTextToClipboardCommand(IClipboardService clipboardService)
    {
        ClipboardService = clipboardService;
    }

    public override void Execute(object? parameter)
    {
        if (ClipboardService == null) return;
        if (parameter == null || string.IsNullOrEmpty(parameter.ToString())) return;

        ClipboardService.CopyTextToClipboard(parameter.ToString()!);
    }
}
