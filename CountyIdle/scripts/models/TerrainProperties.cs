namespace CountyIdle.Models;

/// <summary>
/// 地形属性定义
/// 影响产出倍率和品质概率
/// </summary>
public class TerrainProperties
{
    /// <summary>
    /// 灵气等级 1-5，影响产出品质
    /// </summary>
    public int QiLevel { get; set; }
    
    /// <summary>
    /// 肥力 0-10，影响灵植产出
    /// </summary>
    public int Fertility { get; set; }
    
    /// <summary>
    /// 矿藏丰富度 0-10，影响矿材产出
    /// </summary>
    public int MineralRichness { get; set; }
    
    /// <summary>
    /// 获取预设地形属性
    /// </summary>
    public static TerrainProperties GetPreset(TerrainType terrainType)
    {
        return terrainType switch
        {
            TerrainType.SpiritField => new TerrainProperties { QiLevel = 3, Fertility = 8, MineralRichness = 2 },
            TerrainType.SpiritWood => new TerrainProperties { QiLevel = 3, Fertility = 7, MineralRichness = 3 },
            TerrainType.HerbValley => new TerrainProperties { QiLevel = 4, Fertility = 9, MineralRichness = 2 },
            TerrainType.BeastTerritory => new TerrainProperties { QiLevel = 3, Fertility = 5, MineralRichness = 3 },
            TerrainType.SpiritVein => new TerrainProperties { QiLevel = 5, Fertility = 3, MineralRichness = 6 },
            TerrainType.StoneMine => new TerrainProperties { QiLevel = 2, Fertility = 1, MineralRichness = 7 },
            TerrainType.MysticIronMine => new TerrainProperties { QiLevel = 4, Fertility = 0, MineralRichness = 9 },
            TerrainType.HotSpringValley => new TerrainProperties { QiLevel = 4, Fertility = 4, MineralRichness = 3 },
            TerrainType.Wasteland => new TerrainProperties { QiLevel = 1, Fertility = 0, MineralRichness = 6 },
            TerrainType.QuicksandRuin => new TerrainProperties { QiLevel = 3, Fertility = 1, MineralRichness = 4 },
            TerrainType.LakeSpring => new TerrainProperties { QiLevel = 3, Fertility = 6, MineralRichness = 3 },
            TerrainType.IcePeak => new TerrainProperties { QiLevel = 3, Fertility = 2, MineralRichness = 5 },
            TerrainType.Volcano => new TerrainProperties { QiLevel = 5, Fertility = 0, MineralRichness = 8 },
            TerrainType.MarketRuin => new TerrainProperties { QiLevel = 2, Fertility = 1, MineralRichness = 2 },
            TerrainType.DemonLair => new TerrainProperties { QiLevel = 5, Fertility = 2, MineralRichness = 5 },
            TerrainType.MountainPass => new TerrainProperties { QiLevel = 3, Fertility = 2, MineralRichness = 4 },
            _ => new TerrainProperties { QiLevel = 2, Fertility = 3, MineralRichness = 3 }
        };
    }
}
