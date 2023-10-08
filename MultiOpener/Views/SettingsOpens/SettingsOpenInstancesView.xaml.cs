using MultiOpener.Utils;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Views.SettingsOpens
{
    public partial class SettingsOpenInstancesView : UserControl
    {
        public SettingsOpenInstancesView()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = RegexPatterns.NumbersPattern();
            e.Handled = regex.IsMatch(e.Text);
        }

        private void InstanceNumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(((TextBox)sender).Text + e.Text);
        }

        public static bool IsValid(string str)
        {
            return int.TryParse(str, out int i) && i >= 1 && i <= 32;
        }
    }
}
