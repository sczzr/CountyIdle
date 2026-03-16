using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

// 六方向位掩码（用于河流/道路等边界）
[Flags]
public enum HexDirectionMask
{
    None = 0, // 无方向
    East = 1 << 0, // 东
    NorthEast = 1 << 1, // 东北
    NorthWest = 1 << 2, // 西北
    West = 1 << 3, // 西
    SouthWest = 1 << 4, // 西南
    SouthEast = 1 << 5 // 东南
}

// 五行/阴阳等元素属性
public enum XianxiaElementType
{
    None, // 无
    Wood, // 木
    Fire, // 火
    Earth, // 土
    Metal, // 金
    Water, // 水
    Yin, // 阴
    Yang, // 阳
    Chaos // 混沌
}

// 生物群落类型
public enum XianxiaBiomeType
{
    TemperatePlains, // 温带平原
    BambooValley, // 竹林谷地
    MistyMountains, // 迷雾山脉
    SacredForest, // 圣林
    JadeHighlands, // 翠玉高原
    SnowPeaks, // 雪峰
    CrystalFields, // 水晶原野
    VolcanicWastes, // 火山荒原
    SpiritSwamps, // 灵沼
    AncientRuinsLand, // 古迹之地
    DesertBadlands, // 荒漠劣地
    FloatingIsles // 浮空群岛
}

// 地表基础材质类型
public enum XianxiaTerrainType
{
    GrassLush, // 茂草
    GrassSparse, // 稀草
    WildflowerMeadow, // 野花草甸
    ForestGround, // 林地
    BambooGround, // 竹林地
    AncientForestFloor, // 古林地表
    MountainRock, // 山岩
    MountainMoss, // 山苔
    MountainPlateau, // 山地高台
    DesertSand, // 沙地
    DesertRock, // 沙岩地
    WetlandMud, // 湿地泥
    SwampGround, // 沼泽地
    SnowPlain, // 雪原
    SnowRock, // 雪岩
    VolcanicRock, // 火山岩
    AshGround, // 火山灰地
    CrystalGround, // 水晶地
    SpiritSoil, // 灵土
    AncientStone, // 古石
    RuinedGround, // 废墟地
    FloatingRock, // 浮空岩
    CloudGround // 云地
}

// 水体类型
public enum XianxiaWaterType
{
    None, // 无
    ClearLake, // 清湖
    LotusLake, // 莲湖
    MistLake, // 雾湖
    MountainPond, // 山塘
    SacredPool, // 圣池
    SwampWater, // 沼水
    MarshWater, // 湿地水
    SpiritLake, // 灵湖
    CrystalLake, // 水晶湖
    WaterfallPool, // 瀑潭
    MountainSpring, // 山泉
    RiverBankGrass, // 河岸草
    RiverBankRock, // 河岸岩
    RiverBankMud, // 河岸泥
    AncientWell, // 古井
    SacredFountain, // 圣泉
    UndergroundSpring, // 地下泉
    FloatingLake, // 浮空湖
    SkyReflectionLake // 天镜湖
}

// 悬崖类型
public enum XianxiaCliffType
{
    None, // 无
    RockCliff, // 岩壁
    MossCliff, // 苔壁
    GrassCliff, // 草壁
    BambooCliff, // 竹壁
    SnowCliff, // 雪壁
    SandstoneCliff, // 砂岩壁
    VerticalCliffWall, // 绝壁
    MountainCliff, // 山崖
    PlateauEdge, // 高原边
    StoneStepsCliff, // 石阶崖
    AncientRuinsCliff, // 古迹崖
    WaterfallCliff, // 瀑崖
    ForestCliff, // 林崖
    MistCliff, // 雾崖
    SpiritCliff, // 灵崖
    FloatingCliff, // 浮空崖
    CrystalCliff, // 水晶崖
    DragonBoneCliff, // 龙骨崖
    JadeCliff // 玉崖
}

