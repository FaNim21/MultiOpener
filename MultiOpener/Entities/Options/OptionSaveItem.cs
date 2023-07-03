using MultiOpener.ViewModels;

namespace MultiOpener.Entities.Options;

public class OptionSaveItem
{
    /* GENERAL */
    public bool AlwaysOnTop { get; set; }
    public bool IsMinimizedAfterOpen { get; set; }

    /* TIMINGS */
    //Main
    public int TimeLateRefresh { get; set; }
    public int TimeoutOpen { get; set; }

    //Instance
    public int TimeoutLookingForInstancesData { get; set; }
    public int TimeoutInstanceFinalizingData { get; set; }
    public int TimeoutLookingForSingleInstanceData { get; set; }


    public void ResetToDefault()
    {
        /* GENERAL*/
        AlwaysOnTop = false;
        IsMinimizedAfterOpen = false;

        /* TIMINGS*/
        //Main
        TimeLateRefresh = 3500;
        TimeoutOpen = 3750;

        //Instance
        TimeoutLookingForInstancesData = 40000;
        TimeoutInstanceFinalizingData = 3000;
        TimeoutLookingForSingleInstanceData = 15000;
    }

    public void UpdateUIFromConfig(OptionsViewModel viewModel)
    {
        viewModel.AlwaysOnTop = AlwaysOnTop;
        viewModel.IsMinimizedAfterOpen = IsMinimizedAfterOpen;

        viewModel.TimeLateRefresh = TimeLateRefresh;
        viewModel.TimeoutOpen = TimeoutOpen;

        viewModel.TimeoutLookingForInstancesData = TimeoutLookingForInstancesData;
        viewModel.TimeoutInstanceFinalizingData = TimeoutInstanceFinalizingData;
        viewModel.TimeoutLookingForSingleInstanceData = TimeoutLookingForSingleInstanceData;
    }
}
