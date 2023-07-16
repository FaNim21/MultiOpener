using MultiOpener.Entities.Options;
using MultiOpener.Utils;
using System.Windows;

namespace MultiOpener;

public partial class App : Application
{
    public static OptionSaveItem Config { get; set; } = new();

    public static InputController Input { get; set; } = new();


    public App()
    {
        
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        //((App)Current).

        base.OnStartup(e);
    }
}
