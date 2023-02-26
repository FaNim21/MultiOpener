using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Views
{
    public partial class StartView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public StartView()
        {
            InitializeComponent();

            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Wrzucic do watku cala liste i odpalajac ja sprawdzac czy dayn program juz nie istnieje najprosciej przez zapisywanie procesu do zmiennej czyli na przyszlosc pamietac zeby zabezpieczyc resetowanie programu czy cos albo przez zapamietywanie numeru procesu
            int length = MainWindow.MainViewModel.settings.Opens.Count;
            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (string.IsNullOrEmpty(current.PathExe))
                    continue;

                try
                {
                    //TODO: --PROBLEM-- TU BEDZIE PROBLEM Z LINUX'EM I MOZLIWE ZE Z MAC'IEM
                    string[] splits = current.PathExe.Split('\\');
                    string executable = splits[^1];
                    string path = current.PathExe.Remove(current.PathExe.Length - (executable.Length + 1));

                    ProcessStartInfo processStartInfo = new(executable)
                    {
                        WorkingDirectory = path,
                        UseShellExecute = true
                    };
                    Process? process = Process.Start(processStartInfo);
                    if (process != null)
                        MainWindow.openedProcess.Add(process);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            //TEMPLATKA STARTOWANIA INSTANCJI PIERWSZY MINECRAFT POTRZEBUJE MINIMUM 5 SEKUND NA TO ZEBY MOC RESZTE ODPALIC
            /*ProcessStartInfo processStartInfo = new("C:\\Games\\MultiMC\\MultiMC.exe", "-l \"1.16 speedrun instance 1\"")
            {
                UseShellExecute = true
            };
            var proc = Process.Start(processStartInfo);
            *//*while (proc.Responding)
            {
                Thread.Sleep(100);
                proc.Refresh();
            }*//*
            //Process.Start(processStartInfo);
            //Thread.Sleep(5000);

            for (int i = 1; i < 4; i++)
            {
                processStartInfo = new("C:\\Games\\MultiMC\\MultiMC.exe", $"-l \"1.16 speedrun instance {i + 1}\"")
                {
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }*/
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
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
