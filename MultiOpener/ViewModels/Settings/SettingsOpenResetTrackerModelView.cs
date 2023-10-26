using MultiOpener.Entities.Open;
using System;

namespace MultiOpener.ViewModels.Settings;

public class SettingsOpenResetTrackerModelView : OpenTypeViewModelBase
{
    public override Type ItemType { get; set; } = typeof(OpenResetTracker);


    private string _trackerSheetID = string.Empty;
    public string TrackerSheetID
    {
        get { return _trackerSheetID; }
        set
        {
            _trackerSheetID = value;
            OnPropertyChanged(nameof(TrackerSheetID));
        }
    }


    public SettingsOpenResetTrackerModelView(SettingsViewModel settingsViewModel) : base(settingsViewModel) { }

    public override void UpdatePanelField(OpenItem currentChosen)
    {

        base.UpdatePanelField(currentChosen);
    }

    public override void SetOpenProperties(ref OpenItem open)
    {
        base.SetOpenProperties(ref open);
        open.Type = OpenType.ResetTrackerMC;

    }

    public override void Clear()
    {
        base.Clear();

    }
}
