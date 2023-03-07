using MultiOpener.Commands.StartCommands;
using System;
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

        //TODO: Pomyslec o tym w przyszlosci co do wykrywania zamknietej aplikacji i aktualizowania panelu informacyjnego pod sekwencje w menu start itp itd
        //tez fakt zeby usprawnic wtedy jakos wlaczanie ponownie tych aplikacji zamknietych czy cos
        public void ProcessExited(object sender, EventArgs e)
        {
            /*if (sender is Process process)
                MainWindow.openedProcess.Remove(process);*/
        }
    }
}
