using MultiOpener.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private BaseViewModel? _selectedViewModel;

        public BaseViewModel SelectedViewModel
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
            UpdateViewCommand = new UpdateViewCommand(this, mainWindow);
            UpdateViewCommand.Execute("Start");
        }
    }
}
