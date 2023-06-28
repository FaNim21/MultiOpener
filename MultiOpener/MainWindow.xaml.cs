using MultiOpener.Components.Controls;
using MultiOpener.Properties;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener
{
    public partial class MainWindow : Window
    {
        //TODO: 2 Sprobowac przeniesc main viewModel do App.xaml.cs na bazie tego filmiku https://www.youtube.com/watch?v=dtq6qYlolh8 w 5:00
        public MainViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            labelVersion.Content = Consts.Version;

            MainViewModel = new MainViewModel(this);
            DataContext = MainViewModel;

            if (Settings.Default.MainWindowLeft == -1 && Settings.Default.MainWindowTop == -1)
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            else
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = Settings.Default.MainWindowLeft;
                Top = Settings.Default.MainWindowTop;
            }

            MainViewModel.settings.LoadStartUPPreset(Settings.Default.LastOpenedPresetName);

            Task task = Task.Factory.StartNew(CheckForUpdates);
        }

        public void EnableDisableChoosenHeadButton(string option)
        {
            StartButton.IsEnabled = true;
            SettingsButton.IsEnabled = true;
            OptionsButton.IsEnabled = true;

            switch (option)
            {
                case "Start": StartButton.IsEnabled = false; break;
                case "Settings": SettingsButton.IsEnabled = false; break;
                case "Options": OptionsButton.IsEnabled = false; break;
                default: Trace.WriteLine("Nieznana opcja: " + option); break;
            }
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void MinimizeButtonsClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            if (!MainViewModel.start.OpenedIsEmpty())
            {
                MessageBoxResult result = DialogBox.Show($"Are you certain about closing MultiOpener? There are still some processes that haven't been closed yet.", $"Closing!", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.None);
                if (result != MessageBoxResult.Yes)
                    return;
            }

            if (MainViewModel.SelectedViewModel == MainViewModel.options)
                MainViewModel.options.SaveOptions();

            Close();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Settings.Default.MainWindowLeft = Left;
            Settings.Default.MainWindowTop = Top;
            Settings.Default.LastOpenedPresetName = MainViewModel.settings.PresetName;

            Settings.Default.Save();

            MainViewModel.start.ConsoleViewModel.ConsoleLines.Clear();
        }

        public void OnShow()
        {
            Show();
            if (!App.Config.AlwaysOnTop)
            {
                Topmost = true;
                Topmost = false;
            }
        }

        private async Task CheckForUpdates()
        {
            var checker = new UpdateChecker();
            bool output = false;

            try
            {
                output = await checker.CheckForUpdates();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to check for updates: {ex.Message}");
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                UpdateButton.Visibility = output ? Visibility.Visible : Visibility.Hidden;
            });
        }
        private void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/FaNim21/MultiOpener/releases/latest") { UseShellExecute = true });
        }
    }
}
