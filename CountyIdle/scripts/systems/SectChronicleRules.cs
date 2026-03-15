using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record SectChronicleSummary(
    string PrimaryAlertText,
    string SecondaryAlertText);

public enum SectChronicleCardTone
{
    Neutral,
    Good,
    Warning
}

public sealed record SectChronicleReportCard(
    string TitleText,
    string StatusText,
    string DetailText,
    SectChronicleCardTone Tone);

public sealed record SectChronicleSettlementSnapshot(
    int HourSettlementIndex,
    int GameMinutes,
    int Population,
    double Food,
    double Gold,
    double ContributionPoints,
    double Research,
    double Threat,
    double Happiness,
    double WarehouseLoad);

public enum SectChronicleLogCategory
{
    All,
    Governance,
    Resources,
    Expedition,
    Archive
}

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

    public static string BuildChronicleOverviewText(GameState state, GameCalendarInfo calendarInfo)
    {
        var activeDirection = SectGovernanceRules.GetActiveDevelopmentDefinition(state);
        var activeLaw = SectGovernanceRules.GetActiveLawDefinition(state);
        var activeTalentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(state);

        return $"[{calendarInfo.TimeOfDayName}] 山门现正循【{activeDirection.DisplayName}】推进，辅以【{activeLaw.DisplayName}】与【{activeTalentPlan.DisplayName}】。卷中记录近时警讯、调度回响与诸殿札记，供宗主迅速断局。";
    }

    public static string BuildReportOverviewText(GameState state, GameCalendarInfo calendarInfo)
    {
        var warehouseLoad = state.GetWarehouseUsed() / Math.Max(state.WarehouseCapacity, 1.0);
        var activeQuarterDecree = SectGovernanceRules.GetActiveQuarterDecreeDefinition(state);
        var decreeText = SectGovernanceRules.GetActiveQuarterDecree(state) == SectQuarterDecreeType.None
            ? "本季暂未颁新法旨"
            : $"本季法旨：{activeQuarterDecree.DisplayName}";

        return $"[{calendarInfo.TimeOfDayName}] 当前门人 {state.Population:0}，仓储负载 {warehouseLoad * 100:0}% ，危兆 {state.Threat:0}% 。{decreeText}，以下为当前经营快照。";
    }

    public static IReadOnlyList<SectChronicleReportCard> BuildReportCards(GameState state, GameCalendarInfo calendarInfo)
    {
        var warehouseLoad = state.GetWarehouseUsed() / Math.Max(state.WarehouseCapacity, 1.0);
        var foodReservePerCapita = state.Food / Math.Max(state.Population, 1.0);
        var toolCoverage = IndustryRules.GetToolCoverage(state);
        var assignedPopulation = state.GetAssignedPopulation();
        var activeDirection = SectGovernanceRules.GetActiveDevelopmentDefinition(state);
        var activeLaw = SectGovernanceRules.GetActiveLawDefinition(state);
        var activeTalentPlan = SectGovernanceRules.GetActiveTalentPlanDefinition(state);

        var populationTone = state.Happiness < HappinessLowThreshold || state.SickPopulation > Math.Max(state.Population * 0.12, 6.0)
            ? SectChronicleCardTone.Warning
            : state.Happiness >= 75.0
                ? SectChronicleCardTone.Good
                : SectChronicleCardTone.Neutral;

        var storageTone = warehouseLoad >= WarehouseHighLoadThreshold ||
                          foodReservePerCapita < FoodReserveLowThreshold ||
                          toolCoverage < ToolCoverageLowThreshold
            ? SectChronicleCardTone.Warning
            : warehouseLoad < 0.65 && foodReservePerCapita >= 5.5
                ? SectChronicleCardTone.Good
                : SectChronicleCardTone.Neutral;

        var expeditionTone = state.Threat >= ThreatHighThreshold
            ? SectChronicleCardTone.Warning
            : state.ExplorationEnabled && state.ElitePopulation > 0
                ? SectChronicleCardTone.Good
                : SectChronicleCardTone.Neutral;

        var researchTone = state.TechLevel > 0 || state.Research > 12.0
            ? SectChronicleCardTone.Good
            : SectChronicleCardTone.Neutral;

        return
        [
            new SectChronicleReportCard(
                "门人盘面",
                $"门人 {state.Population:0} · 真传 {state.ElitePopulation:0} · 现役 {assignedPopulation:0}/{state.Population:0}",
                $"新苗 {state.ChildPopulation:0} · 盛年 {state.AdultPopulation:0} · 守峰 {state.ElderPopulation:0} · 病患 {state.SickPopulation:0} · 民心 {state.Happiness:0.#}",
                populationTone),
            new SectChronicleReportCard(
                "库藏供养",
                $"仓储 {warehouseLoad * 100:0}% · 人均存粮 {foodReservePerCapita:0.0}",
                $"食物 {state.Food:0} · 工器覆盖 {toolCoverage * 100:0}% · 营造材料 {state.ConstructionMaterials:0} · 库容 {state.GetWarehouseUsed():0}/{Math.Max(state.WarehouseCapacity, 1.0):0}",
                storageTone),
            new SectChronicleReportCard(
                "护山外务",
                state.ExplorationEnabled
                    ? $"危兆 {state.Threat:0}% · 外务已至第 {Math.Max(state.ExplorationDepth, 1)} 层"
                    : $"危兆 {state.Threat:0}% · 外务暂驻山门",
                state.ExplorationEnabled
                    ? $"骨干 {state.ElitePopulation:0} 名整装在外 · 行装均分 {state.AvgGearScore:0.#} · 当前时段 {calendarInfo.TimeOfDayName}"
                    : $"当前宜整修护山、补给法器与重整队列，再择机放行外务。",
                expeditionTone),
            new SectChronicleReportCard(
                "传承营造",
                $"研修 T{Math.Max(state.TechLevel + 1, 1)} · 研修进度 {state.Research:0.#}",
                $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} {state.AgricultureBuildings:0} · {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} {state.WorkshopBuildings:0} · {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} {state.ResearchBuildings:0} · {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} {state.AdministrationBuildings:0}\n当前治宗：{activeDirection.DisplayName} / {activeLaw.DisplayName} / {activeTalentPlan.DisplayName}",
                researchTone)
        ];
    }

    public static SectChronicleSettlementSnapshot CaptureSnapshot(GameState state)
    {
        var warehouseLoad = state.GetWarehouseUsed() / Math.Max(state.WarehouseCapacity, 1.0);

        return new SectChronicleSettlementSnapshot(
            state.HourSettlements,
            state.GameMinutes,
            state.Population,
            state.Food,
            state.Gold,
            state.ContributionPoints,
            state.Research,
            state.Threat,
            state.Happiness,
            warehouseLoad);
    }

    public static string BuildSettlementTrendText(IReadOnlyList<SectChronicleSettlementSnapshot> snapshots)
    {
        if (snapshots.Count == 0)
        {
            return "[color=#5b4d42]尚未记录时辰结算。[/color]";
        }

        var calendarSystem = new GameCalendarSystem();
        var lines = new List<string>();
        var startIndex = Math.Max(0, snapshots.Count - 3);

        for (var index = snapshots.Count - 1; index >= startIndex; index--)
        {
            var snapshot = snapshots[index];
            var calendarInfo = calendarSystem.Describe(snapshot.GameMinutes);
            var prefix = $"[{calendarInfo.TimeOfDayName}] 第 {Math.Max(snapshot.HourSettlementIndex, 0)} 次结算";

            if (index == 0)
            {
                lines.Add($"[color=#5b4d42]• {prefix}：立卷记入，门人 {snapshot.Population:0} · 粮廪 {snapshot.Food:0} · 灵石 {snapshot.Gold:0} · 危兆 {snapshot.Threat:0}%[/color]");
                continue;
            }

            var previous = snapshots[index - 1];
            var foodDelta = snapshot.Food - previous.Food;
            var goldDelta = snapshot.Gold - previous.Gold;
            var contributionDelta = snapshot.ContributionPoints - previous.ContributionPoints;
            var researchDelta = snapshot.Research - previous.Research;
            var threatDelta = snapshot.Threat - previous.Threat;

            var tone = ResolveTrendTone(foodDelta, goldDelta, researchDelta, threatDelta);
            var color = tone switch
            {
                SectChronicleCardTone.Good => "#61743d",
                SectChronicleCardTone.Warning => "#9e2a22",
                _ => "#5b4d42"
            };

            lines.Add(
                $"[color={color}]• {prefix}：粮廪 {FormatSigned(foodDelta)} · 灵石 {FormatSigned(goldDelta)} · 功绩 {FormatSigned(contributionDelta)} · 研修 {FormatSigned(researchDelta)} · 危兆 {FormatSigned(threatDelta)}%[/color]");
        }

        return string.Join("\n", lines);
    }

    public static string BuildLogOverviewText(SectChronicleLogCategory category, int visibleCount, int totalCount)
    {
        var categoryName = category switch
        {
            SectChronicleLogCategory.Governance => "政务",
            SectChronicleLogCategory.Resources => "库藏",
            SectChronicleLogCategory.Expedition => "外务",
            SectChronicleLogCategory.Archive => "留档",
            _ => "全卷"
        };

        return $"当前筛读：{categoryName} · 收录 {visibleCount}/{totalCount} 条近时札记。";
    }

    public static SectChronicleLogCategory ClassifyLogEntry(string logLine)
    {
        if (string.IsNullOrWhiteSpace(logLine))
        {
            return SectChronicleLogCategory.All;
        }

        if (ContainsAny(logLine, "存档", "读档", "归档", "留影"))
        {
            return SectChronicleLogCategory.Archive;
        }

        if (ContainsAny(logLine, "探险", "历练", "外务", "胜利", "妖", "遭遇", "护山"))
        {
            return SectChronicleLogCategory.Expedition;
        }

        if (ContainsAny(logLine, "仓", "粮", "木", "石", "工器", "灵石", "材料", "矿", "扩建", "营造"))
        {
            return SectChronicleLogCategory.Resources;
        }

        if (ContainsAny(logLine, "法令", "门规", "调拨", "人", "执事", "治宗", "讲法", "峰"))
        {
            return SectChronicleLogCategory.Governance;
        }

        return SectChronicleLogCategory.All;
    }

    public static SectChronicleReportCard BuildQuarterReportCard(
        GameState state,
        GameCalendarInfo calendarInfo,
        IReadOnlyList<SectChronicleSettlementSnapshot> snapshots)
    {
        var seasonalTrend = SummarizeWindowTrend(snapshots, 6);
        var tone = seasonalTrend.ThreatDelta >= 3.0 || seasonalTrend.FoodDelta < -16.0
            ? SectChronicleCardTone.Warning
            : seasonalTrend.GoldDelta > 0.0 || seasonalTrend.ResearchDelta > 0.0
                ? SectChronicleCardTone.Good
                : SectChronicleCardTone.Neutral;

        return new SectChronicleReportCard(
            "季度摘要",
            $"{calendarInfo.QuarterName} · {calendarInfo.QuarterProgressText}",
            $"近季内回看：粮廪 {FormatSigned(seasonalTrend.FoodDelta)} · 灵石 {FormatSigned(seasonalTrend.GoldDelta)} · 研修 {FormatSigned(seasonalTrend.ResearchDelta)} · 危兆 {FormatSigned(seasonalTrend.ThreatDelta)}% · 当前民心 {state.Happiness:0.#}",
            tone);
    }

    public static SectChronicleReportCard BuildYearReportCard(
        GameState state,
        GameCalendarInfo calendarInfo,
        IReadOnlyList<SectChronicleSettlementSnapshot> snapshots)
    {
        var yearlyTrend = SummarizeWindowTrend(snapshots, 12);
        var tone = yearlyTrend.ThreatDelta >= 4.0 || yearlyTrend.FoodDelta < -24.0
            ? SectChronicleCardTone.Warning
            : yearlyTrend.GoldDelta >= 12.0 || yearlyTrend.ContributionDelta >= 10.0
                ? SectChronicleCardTone.Good
                : SectChronicleCardTone.Neutral;

        var yearLabel = calendarInfo.DateText.Split(' ')[0];

        return new SectChronicleReportCard(
            "年度摘要",
            $"{yearLabel} · {calendarInfo.SolarTermName}",
            $"近年内回看：粮廪 {FormatSigned(yearlyTrend.FoodDelta)} · 灵石 {FormatSigned(yearlyTrend.GoldDelta)} · 功绩 {FormatSigned(yearlyTrend.ContributionDelta)} · 危兆 {FormatSigned(yearlyTrend.ThreatDelta)}% · 仓储 {state.GetWarehouseUsed():0}/{Math.Max(state.WarehouseCapacity, 1.0):0}",
            tone);
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static (double FoodDelta, double GoldDelta, double ContributionDelta, double ResearchDelta, double ThreatDelta) SummarizeWindowTrend(
        IReadOnlyList<SectChronicleSettlementSnapshot> snapshots,
        int windowSize)
    {
        if (snapshots.Count < 2)
        {
            return default;
        }

        var startIndex = Math.Max(0, snapshots.Count - windowSize);
        var first = snapshots[startIndex];
        var last = snapshots[^1];

        return (
            last.Food - first.Food,
            last.Gold - first.Gold,
            last.ContributionPoints - first.ContributionPoints,
            last.Research - first.Research,
            last.Threat - first.Threat);
    }

    private static SectChronicleCardTone ResolveTrendTone(double foodDelta, double goldDelta, double researchDelta, double threatDelta)
    {
        if (threatDelta >= 3.0 || (foodDelta < -12.0 && goldDelta < 0.0))
        {
            return SectChronicleCardTone.Warning;
        }

        if (threatDelta <= -2.0 || goldDelta >= 8.0 || researchDelta >= 6.0)
        {
            return SectChronicleCardTone.Good;
        }

        return SectChronicleCardTone.Neutral;
    }

    private static string FormatSigned(double value)
    {
        var rounded = Math.Round(value);
        return rounded >= 0 ? $"+{rounded:0}" : rounded.ToString("0");
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
            return SectNamingRules.ReplaceKnownNames(state,
                $"[{timeOfDay}] 外务历练持续至第 {state.ExplorationDepth} 层，当前有 {state.ElitePopulation} 名骨干在外整装。");
        }

        if (SectGovernanceRules.GetActiveQuarterDecree(state) != SectQuarterDecreeType.None)
        {
            return SectNamingRules.ReplaceKnownNames(state,
                $"[{timeOfDay}] 本季法令【{activeQuarterDecree.DisplayName}】在行：{activeQuarterDecree.ShortEffect}。");
        }

        if (SectPeakSupportRules.GetActiveSupport(state) != SectPeakSupportType.Balanced)
        {
            return SectNamingRules.ReplaceKnownNames(state,
                $"[{timeOfDay}] 协同峰当前为【{activePeakSupport.DisplayName}】，{activePeakSupport.ShortEffect}。");
        }

        if (state.Happiness >= 75.0)
        {
            return SectNamingRules.ReplaceKnownNames(state,
                $"[{timeOfDay}] 山门民心 {state.Happiness:0.#} ，诸殿气象安定，可顺势扩建与收徒。");
        }

        if (state.TechLevel > 0 || state.Research > 0.0)
        {
            return SectNamingRules.ReplaceKnownNames(state,
                $"[{timeOfDay}] 传法院研修推进中，当前科技 T{Math.Max(state.TechLevel + 1, 1)}，可继续叠加讲法与深造。");
        }

        if (warehouseLoad < 0.65 && state.ConstructionMaterials >= 4)
        {
            return SectNamingRules.ReplaceKnownNames(state,
                $"[{timeOfDay}] 仓储仍有余裕，护山构件 {state.ConstructionMaterials:0} ，可择机推进营造。");
        }

        return SectNamingRules.ReplaceKnownNames(state,
            $"[{timeOfDay}] 宗主中枢正行【{activeLawName}】与【{activeTalentPlanName}】，诸堂按令运转。");
    }
}
