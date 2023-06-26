using MultiOpener.Components.Controls;
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
        }

        private void OpenVersionsSite(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (DialogBox.Show($"Do you want to open MultiOpener site to check for new updates or patch notes?", $"Opening Github Release site For MultiOpener", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                var processStart = new ProcessStartInfo(e.Uri.ToString())
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(processStart);
            }
        }

        private void OpenFeedbackSite(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (DialogBox.Show($"Do you want to open MultiOpener site for enter discussions?", $"Opening Github Discussions For MultiOpener", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
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
