using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

/// <summary>
/// 修炼卷的小时结算折算层。
/// </summary>
public sealed class DiscipleCultivationSettlementSystem
{
    public bool TickHour(GameState state, out string? log)
    {
        log = null;
        DiscipleCultivationRules.EnsureDefaults(state);
        DiscipleDirectiveRules.EnsureDefaults(state);

        if (state.DiscipleCultivationAssignments.Count <= 0)
        {
            return false;
        }

        var profiles = DiscipleRosterSystem.BuildRoster(state)
            .Where(profile => state.DiscipleCultivationAssignments.ContainsKey(profile.Id))
            .ToArray();
        if (profiles.Length <= 0)
        {
            return false;
        }

        var researchGain = 0.0;
        var contributionGain = 0.0;
        var toolGain = 0.0;
        var happinessGain = 0.0;
        var threatReduction = 0.0;

        var counts = new Dictionary<DiscipleCultivationAssignmentType, int>();
        var milestoneLogs = new List<string>();
        string? insightChronicleLog = null;
        foreach (var profile in profiles)
        {
            var assignment = DiscipleCultivationRules.GetAssignment(state, profile.Id);
            if (assignment == DiscipleCultivationAssignmentType.None)
            {
                continue;
            }

            counts[assignment] = counts.TryGetValue(assignment, out var count) ? count + 1 : 1;
            var previousProgress = DiscipleCultivationRules.GetLongTermProgress(state, profile.Id, assignment);
            var previousBranchSummary = DiscipleCultivationRules.BuildSpecializationBranchSummary(state, profile.Id);
            var progressGain = ResolveProgressGain(profile, assignment);
            var nextProgress = DiscipleCultivationRules.AddLongTermProgress(state, profile.Id, assignment, progressGain);
            if (milestoneLogs.Count < 2 &&
                DiscipleCultivationRules.TryBuildProgressMilestoneLog(
                    assignment,
                    previousProgress,
                    nextProgress,
                    profile.Name,
                    out var milestoneLog))
            {
                milestoneLogs.Add(milestoneLog);
                DiscipleCultivationRules.AppendHistoryEntry(
                    state,
                    profile.Id,
                    $"{DiscipleCultivationRules.GetTrackDisplayName(assignment)}进至「{DiscipleCultivationRules.BuildTrackProgressSummary(state, profile.Id, assignment)}」。");
            }

            // 十二期补充：当专修分支首次成形或发生转向时，给一条专名成形日志与个人履历。
            if (milestoneLogs.Count < 2 &&
                DiscipleCultivationRules.TryBuildBranchIdentityLog(
                    state,
                    profile.Id,
                    profile.Name,
                    previousBranchSummary,
                    out var branchLog,
                    out var branchHistoryEntry))
            {
                milestoneLogs.Add(branchLog);
                DiscipleCultivationRules.AppendHistoryEntry(state, profile.Id, branchHistoryEntry);
            }

            // 长期火候成形后，按固定节律偶发一则修炼札记，既进入右栏近闻，也写回弟子个人履历。
            if (string.IsNullOrWhiteSpace(insightChronicleLog) &&
                DiscipleCultivationRules.TryBuildInsightChronicle(
                    state,
                    profile,
                    out var chronicleLog,
                    out var historyEntry))
            {
                // 九期补充：札记触发时，顺带结出一缕轻量机缘，让不同培养路线有更可见的“所得”。
                if (DiscipleCultivationRules.TryResolveInsightBoon(state, profile, out var insightBoon) &&
                    insightBoon.HasEffect)
                {
                    researchGain += insightBoon.ResearchGain;
                    contributionGain += insightBoon.ContributionGain;
                    toolGain += insightBoon.ToolGain;
                    happinessGain += insightBoon.HappinessGain;
                    threatReduction += insightBoon.ThreatReduction;

                    insightChronicleLog = string.IsNullOrWhiteSpace(insightBoon.ChronicleSuffix)
                        ? chronicleLog
                        : $"{chronicleLog} {insightBoon.ChronicleSuffix}";
                    historyEntry = string.IsNullOrWhiteSpace(insightBoon.HistorySuffix)
                        ? historyEntry
                        : $"{historyEntry} {insightBoon.HistorySuffix}";
                }
                else
                {
                    insightChronicleLog = chronicleLog;
                }

                DiscipleCultivationRules.AppendHistoryEntry(state, profile.Id, historyEntry);
            }

            switch (assignment)
            {
                case DiscipleCultivationAssignmentType.SkillTraining:
                    researchGain += 0.02 + (profile.Insight * 0.0010) + (profile.Execution * 0.0008);
                    contributionGain += 0.01 + (profile.Execution * 0.0004);
                    break;

                case DiscipleCultivationAssignmentType.TechniquePolish:
                    researchGain += 0.05 + (profile.Insight * 0.0016) + (profile.RealmTier * 0.020);
                    break;

                case DiscipleCultivationAssignmentType.CraftPractice:
                    toolGain += 0.03 + (profile.Craft * 0.0018) + (profile.Execution * 0.0008);
                    contributionGain += 0.01 + (profile.Craft * 0.0003);
                    break;

                case DiscipleCultivationAssignmentType.Meditation:
                    happinessGain += 0.03 + (profile.Mood * 0.0010) + (profile.Health * 0.0005);
                    threatReduction += 0.01 + (profile.Mood * 0.0004);
                    break;
            }
        }

        if (researchGain <= 0 &&
            contributionGain <= 0 &&
            toolGain <= 0 &&
            happinessGain <= 0 &&
            threatReduction <= 0)
        {
            return false;
        }

        state.Research += researchGain;
        var actualContributionGain = contributionGain > 0
            ? InventoryRules.ApplyDelta(state, nameof(GameState.ContributionPoints), contributionGain)
            : 0;
        var actualToolGain = toolGain > 0
            ? InventoryRules.ApplyDelta(state, nameof(GameState.IndustryTools), toolGain)
            : 0;
        state.Happiness = Math.Clamp(state.Happiness + happinessGain, 5, 100);
        state.Threat = Math.Clamp(state.Threat - threatReduction, 0, 100);

        var summaryParts = new List<string>();
        AppendSummary(summaryParts, counts, DiscipleCultivationAssignmentType.SkillTraining);
        AppendSummary(summaryParts, counts, DiscipleCultivationAssignmentType.TechniquePolish);
        AppendSummary(summaryParts, counts, DiscipleCultivationAssignmentType.CraftPractice);
        AppendSummary(summaryParts, counts, DiscipleCultivationAssignmentType.Meditation);

        var gainParts = new List<string>();
        if (researchGain > 0)
        {
            gainParts.Add($"传承研修 +{researchGain:0.0#}");
        }

        if (actualContributionGain > 0)
        {
            gainParts.Add(MaterialSemanticRules.FormatDelta(nameof(GameState.ContributionPoints), actualContributionGain));
        }

        if (actualToolGain > 0)
        {
            gainParts.Add(MaterialSemanticRules.FormatDelta(nameof(GameState.IndustryTools), actualToolGain));
        }

        if (happinessGain > 0.0001)
        {
            gainParts.Add($"民心 +{happinessGain:0.0#}");
        }

        if (threatReduction > 0.0001)
        {
            gainParts.Add($"危兆 -{threatReduction:0.0#}");
        }

        var milestoneSuffix = milestoneLogs.Count > 0
            ? $" {string.Join(" ", milestoneLogs)}"
            : string.Empty;
        var insightSuffix = string.IsNullOrWhiteSpace(insightChronicleLog)
            ? string.Empty
            : $" {insightChronicleLog}";
        log = $"修炼卷运转：{string.Join("，", summaryParts)}；{string.Join("，", gainParts)}。{milestoneSuffix}{insightSuffix}";
        return true;
    }

