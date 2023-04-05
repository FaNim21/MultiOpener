using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    try
                    {
                        var current = MainWindow.opened[i];

                        Process process = Win32.GetProcessByHandle(current.handle.ToInt32());
                        process.Kill(true);
                        process.WaitForExit();
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