// 覆盖物类型
public enum XianxiaOverlayType
{
    None, // 无
    DenseForest, // 密林
    LightForest, // 疏林
    PineForest, // 松林
    BambooForest, // 竹林
    BambooGrove, // 竹林丛
    AncientTree, // 古树
    GiantTree, // 巨木
    SpiritTree, // 灵树
    GlowingTree, // 光树
    WildflowerField, // 野花地
    TallGrass, // 高草
    FernCluster, // 蕨丛
    MossPatch, // 苔藓斑
    RockCluster, // 岩群
    StoneDebris, // 碎石
    FallenTree, // 倒木
    TreeRoots, // 树根
    VineGrowth, // 蔓藤
    JungleVines, // 丛林藤
    LotusCluster, // 莲丛
    LilyCluster, // 百合丛
    MushroomPatch, // 菌丛
    BambooRoots, // 竹根
    AncientVines, // 古藤
    SpiritGrass, // 灵草
    CrystalPlants // 水晶植物
}

// 资源类型
public enum XianxiaResourceType
{
    None, // 无
    JadeOre, // 玉矿
    SpiritStone, // 灵石
    CrystalOre, // 水晶矿
    GoldOre, // 金矿
    IronOre, // 铁矿
    StoneResource, // 石材
    AncientWood, // 古木
    BambooResource, // 竹材
    SaltDeposit, // 盐矿
    ObsidianRock, // 黑曜石
    SpiritHerbs, // 灵药草
    LotusSpirit, // 灵莲
    ImmortalPeach, // 仙桃
    JadeBamboo, // 玉竹
    FireCrystal, // 火晶
    WaterCrystal, // 水灵晶
    EarthCrystal, // 土晶
    WindCrystal, // 风晶
    SpiritCrystal, // 灵晶
    SoulCrystal, // 魂晶
    DragonBone, // 龙骨
    PhoenixFeather, // 凤羽
    HeavenIron, // 天铁
    VoidCrystal // 虚空晶
}

// 灵气/能量区类型
public enum XianxiaSpiritualZoneType
{
    None, // 无
    MinorSpiritVein, // 次灵脉
    MajorSpiritVein, // 主灵脉
    SpiritNode, // 灵节点
    SpiritPool, // 灵池
    QiRichGround, // 灵气沃地
    QiStormField, // 灵气风暴区
    YinEnergyZone, // 阴气区
    YangEnergyZone, // 阳气区
    FiveElementsZone, // 五行区
    FireVein, // 火脉
    WaterVein, // 水脉
    EarthVein, // 土脉
    WoodVein, // 木脉
    MetalVein, // 金脉
    SpiritFogField, // 灵雾地
    DragonVein, // 龙脉
    DragonNode, // 龙节点
    ImmortalEnergyField, // 仙能场
    AncientCultivationGround, // 古修行地
    HeavenlyEnergyNode, // 天能节点
    ChaosEnergyZone // 混沌能域
}

// 建筑/结构类型
public enum XianxiaStructureType
{
    None, // 无
    SectFoundation, // 宗门基址
    SectMainHall, // 宗门大殿
    SectTrainingGround, // 宗门演武场
    TempleFoundation, // 寺庙基址
    TempleComplex, // 寺庙群
    CultivationPlatform, // 修炼台
    MeditationPlatform, // 打坐台
    MartialArena, // 武斗场
    AncientShrine, // 古祠
    RitualAltar, // 祭坛
    VillageBase, // 村落基址
    MarketSquare, // 市集广场
    Watchtower, // 瞭望塔
    BridgeFoundation, // 桥基
    CampSite, // 营地
    FortressBase, // 堡垒基址
    RuinsPlatform, // 遗迹基座
    AncientCityRuins, // 古城遗址
    SpiritObelisk, // 灵碑
    DragonStatue, // 龙像
    HeavenlyGate, // 天门
    ImmortalPavilion // 仙阁
}

