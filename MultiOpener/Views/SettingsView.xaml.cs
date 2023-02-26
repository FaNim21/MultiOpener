using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using MultiOpener.ListView;

namespace MultiOpener.Views
{
    public partial class SettingsView : UserControl
    {
        public ICommand OnListItemClickCommand
        {
            get { return (ICommand)GetValue(OnListItemClickCommandProperty); }
            set { SetValue(OnListItemClickCommandProperty, value); }
        }

        public static readonly DependencyProperty OnListItemClickCommandProperty =
            DependencyProperty.Register("OnListItemClickCommand", typeof(ICommand), typeof(SettingsView), new PropertyMetadata(null));



        public ICommand TextBlockDragOverCommand
        {
            get { return (ICommand)GetValue(TextBlockDragOverCommandProperty); }
            set { SetValue(TextBlockDragOverCommandProperty, value); }
        }

        public static readonly DependencyProperty TextBlockDragOverCommandProperty =
            DependencyProperty.Register("TextBlockDragOverCommand", typeof(ICommand), typeof(SettingsView), new PropertyMetadata(null));


        public SettingsView()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBlockMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement frameworkElement)
            {
                DragDrop.DoDragDrop(frameworkElement, new DataObject(DataFormats.Serializable, frameworkElement.DataContext), DragDropEffects.Move);
            }
        }

        private void OnItemListClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                OnListItemClickCommand?.Execute((OpenItem)item.DataContext);    
            }
        }

        private void TextBlockDragOver(object sender, DragEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var targetedItem = (OpenItem)element.DataContext;
                var insertedItem = (OpenItem)e.Data.GetData(DataFormats.Serializable);

                OpenItem[] elements = new OpenItem[2]{ targetedItem, insertedItem };
                TextBlockDragOverCommand?.Execute(elements);
            }
        }
    }
}
