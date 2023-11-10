namespace MultiOpener.Utils;

public class TimeoutConfigurator
{
    public int Cooldown { get; set; }
    public int ErrorCount { get; set; }

    public TimeoutConfigurator(float time, int errorCount)
    {
        int timePerIteration = (int)(time / errorCount);
        if (timePerIteration <= 1)
        {
            errorCount = 0;
            timePerIteration = 0;
        }
        else
        {
            if (time <= 500)
            {
                timePerIteration = 500;
                errorCount = 1;
            }
            else if (timePerIteration <= 400)
            {
                errorCount = (errorCount + 1) / 3;
                timePerIteration = (int)(time / errorCount);
            }
            else if (timePerIteration >= 2500)
            {
                errorCount = errorCount * 3 / 2;
                timePerIteration = (int)(time / errorCount);
            }
        }

        Cooldown = timePerIteration;
        ErrorCount = errorCount;
    }
}
