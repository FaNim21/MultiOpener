using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
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
            if (MainWindow.opened.Count == 0 || MainWindow.opened == null || Start == null) return;

            if (MessageBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                int length = MainWindow.opened.Count;
                for (int i = 0; i < length; i++)
                {
                    var current = MainWindow.opened[i];

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

                MainWindow.opened = new List<OpenedProcess>();
                Start.OpenButtonName = "OPEN";
            }
        }
    }
}
