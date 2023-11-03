using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MultiOpener.Entities.Misc;


public class RecordTimelinesData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("igt")]
    public long IGT { get; set; }
    [JsonPropertyName("rta")]
    public long RTA { get; set; }
}

public class RecordData
{
    [JsonPropertyName("mc_version")]
    public string Version { get; set; }
    [JsonPropertyName("run_type")]
    public string Type { get; set; }

    [JsonPropertyName("date")]
    public long Date {  get; set; }

    [JsonPropertyName("open_lan")]
    public object? OpenLanTime { get; set; }

    [JsonPropertyName("retimed_igt")]
    public long RetimedIGT { get; set; }
    [JsonPropertyName("final_igt")]
    public long FinalIGT { get; set; }
    [JsonPropertyName("final_rta")]
    public long FinalRTA { get; set; }

    [JsonPropertyName("timelines")]
    public RecordTimelinesData[]? Timelines { get; set; }
    [JsonPropertyName("advancements")]
    public Dictionary<string, object>? Advancements { get; set; }
    [JsonPropertyName("stats")]
    public Dictionary<string, object>? Stats { get; set; }
}