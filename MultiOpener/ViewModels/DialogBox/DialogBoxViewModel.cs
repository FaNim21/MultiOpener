using MultiOpener.Commands.DialogBoxCommands;
using MultiOpener.Components.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.ViewModels.DialogBox;

public class DialogBoxViewModel : BaseViewModel
{
    private string? _text;
    public string? Text
    {
        get { return _text; }
        set
        {
            _text = value;
            OnPropertyChanged(nameof(Text));
        }
    }

    private string? _caption;
    public string? Caption
    {
        get { return _caption; }
        set
        {
            _caption = value;
            OnPropertyChanged(nameof(Caption));
        }
    }

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

    private MessageBoxResult _result = MessageBoxResult.None;
    public MessageBoxResult Result
    {
        get { return _result; }
        set
        {
            _result = value;
            OnPropertyChanged(nameof(Result));
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

    public ICommand ButtonPress { get; set; }

    public DialogBoxViewModel()
    {
        ButtonPress = new DialogBoxButtonClickCommand(this);
    }
}
