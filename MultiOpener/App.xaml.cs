using MultiOpener.Items.Options;
using System.Windows;

namespace MultiOpener
{
    public partial class App : Application
    {
        public static OptionSaveItem config { get; set; } = new();
    }
}
