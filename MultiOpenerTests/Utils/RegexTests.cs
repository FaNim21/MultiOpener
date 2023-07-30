using MultiOpener.Entities.Opened;
using System.Text.RegularExpressions;

namespace MultiOpenerTests.Utils;

[TestFixture]
public class RegexTests
{
    [Test]
    public void InstanceRegex_FindNormalName_ReturnsTrue()
    {
        string expected = "Minecraft* 1.16.1";

        Regex regex = OpenedInstanceProcess.MCPattern();
        bool output = regex.IsMatch(expected);

        Assert.That(output, Is.EqualTo(true));
    }

    [Test]
    public void InstanceRegex_FindNormalWithInstanceName_ReturnsTrue()
    {
        string expected = "Minecraft* - Instance 12";

        Regex regex = OpenedInstanceProcess.MCPattern();
        bool output = regex.IsMatch(expected);

        Assert.That(output, Is.EqualTo(true));
    }
}
