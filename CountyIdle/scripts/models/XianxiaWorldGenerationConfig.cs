using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

// 仙侠世界地图生成配置
public sealed class XianxiaWorldGenerationConfig
{
    // 随机种子
    [JsonPropertyName("seed")]
    public int Seed { get; set; }

    // 世界标题
    [JsonPropertyName("world_title")]
    public string WorldTitle { get; set; } = "世界地图";

    // 地图宽度
    [JsonPropertyName("width")]
    public int Width { get; set; } = 64;

    // 地图高度
    [JsonPropertyName("height")]
    public int Height { get; set; } = 40;

    // 网格线数量
    [JsonPropertyName("grid_lines")]
    public int GridLines { get; set; } = 10;

    // 世界单位缩放
    [JsonPropertyName("unit_scale")]
    public float UnitScale { get; set; } = 0.46f;

    // 山脉数量下限
    [JsonPropertyName("mountain_range_count_min")]
    public int MountainRangeCountMin { get; set; } = 3;

    // 山脉数量上限
    [JsonPropertyName("mountain_range_count_max")]
    public int MountainRangeCountMax { get; set; } = 6;

    // 河流源点数量下限
    [JsonPropertyName("river_source_count_min")]
    public int RiverSourceCountMin { get; set; } = 4;

    // 河流源点数量上限
    [JsonPropertyName("river_source_count_max")]
    public int RiverSourceCountMax { get; set; } = 10;

    // 主龙脉数量下限
    [JsonPropertyName("major_dragon_vein_count_min")]
    public int MajorDragonVeinCountMin { get; set; } = 2;

    // 主龙脉数量上限
    [JsonPropertyName("major_dragon_vein_count_max")]
    public int MajorDragonVeinCountMax { get; set; } = 4;

    // 次龙脉数量下限
    [JsonPropertyName("minor_dragon_vein_count_min")]
    public int MinorDragonVeinCountMin { get; set; } = 4;

    // 次龙脉数量上限
    [JsonPropertyName("minor_dragon_vein_count_max")]
    public int MinorDragonVeinCountMax { get; set; } = 8;

    // 奇观数量下限
    [JsonPropertyName("wonder_count_min")]
    public int WonderCountMin { get; set; } = 6;

    // 奇观数量上限
    [JsonPropertyName("wonder_count_max")]
    public int WonderCountMax { get; set; } = 12;

    // 宗门候选数量
    [JsonPropertyName("sect_candidate_count")]
    public int SectCandidateCount { get; set; } = 12;

    // 聚落数量
    [JsonPropertyName("settlement_count")]
    public int SettlementCount { get; set; } = 8;

    // 遗迹数量
    [JsonPropertyName("ruin_count")]
    public int RuinCount { get; set; } = 10;

    // 是否启用浮岛
    [JsonPropertyName("floating_isles_enabled")]
    public bool FloatingIslesEnabled { get; set; } = true;

    // 是否启用腐化区域
    [JsonPropertyName("corruption_enabled")]
    public bool CorruptionEnabled { get; set; } = true;

    // 是否启用灵气风暴
    [JsonPropertyName("qi_storms_enabled")]
    public bool QiStormsEnabled { get; set; } = true;

    // 基础温度噪声
    [JsonPropertyName("base_temperature")]
    public float BaseTemperature { get; set; } = 0.52f;

    // 基础湿度噪声
    [JsonPropertyName("base_moisture")]
    public float BaseMoisture { get; set; } = 0.50f;

    // 悬崖判定阈值
    [JsonPropertyName("cliff_threshold")]
    public int CliffThreshold { get; set; } = 18;

    // 湖泊判定阈值
    [JsonPropertyName("lake_threshold")]
    public float LakeThreshold { get; set; } = 0.18f;

    // 宗门灵气权重
    [JsonPropertyName("sect_qi_weight")]
    public float SectQiWeight { get; set; } = 0.30f;

    // 宗门资源权重
    [JsonPropertyName("sect_resource_weight")]
    public float SectResourceWeight { get; set; } = 0.18f;

    // 宗门防御性权重
    [JsonPropertyName("sect_defensibility_weight")]
    public float SectDefensibilityWeight { get; set; } = 0.16f;

    // 宗门水源可达权重
    [JsonPropertyName("sect_water_access_weight")]
    public float SectWaterAccessWeight { get; set; } = 0.12f;

    // 宗门奇观权重
    [JsonPropertyName("sect_wonder_weight")]
    public float SectWonderWeight { get; set; } = 0.10f;

    // 宗门连通性权重
    [JsonPropertyName("sect_connectivity_weight")]
    public float SectConnectivityWeight { get; set; } = 0.08f;

    // 宗门肥沃度权重
    [JsonPropertyName("sect_fertility_weight")]
    public float SectFertilityWeight { get; set; } = 0.06f;

    // 宗门腐化惩罚
    [JsonPropertyName("sect_corruption_penalty")]
    public float SectCorruptionPenalty { get; set; } = 0.20f;

    // 宗门妖兽威胁惩罚
    [JsonPropertyName("sect_monster_threat_penalty")]
    public float SectMonsterThreatPenalty { get; set; } = 0.10f;

    // 区域档案配置
    [JsonPropertyName("region_profiles")]
    public List<WorldRegionProfile> RegionProfiles { get; set; } = [];

    // 主类型生成规则
    [JsonPropertyName("primary_type_spawn_rules")]
    public List<WorldPrimaryTypeSpawnRule> PrimaryTypeSpawnRules { get; set; } = [];

    // 次标签生成规则
    [JsonPropertyName("secondary_tag_spawn_rules")]
    public List<WorldSecondaryTagSpawnRule> SecondaryTagSpawnRules { get; set; } = [];

    // 邻接权重规则
    [JsonPropertyName("adjacency_weight_rules")]
    public List<WorldAdjacencyWeightRule> AdjacencyWeightRules { get; set; } = [];

    // 稀有度档案配置
    [JsonPropertyName("rarity_profiles")]
    public List<WorldRarityProfile> RarityProfiles { get; set; } = [];

    // 解锁规则
    [JsonPropertyName("unlock_rules")]
    public List<WorldUnlockRule> UnlockRules { get; set; } = [];

    // 同伴生成规则
    [JsonPropertyName("companion_spawn_rules")]
    public List<WorldCompanionSpawnRule> CompanionSpawnRules { get; set; } = [];
}
