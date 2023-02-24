using MultiOpener.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener
{
    public class OpenItem
    {
        public string Name { get; set; }
        public string PathExe { get; set; }
        public bool IsDelayAfter { get; set; }
        public int DelayAfter { get; set; }

        public OpenItem(string name = "", string pathExe = "", bool isDelayAfter = false, int delay = 0)
        {
            Name = name;
            PathExe = pathExe;
            IsDelayAfter = isDelayAfter;
            DelayAfter = delay;
        }
    }

    public partial class MainWindow : Window
    {
        public List<OpenItem> opens = new();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel(this);

            //tymczasowo do testow
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void AddItem(OpenItem item)
        {
            opens.Add(item);
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
