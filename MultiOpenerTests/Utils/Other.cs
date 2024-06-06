namespace MultiOpenerTests.Utils;

[TestFixture]
public class Other
{
    [Test]
    public void TestStructure1NameIndex()
    {
        // Arrange
        var name = "enter_some_structure";
        var index = name.IndexOf('_');
        string structureName = "";

        // Act
        structureName = name[6..];
        /*if (index >= 0 && index < name.Length - 1)
            structureName = name[(index + 1)..];*/

        // Assert
        Assert.AreEqual("some_structure", structureName);
    }

    [Test]
    public void TestStructure2NameIndex()
    {
        // Arrange
        var name = "enter_another_structure";
        var index = name.IndexOf('_');
        string structureName = "";

        // Act
        structureName = name[6..];

        // Assert
        Assert.AreEqual("another_structure", structureName);
    }
}
