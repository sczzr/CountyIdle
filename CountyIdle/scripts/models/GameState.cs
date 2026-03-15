using System;
using System.Collections.Generic;

namespace CountyIdle.Models;

public class GameState
{
    public int Population { get; set; } = 120;
    public int HousingCapacity { get; set; } = 180;
    public int ElitePopulation { get; set; } = 8;
    public int ChildPopulation { get; set; } = 18;
    public int AdultPopulation { get; set; } = 92;
    public int ElderPopulation { get; set; } = 10;
    public int SickPopulation { get; set; } = 4;
    public double ClothingStock { get; set; } = 140;
    public double AverageCommuteDistanceKm { get; set; } = 1.2;
    public double RoadMobilityMultiplier { get; set; } = 1.0;
    public double MapCommuteReductionBonusKm { get; set; } = 0.0;
    public double MapRoadMobilityBonus { get; set; } = 0.0;

    #region 八大修仙技艺 - 弟子人数
    /// <summary>灵植 - 种植灵药灵谷</summary>
    public int FollowersSpiritPlant { get; set; }
    /// <summary>灵兽 - 培育战斗灵兽</summary>
    public int FollowersSpiritBeast { get; set; }
    /// <summary>炼丹 - 炼制丹药</summary>
    public int FollowersAlchemy { get; set; }
    /// <summary>炼器 - 锻造装备法宝</summary>
    public int FollowersForging { get; set; }
    /// <summary>符箓 - 绘制战斗符箓</summary>
    public int FollowersTalisman { get; set; }
    /// <summary>阵法 - 布置宗门阵法</summary>
    public int FollowersFormation { get; set; }
    /// <summary>傀儡 - 操控自动化傀儡</summary>
    public int FollowersGolem { get; set; }
    /// <summary>天机 - 研究解锁新内容</summary>
    public int FollowersArcane { get; set; }
    #endregion

    #region 旧字段兼容 - 将会在后续迁移后删除
    // 旧兼容字段 - 用于存档迁移
    public int Farmers { get; set; } = 70;
    public int Workers { get; set; } = 25;
    public int Merchants { get; set; } = 12;
    public int Scholars { get; set; } = 8;
    #endregion

    public double Happiness { get; set; } = 72.0;
    public double Threat { get; set; } = 10.0;

    public double Food { get; set; } = 680;
    public double Wood { get; set; } = 220;
    public double Stone { get; set; } = 140;
    public double Timber { get; set; } = 0;
    public double RawStone { get; set; } = 0;
    public double Clay { get; set; } = 0;
    public double Brine { get; set; } = 0;
    public double Herbs { get; set; } = 0;
    public double HempFiber { get; set; } = 0;
    public double Reeds { get; set; } = 0;
    public double Hides { get; set; } = 0;
    public int ForestryChainLevel { get; set; } = 0;
    public int MasonryChainLevel { get; set; } = 0;
    public int MedicinalChainLevel { get; set; } = 0;
    public int FiberChainLevel { get; set; } = 0;
    public double FineSalt { get; set; } = 3;
    public double HerbalMedicine { get; set; } = 2;
    public double HempCloth { get; set; } = 4;
    public double Leather { get; set; } = 3;
    public double Gold { get; set; } = 90;
    public double ContributionPoints { get; set; } = 120;
    public double Research { get; set; } = 0;
    public double RareMaterial { get; set; } = 0;
    public double IronOre { get; set; } = 65;
    public double CopperOre { get; set; } = 42;
    public double Coal { get; set; } = 58;
    public double CopperIngot { get; set; } = 4;
    public double WroughtIron { get; set; } = 6;
    public double MetalIngot { get; set; } = 0;
    public double CompositeMaterial { get; set; } = 0;
    public double IndustrialParts { get; set; } = 0;
    public double ConstructionMaterials { get; set; } = 6;

    public int TechLevel { get; set; } = 0;
    public double FoodProductionMultiplier { get; set; } = 1.0;
    public double IndustryProductionMultiplier { get; set; } = 1.0;
    public double TradeProductionMultiplier { get; set; } = 1.0;
    public double PopulationGrowthMultiplier { get; set; } = 1.0;

    public int ExplorationDepth { get; set; } = 1;
    public bool ExplorationEnabled { get; set; } = true;
    public int ExplorationProgressHours { get; set; } = 0;
    public double AvgGearScore { get; set; } = 12;
    public int CommonGearCount { get; set; } = 0;
    public int RareGearCount { get; set; } = 0;
    public int EpicGearCount { get; set; } = 0;
    public int LegendaryGearCount { get; set; } = 0;
    public int EventCooldownHours { get; set; } = 0;

    public int AgricultureBuildings { get; set; } = 3;
    public int WorkshopBuildings { get; set; } = 2;
    public int ResearchBuildings { get; set; } = 1;
    public int TradeBuildings { get; set; } = 1;
    public int AdministrationBuildings { get; set; } = 4;
    public double IndustryTools { get; set; } = 120;
    public int MiningLevel { get; set; } = 1;
    public int WarehouseLevel { get; set; } = 1;
    public double WarehouseCapacity { get; set; } = 1200;

