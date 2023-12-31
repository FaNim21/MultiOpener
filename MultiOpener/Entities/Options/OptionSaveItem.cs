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
    public int TimeoutWaitingForSingleInstanceToOpen { get; set; }

    //Reset Tracker
    public int UpdateResetTrackerFrequency { get; set; }
    public bool DeleteAllRecordOnActivating { get; set; }


    public void ResetToDefault()
    {
        /* GENERAL*/
        AlwaysOnTop = false;
        IsMinimizedAfterOpen = false;

        /* TIMINGS*/
        //Main
        TimeLateRefresh = 500;
        TimeoutOpen = 3750;

        //Instance
        TimeoutLookingForInstancesData = 40000;
        TimeoutInstanceFinalizingData = 3000;
        TimeoutWaitingForSingleInstanceToOpen = 15000;

        //Reset Tracker
        UpdateResetTrackerFrequency = 30000;
        DeleteAllRecordOnActivating = true;
    }

    public void UpdateUIFromConfig(OptionsViewModel viewModel)
    {
        viewModel.AlwaysOnTop = AlwaysOnTop;
        viewModel.IsMinimizedAfterOpen = IsMinimizedAfterOpen;

        viewModel.TimeLateRefresh = TimeLateRefresh;
        viewModel.TimeoutOpen = TimeoutOpen;

        viewModel.TimeoutLookingForInstancesData = TimeoutLookingForInstancesData;
        viewModel.TimeoutInstanceFinalizingData = TimeoutInstanceFinalizingData;
        viewModel.TimeoutWaitingForSingleInstanceToOpen = TimeoutWaitingForSingleInstanceToOpen;

        viewModel.UpdateResetTrackerFrequency = UpdateResetTrackerFrequency;
        viewModel.DeleteAllRecordOnActivating = DeleteAllRecordOnActivating;
    }
}
