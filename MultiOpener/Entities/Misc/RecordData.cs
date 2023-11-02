using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MultiOpener.Entities.Misc;


public class RecordTimelinesData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("igt")]
    public int IGT { get; set; }
    [JsonPropertyName("rta")]
    public int RTA { get; set; }
}

public class RecordData
{
    [JsonPropertyName("mc_version")]
    public string Version { get; set; }
    [JsonPropertyName("run_type")]
    public string Type { get; set; }

    [JsonPropertyName("open_lan")]
    public object? OpenLanTime { get; set; }

    [JsonPropertyName("retimed_igt")]
    public int RetimedIGT { get; set; }
    [JsonPropertyName("final_igt")]
    public int FinalIGT { get; set; }
    [JsonPropertyName("final_rta")]
    public int FinalRTA { get; set; }

    [JsonPropertyName("timelines")]
    public RecordTimelinesData[]? Timelines { get; set; }
    [JsonPropertyName("advancements")]
    public object[]? Advancements { get; set; }
    [JsonPropertyName("stats")]
    public Dictionary<string, object>? Stats { get; set; }
}