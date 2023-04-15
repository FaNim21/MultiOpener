using MultiOpener.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartCloseCommand : StartCommandBase
    {
        public StartCloseCommand(StartViewModel startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            Task task = Task.Run(Close);
        }

        public async Task Close()
        {
            if (Start == null) return;
            if (Start.OpenedIsEmpty())
            {
                Start.OpenButtonName = "OPEN";
                return;
            }

            if (MessageBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < Start.Opened.Count; i++)
                {
                    var current = Start.Opened[i];
                    current.FastUpdate();

                    bool isSucceed = await current.Close();
                    if (isSucceed || current.Hwnd == IntPtr.Zero || !current.StillExist())
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            Start.RemoveOpened(current);
                            i--;
                        });
                    }
                }

                if (Start.OpenedIsEmpty())
                    Start.OpenButtonName = "OPEN";
            }
        }
    }
}
