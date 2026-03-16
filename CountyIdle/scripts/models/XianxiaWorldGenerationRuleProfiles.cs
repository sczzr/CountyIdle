using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

// 区域档案配置
public sealed class WorldRegionProfile
{
    // 区域标识
    [JsonPropertyName("region_id")]
    public string RegionId { get; set; } = string.Empty;

    // 显示名称
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    // 覆盖权重
    [JsonPropertyName("coverage_weight")]
    public float CoverageWeight { get; set; }

    // 地形偏好列表
    [JsonPropertyName("terrain_affinity")]
    public List<string> TerrainAffinity { get; set; } = [];

    // 灵气密度范围
    [JsonPropertyName("spiritual_density_range")]
    public FloatRange SpiritualDensityRange { get; set; } = new();

    // 道路密度范围
    [JsonPropertyName("road_density_range")]
    public FloatRange RoadDensityRange { get; set; } = new();

    // 威胁基线范围
    [JsonPropertyName("threat_baseline")]
    public FloatRange ThreatBaseline { get; set; } = new();

    // 主类型权重偏好
    [JsonPropertyName("primary_type_bias")]
    public List<WeightedStringValue> PrimaryTypeBias { get; set; } = [];

    // 遗迹倾向
    [JsonPropertyName("ruin_bias")]
    public float RuinBias { get; set; }

    // 市集倾向
    [JsonPropertyName("market_bias")]
    public float MarketBias { get; set; }

    // 解锁层级
    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }
}

// 主类型生成规则
public sealed class WorldPrimaryTypeSpawnRule
{
    // 主类型标识
    [JsonPropertyName("primary_type")]
    public string PrimaryType { get; set; } = string.Empty;

    // 基础权重
    [JsonPropertyName("base_weight")]
    public float BaseWeight { get; set; }

    // 区域权重倍率
    [JsonPropertyName("region_weight_multiplier")]
    public List<WeightedStringValue> RegionWeightMultiplier { get; set; } = [];

    // 地形权重倍率
    [JsonPropertyName("terrain_weight_multiplier")]
    public List<WeightedStringValue> TerrainWeightMultiplier { get; set; } = [];

    // 灵气权重曲线
    [JsonPropertyName("spiritual_weight_curve")]
    public SpawnWeightCurve SpiritualWeightCurve { get; set; } = new();

    // 道路权重曲线
    [JsonPropertyName("road_weight_curve")]
    public SpawnWeightCurve RoadWeightCurve { get; set; } = new();

    // 威胁权重曲线
    [JsonPropertyName("threat_weight_curve")]
    public SpawnWeightCurve ThreatWeightCurve { get; set; } = new();

    // 最小六角格距离
    [JsonPropertyName("min_hex_distance")]
    public int MinHexDistance { get; set; }

    // 区域软上限
    [JsonPropertyName("soft_cap_per_region")]
    public int SoftCapPerRegion { get; set; }

    // 全局上限
    [JsonPropertyName("global_cap")]
    public int GlobalCap { get; set; }

    // 解锁层级
    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }

    // 可见层级
    [JsonPropertyName("visibility_tier")]
    public int VisibilityTier { get; set; }
}

// 次标签生成规则
public sealed class WorldSecondaryTagSpawnRule
{
    // 主类型标识
    [JsonPropertyName("primary_type")]
    public string PrimaryType { get; set; } = string.Empty;

    // 次标签标识
    [JsonPropertyName("secondary_tag")]
    public string SecondaryTag { get; set; } = string.Empty;

    // 基础权重
    [JsonPropertyName("base_weight")]
    public float BaseWeight { get; set; }

    // 区域偏好
    [JsonPropertyName("region_bias")]
    public List<WeightedStringValue> RegionBias { get; set; } = [];

    // 地形偏好
    [JsonPropertyName("terrain_bias")]
    public List<WeightedStringValue> TerrainBias { get; set; } = [];

    // 需要邻接的类型
    [JsonPropertyName("requires_adjacency")]
    public List<string> RequiresAdjacency { get; set; } = [];

    // 避免邻接的类型
    [JsonPropertyName("avoids_adjacency")]
    public List<string> AvoidsAdjacency { get; set; } = [];

