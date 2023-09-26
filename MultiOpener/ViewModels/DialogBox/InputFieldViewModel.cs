using MultiOpener.Commands.DialogBoxCommands;
using MultiOpener.Components.Controls;
using System.Windows;

namespace MultiOpener.ViewModels.DialogBox;

public class InputFieldViewModel : DialogBaseViewModel
{
    public string? _output;
    public string? Output
    {
        get { return _output; }
        set
        {
            _output = value;
            OnPropertyChanged(nameof(Output));
        }
    }

    public string? _errorMessage;
    public string? ErrorMessage
    {
        get { return _errorMessage; }
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    public MessageBoxResult ButtonResult { get; set; } = MessageBoxResult.OK;


    public InputFieldViewModel(ValidateInputFieldAccept validation)
    {
        ButtonPress = new InputFieldButtonClickCommand(this, validation);
    }

    public void LogError(string output)
    {
        //TODO: 9 Na przyszlosc jak bedzie potrzeba to zrobic bardziej complex wyswietlanie errorow walidacji inputfielda z customowymi wiadomosciami itp itd
        if (string.IsNullOrEmpty(output)) return;

        ErrorMessage = output;
    }
}
