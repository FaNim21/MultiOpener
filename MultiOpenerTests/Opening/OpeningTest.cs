using MultiOpener;
using TestStack.White;
using TestStack.White.UIItems.WindowItems;

namespace MultiOpenerTests.Opening;
public class OpeningTest
{
    private Application _app;
    private Window _window;

    [SetUp]
    public void SetUp()
    {
        // Start the application and get a reference to the window
        _app = Application.Launch("MultiOpener.exe");
        _window = _app.GetWindow("MultiOpener");
    }

    [TearDown]
    public void TearDown()
    {
        // Close the window and the application
        _window.Close();
        _app.Close();
    }

    [Test]
    public void WindowIsOpenedCorrect()
    {
        //Assert
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}
