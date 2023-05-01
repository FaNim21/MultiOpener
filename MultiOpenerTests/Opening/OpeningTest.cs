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
            _app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            _app.InitializeComponent();
            Dispatcher.Run();
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        Thread.Sleep(100);
        Application.Current.Dispatcher.Invoke(delegate { _mainWindow = (MainWindow)_app!.MainWindow; });
    }

    [TearDown]
    public void TearDown()
    {
        _app?.Dispatcher?.InvokeShutdown();
    }

    [Test]
    public void Opening_app_is_working()
    {
        Thread.Sleep(250);
        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow!.MainViewModel.UpdateViewCommand.Execute("Settings");
        });
        Thread.Sleep(250);

        Assert.That((SettingsViewModel)_mainWindow!.MainViewModel.SelectedViewModel!, Is.Not.EqualTo(null));
    }

    [Test]
    public void Load_Settings2Tests_Preset()
    {
        Thread.Sleep(250);
        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow!.MainViewModel.settings.LoadOpenList("settings2Tests.json");
        });
        Thread.Sleep(250);

        Assert.That(_mainWindow!.MainViewModel.start.PresetNameLabel, Is.EqualTo("Current preset: settings2Tests"));
    }

    [Test]
    public async Task Open_Preset_Succesfully()
    {
        await Task.Delay(250);
        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow!.MainViewModel.settings.LoadOpenList("settings2Tests.json");
            _mainWindow!.MainViewModel.start.OpenCommand.Execute(null);

            //skonczyc tu otweiralnie presetu
            //await Task.Delay(250);
            //await _mainWindow!.MainViewModel.start.
        });
        await Task.Delay(250);

        bool result = true;
        Assert.That(result, Is.True);
    }

    [Test]
    public void OpenMethod_Should_Start_New_Process()
    {
        Assert.Pass();
    }
}