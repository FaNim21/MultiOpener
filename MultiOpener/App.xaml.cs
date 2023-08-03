using MultiOpener.Entities.Options;
using System.Threading;
using System.Windows;

namespace MultiOpener;

public partial class App : Application
{
    public static OptionSaveItem Config { get; set; } = new();

    private static Mutex? _mutex;

    public App()
    {
        
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        const string appName = "MultiOpener";

        _mutex = new Mutex(true, appName, out bool createdNew);

        if (!createdNew)
        {
            Current.Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        base.OnExit(e);
    }
}
