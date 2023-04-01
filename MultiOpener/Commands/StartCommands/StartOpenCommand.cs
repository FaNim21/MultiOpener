using MultiOpener.ListView;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands
{
    public class StartOpenCommand : StartCommandBase
    {
        public MainWindow MainWindow { get; set; }

        public OpenningProcessLoadingWindow loadingProcesses;

        public CancellationTokenSource source = new();
        public CancellationToken token;


        public StartOpenCommand(StartViewModel startViewModel) : base(startViewModel)
        {
            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        public override void Execute(object? parameter)
        {
            if (string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName)) return;

            if ((MainWindow.openedProcess.Any() && MainWindow.openedProcess.Count != 0) || string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName) || Start == null) return;
            if (MainWindow.MainViewModel.settings.Opens == null || !MainWindow.MainViewModel.settings.Opens.Any()) return;

            //TODO: Zrobic nowy panel gdzie start bedzie mialo tylko open i po kliknieciu normalnie wyskakuje okno z progresem i po zaladowaniu odala sie specjalne okno do kontroli odpalonych aplikacji i do ich poprostu zamkniecia
            //co spowoduje ze nie bedzie mozna sie bawic ustawieniami co w sumie jest na minus, ale nie chce i tak jednak robic feature do odpalania aplikacji uzupelniajach z innych presetow ewentualnie dac tylko multimc jako caly czas odpalone w tle

            source = new();
            token = source.Token;
            Task task = Task.Run(OpenProgramsList, token);
        }

        public async Task OpenProgramsList()
        {
            int length = MainWindow.MainViewModel.settings.Opens.Count;
            int progressLength = length - 1;

            //TODO: tu dac validowanie wszystkich sciezek aplikacji i w momencie zwalidowania typu instances odpalic multimc, albo ewentualnie zapisac ze pod jakim indeksem jest i po pelnej walidacji otworzyc
            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (current.Validate())
                    return;

                if (current.GetType() == typeof(OpenInstance))
                    progressLength += ((OpenInstance)current).Quantity;
            }
            string infoText = "";

            Application.Current.Dispatcher.Invoke(delegate
            {
                Start.OpenButtonEnabled = false;
                MainWindow.Hide();
                float windowPositionX = (float)(MainWindow.Left + (MainWindow.Width / 2));
                float windowPositionY = (float)(MainWindow.Top + (MainWindow.Height / 2));
                loadingProcesses = new(this, windowPositionX, windowPositionY) { Owner = MainWindow };
                loadingProcesses.Show();
                loadingProcesses.progress.Maximum = progressLength;
            });


            for (int i = 0; i < length; i++)
            {
                var current = MainWindow.MainViewModel.settings.Opens[i];
                if (string.IsNullOrEmpty(current.PathExe))
                    continue;

                if (source.IsCancellationRequested)
                    break;

                Application.Current.Dispatcher.Invoke(delegate
                {
                    infoText = $"({i + 1}/{length}) Openning {current.Name}...";
                    loadingProcesses.SetText(infoText);
                    loadingProcesses.progress.Value++;
                });

                if (current.Type == OpenType.InstancesMultiMC)
                    await OpenMultiMcInstances((OpenInstance)current, infoText);
                else
                {
                    try
                    {
                        await Task.Delay(current.DelayBefore);
                        string executable = Path.GetFileName(current.PathExe);
                        string pathDir = Path.GetDirectoryName(current.PathExe) ?? "";

                        ProcessStartInfo processStartInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
                        Process? process = Process.Start(processStartInfo);

                        if (process != null)
                        {
                            process.EnableRaisingEvents = true;
                            process.Exited += Start.ProcessExited;
                            MainWindow.openedProcess.Add(process);
                        }
                        await Task.Delay(current.DelayAfter);
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses.Close();
                MainWindow.OnShow();
            });
        }

        private async Task OpenMultiMcInstances(OpenInstance open, string infoText = "")
        {
            //TODO: Zrobic support na kontrole kazdej instancji oddzielnie, a nie przez tylko glowny proces multimc
            //TODO: Zrobic oddzielne odpalania multimc na starcie, a pozniej kazda instancje po kolei bez juz delaya po pierwszej instancji najlepiej zeby sie odpalalo jako pierwsza apka z calej listy Opens
            try
            {
                await Task.Delay(open.DelayBefore);
                ProcessStartInfo processStartInfo = new(open.PathExe, $"--launch \"{open.Names[0]}\"") { UseShellExecute = true };
                Process? process = Process.Start(processStartInfo);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    loadingProcesses.SetText($"{infoText} -- Instance (1/{open.Quantity})");
                });

                if (process != null)
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += Start.ProcessExited;
                    MainWindow.openedProcess.Add(process);
                }

                await Task.Delay(10000);

                for (int i = 1; i < open.Quantity; i++)
                {
                    if (source.IsCancellationRequested)
                        return;

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        loadingProcesses.SetText($"{infoText} -- Instance ({i + 1}/{open.Quantity})");
                        loadingProcesses.progress.Value++;
                    });

                    processStartInfo = new(open.PathExe, $"--launch \"{open.Names[i]}\"") { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true };
                    Process? proc = Process.Start(processStartInfo);

                    //tu dodac nowa logike

                    MainWindow.openedProcess.Add(proc);

                    await Task.Delay(open.DelayBetweenInstances);
                }
                await Task.Delay(open.DelayAfter);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
