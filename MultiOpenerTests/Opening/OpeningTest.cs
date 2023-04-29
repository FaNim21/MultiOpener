using System.Windows;
using MultiOpener;
using System.Windows.Threading;
using MultiOpener.ViewModels;

namespace MultiOpenerTests.Opening;

[TestFixture]
internal class OpeningTest
{
    private App? _app;
    private MainWindow? _mainWindow;

    [SetUp]
    public void SetUp()
    {
        var t = new Thread(() =>
        {
            _app = new App
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };

            _app.InitializeComponent();
            Dispatcher.Run();
        });

        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        Thread.Sleep(100);

        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow = (MainWindow)_app.MainWindow;
        });
    }

    [TearDown]
    public void TearDown()
    {
        _mainWindow?.Dispatcher?.Invoke(() => _mainWindow.Close());
        _app?.Dispatcher?.InvokeShutdown();
    }

    [Test]
    public void Opening_app_is_working()
    {
        Thread.Sleep(250);

        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow.MainViewModel.UpdateViewCommand.Execute("Settings");
        });
        Thread.Sleep(250);

        Assert.That((SettingsViewModel)_mainWindow.MainViewModel.SelectedViewModel, Is.Not.EqualTo(null));
    }

    /*[Test]
    public async Task OpenMethod_Should_Start_New_Process()
    {
        // Arrange
        var openItem = new OpenItem
        {
            Name = "Notepad",
            PathExe = "C:\\Windows\\System32\\notepad.exe",
            DelayBefore = 0,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await openItem.Open(null, cancellationTokenSource);

        // Assert
        var processName = Path.GetFileNameWithoutExtension(openItem.PathExe);
        var process = Process.GetProcessesByName(processName).FirstOrDefault();
        Assert.That(process, Is.Not.Null);

        cancellationTokenSource.Cancel();
    }*/
}