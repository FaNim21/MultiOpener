using MultiOpener.Utils;

namespace MultiOpenerTests.Utils;

[TestFixture]
public class TimeoutConfiguratorTests
{
    [Test]
    public void ConfigureTimeout_ValidParameters_CooldownAndErrorCountSet()
    {
        float time = 2500;
        int errorCount = 4;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(625));
            Assert.That(configurator.ErrorCount, Is.EqualTo(4));
        });
    }

    [Test]
    public void ConfigureTimeout_ZeroTime_ZeroCooldownAndErrorCount()
    {
        float time = 0;
        int errorCount = 5;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(0));
            Assert.That(configurator.ErrorCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void ConfigureTimeout_TimePerIterationLessThanOrEqualToOne_ZeroCooldownAndErrorCount()
    {
        float time = 100;
        int errorCount = 10;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(500));
            Assert.That(configurator.ErrorCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void ConfigureTimeout_TimePerIterationLessThanOrEqualToOneButErrorCountIsZero_ZeroCooldownAndErrorCount()
    {
        float time = 500;
        int errorCount = 0;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(0));
            Assert.That(configurator.ErrorCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void ConfigureTimeout_NormalTimeAndErrorCount_CooldownAndErrorCountSet()
    {
        float time = 3750;
        int errorCount = 15;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(750));
            Assert.That(configurator.ErrorCount, Is.EqualTo(5));
        });
    }

    [Test]
    public void ConfigureTimeout_NormalTimeAndErrorCount2_CooldownAndErrorCountSet()
    {
        float time = 40000;
        int errorCount = 30;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(1333));
            Assert.That(configurator.ErrorCount, Is.EqualTo(30));
        });
    }

    [Test]
    public void ConfigureTimeout_BiggerTimeAndErrorCount_CooldownAndErrorCountSet()
    {
        float time = 99999;
        int errorCount = 250;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(1204));
            Assert.That(configurator.ErrorCount, Is.EqualTo(83));
        });
    }

    [Test]
    public void ConfigureTimeout_BiggerTimeAndErrorCount2_CooldownAndErrorCountSet()
    {
        float time = 99999;
        int errorCount = 50;

        TimeoutConfigurator configurator = new(time, errorCount);
        Assert.Multiple(() =>
        {
            Assert.That(configurator.Cooldown, Is.EqualTo(1999));
            Assert.That(configurator.ErrorCount, Is.EqualTo(50));
        });
    }
}