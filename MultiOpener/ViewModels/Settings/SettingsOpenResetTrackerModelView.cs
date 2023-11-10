using MultiOpener.Entities.Open;
using System;

namespace MultiOpener.ViewModels.Settings;

public class SettingsOpenResetTrackerModelView : OpenTypeViewModelBase
{
    public override Type ItemType { get; set; } = typeof(OpenResetTracker);


    private string _trackerSheetID = string.Empty;
    public string TrackerSheetID
    {
        get => _trackerSheetID;
        set
        {
            PresetIsNotSaved();
            _trackerSheetID = value;
            OnPropertyChanged(nameof(TrackerSheetID));
        }
    }

    private bool _usingBuiltInTracker = true;
    public bool UsingBuiltInTracker
    {
        get => _usingBuiltInTracker;
        set
        {
            PresetIsNotSaved();
            _usingBuiltInTracker = value;
            OnPropertyChanged(nameof(UsingBuiltInTracker));
        }
    }


    public SettingsOpenResetTrackerModelView(SettingsViewModel settingsViewModel) : base(settingsViewModel) { }

    public override void UpdatePanelField(OpenItem currentChosen)
    {
        if (currentChosen is OpenResetTracker openTracker)
        {
            TrackerSheetID = openTracker.TrackerID;
            UsingBuiltInTracker = openTracker.UsingBuiltInTracker;
        }

        base.UpdatePanelField(currentChosen);
    }

    public override void SetOpenProperties(ref OpenItem open)
    {
        base.SetOpenProperties(ref open);
        open.Type = OpenType.ResetTrackerMC;

        if (open is OpenResetTracker openTracker)
        {
            openTracker.TrackerID = TrackerSheetID;
            openTracker.UsingBuiltInTracker = UsingBuiltInTracker;
        }
    }

    public override void Clear()
    {
        base.Clear();

        TrackerSheetID = string.Empty;
        UsingBuiltInTracker = true;

    }
}
