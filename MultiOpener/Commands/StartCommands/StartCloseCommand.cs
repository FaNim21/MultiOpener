using MultiOpener.Items;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.ObjectModel;
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
            ObservableCollection<OpenedProcess> opened = MainWindow.MainViewModel.start.Opened;

            if (Start == null) return;
            if (opened.Count == 0 || opened == null)
            {
                Start.OpenButtonName = "OPEN";
                return;
            }

            //TODO: 3 Trzeba ogarnac zeby jak nie zamknie jakiegos procesu to zeby nie czyscilo calej listy

            if (MessageBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                int length = opened.Count;
                for (int i = 0; i < length; i++)
                {
                    var current = opened[i];

                    if (current.Hwnd != IntPtr.Zero)
                    {
                        try
                        {
                            Win32.CloseProcessByHwnd(current.Hwnd);
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
                            Win32.CloseProcessByHandle(current.Handle);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }
                }

                opened.Clear();
                MainWindow.MainViewModel.start.ClearOpened();
                Start.OpenButtonName = "OPEN";
            }
        }
    }
}
