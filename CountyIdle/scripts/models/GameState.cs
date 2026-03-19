using System;
using System.Collections.Generic;

namespace CountyIdle.Models;

/// <summary>
/// 游戏主状态：存档与结算的权威数据源。
/// </summary>
public class GameState
{
    // 人口与通勤基础状态
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

    // 民心与威胁
    public double Happiness { get; set; } = 72.0;
    public double Threat { get; set; } = 10.0;

    // 资源与库存（含原料、加工品与产能物资）
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

    // 科技与产出倍率
    public int TechLevel { get; set; } = 0;
    public double FoodProductionMultiplier { get; set; } = 1.0;
    public double IndustryProductionMultiplier { get; set; } = 1.0;
    public double TradeProductionMultiplier { get; set; } = 1.0;
    public double PopulationGrowthMultiplier { get; set; } = 1.0;

    // 探险与装备掉落状态
    public int ExplorationDepth { get; set; } = 1;
    public bool ExplorationEnabled { get; set; } = true;
    public int ExplorationProgressHours { get; set; } = 0;
    public double AvgGearScore { get; set; } = 12;
    public int CommonGearCount { get; set; } = 0;
    public int RareGearCount { get; set; } = 0;
    public int EpicGearCount { get; set; } = 0;
    public int LegendaryGearCount { get; set; } = 0;
    public int EventCooldownHours { get; set; } = 0;

    // 建筑与仓储
    public int AgricultureBuildings { get; set; } = 3;
    public int WorkshopBuildings { get; set; } = 2;
    public int ResearchBuildings { get; set; } = 1;
    public int TradeBuildings { get; set; } = 1;
    public int AdministrationBuildings { get; set; } = 4;
    public List<TownBuildingPlacement> TownBuildingPlacements { get; set; } = new();
    // 营建队列：记录正在排队的建造项目。
    public List<ConstructionQueueItem> ConstructionQueue { get; set; } = new();
    // 营建完工待落点：由主界面在山门图落地时消耗。
    public List<IndustryBuildingType> PendingConstructionCompletions { get; set; } = new();
    public double IndustryTools { get; set; } = 120;
    public int MiningLevel { get; set; } = 1;
    public int WarehouseLevel { get; set; } = 1;
    public double WarehouseCapacity { get; set; } = 1200;

    // 时间与运行期状态
    public int GameMinutes { get; set; } = 0;
    public int HourSettlements { get; set; } = 0;
    public Dictionary<string, double> DiscreteInventoryProgress { get; set; } = new();
    public Dictionary<string, int> DiscipleBackpackInventory { get; set; } = new();
    public Dictionary<string, int> WorkshopCraftedInventory { get; set; } = new();
    public Dictionary<string, double> WorkshopCraftedProgress { get; set; } = new();
    public Dictionary<string, int> TaskOrderUnits { get; set; } = new();
    public Dictionary<string, int> TaskResolvedWorkers { get; set; } = new();
    public Dictionary<int, string> DiscipleDirectives { get; set; } = new();
    public Dictionary<int, string> DiscipleCultivationAssignments { get; set; } = new();
    // 弟子修炼卷长期成长进度（按弟子 ID 持久化）。
    public Dictionary<int, double> DiscipleSkillTrainingProgress { get; set; } = new();
    public Dictionary<int, double> DiscipleTechniquePolishProgress { get; set; } = new();
    public Dictionary<int, double> DiscipleCraftPracticeProgress { get; set; } = new();
    public Dictionary<int, double> DiscipleMeditationProgress { get; set; } = new();
    // 弟子修炼卷的近时履历（按弟子 ID 存最近几条记录）。
    public Dictionary<int, List<string>> DiscipleCultivationHistory { get; set; } = new();
    public Dictionary<int, DiscipleEquipmentProfile> DiscipleEquipmentProfiles { get; set; } = new();
    public Dictionary<string, int> FormalStewardAppointments { get; set; } = new();
    // 宗主治理与门规状态
    public string ActiveDevelopmentDirection { get; set; } = string.Empty;
    public string ActiveSectLaw { get; set; } = string.Empty;
    public string ActiveTalentPlan { get; set; } = string.Empty;
    public string ActiveQuarterDecree { get; set; } = string.Empty;
    public int QuarterDecreeIssuedQuarterIndex { get; set; } = -1;
    public string ActiveAffairsRule { get; set; } = string.Empty;
    public string ActiveDoctrineRule { get; set; } = string.Empty;
    public string ActiveDisciplineRule { get; set; } = string.Empty;
    public string ActivePeakSupport { get; set; } = string.Empty;
    public Dictionary<string, string> SectNameMap { get; set; } = new();

    /// <summary>
    /// 获取当前已分配到八大技艺的人数总和。
    /// </summary>
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

    /// <summary>
    /// 获取未分配人口（不为负）。
    /// </summary>
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

    /// <summary>
    /// 计算当前仓储占用（按非负库存累计）。
    /// </summary>
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

