using MultiOpener.Items;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.ObjectModel;
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
                    bool isSucceed = true;

                    if (current.Hwnd != IntPtr.Zero)
                    {
                        try
                        {
                            isSucceed = await Win32.CloseProcessByHwnd(current.Hwnd);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"Cannot close {current.WindowTitle ?? ""} \n{e}");
                        }
                    }
                    else
                    {
                        try
                        {
                            await Win32.CloseProcessByPid(current.Pid);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }

                    if (isSucceed || current.Hwnd == IntPtr.Zero)
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