    public int GameMinutes { get; set; } = 0;
    public int HourSettlements { get; set; } = 0;
    public Dictionary<string, double> DiscreteInventoryProgress { get; set; } = new();
    public Dictionary<string, int> TaskOrderUnits { get; set; } = new();
    public Dictionary<string, int> TaskResolvedWorkers { get; set; } = new();
    public string ActiveDevelopmentDirection { get; set; } = string.Empty;
    public string ActiveSectLaw { get; set; } = string.Empty;
    public string ActiveTalentPlan { get; set; } = string.Empty;
    public string ActiveQuarterDecree { get; set; } = string.Empty;
    public int QuarterDecreeIssuedQuarterIndex { get; set; } = -1;
    public string ActiveAffairsRule { get; set; } = string.Empty;
    public string ActiveDoctrineRule { get; set; } = string.Empty;
    public string ActiveDisciplineRule { get; set; } = string.Empty;
    public string ActivePeakSupport { get; set; } = string.Empty;

    public int GetAssignedPopulation()
    {
        return FollowersSpiritPlant + 
               FollowersSpiritBeast + 
               FollowersAlchemy + 
               FollowersForging + 
               FollowersTalisman + 
               FollowersFormation + 
               FollowersGolem + 
               FollowersArcane;
    }

    public int GetUnassignedPopulation()
    {
        return Math.Max(Population - GetAssignedPopulation(), 0);
    }

    /// <summary>
    /// 旧存档迁移 - 将原职业数据迁移到八大技艺
    /// </summary>
    public void MigrateFromOldJobs()
    {
        // 迁移规则：原职业 → 对应技艺
        FollowersSpiritPlant = Farmers;      // 农 → 灵植
        FollowersForging = Workers;          // 工 → 炼器
        FollowersGolem = Merchants;          // 商 → 傀儡（暂时）
        FollowersArcane = Scholars;         // 学 → 天机
        
        // 新增技艺初始化为 0
        if (FollowersSpiritBeast == 0) FollowersSpiritBeast = 0;
        if (FollowersAlchemy == 0) FollowersAlchemy = 0;
        if (FollowersTalisman == 0) FollowersTalisman = 0;
        if (FollowersFormation == 0) FollowersFormation = 0;
    }

    public double GetWarehouseUsed()
    {
        return Math.Max(Food, 0) +
               Math.Max(Wood, 0) +
               Math.Max(Stone, 0) +
               Math.Max(Timber, 0) +
               Math.Max(RawStone, 0) +
               Math.Max(Clay, 0) +
               Math.Max(Brine, 0) +
               Math.Max(Herbs, 0) +
               Math.Max(HempFiber, 0) +
               Math.Max(Reeds, 0) +
               Math.Max(Hides, 0) +
               Math.Max(FineSalt, 0) +
               Math.Max(HerbalMedicine, 0) +
               Math.Max(HempCloth, 0) +
               Math.Max(Leather, 0) +
               Math.Max(IndustryTools, 0) +
               Math.Max(RareMaterial, 0) +
               Math.Max(IronOre, 0) +
               Math.Max(CopperOre, 0) +
               Math.Max(Coal, 0) +
               Math.Max(CopperIngot, 0) +
               Math.Max(WroughtIron, 0) +
               Math.Max(MetalIngot, 0) +
               Math.Max(CompositeMaterial, 0) +
               Math.Max(IndustrialParts, 0) +
               Math.Max(ConstructionMaterials, 0);
    }

    /// <summary>
    /// 获取指定技艺当前弟子人数
    /// </summary>
    public int GetSkillFollowers(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => FollowersSpiritPlant,
            CraftSkillType.SpiritBeast => FollowersSpiritBeast,
            CraftSkillType.Alchemy => FollowersAlchemy,
            CraftSkillType.Forging => FollowersForging,
            CraftSkillType.Talisman => FollowersTalisman,
            CraftSkillType.Formation => FollowersFormation,
            CraftSkillType.Golem => FollowersGolem,
            CraftSkillType.Arcane => FollowersArcane,
            _ => 0
        };
    }

    /// <summary>
    /// 设置指定技艺弟子人数
    /// </summary>
    public void SetSkillFollowers(CraftSkillType skillType, int count)
    {
        count = Math.Max(count, 0);
        switch (skillType)
        {
            case CraftSkillType.SpiritPlant: FollowersSpiritPlant = count; break;
            case CraftSkillType.SpiritBeast: FollowersSpiritBeast = count; break;
            case CraftSkillType.Alchemy: FollowersAlchemy = count; break;
            case CraftSkillType.Forging: FollowersForging = count; break;
            case CraftSkillType.Talisman: FollowersTalisman = count; break;
            case CraftSkillType.Formation: FollowersFormation = count; break;
            case CraftSkillType.Golem: FollowersGolem = count; break;
            case CraftSkillType.Arcane: FollowersArcane = count; break;
        }
    }

    public GameState Clone()
    {
        var clone = (GameState)MemberwiseClone();
        clone.DiscreteInventoryProgress = new Dictionary<string, double>(DiscreteInventoryProgress ?? new Dictionary<string, double>());
        clone.TaskOrderUnits = new Dictionary<string, int>(TaskOrderUnits ?? new Dictionary<string, int>());
        clone.TaskResolvedWorkers = new Dictionary<string, int>(TaskResolvedWorkers ?? new Dictionary<string, int>());
        return clone;
    }
}
