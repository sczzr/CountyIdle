using System.Text.Json.Serialization;

namespace CountyIdle.Models;

public sealed class XianxiaWorldGenerationConfig
{
    [JsonPropertyName("seed")]
    public int Seed { get; set; }

    [JsonPropertyName("world_title")]
    public string WorldTitle { get; set; } = "世界地图";

    [JsonPropertyName("width")]
    public int Width { get; set; } = 64;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 40;

    [JsonPropertyName("grid_lines")]
    public int GridLines { get; set; } = 10;

    [JsonPropertyName("unit_scale")]
    public float UnitScale { get; set; } = 0.46f;

    [JsonPropertyName("mountain_range_count_min")]
    public int MountainRangeCountMin { get; set; } = 3;

    [JsonPropertyName("mountain_range_count_max")]
    public int MountainRangeCountMax { get; set; } = 6;

    [JsonPropertyName("river_source_count_min")]
    public int RiverSourceCountMin { get; set; } = 4;

    [JsonPropertyName("river_source_count_max")]
    public int RiverSourceCountMax { get; set; } = 10;

    [JsonPropertyName("major_dragon_vein_count_min")]
    public int MajorDragonVeinCountMin { get; set; } = 2;

    [JsonPropertyName("major_dragon_vein_count_max")]
    public int MajorDragonVeinCountMax { get; set; } = 4;

    [JsonPropertyName("minor_dragon_vein_count_min")]
    public int MinorDragonVeinCountMin { get; set; } = 4;

    [JsonPropertyName("minor_dragon_vein_count_max")]
    public int MinorDragonVeinCountMax { get; set; } = 8;

    [JsonPropertyName("wonder_count_min")]
    public int WonderCountMin { get; set; } = 6;

    [JsonPropertyName("wonder_count_max")]
    public int WonderCountMax { get; set; } = 12;

    [JsonPropertyName("sect_candidate_count")]
    public int SectCandidateCount { get; set; } = 12;

    [JsonPropertyName("settlement_count")]
    public int SettlementCount { get; set; } = 8;

    [JsonPropertyName("ruin_count")]
    public int RuinCount { get; set; } = 10;

    [JsonPropertyName("floating_isles_enabled")]
    public bool FloatingIslesEnabled { get; set; } = true;

    [JsonPropertyName("corruption_enabled")]
    public bool CorruptionEnabled { get; set; } = true;

    [JsonPropertyName("qi_storms_enabled")]
    public bool QiStormsEnabled { get; set; } = true;

    [JsonPropertyName("base_temperature")]
    public float BaseTemperature { get; set; } = 0.52f;

    [JsonPropertyName("base_moisture")]
    public float BaseMoisture { get; set; } = 0.50f;

    [JsonPropertyName("cliff_threshold")]
    public int CliffThreshold { get; set; } = 18;

    [JsonPropertyName("lake_threshold")]
    public float LakeThreshold { get; set; } = 0.18f;

    [JsonPropertyName("sect_qi_weight")]
    public float SectQiWeight { get; set; } = 0.30f;

    [JsonPropertyName("sect_resource_weight")]
    public float SectResourceWeight { get; set; } = 0.18f;

    [JsonPropertyName("sect_defensibility_weight")]
    public float SectDefensibilityWeight { get; set; } = 0.16f;

    [JsonPropertyName("sect_water_access_weight")]
    public float SectWaterAccessWeight { get; set; } = 0.12f;

    [JsonPropertyName("sect_wonder_weight")]
    public float SectWonderWeight { get; set; } = 0.10f;

    [JsonPropertyName("sect_connectivity_weight")]
    public float SectConnectivityWeight { get; set; } = 0.08f;

    [JsonPropertyName("sect_fertility_weight")]
    public float SectFertilityWeight { get; set; } = 0.06f;

    [JsonPropertyName("sect_corruption_penalty")]
    public float SectCorruptionPenalty { get; set; } = 0.20f;

    [JsonPropertyName("sect_monster_threat_penalty")]
    public float SectMonsterThreatPenalty { get; set; } = 0.10f;
}