    /// <summary>
    /// 深拷贝运行态集合，避免 UI 订阅误改原状态。
    /// </summary>
    public GameState Clone()
    {
        var clone = (GameState)MemberwiseClone();
        clone.DiscreteInventoryProgress = new Dictionary<string, double>(DiscreteInventoryProgress ?? new Dictionary<string, double>());
        clone.DiscipleBackpackInventory = new Dictionary<string, int>(DiscipleBackpackInventory ?? new Dictionary<string, int>());
        clone.WorkshopCraftedInventory = new Dictionary<string, int>(WorkshopCraftedInventory ?? new Dictionary<string, int>());
        clone.WorkshopCraftedProgress = new Dictionary<string, double>(WorkshopCraftedProgress ?? new Dictionary<string, double>());
        clone.TaskOrderUnits = new Dictionary<string, int>(TaskOrderUnits ?? new Dictionary<string, int>());
        clone.TaskResolvedWorkers = new Dictionary<string, int>(TaskResolvedWorkers ?? new Dictionary<string, int>());
        clone.DiscipleDirectives = new Dictionary<int, string>(DiscipleDirectives ?? new Dictionary<int, string>());
        clone.DiscipleCultivationAssignments = new Dictionary<int, string>(
            DiscipleCultivationAssignments ?? new Dictionary<int, string>());
        clone.DiscipleSkillTrainingProgress = new Dictionary<int, double>(
            DiscipleSkillTrainingProgress ?? new Dictionary<int, double>());
        clone.DiscipleTechniquePolishProgress = new Dictionary<int, double>(
            DiscipleTechniquePolishProgress ?? new Dictionary<int, double>());
        clone.DiscipleCraftPracticeProgress = new Dictionary<int, double>(
            DiscipleCraftPracticeProgress ?? new Dictionary<int, double>());
        clone.DiscipleMeditationProgress = new Dictionary<int, double>(
            DiscipleMeditationProgress ?? new Dictionary<int, double>());
        clone.DiscipleCultivationHistory = CloneDiscipleCultivationHistory(
            DiscipleCultivationHistory);
        clone.DiscipleEquipmentProfiles = new Dictionary<int, DiscipleEquipmentProfile>(
            DiscipleEquipmentProfiles ?? new Dictionary<int, DiscipleEquipmentProfile>());
        clone.FormalStewardAppointments = new Dictionary<string, int>(FormalStewardAppointments ?? new Dictionary<string, int>());
        clone.SectNameMap = new Dictionary<string, string>(SectNameMap ?? new Dictionary<string, string>());
        clone.TownBuildingPlacements = CloneTownBuildingPlacements(TownBuildingPlacements);
        clone.ConstructionQueue = CloneConstructionQueue(ConstructionQueue);
        clone.PendingConstructionCompletions = PendingConstructionCompletions == null
            ? new List<IndustryBuildingType>()
            : new List<IndustryBuildingType>(PendingConstructionCompletions);
        return clone;
    }

    /// <summary>
    /// 克隆地块建筑落点列表。
    /// </summary>
    private static List<TownBuildingPlacement> CloneTownBuildingPlacements(
        List<TownBuildingPlacement>? placements)
    {
        if (placements == null || placements.Count == 0)
        {
            return new List<TownBuildingPlacement>();
        }

        var clone = new List<TownBuildingPlacement>(placements.Count);
        foreach (var placement in placements)
        {
            if (placement == null)
            {
                continue;
            }

            clone.Add(new TownBuildingPlacement(placement.BuildingType, placement.X, placement.Y));
        }

        return clone;
    }

    /// <summary>
    /// 克隆营建队列列表，避免 UI 修改运行态。
    /// </summary>
    private static List<ConstructionQueueItem> CloneConstructionQueue(List<ConstructionQueueItem>? queue)
    {
        if (queue == null || queue.Count == 0)
        {
            return new List<ConstructionQueueItem>();
        }

        var clone = new List<ConstructionQueueItem>(queue.Count);
        foreach (var item in queue)
        {
            if (item == null)
            {
                continue;
            }

            clone.Add(new ConstructionQueueItem(
                item.BuildingType,
                item.TotalHours,
                item.RemainingHours,
                item.WoodCost,
                item.StoneCost,
                item.GoldCost,
                item.ContributionCost,
                item.ConstructionCost));
        }

        return clone;
    }

    /// <summary>
    /// 克隆弟子修炼履历字典，避免 UI 文本列表与运行态共享引用。
    /// </summary>
    private static Dictionary<int, List<string>> CloneDiscipleCultivationHistory(
        Dictionary<int, List<string>>? history)
    {
        var clone = new Dictionary<int, List<string>>();
        if (history == null || history.Count <= 0)
        {
            return clone;
        }

        foreach (var (discipleId, entries) in history)
        {
            if (discipleId <= 0 || entries == null)
            {
                continue;
            }

            clone[discipleId] = new List<string>(entries);
        }

        return clone;
    }
}
