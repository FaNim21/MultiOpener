using MultiOpener.Commands.StartCommands;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public ICommand OpenCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        private bool _openButtonEnabled = true;
        public bool OpenButtonEnabled
        {
            get { return _openButtonEnabled; }
            set
            {
                _openButtonEnabled = value;
                OnPropertyChanged(nameof(OpenButtonEnabled));
            }
        }


        public StartViewModel()
        {
            OpenCommand = new StartOpenCommand(this);
            CloseCommand = new StartCloseCommand(this);
        }
    }
}
