namespace MultiOpenerTests.Utils;

[TestFixture]
public class Other
{
    [Test]
    public void TestStructure1NameIndex()
    {
        // Arrange
        var name = "enter_some_structure";
        //var index = name.IndexOf('_');
        string structureName = name[6..];

        // Act
        /*if (index >= 0 && index < name.Length - 1)
            structureName = name[(index + 1)..];*/

        // Assert
        Assert.Equals("some_structure", structureName);
    }

    [Test]
    public void TestStructure2NameIndex()
    {
        // Arrange
        var name = "enter_another_structure";
        //var index = name.IndexOf('_');
        string structureName = name[6..];

        // Assert
        Assert.Equals("another_structure", structureName);
    }
}
