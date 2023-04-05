using MultiOpener.ListView;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            if (Start == null) return;

            if (Start.OpenButtonName.Equals("OPEN"))
            {
                if (string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName)) return;

                if ((MainWindow.opened.Any() && MainWindow.opened.Count != 0) || string.IsNullOrEmpty(MainWindow.MainViewModel.settings.PresetName) || Start == null) return;
                if (MainWindow.MainViewModel.settings.Opens == null || !MainWindow.MainViewModel.settings.Opens.Any()) return;

                Start.OpenButtonName = "CLOSE";
                //Uwzglednic cancelowanie i bledy w validowaniu do zamiany guzika spowrotem na close


                //TODO: Zrobic nowy panel gdzie start bedzie mialo tylko open i po kliknieciu normalnie wyskakuje okno z progresem i po zaladowaniu odala sie specjalne okno do kontroli odpalonych aplikacji i do ich poprostu zamkniecia
                //co spowoduje ze nie bedzie mozna sie bawic ustawieniami co w sumie jest na minus, ale nie chce i tak jednak robic feature do odpalania aplikacji uzupelniajach z innych presetow ewentualnie dac tylko multimc jako caly czas odpalone w tle

                source = new();
                token = source.Token;
                Task task = Task.Run(OpenProgramsList, token);
            }
            else
            {
                Start.CloseCommand.Execute(null);
            }
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
                {
                    try
                    {
                        ProcessStartInfo startInfo = new(current.PathExe) { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
                        Process? process = Process.Start(startInfo);
                        if (process != null)
                        {
                            OpenedProcess open = new();
                            open.handle = process.Handle;
                            MainWindow.opened.Add(open);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }

                    progressLength += ((OpenInstance)current).Quantity;
                }
            }
            string infoText = "";

            Application.Current.Dispatcher.Invoke(delegate
            {
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

                            process.WaitForInputIdle();

                            OpenedProcess open = new();
                            open.handle = process.Handle;

                            //TODO: stestowac definicje odpalania aplikacji konsolowych lub non-gui authotkey etc etc
                            int errors = 0;
                            while (!open.SetHwnd() && errors < 100)
                            {
                                await Task.Delay(200);
                                errors++;
                            }
                            MainWindow.opened.Add(open);
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
                infoText = $"Waiting for apps to open...";
                loadingProcesses.SetText(infoText);
            });

            await Task.Delay(500);

            //to trzeba dostosowac do odpalanych programow zeby sprawdzac czy wszystkiego programy maja okno ewentualnie?
            //zeby nie ustalac statycznie delaya do ustalania hwnd
            /*for (int i = 0; i < MainWindow.opened.Count; i++)
            {
                var current = MainWindow.opened[i];
                current.SetHwnd();
            }*/

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadingProcesses.Close();
                MainWindow.OnShow();
            });
        }

        private async Task OpenMultiMcInstances(OpenInstance open, string infoText = "")
        {
            //TODO: Zrobic support na kontrole kazdej instancji oddzielnie, a nie przez tylko glowny proces multimc
            try
            {
                await Task.Delay(open.DelayBefore);

                ProcessStartInfo startInfo = new(open.PathExe) { UseShellExecute = true };
                for (int i = 0; i < open.Quantity; i++)
                {
                    await Task.Delay(i == 0 ? 0 : i == 1 ? 10000 : open.DelayBetweenInstances);

                    if (source.IsCancellationRequested)
                        return;

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        loadingProcesses.SetText($"{infoText} -- Instance ({i + 1}/{open.Quantity})");
                        loadingProcesses.progress.Value++;
                    });

                    startInfo.Arguments = $"--launch \"{open.Names[i]}\"";
                    Process? process = Process.Start(startInfo);
                    if(process != null)
                    {
                        OpenedProcess opened = new();
                        opened.handle = process.Handle;
                        MainWindow.opened.Add(opened);
                    } 
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
