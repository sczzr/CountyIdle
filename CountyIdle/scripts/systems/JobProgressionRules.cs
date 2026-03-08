using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class JobProgressionRules
{
    private sealed class RoleStage
    {
        public RoleStage(
            string roleName,
            int minTechLevel,
            int minPrimaryBuildings,
            int minSecondaryBuildings,
            string unlockConditionText)
        {
            RoleName = roleName;
            MinTechLevel = minTechLevel;
            MinPrimaryBuildings = minPrimaryBuildings;
            MinSecondaryBuildings = minSecondaryBuildings;
            UnlockConditionText = unlockConditionText;
        }

        public string RoleName { get; }

        public int MinTechLevel { get; }

        public int MinPrimaryBuildings { get; }

        public int MinSecondaryBuildings { get; }

        public string UnlockConditionText { get; }
    }

    private static readonly Dictionary<JobType, RoleStage[]> StageDefinitions = new()
    {
        [JobType.Farmer] =
        [
            new RoleStage("田亩农户", 0, 1, 0, "默认解锁"),
            new RoleStage("垄作农师", 1, 3, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ≥ 3"),
            new RoleStage("农械整备员", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2"),
            new RoleStage("良种司圃", 3, 5, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 2")
        ],
        [JobType.Worker] =
        [
            new RoleStage("宗务书吏", 0, 1, 0, "默认解锁"),
            new RoleStage("营造执事", 1, 3, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3"),
            new RoleStage("炼器监造", 2, 3, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 3"),
            new RoleStage("都料匠正", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} ≥ 3")
        ],
        [JobType.Merchant] =
        [
            new RoleStage("坊市行商", 0, 1, 0, "默认解锁"),
            new RoleStage("商路牙郎", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 2"),
            new RoleStage("坊市账房", 2, 3, 2, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 2"),
            new RoleStage("商栈掌柜", 3, 5, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} ≥ 5、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3")
        ],
        [JobType.Scholar] =
        [
            new RoleStage("蒙学塾师", 0, 1, 0, "默认解锁"),
            new RoleStage("藏经讲郎", 1, 2, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T1 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 2"),
            new RoleStage("格物博士", 2, 3, 0, $"{SectMapSemanticRules.GetTechnologyTrackName()} T2 且 {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 3"),
            new RoleStage("司天校书", 3, 4, 3, $"{SectMapSemanticRules.GetTechnologyTrackName()} T3、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} ≥ 4、{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} ≥ 3")
        ]
    };

    public static JobPanelInfo GetPanelInfo(GameState state, JobType jobType)
    {
        var stages = StageDefinitions[jobType];
        var activeStageIndex = ResolveStageIndex(state, jobType);
        var activeStage = stages[activeStageIndex];
        var nextStage = activeStageIndex + 1 < stages.Length ? stages[activeStageIndex + 1] : null;
        var assigned = IndustryRules.GetAssigned(state, jobType);
        var capacity = IndustryRules.GetCapacity(state, jobType);

        return new JobPanelInfo(
            jobType,
            activeStage.RoleName,
            $"{GetIcon(jobType)} {activeStage.RoleName}",
            BuildSummaryText(state, jobType, assigned, capacity),
            BuildDetailText(state, jobType, assigned, capacity, nextStage),
            GetDefaultPriorityText(jobType));
    }

    public static string GetActiveRoleName(GameState state, JobType jobType)
    {
        return GetPanelInfo(state, jobType).ActiveRoleName;
    }

    public static string GetDefaultPriorityText(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "★ 粮务优先",
            JobType.Worker => "☆ 工务常序",
            JobType.Merchant => "☆ 商务常序",
            JobType.Scholar => "☆ 学务常序",
            _ => "☆ 常序"
        };
    }

    private static int ResolveStageIndex(GameState state, JobType jobType)
    {
        var stages = StageDefinitions[jobType];
        var primaryBuildings = GetPrimaryBuildingCount(state, jobType);
        var secondaryBuildings = GetSecondaryBuildingCount(state, jobType);

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

    private static string BuildSummaryText(GameState state, JobType jobType, int assigned, int capacity)
    {
        return $"{BuildBuildingSnapshot(state, jobType)} · 已派 {assigned}/{capacity} · {GetTechnologyLabel(state.TechLevel)}";
    }

    private static string BuildDetailText(
        GameState state,
        JobType jobType,
        int assigned,
        int capacity,
        RoleStage? nextStage)
    {
        var nextStageText = nextStage == null
            ? "已达当前岗位最高阶。"
            : $"下一阶：{nextStage.RoleName}（需 {nextStage.UnlockConditionText}）。";

        return $"规则：{BuildCapacityFormula(state, jobType)}；已派 {assigned}/{capacity}；{GetTechnologyLabel(state.TechLevel)}；{nextStageText}";
    }

    private static string BuildCapacityFormula(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} {state.AgricultureBuildings}×{IndustryRules.ProductionPerAgricultureBuilding} + {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} {state.WorkshopBuildings}×{IndustryRules.ProductionPerWorkshopBuilding} = {IndustryRules.GetProductionCapacity(state)}",
            JobType.Worker => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} {state.AdministrationBuildings}×{IndustryRules.ManagementPerBuilding} = {IndustryRules.GetManagementCapacity(state)}",
            JobType.Merchant => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} {state.TradeBuildings}×{IndustryRules.CommercePerBuilding} = {IndustryRules.GetCommerceCapacity(state)}",
            JobType.Scholar => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} {state.ResearchBuildings}×{IndustryRules.ResearchPerBuilding} = {IndustryRules.GetResearchCapacity(state)}",
            _ => $"容量 {IndustryRules.GetCapacity(state, jobType)}"
        };
    }

    private static string BuildBuildingSnapshot(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} {state.AgricultureBuildings} / {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} {state.WorkshopBuildings}",
            JobType.Worker => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} {state.AdministrationBuildings} / {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} {state.WorkshopBuildings}",
            JobType.Merchant => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} {state.TradeBuildings} / {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} {state.AdministrationBuildings}",
            JobType.Scholar => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} {state.ResearchBuildings} / {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} {state.AdministrationBuildings}",
            _ => "岗位"
        };
    }

    private static int GetPrimaryBuildingCount(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => state.AgricultureBuildings,
            JobType.Worker => state.AdministrationBuildings,
            JobType.Merchant => state.TradeBuildings,
            JobType.Scholar => state.ResearchBuildings,
            _ => 0
        };
    }

    private static int GetSecondaryBuildingCount(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => state.WorkshopBuildings,
            JobType.Worker => state.WorkshopBuildings,
            JobType.Merchant => state.AdministrationBuildings,
            JobType.Scholar => state.AdministrationBuildings,
            _ => 0
        };
    }

    private static string GetTechnologyLabel(int techLevel)
    {
        return SectMapSemanticRules.GetTechnologyLevelLabel(techLevel);
    }

    private static string GetIcon(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "🌾",
            JobType.Worker => "⛏",
            JobType.Merchant => "💰",
            JobType.Scholar => "📜",
            _ => "👥"
        };
    }
}
