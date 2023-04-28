using MultiOpener.ListView;

namespace MultiOpenerTests.Opening.Validation;
[TestFixture]
internal class OpenItemTests
{
    [Test]
    public void Validate_InvalidPath_ReturnsTrue()
    {
        var openItem = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\Program Files\\log.txt",
            DelayBefore = 0,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var result = openItem.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_NegativeDelay_ReturnsTrue()
    {
        var openItem = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\log.txt",
            DelayBefore = -1,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var result = openItem.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_DelayTooHigh_ReturnsTrue()
    {
        var openItem = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\log.txt",
            DelayBefore = 0,
            DelayAfter = 1000000,
            Type = OpenType.Normal
        };

        var result = openItem.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_ValidInput_ReturnsFalse()
    {
        var openItem = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\log.txt",
            DelayBefore = 0,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var result = openItem.Validate();
        Assert.That(result, Is.False);
    }
}
