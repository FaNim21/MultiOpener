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

    private RecordType _recordRunType = RecordType.RSG;
    public RecordType RecordRunType
    {
        get => _recordRunType;
        set
        {
            PresetIsNotSaved();
            _recordRunType = value;
            OnPropertyChanged(nameof(RecordRunType));
        }
    }


    public SettingsOpenResetTrackerModelView(SettingsViewModel settingsViewModel) : base(settingsViewModel) { }

    public override void UpdatePanelField(OpenItem currentChosen)
    {
        if (currentChosen is OpenResetTracker openTracker)
        {
            TrackerSheetID = openTracker.TrackerID;
            RecordRunType = openTracker.RecordType;
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
            openTracker.RecordType = RecordRunType;
        }
    }

    public override void Clear()
    {
        base.Clear();

        TrackerSheetID = string.Empty;
        RecordRunType = RecordType.RSG;
    }
}
