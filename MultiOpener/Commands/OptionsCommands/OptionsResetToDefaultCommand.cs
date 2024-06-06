using MultiOpener.Components.Controls;
using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.OptionsCommands;

public class OptionsResetToDefaultCommand : OptionsCommandBase
{
    public OptionsResetToDefaultCommand(OptionsViewModel Options) : base(Options)
    {
    }

    public override void Execute(object? parameter)
    {
        if (Options == null) return;

        if (DialogBox.Show("Are you sure you want to reset all options to default?", "", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            Options.ResetToDefault();
    }
}