// 奇观类型
public enum XianxiaWonderType
{
    None, // 无
    FloatingMountainCluster, // 浮空山群
    GiantWorldTree, // 世界巨树
    CelestialPalaceRuins, // 天宫遗迹
    DragonBoneValley, // 龙骨谷
    ImmortalPeak, // 仙峰
    JadeMountain, // 玉山
    SpiritForestHeart, // 灵林核心
    ThousandLotusLake, // 千莲湖
    SacredBambooSea, // 圣竹海
    HeavenGateRuins, // 天门遗址
    PhoenixNestPeak, // 凤巢峰
    AncientImmortalRuins, // 远古仙遗
    FiveElementsPillar, // 五行柱
    DragonVeinCore, // 龙脉核心
    CrystalMountainRange, // 水晶山脉
    FloatingIslandChain // 浮岛链
}

// 站点角色类型
public enum XianxiaSiteRoleType
{
    SectCandidate, // 宗门候选
    Settlement, // 聚落
    Ruin, // 遗迹
    WonderAnchor, // 奇观锚点
    ResourceHub // 资源中心
}

// 六角坐标（轴向坐标）
public sealed class HexAxialCoordData
{
    // 轴向坐标 q
    [JsonPropertyName("q")]
    public int Q { get; set; }

    // 轴向坐标 r
    [JsonPropertyName("r")]
    public int R { get; set; }
}

// 六角格渲染数据（tile key + 变体）
public sealed class XianxiaHexCellRenderData
{
    // 基础地表 tile key
    [JsonPropertyName("base_tile_key")]
    public string BaseTileKey { get; set; } = string.Empty;

    // 过渡 tile key
    [JsonPropertyName("transition_tile_key")]
    public string TransitionTileKey { get; set; } = string.Empty;

    // 水体 tile key
    [JsonPropertyName("water_tile_key")]
    public string WaterTileKey { get; set; } = string.Empty;

    // 悬崖 tile key
    [JsonPropertyName("cliff_tile_key")]
    public string CliffTileKey { get; set; } = string.Empty;

    // 覆盖物 tile key
    [JsonPropertyName("overlay_tile_key")]
    public string OverlayTileKey { get; set; } = string.Empty;

    // 资源 tile key
    [JsonPropertyName("resource_tile_key")]
    public string ResourceTileKey { get; set; } = string.Empty;

    // 灵气区 tile key
    [JsonPropertyName("spiritual_tile_key")]
    public string SpiritualTileKey { get; set; } = string.Empty;

    // 结构 tile key
    [JsonPropertyName("structure_tile_key")]
    public string StructureTileKey { get; set; } = string.Empty;

    // 奇观 tile key
    [JsonPropertyName("wonder_tile_key")]
    public string WonderTileKey { get; set; } = string.Empty;

    // 变体索引
    [JsonPropertyName("variant_index")]
    public int VariantIndex { get; set; }

    // 生物群落皮肤 key
    [JsonPropertyName("biome_skin_key")]
    public string BiomeSkinKey { get; set; } = string.Empty;
}

// 六角格逻辑数据
public sealed class XianxiaHexCellData
{
    // 轴向坐标
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    // 海拔/高度
    [JsonPropertyName("height")]
    public int Height { get; set; }

