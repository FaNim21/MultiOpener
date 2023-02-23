using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiOpener.ListView;

namespace MultiOpener.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void TextBlockMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement frameworkElement)
            {
                DragDrop.DoDragDrop(frameworkElement, new DataObject(DataFormats.Serializable, frameworkElement.DataContext), DragDropEffects.Move);
            }
        }

        private void AddNewOpenItem(object sender, RoutedEventArgs e)
        {
            itemList.Items.Add(new OpenListItem(AddNameField.Text));
        }

        private void SaveCurrentOpenButtonClick(object sender, RoutedEventArgs e)
        {
            //TODO: Aktualizowac element z listy po indexie
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            //TODO: Zapisac cala liste do pliku json
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            //TODO: Wczytac json do listy
        }
    }
}
