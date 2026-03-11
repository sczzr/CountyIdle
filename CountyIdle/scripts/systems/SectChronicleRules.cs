using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record SectChronicleSummary(
    string PrimaryAlertText,
    string SecondaryAlertText);

public static class SectChronicleRules
{
    private const double ThreatHighThreshold = 42.0;
    private const double ThreatWatchThreshold = 24.0;
    private const double WarehouseHighLoadThreshold = 0.88;
    private const double WarehouseWatchLoadThreshold = 0.72;
    private const double FoodReserveLowThreshold = 4.0;
    private const double HappinessLowThreshold = 60.0;
    private const double ToolCoverageLowThreshold = 0.65;

    public static SectChronicleSummary BuildSummary(GameState state, GameCalendarInfo calendarInfo)
    {
        var timeOfDay = calendarInfo.TimeOfDayName;
        var warehouseLoad = state.GetWarehouseUsed() / Math.Max(state.WarehouseCapacity, 1.0);
        var foodReservePerCapita = state.Food / Math.Max(state.Population, 1.0);
        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var activeDirection = SectGovernanceRules.GetActiveDevelopmentDefinition(state);
        var activeLaw = SectGovernanceRules.GetActiveLawDefinition(state);
        var activeTalentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(state);
        var activeQuarterDecree = SectGovernanceRules.GetActiveQuarterDecreeDefinition(state);
        var activePeakSupport = SectPeakSupportRules.GetActiveDefinition(state);

        var primaryAlert = BuildPrimaryAlert(
            state,
            timeOfDay,
            warehouseLoad,
            foodReservePerCapita,
            toolCoverage,
            activeDirection.DisplayName);

        var secondaryAlert = BuildSecondaryAlert(
            state,
            timeOfDay,
            warehouseLoad,
            activeLaw.DisplayName,
            activeTalentPlan.DisplayName,
            activeQuarterDecree,
            activePeakSupport);

        return new SectChronicleSummary(primaryAlert, secondaryAlert);
    }

    private static string BuildPrimaryAlert(
        GameState state,
        string timeOfDay,
        double warehouseLoad,
        double foodReservePerCapita,
        double toolCoverage,
        string activeDirectionName)
    {
        if (state.Threat >= ThreatHighThreshold)
        {
            return $"[{timeOfDay}] 巡山警讯升高，山门威胁 {state.Threat:0}% ，宜先压住护山与夜巡。";
        }

        if (warehouseLoad >= WarehouseHighLoadThreshold)
        {
            return $"[{timeOfDay}] 仓储负载 {warehouseLoad * 100:0}% ，宜尽快扩仓或清理积压资材。";
        }

        if (foodReservePerCapita < FoodReserveLowThreshold)
        {
            return $"[{timeOfDay}] 粮廪余量偏紧，人均存粮 {foodReservePerCapita:0.0} ，宜先稳住阵材与供养。";
        }

        if (toolCoverage < ToolCoverageLowThreshold)
        {
            return $"[{timeOfDay}] 工器覆盖率仅 {toolCoverage * 100:0}% ，工坊与营造效率仍有掣肘。";
        }

        if (state.Happiness < HappinessLowThreshold)
        {
            return $"[{timeOfDay}] 门人心气回落至 {state.Happiness:0.#} ，宜以赈济、讲法或巡坊安抚。";
        }

        if (state.Threat >= ThreatWatchThreshold)
        {
            return $"[{timeOfDay}] 山门戒备仍在高位，当前威胁 {state.Threat:0}% ，不宜长时松巡。";
        }

        if (warehouseLoad >= WarehouseWatchLoadThreshold)
        {
            return $"[{timeOfDay}] 仓储已行至 {warehouseLoad * 100:0}% 负载，扩建节奏可提前筹备。";
        }

        return $"[{timeOfDay}] 山门暂无大警，当前宜围绕【{activeDirectionName}】继续稳步推进。";
    }

    private static string BuildSecondaryAlert(
        GameState state,
        string timeOfDay,
        double warehouseLoad,
        string activeLawName,
        string activeTalentPlanName,
        SectQuarterDecreeDefinition activeQuarterDecree,
        SectPeakSupportDefinition activePeakSupport)
    {
        if (state.ExplorationEnabled && state.ElitePopulation > 0)
        {
            return $"[{timeOfDay}] 外务历练持续至第 {state.ExplorationDepth} 层，当前有 {state.ElitePopulation} 名骨干在外整装。";
        }

        if (SectGovernanceRules.GetActiveQuarterDecree(state) != SectQuarterDecreeType.None)
        {
            return $"[{timeOfDay}] 本季法令【{activeQuarterDecree.DisplayName}】在行：{activeQuarterDecree.ShortEffect}。";
        }

        if (SectPeakSupportRules.GetActiveSupport(state) != SectPeakSupportType.Balanced)
        {
            return $"[{timeOfDay}] 协同峰当前为【{activePeakSupport.DisplayName}】，{activePeakSupport.ShortEffect}。";
        }

        if (state.Happiness >= 75.0)
        {
            return $"[{timeOfDay}] 山门民心 {state.Happiness:0.#} ，诸殿气象安定，可顺势扩建与收徒。";
        }

        if (state.TechLevel > 0 || state.Research > 0.0)
        {
            return $"[{timeOfDay}] 传法院研修推进中，当前科技 T{Math.Max(state.TechLevel + 1, 1)}，可继续叠加讲法与深造。";
        }

        if (warehouseLoad < 0.65 && state.ConstructionMaterials >= 4)
        {
            return $"[{timeOfDay}] 仓储仍有余裕，护山构件 {state.ConstructionMaterials:0} ，可择机推进营造。";
        }

        return $"[{timeOfDay}] 宗主中枢正行【{activeLawName}】与【{activeTalentPlanName}】，诸堂按令运转。";
    }
}
