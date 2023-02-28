using MultiOpener.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenCommand : StartCommandBase
    {
        public MainWindow MainWindow { get; set; }

        public StartOpenCommand(StartViewModel startViewModel) : base(startViewModel)
        {
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public override void Execute(object? parameter)
        {
            //TODO: -- INFO/FUTURE -- Temporary blockade preventing too much opennings for future with chaning preset feature
            if (MainWindow.openedProcess.Any() && MainWindow.openedProcess.Count != 0)
                return;
            if (Start == null)
                return;

            Start.OpenButtonEnabled = false;

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
                    string path = current.PathExe;
                    string[] splits = path.Split('\\');
                    string executable = splits[^1];
                    string pathDir = path.Remove(path.Length - (executable.Length + 1));

                    ProcessStartInfo processStartInfo = new()
                    {
                        WorkingDirectory = pathDir,
                        FileName = executable,
                        UseShellExecute = true
                    };

                    Process? process = Process.Start(processStartInfo);
                    if (process != null)
                    {
                        process.EnableRaisingEvents = true;
                        process.Exited += ProcessExited;
                        MainWindow.openedProcess.Add(process);
                    }
                }
                catch (Win32Exception ex)
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

        //TODO: Pomyslec o tym w przyszlosci co do wykrywania zamknietej aplikacji i aktualizowania panelu informacyjnego pod sekwencje w menu start itp itd
        //tez fakt zeby usprawnic wtedy jakos wlaczanie ponownie tych aplikacji zamknietych czy cos
        public void ProcessExited(object sender, EventArgs e)
        {
            if (sender is Process process)
                MainWindow.openedProcess.Remove(process);
        }
    }
}
