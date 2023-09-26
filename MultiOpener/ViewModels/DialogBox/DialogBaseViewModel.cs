using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.ViewModels.DialogBox;

public class DialogBaseViewModel : BaseViewModel
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

    public ICommand? ButtonPress { get; set; }

    
    public void Close()
    {
        Window? activeWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        activeWindow?.Close();
    }
}
