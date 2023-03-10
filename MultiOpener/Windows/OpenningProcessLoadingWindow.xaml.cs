using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Windows
{
    public partial class OpenningProcessLoadingWindow : Window
    {
        public OpenningProcessLoadingWindow()
        {
            InitializeComponent();
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
