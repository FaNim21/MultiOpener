using MultiOpener.Components;
using MultiOpener.Utils;
using MultiOpener.ViewModels;

namespace MultiOpener.Commands.SettingsCommands;

public class SettingsRenameItemCommand : SettingsCommandBase
{
    public SettingsRenameItemCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null) return;

        EditableTextBlock? textBlock = Helper.GetFocusedUIElement<EditableTextBlock>();
        if (textBlock != null && textBlock.IsEditable)
            textBlock.IsInEditMode = true;
    }
}