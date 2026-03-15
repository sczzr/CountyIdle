using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class IndustryRules
{
    // 八大技艺容量系数 - 每个建筑提供的道额
    public const int CapacityPerSpiritPlantBuilding = 16;
    public const int CapacityPerForgingBuilding = 12;
    public const int CapacityPerAlchemyBuilding = 8;
    public const int CapacityPerTalismanBuilding = 10;
    public const int CapacityPerFormationBuilding = 6;
    public const int CapacityPerGolemBuilding = 14;
    public const int CapacityPerArcaneBuilding = 10;
    public const int CapacityPerBeastBuilding = 8;
    
    // 旧常量兼容 - 保留给旧代码使用
    public const int ProductionPerAgricultureBuilding = 16;
    public const int ProductionPerWorkshopBuilding = 12;
    public const int ResearchPerBuilding = 10;
    public const int CommercePerBuilding = 14;
    public const int ManagementPerBuilding = 8;
    
    private const int BaseWarehouseCapacity = 900;
    private const int WarehouseCapacityPerLevel = 260;
    private const int WarehouseCapacityPerAdministration = 45;

    public static void EnsureDefaults(GameState state)
    {
        state.AgricultureBuildings = Math.Max(state.AgricultureBuildings, 1);
        state.WorkshopBuildings = Math.Max(state.WorkshopBuildings, 1);
        state.ResearchBuildings = Math.Max(state.ResearchBuildings, 1);
        state.TradeBuildings = Math.Max(state.TradeBuildings, 1);
        state.AdministrationBuildings = Math.Max(state.AdministrationBuildings, 1);
        state.IndustryTools = Math.Max(state.IndustryTools, 0);
        state.MiningLevel = Math.Max(state.MiningLevel, 1);
        state.WarehouseLevel = Math.Max(state.WarehouseLevel, 1);
        state.WarehouseCapacity = Math.Max(state.WarehouseCapacity, CalculateWarehouseCapacity(state));
    }

    /// <summary>
    /// 获取指定技艺的容量上限
    /// </summary>
    public static int GetCapacity(GameState state, CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => GetSpiritPlantCapacity(state),
            CraftSkillType.SpiritBeast => GetSpiritBeastCapacity(state),
            CraftSkillType.Alchemy => GetAlchemyCapacity(state),
            CraftSkillType.Forging => GetForgingCapacity(state),
            CraftSkillType.Talisman => GetTalismanCapacity(state),
            CraftSkillType.Formation => GetFormationCapacity(state),
            CraftSkillType.Golem => GetGolemCapacity(state),
            CraftSkillType.Arcane => GetArcaneCapacity(state),
            _ => 0
        };
    }

    /// <summary>
    /// 获取指定职业的容量上限 - 兼容旧接口
    /// </summary>
    public static int GetCapacity(GameState state, JobType jobType)
    {
        return GetCapacity(state, MapJobToSkill(jobType));
    }

    /// <summary>
    /// 获取指定技艺当前已分配弟子数
    /// </summary>
    public static int GetAssigned(GameState state, CraftSkillType skillType)
    {
        return state.GetSkillFollowers(skillType);
    }

    /// <summary>
    /// 获取指定职业已分配弟子数 - 兼容旧接口
    /// </summary>
    public static int GetAssigned(GameState state, JobType jobType)
    {
        return GetAssigned(state, MapJobToSkill(jobType));
    }

    /// <summary>
    /// 设置指定技艺弟子数
    /// </summary>
    public static void SetAssigned(GameState state, CraftSkillType skillType, int value)
    {
        state.SetSkillFollowers(skillType, value);
    }

    /// <summary>
    /// 设置指定职业弟子数 - 兼容旧接口
    /// </summary>
    public static void SetAssigned(GameState state, JobType jobType, int value)
    {
        SetAssigned(state, MapJobToSkill(jobType), value);
    }

    // JobType 到 CraftSkillType 的映射
    private static CraftSkillType MapJobToSkill(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => CraftSkillType.SpiritPlant,
            JobType.Worker => CraftSkillType.Forging,
            JobType.Merchant => CraftSkillType.Golem,
            JobType.Scholar => CraftSkillType.Arcane,
            _ => CraftSkillType.SpiritPlant
        };
    }

    // 旧方法兼容 - GetProductionCapacity
    public static int GetProductionCapacity(GameState state)
    {
        return GetSpiritPlantCapacity(state);
    }

    // 旧方法兼容 - GetResearchCapacity
    public static int GetResearchCapacity(GameState state)
    {
        return GetTalismanCapacity(state) + GetArcaneCapacity(state);
    }

    // 旧方法兼容 - GetCommerceCapacity
    public static int GetCommerceCapacity(GameState state)
    {
        return GetSpiritBeastCapacity(state) + GetGolemCapacity(state);
    }

    // 旧方法兼容 - GetManagementCapacity
    public static int GetManagementCapacity(GameState state)
    {
        return GetAlchemyCapacity(state) + GetFormationCapacity(state);
    }

    /// <summary>
    /// 灵植 -  capacity 来自 Agriculture 建筑 + Workshop 加工建筑
    /// </summary>
    public static int GetSpiritPlantCapacity(GameState state)
    {
        return (state.AgricultureBuildings * CapacityPerSpiritPlantBuilding) +
               (state.WorkshopBuildings * CapacityPerForgingBuilding / 2);
    }

    /// <summary>
    /// 灵兽 - capacity 来自 Trade 建筑（兽园）
    /// </summary>
    public static int GetSpiritBeastCapacity(GameState state)
    {
        return state.TradeBuildings * CapacityPerBeastBuilding;
    }

    /// <summary>
    /// 炼丹 - capacity 来自 Administration 建筑（丹房）
    /// </summary>
    public static int GetAlchemyCapacity(GameState state)
    {
        return state.AdministrationBuildings * CapacityPerAlchemyBuilding;
    }

    /// <summary>
    /// 炼器 - capacity 来自 Workshop 建筑
    /// </summary>
    public static int GetForgingCapacity(GameState state)
    {
        return state.WorkshopBuildings * CapacityPerForgingBuilding;
    }

    /// <summary>
    /// 符箓 - capacity 来自 Research 建筑
    /// </summary>
    public static int GetTalismanCapacity(GameState state)
    {
        return state.ResearchBuildings * CapacityPerTalismanBuilding;
    }

    /// <summary>
    /// 阵法 - capacity 来自 Administration 建筑
    /// </summary>
    public static int GetFormationCapacity(GameState state)
    {
        return state.AdministrationBuildings * CapacityPerFormationBuilding;
    }

    /// <summary>
    /// 傀儡 - capacity 来自 Trade 建筑（傀儡房）
    /// </summary>
    public static int GetGolemCapacity(GameState state)
    {
        return state.TradeBuildings * CapacityPerGolemBuilding;
    }

    /// <summary>
    /// 天机 - capacity 来自 Research 建筑
    /// </summary>
    public static int GetArcaneCapacity(GameState state)
    {
        return state.ResearchBuildings * CapacityPerArcaneBuilding;
    }

    public static double GetRequiredTools(GameState state)
    {
        return (state.FollowersSpiritPlant * 0.24) + 
               (state.FollowersForging * 0.48) + 
               (state.FollowersAlchemy * 0.36) +
               (state.FollowersTalisman * 0.32) + 
               (state.FollowersArcane * 0.52) + 
               (state.FollowersGolem * 0.42);
    }

    public static double GetToolCoverage(GameState state)
    {
        var requiredTools = GetRequiredTools(state);
        if (requiredTools <= 0.01)
        {
            return 1.0;
        }

        return Math.Clamp(state.IndustryTools / requiredTools, 0.25, 1.0);
    }

    public static double GetManagementBoost(GameState state)
    {
        // 傀儡/炼器整体提供管理增益
        var total = state.FollowersForging + state.FollowersGolem;
        var ratio = state.Population <= 0 ? 0 : (double)total / state.Population;
        return 1.0 + Math.Clamp(ratio, 0, 0.28);
    }

    public static double CalculateWarehouseCapacity(GameState state)
    {
        return BaseWarehouseCapacity +
               (state.WarehouseLevel * WarehouseCapacityPerLevel) +
               (state.AdministrationBuildings * WarehouseCapacityPerAdministration);
    }
}
