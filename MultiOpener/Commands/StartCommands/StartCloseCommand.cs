using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartCloseCommand : StartCommandBase
    {
        public MainWindow MainWindow { get; set; }

        public StartCloseCommand(StartViewModel startViewModel) : base(startViewModel)
        {
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public override void Execute(object? parameter)
        {
            int length = MainWindow.openedProcess.Count;
            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.openedProcess[i];
                current.Kill();
            }
        }
    }
}
