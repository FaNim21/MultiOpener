using MultiOpener.Commands;
using System.Collections.Generic;
using System.Windows.Input;

namespace MultiOpener.ViewModels;

public class MainViewModel : BaseViewModel
{
    public MainWindow MainWindow { get; set; }

    public List<BaseViewModel> baseViewModels = new();

    public readonly SettingsViewModel settings;
    public readonly StartViewModel start;


    private BaseViewModel? _selectedViewModel;
    public BaseViewModel? SelectedViewModel
    {
        get => _selectedViewModel;
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

        baseViewModels.Add(start);
        baseViewModels.Add(settings);
        baseViewModels.Add(new OptionsViewModel());

        //More sub menu
        //baseViewModels.Add(new)  //TODO: 0 dodac reset tracker view model do wyswietlania wszystkich statystyk zebranych z procesu wbudowanego reset trackera

        UpdateViewCommand = new UpdateViewCommand(this);
        UpdateViewCommand.Execute("Start");
        MainWindow.StartItem.IsChecked = true;
    }
}
