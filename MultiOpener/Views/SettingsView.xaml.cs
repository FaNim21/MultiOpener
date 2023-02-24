using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Views
{
    public partial class SettingsView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public SettingsView()
        {
            InitializeComponent();

            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        private void AddNewOpenItem(object sender, RoutedEventArgs e)
        {
            var newOpen = new OpenItem(AddNameField.Text);
            itemList.Items.Add(newOpen);

            /*newOpen.PathExe
            MainWindow.AddItem();*/
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
                /*TargetTodoItem = element.DataContext;
                InsertedTodoItem = e.Data.GetData(DataFormats.Serializable);

                TodoItemInsertedCommand?.Execute(null);*/
            }
        }

        private void ItemListDragOver(object sender, DragEventArgs e)
        {
            object todoItem = e.Data.GetData(DataFormats.Serializable);
            //itemList.Items.Add(new OpenListItem(todoItem.ToString()));
        }

        private void ItemListDragLeave(object sender, DragEventArgs e)
        {
            /*HitTestResult result = VisualTreeHelper.HitTest(lvItems, e.GetPosition(lvItems));

            if (result == null)
            {
                if (TodoItemRemovedCommand?.CanExecute(null) ?? false)
                {
                    RemovedTodoItem = e.Data.GetData(DataFormats.Serializable);
                    TodoItemRemovedCommand?.Execute(null);
                }
            }*/
        }

        private void OnItemListClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                //TODO: Ustawic tutaj caly lewy panel pod to zeby zmieniac wartosci dla tego kliknietego itemu
                MessageBox.Show("ELOOO");
            }
        }
    }
}
