using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
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
                MainWindow.opened = new List<OpenedProcess>();


                /*int length = MainWindow.openedProcess.Count;
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        var current = MainWindow.openedProcess[i];

                        if (current.MainModule != null)
                            Trace.WriteLine(current.MainModule.ModuleName + " -- " + current.MainModule.FileName);

                        if (current.ProcessName.ToLower().StartsWith("autohotkey"))
                        {
                            Process[] processes = Process.GetProcessesByName("AutoHotkey");
                            for (int j = 0; j < processes.Length; j++)
                            {
                                processes[j].Kill();
                                processes[j].WaitForExit();
                            }
                        }
                        else
                        {
                            current.Kill(true);
                            current.WaitForExit();
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.ToString());
                    }
                }*/
            }

            MainWindow.openedProcess = new();
            Start.OpenButtonName = "OPEN";
        }
    }
}
