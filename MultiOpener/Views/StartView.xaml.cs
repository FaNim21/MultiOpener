using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Views
{
    public partial class StartView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public StartView()
        {
            InitializeComponent();

            MainWindow = (MainWindow)Application.Current.MainWindow;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Wrzucic do watku cala liste i odpalajac ja sprawdzac czy dayn program juz nie istnieje najprosciej przez zapisywanie procesu do zmiennej czyli na przyszlosc pamietac zeby zabezpieczyc resetowanie programu czy cos albo przez zapamietywanie numeru procesu
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: TO samo co z OpenButton tylko zeby zamknac wszystkie procesy z listy
        }
    }
}
