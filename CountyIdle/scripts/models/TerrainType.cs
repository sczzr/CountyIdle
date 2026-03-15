namespace CountyIdle.Models;

/// <summary>
/// 十六种 Hex 地形类型
/// 每种地形对应至少一种技艺产出，不做无用地形
/// </summary>
public enum TerrainType
{
    /// <summary>
    /// 灵田 - 灵药、灵谷种植
    /// </summary>
    SpiritField,
    
    /// <summary>
    /// 灵木林 - 灵木、灵果、灵木浆
    /// </summary>
    SpiritWood,
    
    /// <summary>
    /// 药谷 - 高阶灵药、特殊药材
    /// </summary>
    HerbValley,
    
    /// <summary>
    /// 兽域 - 野生灵兽刷新
    /// </summary>
    BeastTerritory,
    
    /// <summary>
    /// 空山灵脉 - 聚灵阵最佳选址
    /// </summary>
    SpiritVein,
    
    /// <summary>
    /// 青石矿脉 - 青石、铜矿
    /// </summary>
    StoneMine,
    
    /// <summary>
    /// 玄铁矿脉 - 玄铁、精金
    /// </summary>
    MysticIronMine,
    
    /// <summary>
    /// 温泉雾谷 - 加快疗伤恢复
    /// </summary>
    HotSpringValley,
    
    /// <summary>
    /// 荒原戈壁 - 灵石矿、特殊矿物
    /// </summary>
    Wasteland,
    
    /// <summary>
    /// 流沙秘境 - 遗迹产出、古修遗物
    /// </summary>
    QuicksandRuin,
    
    /// <summary>
    /// 湖泊灵泉 - 水灵草、水灵根修炼
    /// </summary>
    LakeSpring,
    
    /// <summary>
    /// 雪峰冰谷 - 寒冰材料、冰属性灵药
    /// </summary>
    IcePeak,
    
    /// <summary>
    /// 火山熔岩 - 火焰石、炎晶
    /// </summary>
    Volcano,
    
    /// <summary>
    /// 坊市废墟 - 古修典籍、天机研究
    /// </summary>
    MarketRuin,
    
    /// <summary>
    /// 妖魔巢穴 - 高威胁高产出（妖丹/材料）
    /// </summary>
    DemonLair,
    
    /// <summary>
    /// 天险关隘 - 宗门边界、护山防线
    /// </summary>
    MountainPass
}
