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
            if (MainWindow.openedProcess.Count == 0 || MainWindow.openedProcess == null || Start == null) return;

            if (MessageBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                int length = MainWindow.openedProcess.Count;
                for (int i = 0; i < length; i++)
                {
                    var current = MainWindow.openedProcess[i];
                    current.Kill();
                }
            }

            Start.OpenButtonEnabled = true;
        }
    }
}
