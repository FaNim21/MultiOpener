using MultiOpener.Components;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRenameItemCommand : SettingsCommandBase
{
    public SettingsRenameItemCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;

        IInputElement focusedControl = Keyboard.FocusedElement;
        EditableTextBlock? textBlock = Helper.FindChild<EditableTextBlock>((DependencyObject)focusedControl);
        if (textBlock != null && textBlock.IsEditable)
            textBlock.IsInEditMode = true;
    }
}