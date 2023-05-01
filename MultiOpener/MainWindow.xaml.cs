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
            bool result = option.Equals("Start");
            if (option.Equals("Start"))
            {
                StartButton.IsEnabled = false;
                SettingsButton.IsEnabled = true;
                InformationsButton.IsEnabled = true;
            }
            else if (option.Equals("Settings"))
            {
                StartButton.IsEnabled = true;
                SettingsButton.IsEnabled = false;
                InformationsButton.IsEnabled = true;
            }
            else if (option.Equals("Informations"))
            {
                StartButton.IsEnabled = true;
                SettingsButton.IsEnabled = true;
                InformationsButton.IsEnabled = false;
            }
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void MinimizeButtonsClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void ExitButtonClick(object sender, RoutedEventArgs e) => Close();

        private void OnClosed(object sender, EventArgs e)
        {
            Settings.Default.MainWindowLeft = Left;
            Settings.Default.MainWindowTop = Top;
            Settings.Default.LastOpenedPresetName = MainViewModel.settings.PresetName;

            Settings.Default.Save();
        }

        public void OnShow()
        {
            Show();
            Topmost = true;
            Topmost = false;
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
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
