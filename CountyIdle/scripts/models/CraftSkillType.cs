namespace CountyIdle.Models;

/// <summary>
/// 八大修仙技艺
/// 每个技艺有独立不重叠的核心价值
/// </summary>
public enum CraftSkillType
{
    /// <summary>
    /// 灵植 - 资源生产：种植灵药、灵谷、灵木
    /// </summary>
    SpiritPlant,
    
    /// <summary>
    /// 灵兽 - 单位培育：捕捉驯化、战斗灵兽、坐骑
    /// </summary>
    SpiritBeast,
    
    /// <summary>
    /// 炼丹 - 成长消耗：聚气丹、疗伤丹、洗髓丹
    /// </summary>
    Alchemy,
    
    /// <summary>
    /// 炼器 - 装备制造：武器、防具、法宝、半成品
    /// </summary>
    Forging,
    
    /// <summary>
    /// 符箓 - 战斗法术：一次性爆发、功能符
    /// </summary>
    Talisman,
    
    /// <summary>
    /// 阵法 - 区域建设：聚灵阵、防御阵、杀阵
    /// </summary>
    Formation,
    
    /// <summary>
    /// 傀儡 - 自动化：采矿傀儡、守卫傀儡、采集傀儡
    /// </summary>
    Golem,
    
    /// <summary>
    /// 天机 - 科技研究：解锁丹方、器谱、阵法
    /// </summary>
    Arcane
}
