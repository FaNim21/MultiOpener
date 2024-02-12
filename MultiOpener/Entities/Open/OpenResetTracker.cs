using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace MultiOpener.Entities.Open;

public enum RecordType
{
    [Description("RSG")]
    RSG,
    [Description("FSG")]
    FSG
}

public sealed class OpenResetTracker : OpenItem
{
    public string TrackerID { get; set; }
    public RecordType RecordType { get; set; }  //temp


    [JsonConstructor]
    public OpenResetTracker(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = OpenType.ResetTrackerMC, bool MinimizeOnOpen = false, string TrackerID = "", RecordType RecordType = RecordType.RSG)
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
        //...
        this.TrackerID = TrackerID;
        //...
        this.RecordType = RecordType;
    }
    public OpenResetTracker(string Name) : this(Name, "") { }
    public OpenResetTracker(OpenResetTracker item) : this(item.Name, item.PathExe, item.DelayBefore, item.DelayAfter, item.Type, item.MinimizeOnOpen, item.TrackerID, item.RecordType) { }

    public override string Validate()
    {
        if (DelayAfter < 0 || DelayBefore < 0)
            return $"You set delay lower than 0 in {Name}";

        if (DelayAfter > 999999 || DelayBefore > 99999)
            return $"Your delay can't be higher than 99999 in {Name}";
        return "";
    }

    public override async Task Open(StartViewModel startModel, CancellationToken token)
    {
        bool isCancelled = token.IsCancellationRequested;

        OpenedResetTrackerProcess? opened = new(RecordType);
        opened.Initialize(null, "Tracker", string.Empty, MinimizeOnOpen);
        if (!isCancelled) opened.ActivateTracker();

        Application.Current?.Dispatcher.Invoke(delegate { StartViewModel.Instance?.AddOpened(opened); });
    }
}
