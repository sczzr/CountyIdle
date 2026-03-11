using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

[Flags]
public enum HexDirectionMask
{
    None = 0,
    East = 1 << 0,
    NorthEast = 1 << 1,
    NorthWest = 1 << 2,
    West = 1 << 3,
    SouthWest = 1 << 4,
    SouthEast = 1 << 5
}

public enum XianxiaElementType
{
    None,
    Wood,
    Fire,
    Earth,
    Metal,
    Water,
    Yin,
    Yang,
    Chaos
}

public enum XianxiaBiomeType
{
    TemperatePlains,
    BambooValley,
    MistyMountains,
    SacredForest,
    JadeHighlands,
    SnowPeaks,
    CrystalFields,
    VolcanicWastes,
    SpiritSwamps,
    AncientRuinsLand,
    DesertBadlands,
    FloatingIsles
}

public enum XianxiaTerrainType
{
    GrassLush,
    GrassSparse,
    WildflowerMeadow,
    ForestGround,
    BambooGround,
    AncientForestFloor,
    MountainRock,
    MountainMoss,
    MountainPlateau,
    DesertSand,
    DesertRock,
    WetlandMud,
    SwampGround,
    SnowPlain,
    SnowRock,
    VolcanicRock,
    AshGround,
    CrystalGround,
    SpiritSoil,
    AncientStone,
    RuinedGround,
    FloatingRock,
    CloudGround
}

public enum XianxiaWaterType
{
    None,
    ClearLake,
    LotusLake,
    MistLake,
    MountainPond,
    SacredPool,
    SwampWater,
    MarshWater,
    SpiritLake,
    CrystalLake,
    WaterfallPool,
    MountainSpring,
    RiverBankGrass,
    RiverBankRock,
    RiverBankMud,
    AncientWell,
    SacredFountain,
    UndergroundSpring,
    FloatingLake,
    SkyReflectionLake
}

public enum XianxiaCliffType
{
    None,
    RockCliff,
    MossCliff,
    GrassCliff,
    BambooCliff,
    SnowCliff,
    SandstoneCliff,
    VerticalCliffWall,
    MountainCliff,
    PlateauEdge,
    StoneStepsCliff,
    AncientRuinsCliff,
    WaterfallCliff,
    ForestCliff,
    MistCliff,
    SpiritCliff,
    FloatingCliff,
    CrystalCliff,
    DragonBoneCliff,
    JadeCliff
}

public enum XianxiaOverlayType
{
    None,
    DenseForest,
    LightForest,
    PineForest,
    BambooForest,
    BambooGrove,
    AncientTree,
    GiantTree,
    SpiritTree,
    GlowingTree,
    WildflowerField,
    TallGrass,
    FernCluster,
    MossPatch,
    RockCluster,
    StoneDebris,
    FallenTree,
    TreeRoots,
    VineGrowth,
    JungleVines,
    LotusCluster,
    LilyCluster,
    MushroomPatch,
    BambooRoots,
    AncientVines,
    SpiritGrass,
    CrystalPlants
}

public enum XianxiaResourceType
{
    None,
    JadeOre,
    SpiritStone,
    CrystalOre,
    GoldOre,
    IronOre,
    StoneResource,
    AncientWood,
    BambooResource,
    SaltDeposit,
    ObsidianRock,
    SpiritHerbs,
    LotusSpirit,
    ImmortalPeach,
    JadeBamboo,
    FireCrystal,
    WaterCrystal,
    EarthCrystal,
    WindCrystal,
    SpiritCrystal,
    SoulCrystal,
    DragonBone,
    PhoenixFeather,
    HeavenIron,
    VoidCrystal
}

public enum XianxiaSpiritualZoneType
{
    None,
    MinorSpiritVein,
    MajorSpiritVein,
    SpiritNode,
    SpiritPool,
    QiRichGround,
    QiStormField,
    YinEnergyZone,
    YangEnergyZone,
    FiveElementsZone,
    FireVein,
    WaterVein,
    EarthVein,
    WoodVein,
    MetalVein,
    SpiritFogField,
    DragonVein,
    DragonNode,
    ImmortalEnergyField,
    AncientCultivationGround,
    HeavenlyEnergyNode,
    ChaosEnergyZone
}

public enum XianxiaStructureType
{
    None,
    SectFoundation,
    SectMainHall,
    SectTrainingGround,
    TempleFoundation,
    TempleComplex,
    CultivationPlatform,
    MeditationPlatform,
    MartialArena,
    AncientShrine,
    RitualAltar,
    VillageBase,
    MarketSquare,
    Watchtower,
    BridgeFoundation,
    CampSite,
    FortressBase,
    RuinsPlatform,
    AncientCityRuins,
    SpiritObelisk,
    DragonStatue,
    HeavenlyGate,
    ImmortalPavilion
}

