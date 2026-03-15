using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

/// <summary>
/// 八大修仙技艺的阶位系统
/// 每个技艺有4个阶位，从凡阶到仙阶
/// </summary>
public static class SkillProgressionRules
{
    private sealed class SkillStage
    {
        public SkillStage(
            string stageName,
            string displayName,
            int minTechLevel,
            int minPrimaryBuildings,
            int minSecondaryBuildings,
            string unlockConditionText)
        {
            StageName = stageName;
            DisplayName = displayName;
            MinTechLevel = minTechLevel;
            MinPrimaryBuildings = minPrimaryBuildings;
            MinSecondaryBuildings = minSecondaryBuildings;
            UnlockConditionText = unlockConditionText;
        }

        public string StageName { get; }     // 内部名称：凡阶/灵阶/宝阶/仙阶
        public string DisplayName { get; }   // 玩家可见名称
        public int MinTechLevel { get; }
        public int MinPrimaryBuildings { get; }
        public int MinSecondaryBuildings { get; }
        public string UnlockConditionText { get; }
    }

    private static readonly Dictionary<CraftSkillType, SkillStage[]> StageDefinitions = new()
    {
        [CraftSkillType.SpiritPlant] =
        [
            new SkillStage("凡阶", "灵植凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "灵植灵士", 1, 3, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ≥ 3"),
            new SkillStage("宝阶", "灵植宝师", 2, 4, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ≥ 4、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2"),
            new SkillStage("仙阶", "灵植真君", 3, 6, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ≥ 6、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 3")
        ],
        [CraftSkillType.SpiritBeast] =
        [
            new SkillStage("凡阶", "御兽凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "御兽灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 2"),
            new SkillStage("宝阶", "御兽宝师", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 2"),
            new SkillStage("仙阶", "御兽真君", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3")
        ],
        [CraftSkillType.Alchemy] =
        [
            new SkillStage("凡阶", "炼丹凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "炼丹灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 2"),
            new SkillStage("宝阶", "炼丹宝师", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2"),
            new SkillStage("仙阶", "炼丹真君", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 3")
        ],
        [CraftSkillType.Forging] =
        [
            new SkillStage("凡阶", "炼器凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "炼器灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2"),
            new SkillStage("宝阶", "炼器宝师", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 2"),
            new SkillStage("仙阶", "炼器真君", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 3")
        ],
        [CraftSkillType.Talisman] =
        [
            new SkillStage("凡阶", "符箓凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "符箓灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 2"),
            new SkillStage("宝阶", "符箓宝师", 2, 3, 1, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 1"),
            new SkillStage("仙阶", "符箓真君", 3, 5, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2")
        ],
        [CraftSkillType.Formation] =
        [
            new SkillStage("凡阶", "阵法凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "阵法灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 2"),
            new SkillStage("宝阶", "阵法宝师", 2, 4, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 4、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 2"),
            new SkillStage("仙阶", "阵法真君", 3, 6, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 6、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 3")
        ],
        [CraftSkillType.Golem] =
        [
            new SkillStage("凡阶", "傀儡凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "傀儡灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 2"),
            new SkillStage("宝阶", "傀儡宝师", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2"),
            new SkillStage("仙阶", "傀儡真君", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 3")
        ],
        [CraftSkillType.Arcane] =
        [
            new SkillStage("凡阶", "天机凡徒", 0, 1, 0, "默认解锁"),
            new SkillStage("灵阶", "天机灵士", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 2"),
            new SkillStage("宝阶", "天机宝师", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 2"),
            new SkillStage("仙阶", "天机真君", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3")
        ]
    };

    /// <summary>
    /// 获取技艺面板信息
    /// </summary>
    public static SkillPanelInfo GetPanelInfo(GameState state, CraftSkillType skillType)
    {
        var stages = StageDefinitions[skillType];
        var activeStageIndex = ResolveStageIndex(state, skillType);
        var activeStage = stages[activeStageIndex];
        var nextStage = activeStageIndex + 1 < stages.Length ? stages[activeStageIndex + 1] : null;
        var assigned = IndustryRules.GetAssigned(state, skillType);
        var capacity = IndustryRules.GetCapacity(state, skillType);

        return new SkillPanelInfo(
            skillType,
            activeStage.DisplayName,
            $"{GetIcon(skillType)} {activeStage.DisplayName}",
            BuildSummaryText(state, skillType, assigned, capacity),
            BuildDetailText(state, skillType, assigned, capacity, nextStage),
            GetDefaultPriorityText(skillType));
    }

    /// <summary>
    /// 获取当前激活的技艺名称
    /// </summary>
    public static string GetActiveSkillName(GameState state, CraftSkillType skillType)
    {
        return GetPanelInfo(state, skillType).ActiveSkillName;
    }

    /// <summary>
    /// 获取技艺默认优先级文案
    /// </summary>
    public static string GetDefaultPriorityText(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => "★ 灵植优先",
            CraftSkillType.SpiritBeast => "☆ 灵兽常序",
            CraftSkillType.Alchemy => "☆ 炼丹常序",
            CraftSkillType.Forging => "☆ 炼器常序",
            CraftSkillType.Talisman => "☆ 符箓常序",
            CraftSkillType.Formation => "☆ 阵法常序",
            CraftSkillType.Golem => "☆ 傀儡常序",
            CraftSkillType.Arcane => "☆ 天机常序"
        };
    }

    private static int ResolveStageIndex(GameState state, CraftSkillType skillType)
    {
        var stages = StageDefinitions[skillType];
        var primaryBuildings = GetPrimaryBuildingCount(state, skillType);
        var secondaryBuildings = GetSecondaryBuildingCount(state, skillType);

        for (var index = stages.Length - 1; index >= 0; index--)
        {
            var stage = stages[index];
            if (state.TechLevel >= stage.MinTechLevel &&
                primaryBuildings >= stage.MinPrimaryBuildings &&
                secondaryBuildings >= stage.MinSecondaryBuildings)
            {
                return index;
            }
        }

        return 0;
    }

    private static string BuildSummaryText(GameState state, CraftSkillType skillType, int assigned, int capacity)
    {
        return $"{BuildBuildingSnapshot(state, skillType)} · 已派 {assigned}/{capacity} · {GetTechnologyLabel(state.TechLevel)}";
    }

    private static string BuildDetailText(
        GameState state,
        CraftSkillType skillType,
        int assigned,
        int capacity,
        SkillStage? nextStage)
    {
        var nextStageText = nextStage == null
            ? "已达当前技艺最高阶。"
            : $"下一阶：{nextStage.DisplayName}（需 {nextStage.UnlockConditionText}）。";

        return $"规则：{BuildCapacityFormula(state, skillType)}；已派 {assigned}/{capacity}；{GetTechnologyLabel(state.TechLevel)}；{nextStageText}";
    }

    private static string BuildCapacityFormula(GameState state, CraftSkillType skillType)
    {
        var primaryName = GetPrimaryBuildingName(skillType);
        var secondaryName = GetSecondaryBuildingName(skillType);
        var primaryCount = GetPrimaryBuildingCount(state, skillType);
        var secondaryCount = GetSecondaryBuildingCount(state, skillType);
        
        var capacity = IndustryRules.GetCapacity(state, skillType);
        return $"{primaryName} {primaryCount}×{GetPrimaryCapacity(skillType)} + {secondaryName} {secondaryCount}×{GetSecondaryCapacity(skillType)} = {capacity}";
    }

    private static string BuildBuildingSnapshot(GameState state, CraftSkillType skillType)
    {
        var primaryName = GetPrimaryBuildingName(skillType);
        var secondaryName = GetSecondaryBuildingName(skillType);
        var primaryCount = GetPrimaryBuildingCount(state, skillType);
        var secondaryCount = GetSecondaryBuildingCount(state, skillType);
        
        return $"{primaryName} {primaryCount} / {secondaryName} {secondaryCount}";
    }

    private static int GetPrimaryBuildingCount(GameState state, CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => state.AgricultureBuildings,
            CraftSkillType.SpiritBeast => state.TradeBuildings,
            CraftSkillType.Alchemy => state.AdministrationBuildings,
            CraftSkillType.Forging => state.WorkshopBuildings,
            CraftSkillType.Talisman => state.ResearchBuildings,
            CraftSkillType.Formation => state.AdministrationBuildings,
            CraftSkillType.Golem => state.TradeBuildings,
            CraftSkillType.Arcane => state.ResearchBuildings,
            _ => 0
        };
    }

    private static int GetSecondaryBuildingCount(GameState state, CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => state.WorkshopBuildings,
            CraftSkillType.SpiritBeast => state.AdministrationBuildings,
            CraftSkillType.Alchemy => state.WorkshopBuildings,
            CraftSkillType.Forging => state.ResearchBuildings,
            CraftSkillType.Talisman => state.WorkshopBuildings,
            CraftSkillType.Formation => state.ResearchBuildings,
            CraftSkillType.Golem => state.WorkshopBuildings,
            CraftSkillType.Arcane => state.AdministrationBuildings,
            _ => 0
        };
    }

    private static string GetPrimaryBuildingName(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture),
            CraftSkillType.SpiritBeast => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade),
            CraftSkillType.Alchemy => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration),
            CraftSkillType.Forging => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop),
            CraftSkillType.Talisman => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research),
            CraftSkillType.Formation => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration),
            CraftSkillType.Golem => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade),
            CraftSkillType.Arcane => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research),
            _ => "建筑"
        };
    }

    private static string GetSecondaryBuildingName(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop),
            CraftSkillType.SpiritBeast => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration),
            CraftSkillType.Alchemy => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop),
            CraftSkillType.Forging => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research),
            CraftSkillType.Talisman => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop),
            CraftSkillType.Formation => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research),
            CraftSkillType.Golem => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop),
            CraftSkillType.Arcane => SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration),
            _ => "建筑"
        };
    }

    private static int GetPrimaryCapacity(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => IndustryRules.CapacityPerSpiritPlantBuilding,
            CraftSkillType.SpiritBeast => IndustryRules.CapacityPerBeastBuilding,
            CraftSkillType.Alchemy => IndustryRules.CapacityPerAlchemyBuilding,
            CraftSkillType.Forging => IndustryRules.CapacityPerForgingBuilding,
            CraftSkillType.Talisman => IndustryRules.CapacityPerTalismanBuilding,
            CraftSkillType.Formation => IndustryRules.CapacityPerFormationBuilding,
            CraftSkillType.Golem => IndustryRules.CapacityPerGolemBuilding,
            CraftSkillType.Arcane => IndustryRules.CapacityPerArcaneBuilding,
            _ => 10
        };
    }

    private static int GetSecondaryCapacity(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => IndustryRules.CapacityPerForgingBuilding / 2,
            CraftSkillType.SpiritBeast => IndustryRules.CapacityPerAlchemyBuilding,
            CraftSkillType.Alchemy => IndustryRules.CapacityPerForgingBuilding / 2,
            CraftSkillType.Forging => IndustryRules.CapacityPerTalismanBuilding,
            CraftSkillType.Talisman => IndustryRules.CapacityPerForgingBuilding / 2,
            CraftSkillType.Formation => IndustryRules.CapacityPerArcaneBuilding,
            CraftSkillType.Golem => IndustryRules.CapacityPerForgingBuilding / 2,
            CraftSkillType.Arcane => IndustryRules.CapacityPerFormationBuilding,
            _ => 5
        };
    }

    private static string GetTechnologyLabel(int techLevel)
    {
        return SectMapSemanticRules.GetTechnologyLevelLabel(techLevel);
    }

    private static string GetIcon(CraftSkillType skillType)
    {
        return skillType switch
        {
            CraftSkillType.SpiritPlant => "🌱",
            CraftSkillType.SpiritBeast => "🦊",
            CraftSkillType.Alchemy => "⚗️",
            CraftSkillType.Forging => "🔨",
            CraftSkillType.Talisman => "📿",
            CraftSkillType.Formation => "🔯",
            CraftSkillType.Golem => "⚙️",
            CraftSkillType.Arcane => "🔮",
            _ => "✨"
        };
    }
}