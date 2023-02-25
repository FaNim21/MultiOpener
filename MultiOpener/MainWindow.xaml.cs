using MultiOpener.ViewModels;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
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

        [JsonConstructor]
        public OpenItem(string Name = "", string PathExe = "", bool IsDelayAfter = false, int DelayAfter = 0)
        {
            this.Name = Name;
            this.PathExe = PathExe;
            this.IsDelayAfter = IsDelayAfter;
            this.DelayAfter = DelayAfter;
        }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<OpenItem> Opens { get; set; }

        public  MainViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Opens = new ObservableCollection<OpenItem>();
            MainViewModel = new MainViewModel(this);
            DataContext = MainViewModel;

            //tymczasowo do testow
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void AddItem(OpenItem item)
        {
            Opens.Add(item);
        }
        public void RemoveItem(OpenItem item)
        {
            Opens.Remove(item);
        }

        public void InsertItem(OpenItem insertedItem, OpenItem targetItem)
        {
            if (insertedItem == targetItem)
                return;

            int oldIndex = Opens.IndexOf(insertedItem);
            int nextIndex = Opens.IndexOf(targetItem);

            if (oldIndex != -1 && nextIndex != -1)
                Opens.Move(oldIndex, nextIndex);
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
