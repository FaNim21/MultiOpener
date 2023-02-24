using MultiOpener.Commands;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainWindow MainWindow { get; set; }

        public SettingsViewModel Settings { get; } = new SettingsViewModel();
        public StartViewModel Start { get; } = new StartViewModel();

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

            UpdateViewCommand = new UpdateViewCommand(this);
            UpdateViewCommand.Execute("Start");
        }
    }
}
