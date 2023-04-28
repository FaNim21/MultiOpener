using TestStack.White;
using TestStack.White.UIItems.WindowItems;

namespace MultiOpenerTests.Opening;

[TestFixture]
internal class OpeningTest
{
    private Application _app;
    private Window _window;

    [SetUp]
    public void SetUp()
    {
        _app = Application.Launch("MultiOpener.exe");
        _window = _app.GetWindow("MultiOpener");
    }

    [TearDown]
    public void TearDown()
    {
        _window.Close();
        _app.Close();
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}
