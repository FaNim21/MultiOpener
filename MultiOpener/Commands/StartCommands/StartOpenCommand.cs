using MultiOpener.ListView;
using MultiOpener.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            //TODO: -- INFO/FUTURE -- Temporary blockade preventing too much opennings for future with changing preset feature
            if (MainWindow.openedProcess.Any() && MainWindow.openedProcess.Count != 0)
                return;
            if (Start == null)
                return;

            Start.OpenButtonEnabled = false;

            //TODO: Wrzucic do watku cala liste i odpalajac ja sprawdzac czy dayn program juz nie istnieje najprosciej przez zapisywanie procesu do zmiennej czyli na przyszlosc pamietac zeby zabezpieczyc resetowanie programu czy cos albo przez zapamietywanie numeru procesu
            //TODO: Zabezpieczyc klikanie czekego kolwiek w programie typu wylaczanie go opcja zamkniecia watku, zeby nie musiec czekac do konca odpalenia wszystkiego i tez fakt zeby podczas odpalania nie zamykac procesow trzeba to zablokowac
            Thread thread = new(new ThreadStart(OpenProgramsList));
            thread.Start();
            //OpenProgramsList();
        }

        public void OpenProgramsList()
        {
            int length = MainWindow.MainViewModel.settings.Opens.Count;
            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (string.IsNullOrEmpty(current.PathExe))
                    continue;

                if (current.Type == OpenType.InstancesMultiMC)
                {
                    OpenMultiMcInstances((OpenInstance)current);
                }
                else
                {
                    try
                    {
                        Thread.Sleep(current.DelayBefore);
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
                            process.Exited += Start.ProcessExited;
                            MainWindow.openedProcess.Add(process);
                        }
                        Thread.Sleep(current.DelayAfter);
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void OpenMultiMcInstances(OpenInstance open)
        {
            try
            {
                Thread.Sleep(open.DelayBefore);
                ProcessStartInfo processStartInfo = new(open.PathExe, $"-l \"{open.Names[0]}\"")
                {
                    UseShellExecute = true
                };
                Process? process = Process.Start(processStartInfo);
                if (process != null)
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += Start.ProcessExited;
                    MainWindow.openedProcess.Add(process);
                }
                Thread.Sleep(10000);

                for (int i = 1; i < open.Names.Count; i++)
                {
                    processStartInfo = new(open.PathExe, $"-l \"{open.Names[i]}\"") { UseShellExecute = true };
                    Process.Start(processStartInfo);
                    Thread.Sleep(open.DelayBetweenInstances);
                }
                Thread.Sleep(open.DelayAfter);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