    /// <summary>
    /// 计算长期成长火候增量；要求可见但不压过主链收益。
    /// </summary>
    private static double ResolveProgressGain(DiscipleProfile profile, DiscipleCultivationAssignmentType assignmentType)
    {
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining =>
                0.08 + (profile.Insight * 0.0007) + (profile.Execution * 0.0005),
            DiscipleCultivationAssignmentType.TechniquePolish =>
                0.10 + (profile.Insight * 0.0009) + (profile.RealmTier * 0.015),
            DiscipleCultivationAssignmentType.CraftPractice =>
                0.09 + (profile.Craft * 0.0008) + (profile.Execution * 0.0005),
            DiscipleCultivationAssignmentType.Meditation =>
                0.08 + (profile.Mood * 0.0007) + (profile.Health * 0.0004),
            _ => 0
        };
    }

    private static void AppendSummary(
        List<string> summaryParts,
        IReadOnlyDictionary<DiscipleCultivationAssignmentType, int> counts,
        DiscipleCultivationAssignmentType assignmentType)
    {
        if (!counts.TryGetValue(assignmentType, out var count) || count <= 0)
        {
            return;
        }

        summaryParts.Add($"{DiscipleCultivationRules.GetAssignmentDisplayName(assignmentType)} {count} 名");
    }
}