    // 温度
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }

    // 湿度
    [JsonPropertyName("moisture")]
    public float Moisture { get; set; }

    // 肥沃度
    [JsonPropertyName("fertility")]
    public float Fertility { get; set; }

    // 腐化度
    [JsonPropertyName("corruption")]
    public float Corruption { get; set; }

    // 灵气密度
    [JsonPropertyName("qi_density")]
    public float QiDensity { get; set; }

    // 元素亲和
    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;

    // 生物群落
    [JsonPropertyName("biome")]
    public XianxiaBiomeType Biome { get; set; } = XianxiaBiomeType.TemperatePlains;

    // 地表材质
    [JsonPropertyName("terrain")]
    public XianxiaTerrainType Terrain { get; set; } = XianxiaTerrainType.GrassSparse;

    // 水体类型
    [JsonPropertyName("water")]
    public XianxiaWaterType Water { get; set; } = XianxiaWaterType.None;

    // 悬崖类型
    [JsonPropertyName("cliff")]
    public XianxiaCliffType Cliff { get; set; } = XianxiaCliffType.None;

    // 覆盖物类型
    [JsonPropertyName("overlay")]
    public XianxiaOverlayType Overlay { get; set; } = XianxiaOverlayType.None;

    // 资源类型
    [JsonPropertyName("resource")]
    public XianxiaResourceType Resource { get; set; } = XianxiaResourceType.None;

    // 灵气区类型
    [JsonPropertyName("spiritual_zone")]
    public XianxiaSpiritualZoneType SpiritualZone { get; set; } = XianxiaSpiritualZoneType.None;

    // 结构类型
    [JsonPropertyName("structure")]
    public XianxiaStructureType Structure { get; set; } = XianxiaStructureType.None;

    // 奇观类型
    [JsonPropertyName("wonder")]
    public XianxiaWonderType Wonder { get; set; } = XianxiaWonderType.None;

    // 河流方向掩码
    [JsonPropertyName("river_mask")]
    public HexDirectionMask RiverMask { get; set; } = HexDirectionMask.None;

    // 悬崖方向掩码
    [JsonPropertyName("cliff_mask")]
    public HexDirectionMask CliffMask { get; set; } = HexDirectionMask.None;

    // 过渡方向掩码
    [JsonPropertyName("transition_mask")]
    public HexDirectionMask TransitionMask { get; set; } = HexDirectionMask.None;

    // 道路方向掩码
    [JsonPropertyName("road_mask")]
    public HexDirectionMask RoadMask { get; set; } = HexDirectionMask.None;

    // 是否可通行
    [JsonPropertyName("is_passable")]
    public bool IsPassable { get; set; } = true;

    // 是否为河流源点
    [JsonPropertyName("is_river_source")]
    public bool IsRiverSource { get; set; }

    // 是否为湖泊
    [JsonPropertyName("is_lake")]
    public bool IsLake { get; set; }

    // 是否为龙脉核心
    [JsonPropertyName("is_dragon_vein_core")]
    public bool IsDragonVeinCore { get; set; }

    // 是否为宗门候选地
    [JsonPropertyName("is_sect_candidate")]
    public bool IsSectCandidate { get; set; }

    // 妖兽威胁值
    [JsonPropertyName("monster_threat")]
    public float MonsterThreat { get; set; }

    // 势力影响值
    [JsonPropertyName("faction_influence")]
    public int FactionInfluence { get; set; }

    // 渲染数据
    [JsonPropertyName("render")]
    public XianxiaHexCellRenderData Render { get; set; } = new();
}

// 路径节点数据
public sealed class XianxiaPathNodeData
{
    // 轴向坐标
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    // 权重
    [JsonPropertyName("weight")]
    public float Weight { get; set; }
}

// 龙脉路径数据
public sealed class DragonVeinPathData
{
    // 标识
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    // 是否主龙脉
    [JsonPropertyName("is_major")]
    public bool IsMajor { get; set; }

    // 元素亲和
    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;

    // 路径节点
    [JsonPropertyName("nodes")]
    public List<XianxiaPathNodeData> Nodes { get; set; } = [];

    // 源点坐标
    [JsonPropertyName("source_coord")]
    public HexAxialCoordData SourceCoord { get; set; } = new();

    // 汇入坐标
    [JsonPropertyName("sink_coord")]
    public HexAxialCoordData SinkCoord { get; set; } = new();
}

// 河流路径数据
public sealed class RiverPathData
{
    // 标识
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    // 源点坐标
    [JsonPropertyName("source_coord")]
    public HexAxialCoordData SourceCoord { get; set; } = new();

    // 入海/汇入口坐标
    [JsonPropertyName("mouth_coord")]
    public HexAxialCoordData MouthCoord { get; set; } = new();

    // 路径节点
    [JsonPropertyName("nodes")]
    public List<XianxiaPathNodeData> Nodes { get; set; } = [];

