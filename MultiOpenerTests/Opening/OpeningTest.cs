using System.Windows;
using MultiOpener;
using System.Windows.Threading;
using MultiOpener.Commands.StartCommands;

namespace MultiOpenerTests.Opening;

[TestFixture]
internal class OpeningTest
{
    /// <summary>
    /// Ta klasa służy rzeczywistemu odpalaniu MO do ciekawszego testowania zawartości wraz ui wizualnie bez ingerencji części ui w kodzie (czyli ta klasa jest bardziej for fun z nie koniecznie dokładnym testem)
    /// </summary>

    private App? _app;
    private MainWindow? _mainWindow;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var t = new Thread(() =>
        {
            _app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            _app.InitializeComponent();
            Dispatcher.Run();
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        Thread.Sleep(250);
        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow = (MainWindow)_app!.MainWindow;
            _mainWindow.Topmost = true;
        });
    }
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (_app != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow?.Close();
                _mainWindow = null;

                _app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                _app.Dispatcher.InvokeShutdown();
                _app = null;

                Dispatcher.Run();
            });
        }
    }

    [Test, Order(0)]
    public void Load_Settings2Tests_Preset()
    {
        Thread.Sleep(250);
        Application.Current.Dispatcher.Invoke(delegate
        {
            _mainWindow!.MainViewModel.settings.LoadOpenList("OpeningUnitTest.json");
        });
        Thread.Sleep(250);

        Assert.That(_mainWindow!.MainViewModel.start.PresetNameLabel, Is.EqualTo("Current preset: OpeningUnitTest"));
    }

    [Test, Order(1)]
    public async Task Open_Preset_Succesfully()
    {
        await Task.Delay(250);

        Application.Current.Dispatcher.Invoke(delegate
        {
            StartOpenCommand? openCommand = (StartOpenCommand)_mainWindow!.MainViewModel.start.OpenCommand;
            openCommand.Execute(null);
        });

        await Task.Delay(100);
        while (Consts.IsStartPanelWorkingNow)
            await Task.Delay(1000);
        await Task.Delay(250);

        bool result = false;
        Application.Current.Dispatcher.Invoke(delegate
        {
            if (_mainWindow!.MainViewModel.start.Opened.Count == 7)
            {
                bool isScriptClosed = false;
                bool isRestOpened = false;
                int length = _mainWindow!.MainViewModel.start.Opened.Count;
                for (int i = 0; i < length; i++)
                {
                    var current = _mainWindow!.MainViewModel.start.Opened[i];
                    if (i == 2)
                    {
                        current.UpdateStatus();
                        isScriptClosed = !current.IsOpened();
                    }
                    else
                    {
                        isRestOpened = current.IsOpened();
                        if (!isRestOpened)
                            break;
                    }
                }

                result = isRestOpened && isScriptClosed;
            }
        });

        Assert.That(result, Is.True);
    }

    [Test, Order(2)]
    public async Task Close_Preset_Succesfully()
    {
        Application.Current.Dispatcher.Invoke(delegate
        {
            StartCloseCommand? closeCommand = (StartCloseCommand)_mainWindow!.MainViewModel.start.CloseCommand;
            closeCommand.isForcedToClose = true;
            closeCommand.Execute(null);
        });

        await Task.Delay(100);
        while (!Consts.IsStartPanelWorkingNow)
            await Task.Delay(250);
        await Task.Delay(250);

        Assert.That(_mainWindow!.MainViewModel.start.OpenedIsEmpty(), Is.True);
    }
}