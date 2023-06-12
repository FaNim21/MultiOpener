namespace MultiOpener.Items.Options;

public class OptionSaveItem
{
    public int TimeLookingForInstancesData { get; set; }

    public void ResetToDefault()
    {
        TimeLookingForInstancesData = 40000;
    }
}
