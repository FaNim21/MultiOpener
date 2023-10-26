using MultiOpener.ViewModels;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MultiOpener.Entities.Open;

public class OpenInstanceResetTracker : OpenItem
{
    public bool UsingBuiltInTracker { get; set; }


    [JsonConstructor]
    public OpenInstanceResetTracker(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, bool MinimizeOnOpen = false)
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
        //...
    }
    public OpenInstanceResetTracker(OpenInstanceResetTracker item)
    {
        Name = item.Name;
        PathExe = item.PathExe;
        DelayBefore = item.DelayBefore;
        DelayAfter = item.DelayAfter;
        Type = item.Type;
        MinimizeOnOpen = item.MinimizeOnOpen;
        //...
    }

    public override string Validate()
    {
        return base.Validate();
    }

    public override async Task Open(StartViewModel startModel, CancellationToken token)
    {
        await Task.Delay(500);
        await base.Open(startModel, token);
    }

    public override ushort GetAdditionalProgressCount() => 1;
}
