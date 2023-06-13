namespace MultiOpener.Items.Options;

public class OptionSaveItem
{
    //Main
    public int TimeLateRefresh { get; set; }
    public int TimeoutOpen { get; set; }
    public int TimeoutSingleOpen { get; set; }

    //Instance
    public int TimeoutLookingForInstancesData { get; set; }
    public int TimeoutInstanceFinalizingData { get; set; }
    public int TimeoutLookingForSingleInstanceData { get; set; }


    public void ResetToDefault()
    {
        //Main
        TimeLateRefresh = 3500;
        TimeoutOpen = 3750;
        TimeoutSingleOpen = 3750;

        //Instance
        TimeoutLookingForInstancesData = 40000;
        TimeoutInstanceFinalizingData = 3000;
        TimeoutLookingForSingleInstanceData = 15000;

        //TODO: 0 DOROBIC TE CO NIE ZNALAZLEM JESZCZE
    }
}
