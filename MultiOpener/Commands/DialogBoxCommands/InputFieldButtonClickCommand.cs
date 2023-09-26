using MultiOpener.Components.Controls;
using MultiOpener.ViewModels.DialogBox;
using System.Windows;

namespace MultiOpener.Commands.DialogBoxCommands;

public class InputFieldButtonClickCommand : BaseCommand
{
    public InputFieldViewModel InputFieldViewModel { get; set; }

    public ValidateInputFieldAccept Validation { get; set; }

    public InputFieldButtonClickCommand(InputFieldViewModel InputFieldViewModel, ValidateInputFieldAccept Validation)
    {
        this.InputFieldViewModel = InputFieldViewModel;
        this.Validation = Validation;
        this.Validation ??= ((string n) => true);
    }

    public override void Execute(object? parameter)
    {
        if (parameter == null) return;

        MessageBoxResult variable = (MessageBoxResult)parameter;
        string output = variable.ToString().ToLower();
        if (string.IsNullOrEmpty(output)) return;

        string fieldResult = InputFieldViewModel.Output!;
        if (string.IsNullOrEmpty(fieldResult))
        {
            InputFieldViewModel.LogError("Field is empty!");
            return;
        }

        if (output.Equals("ok"))
        {
            if (!Validation(InputFieldViewModel.Output!))
            {
                InputFieldViewModel.LogError($"Item named '{fieldResult}' already exist!");
                return;
            }

            InputFieldViewModel.Result = MessageBoxResult.OK;
        }
        else
            InputFieldViewModel.Result = MessageBoxResult.None;

        InputFieldViewModel.Close();
    }
}
