using System.Diagnostics;
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (MessageBox.Show($"Do you want to open MultiOpener site to check for new updates or patch notes?", $"Opening Github Release site For MultiOpener", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var processStart = new ProcessStartInfo(e.Uri.ToString())
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(processStart);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(((MainWindow)Application.Current.MainWindow).openedProcess.Count);

            var pr = Process.GetProcessesByName("javaw");
            if (pr == null) return;

            for (int i = 0; i < pr.Length; i++)
            {
                var current = pr[i];
                Trace.WriteLine(current.MainWindowTitle);
                //Trace.WriteLine(current.MainWindowTitle);
            }
        }
    }
}