public enum XianxiaWonderType
{
    None,
    FloatingMountainCluster,
    GiantWorldTree,
    CelestialPalaceRuins,
    DragonBoneValley,
    ImmortalPeak,
    JadeMountain,
    SpiritForestHeart,
    ThousandLotusLake,
    SacredBambooSea,
    HeavenGateRuins,
    PhoenixNestPeak,
    AncientImmortalRuins,
    FiveElementsPillar,
    DragonVeinCore,
    CrystalMountainRange,
    FloatingIslandChain
}

public enum XianxiaSiteRoleType
{
    SectCandidate,
    Settlement,
    Ruin,
    WonderAnchor,
    ResourceHub
}

public sealed class HexAxialCoordData
{
    [JsonPropertyName("q")]
    public int Q { get; set; }

    [JsonPropertyName("r")]
    public int R { get; set; }
}

public sealed class XianxiaHexCellRenderData
{
    [JsonPropertyName("base_tile_key")]
    public string BaseTileKey { get; set; } = string.Empty;

    [JsonPropertyName("transition_tile_key")]
    public string TransitionTileKey { get; set; } = string.Empty;

    [JsonPropertyName("water_tile_key")]
    public string WaterTileKey { get; set; } = string.Empty;

    [JsonPropertyName("cliff_tile_key")]
    public string CliffTileKey { get; set; } = string.Empty;

    [JsonPropertyName("overlay_tile_key")]
    public string OverlayTileKey { get; set; } = string.Empty;

    [JsonPropertyName("resource_tile_key")]
    public string ResourceTileKey { get; set; } = string.Empty;

    [JsonPropertyName("spiritual_tile_key")]
    public string SpiritualTileKey { get; set; } = string.Empty;

    [JsonPropertyName("structure_tile_key")]
    public string StructureTileKey { get; set; } = string.Empty;

    [JsonPropertyName("wonder_tile_key")]
    public string WonderTileKey { get; set; } = string.Empty;

    [JsonPropertyName("variant_index")]
    public int VariantIndex { get; set; }

    [JsonPropertyName("biome_skin_key")]
    public string BiomeSkinKey { get; set; } = string.Empty;
}

public sealed class XianxiaHexCellData
{
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }

    [JsonPropertyName("moisture")]
    public float Moisture { get; set; }

    [JsonPropertyName("fertility")]
    public float Fertility { get; set; }

    [JsonPropertyName("corruption")]
    public float Corruption { get; set; }

    [JsonPropertyName("qi_density")]
    public float QiDensity { get; set; }

    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;

    [JsonPropertyName("biome")]
    public XianxiaBiomeType Biome { get; set; } = XianxiaBiomeType.TemperatePlains;

    [JsonPropertyName("terrain")]
    public XianxiaTerrainType Terrain { get; set; } = XianxiaTerrainType.GrassSparse;

    [JsonPropertyName("water")]
    public XianxiaWaterType Water { get; set; } = XianxiaWaterType.None;

    [JsonPropertyName("cliff")]
    public XianxiaCliffType Cliff { get; set; } = XianxiaCliffType.None;

    [JsonPropertyName("overlay")]
    public XianxiaOverlayType Overlay { get; set; } = XianxiaOverlayType.None;

    [JsonPropertyName("resource")]
    public XianxiaResourceType Resource { get; set; } = XianxiaResourceType.None;

    [JsonPropertyName("spiritual_zone")]
    public XianxiaSpiritualZoneType SpiritualZone { get; set; } = XianxiaSpiritualZoneType.None;

    [JsonPropertyName("structure")]
    public XianxiaStructureType Structure { get; set; } = XianxiaStructureType.None;

    [JsonPropertyName("wonder")]
    public XianxiaWonderType Wonder { get; set; } = XianxiaWonderType.None;

    [JsonPropertyName("river_mask")]
    public HexDirectionMask RiverMask { get; set; } = HexDirectionMask.None;

    [JsonPropertyName("cliff_mask")]
    public HexDirectionMask CliffMask { get; set; } = HexDirectionMask.None;

    [JsonPropertyName("transition_mask")]
    public HexDirectionMask TransitionMask { get; set; } = HexDirectionMask.None;

    [JsonPropertyName("road_mask")]
    public HexDirectionMask RoadMask { get; set; } = HexDirectionMask.None;

    [JsonPropertyName("is_passable")]
    public bool IsPassable { get; set; } = true;

    [JsonPropertyName("is_river_source")]
    public bool IsRiverSource { get; set; }

    [JsonPropertyName("is_lake")]
    public bool IsLake { get; set; }

    [JsonPropertyName("is_dragon_vein_core")]
    public bool IsDragonVeinCore { get; set; }

    [JsonPropertyName("is_sect_candidate")]
    public bool IsSectCandidate { get; set; }

    [JsonPropertyName("monster_threat")]
    public float MonsterThreat { get; set; }

    [JsonPropertyName("faction_influence")]
    public int FactionInfluence { get; set; }

    [JsonPropertyName("render")]
    public XianxiaHexCellRenderData Render { get; set; } = new();
}

