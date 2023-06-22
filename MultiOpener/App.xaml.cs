using MultiOpener.Items.Options;
using MultiOpener.Utils;
using System.Windows;

namespace MultiOpener
{
    public partial class App : Application
    {
        public static OptionSaveItem Config { get; set; } = new();

        public static InputController Input { get; set; } = new();

        public App()
        {
            InitializeComponent();
        }
    }
}
