using MultiOpener.Commands;
using System.Windows.Input;

namespace MultiOpener.ViewModels;

public class MainViewModel : BaseViewModel
{
    public MainWindow MainWindow { get; set; }

    public readonly SettingsViewModel settings;
    public readonly StartViewModel start;
    public readonly OptionsViewModel options;


    private BaseViewModel? _selectedViewModel;
    public BaseViewModel? SelectedViewModel
    {
        get { return _selectedViewModel; }
        set
        {
            _selectedViewModel = value;
            OnPropertyChanged(nameof(SelectedViewModel));
        }
    }

    public ICommand UpdateViewCommand { get; set; }

    public MainViewModel(MainWindow mainWindow)
    {
        MainWindow = mainWindow;

        settings = new SettingsViewModel(this);
        start = new StartViewModel(MainWindow);
        options = new OptionsViewModel();

        UpdateViewCommand = new UpdateViewCommand(this);
        UpdateViewCommand.Execute("Start");
    }
}
