using MultiOpener.ListView;
using System.Collections.ObjectModel;

namespace MultiOpenerTests.Opening.Validation;

[TestFixture]
internal class OpenInstanceTests
{
    [Test]
    public void InstanceNamesValidation()
    {
        OpenInstance open = new();
        open.PathExe = "C:\\Games\\MultiMC\\MultiMC.exe";
        open.DelayBefore = 100;
        open.DelayAfter = 100;
        open.DelayBetweenInstances = 100;
        open.Quantity = 5;
        open.Names = new()
        {
            "1.16 speedrun instance 3",
            "1.16 speedrun instance 4",
            "1.15.2 speedrun instance 1",
            "1.16 speedrun instance 5",
            "1.16 speedrun instance 1"
        };

        Assert.That(open.Validate(), Is.False);
    }

    [Test]
    public void Validate_Should_Return_True_If_MultiMC_Path_Is_Incorrect()
    {
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\Program Files (x86)\\Test\\Test.exe"
        };

        var result = instance.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_Should_Return_True_If_MultiMC_Path_Does_Not_Exist()
    {
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\MultiMC\\MultiMC.exe"
        };

        var result = instance.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_Should_Return_True_If_Delay_Between_Instances_Is_Out_Of_Range()
    {
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\Games\\MultiMC\\MultiMC.exe",
            DelayBetweenInstances = 100000
        };

        var result = instance.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_Should_Return_True_If_Quantity_Is_Out_Of_Range()
    {
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\Games\\MultiMC\\MultiMC.exe",
            Quantity = 33
        };

        var result = instance.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_Should_Return_True_If_Instance_Name_Is_Missing()
    {
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\Games\\MultiMC\\MultiMC.exe",
            Quantity = 1,
            Names = new ObservableCollection<string> { "1.16 speedrun instance 1", "" }
        };

        var result = instance.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_Should_Return_True_If_Instance_Directory_Does_Not_Exist()
    {
        // Arrange
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\Games\\MultiMC\\MultiMC.exe",
            Quantity = 1,
            Names = new ObservableCollection<string> { "TestInstance" }
        };

        var result = instance.Validate();
        Assert.That(result, Is.True);
    }

    [Test]
    public void Validate_Should_Return_False_If_Validation_Is_Successful()
    {
        var instance = new OpenInstance
        {
            Name = "Test",
            PathExe = "C:\\Games\\MultiMC\\MultiMC.exe",
            Quantity = 1,
            Names = new ObservableCollection<string> { "1.16 speedrun instance 1" }
        };

        var result = instance.Validate();
        Assert.That(result, Is.False);
    }
}
