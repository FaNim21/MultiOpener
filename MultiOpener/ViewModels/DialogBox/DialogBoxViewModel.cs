using MultiOpener.Commands.DialogBoxCommands;
using MultiOpener.Components.Controls;
using System.Collections.Generic;
using System.Windows;

namespace MultiOpener.ViewModels.DialogBox;

public class DialogBoxViewModel : DialogBaseViewModel
{
    private MessageBoxImage _icon;
    public MessageBoxImage Icon
    {
        get { return _icon; }
        set
        {
            _icon = value;
            OnPropertyChanged(nameof(Icon));
        }
    }

    private IEnumerable<DialogBoxButton> _buttons = new List<DialogBoxButton>();
    public IEnumerable<DialogBoxButton> Buttons
    {
        get { return _buttons; }
        set
        {
            _buttons = value;
            OnPropertyChanged(nameof(Buttons));
        }
    }

    public DialogBoxViewModel()
    {
        ButtonPress = new DialogBoxButtonClickCommand(this);
    }
}
