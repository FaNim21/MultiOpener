using System;

namespace MultiOpener.Utils;

public class TimeoutConfigurator
{
    public int Cooldown { get; set; }
    public int ErrorCount { get; set; }

    public TimeoutConfigurator(float time, int errorCount)
    {
        //TODO: 0 Zrobic do tego unit testy
        int timePerIteration = (int)Math.Floor(time / errorCount);
        if (timePerIteration <= 1)
        {
            errorCount = 0;
            timePerIteration = 0;
        }
        else
        {
            while (timePerIteration <= 100 && errorCount >= timePerIteration / 4)
            {
                errorCount /= 2;
                timePerIteration = (int)Math.Floor(time / errorCount);
            }
        }

        Cooldown = timePerIteration;
        ErrorCount = errorCount;
    }
}
