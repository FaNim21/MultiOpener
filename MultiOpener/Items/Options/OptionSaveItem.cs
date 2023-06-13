namespace MultiOpener.Items.Options;

public class OptionSaveItem
{
    //Main
    public int TimeLateRefresh { get; set; }

    //Instance
    public int TimeLookingForInstancesData { get; set; }
    public int TimeInstanceFinalizingData { get; set; }


    public void ResetToDefault()
    {
        //Main
        TimeLateRefresh = 3500;

        //Instance
        TimeLookingForInstancesData = 40000;
        TimeInstanceFinalizingData = 3000;

        //TODO: 0 DOROBIC RESZTE Z TODO LISTY I TEZ TE CO NIE ZNALAZLEM JESZCZE

    }
}
