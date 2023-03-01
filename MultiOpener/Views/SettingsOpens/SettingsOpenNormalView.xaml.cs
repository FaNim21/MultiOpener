using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Views.SettingsOpens
{
    /// <summary>
    /// Logika interakcji dla klasy SettingsOpenNormalView.xaml
    /// </summary>
    public partial class SettingsOpenNormalView : UserControl
    {
        public SettingsOpenNormalView()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
