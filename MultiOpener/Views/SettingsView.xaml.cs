using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;

namespace MultiOpener.Views
{
    public partial class SettingsView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public OpenItem? currentChosen;

        private const string _saveFileName = "settings.json";
        private static string _directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + _saveFileName ?? "C:\\" + _saveFileName;   //Tymczasowo

        public SettingsView()
        {
            InitializeComponent();

            MainWindow = (MainWindow)Application.Current.MainWindow;
            DataContext = MainWindow;

            LeftPanel.Visibility = Visibility.Hidden;

            //TYMCZASOWO TU JEST LADOWANIE
            if (MainWindow.Opens != null && !MainWindow.Opens.Any())
            {
                _directoryPath = "C:\\Users\\Filip\\Desktop\\test\\" + _saveFileName;

                if (!File.Exists(_directoryPath))
                    return;

                string text = File.ReadAllText(_directoryPath) ?? "";
                if (string.IsNullOrEmpty(text))
                    return;

                var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
                MainWindow.Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());
            }
        }

        private void AddNewOpenItem(object sender, RoutedEventArgs e)
        {
            var newOpen = new OpenItem(AddNameField.Text);

            MainWindow.AddItem(newOpen);
            AddNameField.Text = "";
        }

        private void RemoveCurrentOpenButtonClick(object sender, RoutedEventArgs e)
        {
            LeftPanel.Visibility = Visibility.Hidden;
            if(currentChosen != null)
                MainWindow.RemoveItem(currentChosen);
        }

        private void SaveCurrentOpenButtonClick(object sender, RoutedEventArgs e)
        {
            int n = MainWindow.Opens.Count;
            for (int i = 0; i < n; i++)
            {
                var open = MainWindow.Opens[i];
                if (open == currentChosen)
                {
                    open.PathExe = AppDirectoryPathField.Text;
                    open.IsDelayAfter = delayCheckBox.IsChecked.Value;
                    open.DelayAfter = int.Parse(delayTimeField.Text);

                }
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var data = JsonSerializer.Serialize(MainWindow.Opens);
            //TODO: PAMIETAC O TYM ZEBY PRZYWROCIC SCIEZKE DO PLIKU
            _directoryPath = "C:\\Users\\Filip\\Desktop\\test\\" + _saveFileName;
            File.WriteAllText(_directoryPath, data);
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            //TODO: --FUTURE-- Wczytac json do listy
        }

        private void TextBlockMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement frameworkElement)
            {
                DragDrop.DoDragDrop(frameworkElement, new DataObject(DataFormats.Serializable, frameworkElement.DataContext), DragDropEffects.Move);
            }
        }

        private void TextBlockDragOver(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var targetedItem = (OpenItem)element.DataContext;
                var insertedItem = (OpenItem)e.Data.GetData(DataFormats.Serializable);

                MainWindow.InsertItem(insertedItem, targetedItem);
            }
        }

        private void OnItemListClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                currentChosen = (OpenItem)item.DataContext;

                if (LeftPanel.Visibility == Visibility.Hidden)
                    LeftPanel.Visibility = Visibility.Visible;

                NameLabel.Content = currentChosen.Name;
                AppDirectoryPathField.Text = currentChosen.PathExe;
                delayCheckBox.IsChecked = currentChosen.IsDelayAfter;
                delayTimeField.Text = currentChosen.DelayAfter.ToString();
            }
        }
    }
}
