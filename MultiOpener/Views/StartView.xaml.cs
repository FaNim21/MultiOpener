using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Views
{
    public partial class StartView : UserControl
    {
        public StartView()
        {
            InitializeComponent();
        }

        //TODO: 9 --REMOVE-- po tym jak juz skoncze testowac
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(new Items.OpenedProcess());
        }
    }
}