    // 解锁层级
    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }

    // 稀有度层级
    [JsonPropertyName("rarity_tier")]
    public string RarityTier { get; set; } = "Common";

    // 是否可生成同伴
    [JsonPropertyName("can_companion_spawn")]
    public bool CanCompanionSpawn { get; set; } = true;
}

// 邻接权重规则
public sealed class WorldAdjacencyWeightRule
{
    // 来源类型
    [JsonPropertyName("source_type")]
    public string SourceType { get; set; } = string.Empty;

    // 目标类型
    [JsonPropertyName("target_type")]
    public string TargetType { get; set; } = string.Empty;

    // 权重变化量
    [JsonPropertyName("weight_delta")]
    public float WeightDelta { get; set; }

    // 邻接半径
    [JsonPropertyName("radius")]
    public int Radius { get; set; } = 1;

    // 规则模式（吸引/排斥）
    [JsonPropertyName("rule_mode")]
    public string RuleMode { get; set; } = "Attract";
}

// 稀有度档案
public sealed class WorldRarityProfile
{
    // 稀有度层级
    [JsonPropertyName("rarity_tier")]
    public string RarityTier { get; set; } = "Common";

    // 生成倍率
    [JsonPropertyName("spawn_multiplier")]
    public float SpawnMultiplier { get; set; } = 1f;

    // 默认揭示
    [JsonPropertyName("reveal_by_default")]
    public bool RevealByDefault { get; set; } = true;

    // 迷雾优先级
    [JsonPropertyName("fog_priority")]
    public int FogPriority { get; set; }

    // 发现提示概率范围
    [JsonPropertyName("discovery_hint_chance")]
    public FloatRange DiscoveryHintChance { get; set; } = new();
}

// 解锁规则
public sealed class WorldUnlockRule
{
    // 解锁层级
    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }

    // 宗门声望门槛
    [JsonPropertyName("min_sect_reputation")]
    public FloatRange MinSectReputation { get; set; } = new();

    // 探索深度门槛
    [JsonPropertyName("min_expedition_depth")]
    public FloatRange MinExpeditionDepth { get; set; } = new();

    // 英雄战力门槛
    [JsonPropertyName("min_hero_power")]
    public FloatRange MinHeroPower { get; set; } = new();

    // 必须具备的传闻标签
    [JsonPropertyName("required_rumor_tags")]
    public List<string> RequiredRumorTags { get; set; } = [];

    // 必须具备的势力关系
    [JsonPropertyName("required_faction_relation")]
    public List<string> RequiredFactionRelation { get; set; } = [];
}

// 同伴生成规则
public sealed class WorldCompanionSpawnRule
{
    // 宿主类型
    [JsonPropertyName("host_type")]
    public string HostType { get; set; } = string.Empty;

    // 宿主标签
    [JsonPropertyName("host_tag")]
    public string HostTag { get; set; } = string.Empty;

    // 同伴类型
    [JsonPropertyName("companion_type")]
    public string CompanionType { get; set; } = string.Empty;

    // 同伴标签
    [JsonPropertyName("companion_tag")]
    public string CompanionTag { get; set; } = string.Empty;

    // 生成概率范围
    [JsonPropertyName("spawn_chance")]
    public FloatRange SpawnChance { get; set; } = new();

    // 与宿主最小距离
    [JsonPropertyName("min_distance_from_host")]
    public int MinDistanceFromHost { get; set; }

    // 与宿主最大距离
    [JsonPropertyName("max_distance_from_host")]
    public int MaxDistanceFromHost { get; set; } = 1;
}

// 浮点区间
public sealed class FloatRange
{
    // 最小值
    [JsonPropertyName("min")]
    public float Min { get; set; }

    // 最大值
    [JsonPropertyName("max")]
    public float Max { get; set; }
}

// 生成权重曲线
public sealed class SpawnWeightCurve
{
    // 低位权重
    [JsonPropertyName("low")]
    public float Low { get; set; } = 1f;

    // 中位权重
    [JsonPropertyName("mid")]
    public float Mid { get; set; } = 1f;

    // 高位权重
    [JsonPropertyName("high")]
    public float High { get; set; } = 1f;
}

// 字符串-权重对
public sealed class WeightedStringValue
{
    // 关键字
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    // 权重
    [JsonPropertyName("weight")]
    public float Weight { get; set; } = 1f;
}
