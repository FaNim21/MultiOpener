using System;
using System.Windows;

namespace MultiOpener.Windows
{
    public partial class WindowOpenInstancesSetup : Window
    {
        public WindowOpenInstancesSetup()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            Application.Current.MainWindow.Show();

            base.OnClosed(e);
        }
    }
}
