using MultiOpener.ViewModels;
using System;
using System.Threading.Tasks;
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
            Task task = Task.Run(Close);
        }

        public async Task Close()
        {
            if (Start == null) return;
            if (MainWindow.MainViewModel.start.Opened.Count == 0 || MainWindow.MainViewModel.start.Opened == null)
            {
                Start.OpenButtonName = "OPEN";
                return;
            }

            if (MessageBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < MainWindow.MainViewModel.start.Opened.Count; i++)
                {
                    var current = MainWindow.MainViewModel.start.Opened[i];

                    bool isSucceed = await current.Close();

                    if (isSucceed || current.Hwnd == IntPtr.Zero || !current.StillExist())
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            MainWindow.MainViewModel.start.RemoveOpened(current);
                            i--;
                        });
                    }
                }

                if (MainWindow.MainViewModel.start.Opened.Count == 0)
                    Start.OpenButtonName = "OPEN";
            }
        }
    }
}
