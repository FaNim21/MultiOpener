using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Views.SettingsOpens;

public partial class SettingsOpenResetTrackerView : UserControl
{
    public SettingsOpenResetTrackerView()
    {
        InitializeComponent();
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
