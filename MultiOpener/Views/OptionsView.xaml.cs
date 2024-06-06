using MultiOpener.Components.Controls;
using MultiOpener.Utils;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = RegexPatterns.NumbersPattern();
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
