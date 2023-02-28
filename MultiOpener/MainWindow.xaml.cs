using MultiOpener.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener
{
    public partial class MainWindow : Window
    {
        public List<Process> openedProcess = new();

        public  MainViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel = new MainViewModel(this);
            DataContext = MainViewModel;

            //tymczasowo do testow
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void EnableDisableChoosenHeadButton(string option)
        {
            bool result = option.Equals("Start");
            StartButton.IsEnabled = !result;
            SettingsButton.IsEnabled = result;
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void MinimizeButtonsClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void ExitButtonClick(object sender, RoutedEventArgs e) => Close();
    }
}
