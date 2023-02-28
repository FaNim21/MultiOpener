using MultiOpener.Commands.StartCommands;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public ICommand OpenCommand { get; set; }
        public ICommand CloseCommand { get; set; }


        public StartViewModel()
        {
            OpenCommand = new StartOpenCommand(this);
            CloseCommand = new StartCloseCommand(this);
        }
    }
}
