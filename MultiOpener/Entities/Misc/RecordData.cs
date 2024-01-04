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

public class RecordAdvancementsData
{
    [JsonPropertyName("complete")] public bool IsCompleted { get; set; }

    [JsonPropertyName("criteria")] public RecordAdvancementCriteriaData? Criteria { get; set; }
}

public class RecordAdvancementCriteriaData
{
    [JsonPropertyName("iron_pickaxe")] public Dictionary<string, object>? IronPickaxe { get; set; }
}

public class RecordStatsData
{
    [JsonPropertyName("stats")] public RecordStatsCategoriesData? StatsData { get; set; }
    [JsonPropertyName("DataVersion")] public int DataVersion { get; set; }
}

public class RecordStatsCategoriesData
{
    [JsonPropertyName("minecraft:dropped")]
    public Dictionary<string, int>? Dropped { get; set; }

    [JsonPropertyName("minecraft:custom")]
    public Dictionary<string, int>? Custom { get; set; }

    [JsonPropertyName("minecraft:used")]
    public Dictionary<string, int>? Used { get; set; }

    [JsonPropertyName("minecraft:mined")]
    public Dictionary<string, int>? Mined { get; set; }

    [JsonPropertyName("minecraft:picked_up")]
    public Dictionary<string, int>? PickedUp { get; set; }

    [JsonPropertyName("minecraft:crafted")]
    public Dictionary<string, int>? Crafted { get; set; }
}

public class RecordData
{
    [JsonPropertyName("mc_version")] public string? Version { get; set; }
    [JsonPropertyName("speedrunigt_version")] public string? SpeedrunIGTVersion { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("run_type")] public string? Type { get; set; }

    [JsonPropertyName("is_cheat_allowed")] public bool IsCheatAllowed { get; set; }
    [JsonPropertyName("default_gamemode")] public int DefaultGameMode { get; set; }
    [JsonPropertyName("open_lan")] public object? OpenLanTime { get; set; }

    [JsonPropertyName("date")] public long Date {  get; set; }

    [JsonPropertyName("retimed_igt")] public long RetimedIGT { get; set; }
    [JsonPropertyName("final_igt")] public long FinalIGT { get; set; }
    [JsonPropertyName("final_rta")] public long FinalRTA { get; set; }

    [JsonPropertyName("timelines")] public RecordTimelinesData[]? Timelines { get; set; }
    [JsonPropertyName("advancements")] public Dictionary<string, RecordAdvancementsData>? Advancements { get; set; }
    [JsonPropertyName("stats")] public Dictionary<string, RecordStatsData>? Stats { get; set; }
}