    // 是否滋养灵气区
    [JsonPropertyName("feeds_spirit_zone")]
    public bool FeedsSpiritZone { get; set; }
}

// 宗门候选地数据
public sealed class SectCandidateSiteData
{
    // 轴向坐标
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    // 综合评分
    [JsonPropertyName("score")]
    public float Score { get; set; }

    // 元素亲和
    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;

    // 附近资源
    [JsonPropertyName("nearby_resources")]
    public List<XianxiaResourceType> NearbyResources { get; set; } = [];

    // 附近奇观
    [JsonPropertyName("nearby_wonders")]
    public List<XianxiaWonderType> NearbyWonders { get; set; } = [];

    // 主要灵气区
    [JsonPropertyName("primary_spiritual_zone")]
    public XianxiaSpiritualZoneType PrimarySpiritualZone { get; set; } = XianxiaSpiritualZoneType.None;

    // 防御性评分
    [JsonPropertyName("defensibility")]
    public float Defensibility { get; set; }

    // 水源可达评分
    [JsonPropertyName("water_access")]
    public float WaterAccess { get; set; }

    // 交通连通评分
    [JsonPropertyName("travel_connectivity")]
    public float TravelConnectivity { get; set; }
}

// 奇观站点数据
public sealed class WonderSiteData
{
    // 奇观类型
    [JsonPropertyName("wonder")]
    public XianxiaWonderType Wonder { get; set; } = XianxiaWonderType.None;

    // 轴向坐标
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    // 影响半径
    [JsonPropertyName("influence_radius")]
    public int InfluenceRadius { get; set; } = 2;

    // 灵气加成
    [JsonPropertyName("qi_bonus")]
    public float QiBonus { get; set; }

    // 元素亲和
    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;
}

// 通用站点数据
public sealed class XianxiaSiteData
{
    // 站点角色
    [JsonPropertyName("role")]
    public XianxiaSiteRoleType Role { get; set; } = XianxiaSiteRoleType.Settlement;

    // 轴向坐标
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    // 结构类型
    [JsonPropertyName("structure")]
    public XianxiaStructureType Structure { get; set; } = XianxiaStructureType.None;

    // 显示标签
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    // 重要度
    [JsonPropertyName("importance")]
    public int Importance { get; set; } = 1;

    // 主类型标识
    [JsonPropertyName("primary_type")]
    public string PrimaryType { get; set; } = string.Empty;

    // 次标签
    [JsonPropertyName("secondary_tag")]
    public string SecondaryTag { get; set; } = string.Empty;

    // 所属区域
    [JsonPropertyName("region_id")]
    public string RegionId { get; set; } = string.Empty;

    // 稀有度层级
    [JsonPropertyName("rarity_tier")]
    public string RarityTier { get; set; } = "Common";

    // 解锁层级
    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }
}

// 仙侠世界地图数据
public sealed class XianxiaWorldMapData
{
    // 随机种子
    [JsonPropertyName("seed")]
    public int Seed { get; set; }

    // 地图宽度
    [JsonPropertyName("width")]
    public int Width { get; set; } = 64;

    // 地图高度
    [JsonPropertyName("height")]
    public int Height { get; set; } = 40;

    // 六角格数据
    [JsonPropertyName("cells")]
    public List<XianxiaHexCellData> Cells { get; set; } = [];

    // 龙脉路径
    [JsonPropertyName("dragon_veins")]
    public List<DragonVeinPathData> DragonVeins { get; set; } = [];

    // 河流路径
    [JsonPropertyName("rivers")]
    public List<RiverPathData> Rivers { get; set; } = [];

    // 宗门候选地
    [JsonPropertyName("sect_candidates")]
    public List<SectCandidateSiteData> SectCandidates { get; set; } = [];

    // 奇观列表
    [JsonPropertyName("wonders")]
    public List<WonderSiteData> Wonders { get; set; } = [];

    // 站点列表
    [JsonPropertyName("sites")]
    public List<XianxiaSiteData> Sites { get; set; } = [];
}
