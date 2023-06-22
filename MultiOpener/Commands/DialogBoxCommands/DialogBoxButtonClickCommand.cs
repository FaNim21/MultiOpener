using MultiOpener.ViewModels.DialogBox;
using System.Linq;
using System.Windows;

namespace MultiOpener.Commands.DialogBoxCommands;

class DialogBoxButtonClickCommand : BaseCommand
{
    public DialogBoxViewModel DialogBox { get; set; }

    public DialogBoxButtonClickCommand(DialogBoxViewModel DialogBox)
    {
        this.DialogBox = DialogBox;
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null) return;

        MessageBoxResult variable = (MessageBoxResult)parameter;
        string output = variable.ToString();
        if (string.IsNullOrEmpty(output)) return;

        DialogBox.Result = output switch
        {
            "Ok" => MessageBoxResult.OK,
            "Yes" => MessageBoxResult.Yes,
            "No" => MessageBoxResult.No,
            "Cancel" => MessageBoxResult.Cancel,
            _ => MessageBoxResult.None,
        };

        Window? activeWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        activeWindow?.Close();
    }
}
