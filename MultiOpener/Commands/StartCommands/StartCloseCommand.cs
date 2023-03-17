using MultiOpener.ViewModels;
using System;
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
            if (MainWindow.openedProcess.Count == 0 || MainWindow.openedProcess == null || Start == null) return;

            if (MessageBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                int length = MainWindow.openedProcess.Count;
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        var current = MainWindow.openedProcess[i];
                        current.Exited -= Start.ProcessExited;

                        Trace.WriteLine(current.ProcessName + " -- " + current.Id + " -- " + current.MainWindowTitle + " -- " + current.StartInfo.FileName);
                        if (current.MainModule != null)
                            Trace.WriteLine(current.MainModule.ModuleName + " -- " + current.MainModule.FileName);

                        if (current.ProcessName.ToLower().StartsWith("autohotkey"))
                        {
                            Process[] processes = Process.GetProcessesByName("AutoHotkey");
                            for (int j = 0; j < processes.Length; j++)
                            {
                                processes[j].Kill();
                            }
                        }
                        else
                            current.Kill(true);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.ToString());
                    }
                }
            }

            MainWindow.openedProcess = new();
            Start.OpenButtonEnabled = true;
        }
    }
}