public sealed class XianxiaPathNodeData
{
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    [JsonPropertyName("weight")]
    public float Weight { get; set; }
}

public sealed class DragonVeinPathData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("is_major")]
    public bool IsMajor { get; set; }

    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;

    [JsonPropertyName("nodes")]
    public List<XianxiaPathNodeData> Nodes { get; set; } = [];

    [JsonPropertyName("source_coord")]
    public HexAxialCoordData SourceCoord { get; set; } = new();

    [JsonPropertyName("sink_coord")]
    public HexAxialCoordData SinkCoord { get; set; } = new();
}

public sealed class RiverPathData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("source_coord")]
    public HexAxialCoordData SourceCoord { get; set; } = new();

    [JsonPropertyName("mouth_coord")]
    public HexAxialCoordData MouthCoord { get; set; } = new();

    [JsonPropertyName("nodes")]
    public List<XianxiaPathNodeData> Nodes { get; set; } = [];

    [JsonPropertyName("feeds_spirit_zone")]
    public bool FeedsSpiritZone { get; set; }
}

public sealed class SectCandidateSiteData
{
    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;

    [JsonPropertyName("nearby_resources")]
    public List<XianxiaResourceType> NearbyResources { get; set; } = [];

    [JsonPropertyName("nearby_wonders")]
    public List<XianxiaWonderType> NearbyWonders { get; set; } = [];

    [JsonPropertyName("primary_spiritual_zone")]
    public XianxiaSpiritualZoneType PrimarySpiritualZone { get; set; } = XianxiaSpiritualZoneType.None;

    [JsonPropertyName("defensibility")]
    public float Defensibility { get; set; }

    [JsonPropertyName("water_access")]
    public float WaterAccess { get; set; }

    [JsonPropertyName("travel_connectivity")]
    public float TravelConnectivity { get; set; }
}

public sealed class WonderSiteData
{
    [JsonPropertyName("wonder")]
    public XianxiaWonderType Wonder { get; set; } = XianxiaWonderType.None;

    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    [JsonPropertyName("influence_radius")]
    public int InfluenceRadius { get; set; } = 2;

    [JsonPropertyName("qi_bonus")]
    public float QiBonus { get; set; }

    [JsonPropertyName("element_affinity")]
    public XianxiaElementType ElementAffinity { get; set; } = XianxiaElementType.None;
}

public sealed class XianxiaSiteData
{
    [JsonPropertyName("role")]
    public XianxiaSiteRoleType Role { get; set; } = XianxiaSiteRoleType.Settlement;

    [JsonPropertyName("coord")]
    public HexAxialCoordData Coord { get; set; } = new();

    [JsonPropertyName("structure")]
    public XianxiaStructureType Structure { get; set; } = XianxiaStructureType.None;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("importance")]
    public int Importance { get; set; } = 1;

    [JsonPropertyName("primary_type")]
    public string PrimaryType { get; set; } = string.Empty;

    [JsonPropertyName("secondary_tag")]
    public string SecondaryTag { get; set; } = string.Empty;

    [JsonPropertyName("region_id")]
    public string RegionId { get; set; } = string.Empty;

    [JsonPropertyName("rarity_tier")]
    public string RarityTier { get; set; } = "Common";

    [JsonPropertyName("unlock_tier")]
    public int UnlockTier { get; set; }
}

public sealed class XianxiaWorldMapData
{
    [JsonPropertyName("seed")]
    public int Seed { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; } = 64;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 40;

    [JsonPropertyName("cells")]
    public List<XianxiaHexCellData> Cells { get; set; } = [];

    [JsonPropertyName("dragon_veins")]
    public List<DragonVeinPathData> DragonVeins { get; set; } = [];

    [JsonPropertyName("rivers")]
    public List<RiverPathData> Rivers { get; set; } = [];

    [JsonPropertyName("sect_candidates")]
    public List<SectCandidateSiteData> SectCandidates { get; set; } = [];

    [JsonPropertyName("wonders")]
    public List<WonderSiteData> Wonders { get; set; } = [];

    [JsonPropertyName("sites")]
    public List<XianxiaSiteData> Sites { get; set; } = [];
}
