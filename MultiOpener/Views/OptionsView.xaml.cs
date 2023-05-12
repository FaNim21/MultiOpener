using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Views
{
    public partial class OptionsView : UserControl
    {
        public OptionsView()
        {
            InitializeComponent();

            versionLabel.Content = Consts.Version;
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
    }
}
