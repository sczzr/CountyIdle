using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

public sealed class WorldRegionProfile
{
    [JsonPropertyName("region_id")]
    public string RegionId { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("coverage_weight")]
    public float CoverageWeight { get; set; }

    [JsonPropertyName("terrain_affinity")]
    public List<string> TerrainAffinity { get; set; } = [];

    [JsonPropertyName("spiritual_density_range")]
    public FloatRange SpiritualDensityRange { get; set; } = new();

    [JsonPropertyName("road_density_range")]
    public FloatRange RoadDensityRange { get; set; } = new();

    [JsonPropertyName("threat_baseline")]
    public FloatRange ThreatBaseline { get; set; } = new();

    [JsonPropertyName("primary_type_bias")]
    public List<WeightedStringValue> PrimaryTypeBias { get; set; } = [];

    [JsonPropertyName("ruin_bias")]
    public float RuinBias { get; set; }

    [JsonPropertyName("market_bias")]
    public float MarketBias { get; set; }

    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }
}

public sealed class WorldPrimaryTypeSpawnRule
{
    [JsonPropertyName("primary_type")]
    public string PrimaryType { get; set; } = string.Empty;

    [JsonPropertyName("base_weight")]
    public float BaseWeight { get; set; }

    [JsonPropertyName("region_weight_multiplier")]
    public List<WeightedStringValue> RegionWeightMultiplier { get; set; } = [];

    [JsonPropertyName("terrain_weight_multiplier")]
    public List<WeightedStringValue> TerrainWeightMultiplier { get; set; } = [];

    [JsonPropertyName("spiritual_weight_curve")]
    public SpawnWeightCurve SpiritualWeightCurve { get; set; } = new();

    [JsonPropertyName("road_weight_curve")]
    public SpawnWeightCurve RoadWeightCurve { get; set; } = new();

    [JsonPropertyName("threat_weight_curve")]
    public SpawnWeightCurve ThreatWeightCurve { get; set; } = new();

    [JsonPropertyName("min_hex_distance")]
    public int MinHexDistance { get; set; }

    [JsonPropertyName("soft_cap_per_region")]
    public int SoftCapPerRegion { get; set; }

    [JsonPropertyName("global_cap")]
    public int GlobalCap { get; set; }

    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }

    [JsonPropertyName("visibility_tier")]
    public int VisibilityTier { get; set; }
}

public sealed class WorldSecondaryTagSpawnRule
{
    [JsonPropertyName("primary_type")]
    public string PrimaryType { get; set; } = string.Empty;

    [JsonPropertyName("secondary_tag")]
    public string SecondaryTag { get; set; } = string.Empty;

    [JsonPropertyName("base_weight")]
    public float BaseWeight { get; set; }

    [JsonPropertyName("region_bias")]
    public List<WeightedStringValue> RegionBias { get; set; } = [];

    [JsonPropertyName("terrain_bias")]
    public List<WeightedStringValue> TerrainBias { get; set; } = [];

    [JsonPropertyName("requires_adjacency")]
    public List<string> RequiresAdjacency { get; set; } = [];

    [JsonPropertyName("avoids_adjacency")]
    public List<string> AvoidsAdjacency { get; set; } = [];

    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }

    [JsonPropertyName("rarity_tier")]
    public string RarityTier { get; set; } = "Common";

    [JsonPropertyName("can_companion_spawn")]
    public bool CanCompanionSpawn { get; set; } = true;
}

public sealed class WorldAdjacencyWeightRule
{
    [JsonPropertyName("source_type")]
    public string SourceType { get; set; } = string.Empty;

    [JsonPropertyName("target_type")]
    public string TargetType { get; set; } = string.Empty;

    [JsonPropertyName("weight_delta")]
    public float WeightDelta { get; set; }

    [JsonPropertyName("radius")]
    public int Radius { get; set; } = 1;

    [JsonPropertyName("rule_mode")]
    public string RuleMode { get; set; } = "Attract";
}

public sealed class WorldRarityProfile
{
    [JsonPropertyName("rarity_tier")]
    public string RarityTier { get; set; } = "Common";

    [JsonPropertyName("spawn_multiplier")]
    public float SpawnMultiplier { get; set; } = 1f;

    [JsonPropertyName("reveal_by_default")]
    public bool RevealByDefault { get; set; } = true;

    [JsonPropertyName("fog_priority")]
    public int FogPriority { get; set; }

    [JsonPropertyName("discovery_hint_chance")]
    public FloatRange DiscoveryHintChance { get; set; } = new();
}

public sealed class WorldUnlockRule
{
    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }

    [JsonPropertyName("min_sect_reputation")]
    public FloatRange MinSectReputation { get; set; } = new();

    [JsonPropertyName("min_expedition_depth")]
    public FloatRange MinExpeditionDepth { get; set; } = new();

    [JsonPropertyName("min_hero_power")]
    public FloatRange MinHeroPower { get; set; } = new();

    [JsonPropertyName("required_rumor_tags")]
    public List<string> RequiredRumorTags { get; set; } = [];

    [JsonPropertyName("required_faction_relation")]
    public List<string> RequiredFactionRelation { get; set; } = [];
}

public sealed class WorldCompanionSpawnRule
{
    [JsonPropertyName("host_type")]
    public string HostType { get; set; } = string.Empty;

    [JsonPropertyName("host_tag")]
    public string HostTag { get; set; } = string.Empty;

    [JsonPropertyName("companion_type")]
    public string CompanionType { get; set; } = string.Empty;

    [JsonPropertyName("companion_tag")]
    public string CompanionTag { get; set; } = string.Empty;

    [JsonPropertyName("spawn_chance")]
    public FloatRange SpawnChance { get; set; } = new();

    [JsonPropertyName("min_distance_from_host")]
    public int MinDistanceFromHost { get; set; }

    [JsonPropertyName("max_distance_from_host")]
    public int MaxDistanceFromHost { get; set; } = 1;
}

public sealed class FloatRange
{
    [JsonPropertyName("min")]
    public float Min { get; set; }

    [JsonPropertyName("max")]
    public float Max { get; set; }
}

public sealed class SpawnWeightCurve
{
    [JsonPropertyName("low")]
    public float Low { get; set; } = 1f;

    [JsonPropertyName("mid")]
    public float Mid { get; set; } = 1f;

    [JsonPropertyName("high")]
    public float High { get; set; } = 1f;
}

public sealed class WeightedStringValue
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public float Weight { get; set; } = 1f;
}
