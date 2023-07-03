using MultiOpener.Entities.Open;

namespace MultiOpenerTests.Opening.Validation;

[TestFixture]
internal class OpenItemTests
{
    [Test]
    public void Validate_InvalidPath_ReturnsTrue()
    {
        var instance = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\Program Files\\log.txt",
            DelayBefore = 0,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var result = instance.Validate();
        Assert.That(result, Is.EqualTo($"You set a path to file that not exist in {instance.Name}"));
    }

    [Test]
    public void Validate_NegativeDelay_ReturnsTrue()
    {
        var instance = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\log.txt",
            DelayBefore = -1,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var result = instance.Validate();
        Assert.That(result, Is.EqualTo($"You set delay lower than 0 in {instance.Name}"));
    }

    [Test]
    public void Validate_DelayTooHigh_ReturnsTrue()
    {
        var instance = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\log.txt",
            DelayBefore = 0,
            DelayAfter = 1000000,
            Type = OpenType.Normal
        };

        var result = instance.Validate();
        Assert.That(result, Is.EqualTo($"Your delay can't be higher than 99999 in {instance.Name}"));
    }

    [Test]
    public void Validate_ValidInput_ReturnsFalse()
    {
        var instance = new OpenItem
        {
            Name = "MyApp",
            PathExe = "C:\\log.txt",
            DelayBefore = 0,
            DelayAfter = 0,
            Type = OpenType.Normal
        };

        var result = instance.Validate();
        Assert.That(result, Is.EqualTo(string.Empty));
    }
}
