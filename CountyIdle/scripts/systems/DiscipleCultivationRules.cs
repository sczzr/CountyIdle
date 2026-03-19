using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

/// <summary>
/// 弟子修炼对个体表现的温和反哺摘要。
/// </summary>
public readonly record struct DiscipleCultivationFeedback(
    int HealthBonus,
    int MoodBonus,
    int CombatBonus,
    int CraftBonus,
    int InsightBonus,
    int ExecutionBonus,
    int ContributionBonus);

/// <summary>
/// 修炼札记触发后的轻量机缘收益；保持可见，但不压过主结算链。
/// </summary>
public readonly record struct DiscipleCultivationInsightBoon(
    double ResearchGain,
    int ContributionGain,
    int ToolGain,
    double HappinessGain,
    double ThreatReduction,
    string ChronicleSuffix,
    string HistorySuffix)
{
    public bool HasEffect =>
        ResearchGain > 0.0001 ||
        ContributionGain > 0 ||
        ToolGain > 0 ||
        HappinessGain > 0.0001 ||
        ThreatReduction > 0.0001 ||
        !string.IsNullOrWhiteSpace(ChronicleSuffix) ||
        !string.IsNullOrWhiteSpace(HistorySuffix);
}

/// <summary>
/// 弟子修炼卷的持久化、长期成长与展示规则。
/// </summary>
public static class DiscipleCultivationRules
{
    // 每段修炼火候所需的累计值；当前先做四段式长期成长。
    private const double CultivationStageStep = 3.0;
    private const double CultivationProgressCap = 9.0;
    private const double BranchFormationThreshold = 1.5;
    private const int MaxHistoryEntries = 6;
    private const int DefaultHistoryPreviewEntries = 3;

    private static readonly DiscipleCultivationAssignmentType[] PersistentTrackOrder =
    [
        DiscipleCultivationAssignmentType.SkillTraining,
        DiscipleCultivationAssignmentType.TechniquePolish,
        DiscipleCultivationAssignmentType.CraftPractice,
        DiscipleCultivationAssignmentType.Meditation
    ];

    /// <summary>
    /// 主导修炼路数下进一步牵出的专修分支；用于十二期更细的培养辨识与轻量联动。
    /// </summary>
    private enum CultivationBranchType
    {
        None,
        SkillFoundation,
        SkillPatrol,
        SkillWorkshop,
        SkillStillness,
        TechniquePure,
        TechniqueExpedition,
        TechniqueCraft,
        TechniqueLecture,
        CraftSteward,
        CraftField,
        CraftArtifact,
        CraftStillness,
        MeditationPure,
        MeditationGuard,
        MeditationLecture,
        MeditationCraft
    }

    /// <summary>
    /// 专修分支解析结果，记录主导路数下当前分出的偏锋方向与次修来源。
    /// </summary>
    private readonly record struct CultivationBranchResolution(
        CultivationBranchType BranchType,
        DiscipleCultivationAssignmentType SecondaryTrack,
        double SecondaryProgress);

    public static void EnsureDefaults(GameState state)
    {
        state.DiscipleCultivationAssignments ??= new Dictionary<int, string>();
        state.DiscipleSkillTrainingProgress ??= new Dictionary<int, double>();
        state.DiscipleTechniquePolishProgress ??= new Dictionary<int, double>();
        state.DiscipleCraftPracticeProgress ??= new Dictionary<int, double>();
        state.DiscipleMeditationProgress ??= new Dictionary<int, double>();
        state.DiscipleCultivationHistory ??= new Dictionary<int, List<string>>();

        var normalizedAssignments = new Dictionary<int, string>();
        foreach (var (discipleId, rawAssignment) in state.DiscipleCultivationAssignments)
        {
            if (discipleId <= 0 || discipleId > Math.Max(state.Population, 0))
            {
                continue;
            }

            var assignmentType = NormalizeAssignment(rawAssignment);
            if (assignmentType == DiscipleCultivationAssignmentType.None)
            {
                continue;
            }

            normalizedAssignments[discipleId] = assignmentType.ToString();
        }

        state.DiscipleCultivationAssignments = normalizedAssignments;
        var population = Math.Max(state.Population, 0);
        state.DiscipleSkillTrainingProgress = NormalizeProgressMap(state.DiscipleSkillTrainingProgress, population);
        state.DiscipleTechniquePolishProgress = NormalizeProgressMap(state.DiscipleTechniquePolishProgress, population);
        state.DiscipleCraftPracticeProgress = NormalizeProgressMap(state.DiscipleCraftPracticeProgress, population);
        state.DiscipleMeditationProgress = NormalizeProgressMap(state.DiscipleMeditationProgress, population);
        state.DiscipleCultivationHistory = NormalizeHistoryMap(state.DiscipleCultivationHistory, population);
    }

    public static DiscipleCultivationAssignmentType GetAssignment(GameState state, int discipleId)
    {
        EnsureDefaults(state);
        if (discipleId <= 0 || !state.DiscipleCultivationAssignments.TryGetValue(discipleId, out var rawAssignment))
        {
            return DiscipleCultivationAssignmentType.None;
        }

        return NormalizeAssignment(rawAssignment);
    }

    public static void SetAssignment(GameState state, int discipleId, DiscipleCultivationAssignmentType assignmentType)
    {
        EnsureDefaults(state);
        if (discipleId <= 0)
        {
            return;
        }

        if (assignmentType == DiscipleCultivationAssignmentType.None)
        {
            state.DiscipleCultivationAssignments.Remove(discipleId);
            return;
        }

        state.DiscipleCultivationAssignments[discipleId] = assignmentType.ToString();
    }

    public static int GetAssignmentCount(GameState state, DiscipleCultivationAssignmentType assignmentType)
    {
        if (assignmentType == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        EnsureDefaults(state);
        return state.DiscipleCultivationAssignments.Count(item => NormalizeAssignment(item.Value) == assignmentType);
    }

    public static string BuildAssignmentSummary(GameState state)
    {
        EnsureDefaults(state);
        return
            $"技能 {GetAssignmentCount(state, DiscipleCultivationAssignmentType.SkillTraining)} · " +
            $"功法 {GetAssignmentCount(state, DiscipleCultivationAssignmentType.TechniquePolish)} · " +
            $"技艺 {GetAssignmentCount(state, DiscipleCultivationAssignmentType.CraftPractice)} · " +
            $"打坐 {GetAssignmentCount(state, DiscipleCultivationAssignmentType.Meditation)}";
    }

    public static string GetAssignmentDisplayName(DiscipleCultivationAssignmentType assignmentType)
    {
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "技能修炼",
            DiscipleCultivationAssignmentType.TechniquePolish => "功法打磨",
            DiscipleCultivationAssignmentType.CraftPractice => "技艺练习",
            DiscipleCultivationAssignmentType.Meditation => "打坐修炼",
            _ => "常制修行"
        };
    }

    public static string GetAssignmentShortEffect(DiscipleCultivationAssignmentType assignmentType)
    {
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "每次时辰结算少量补入传承研修与贡献，适合稳固根基。",
            DiscipleCultivationAssignmentType.TechniquePolish => "每次时辰结算提供更高传承研修，适合专注打磨法门。",
            DiscipleCultivationAssignmentType.CraftPractice => "每次时辰结算少量产出工器与贡献，适合轮修炼器、炼丹、阵法等术门。",
            DiscipleCultivationAssignmentType.Meditation => "每次时辰结算小幅提振民心并缓和危兆，适合静养气海与心境。",
            _ => "当前未指定额外修炼安排，维持常制修行。"
        };
    }

    /// <summary>
    /// 获取指定修炼轨当前累计火候。
    /// </summary>
    public static double GetLongTermProgress(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType assignmentType)
    {
        EnsureDefaults(state);
        if (discipleId <= 0 || assignmentType == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var progressMap = GetProgressMap(state, assignmentType);
        return progressMap.TryGetValue(discipleId, out var progress)
            ? Math.Clamp(progress, 0, CultivationProgressCap)
            : 0;
    }

    /// <summary>
    /// 为指定修炼轨追加长期成长进度，并返回追加后的火候值。
    /// </summary>
    public static double AddLongTermProgress(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType assignmentType,
        double progressDelta)
    {
        EnsureDefaults(state);
        if (discipleId <= 0 ||
            assignmentType == DiscipleCultivationAssignmentType.None ||
            progressDelta <= 0)
        {
            return GetLongTermProgress(state, discipleId, assignmentType);
        }

        var progressMap = GetProgressMap(state, assignmentType);
        var current = progressMap.TryGetValue(discipleId, out var existing) ? existing : 0;
        var next = Math.Clamp(current + progressDelta, 0, CultivationProgressCap);
        if (next <= 0.0001)
        {
            progressMap.Remove(discipleId);
            return 0;
        }

        progressMap[discipleId] = next;
        return next;
    }

    /// <summary>
    /// 判断当前修炼火候是否跨过长期成长档位，用于结算日志提示。
    /// </summary>
    public static bool TryBuildProgressMilestoneLog(
        DiscipleCultivationAssignmentType assignmentType,
        double previousProgress,
        double currentProgress,
        string discipleName,
        out string log)
    {
        log = string.Empty;
        if (assignmentType == DiscipleCultivationAssignmentType.None || string.IsNullOrWhiteSpace(discipleName))
        {
            return false;
        }

        var previousTier = ResolveMilestoneTier(previousProgress);
        var currentTier = ResolveMilestoneTier(currentProgress);
        if (currentTier <= previousTier || currentTier <= 0)
        {
            return false;
        }

        log = $"“{discipleName}”的{GetTrackDisplayName(assignmentType)}进至「{GetTrackStageName(assignmentType, currentProgress)}」。";
        return true;
    }

    /// <summary>
    /// 构建单个弟子的长期修炼积累摘要。
    /// </summary>
    public static string BuildLongTermProgressSummary(GameState state, int discipleId)
    {
        EnsureDefaults(state);
        var parts = PersistentTrackOrder
            .Select(track => $"{GetTrackShortDisplayName(track)} {FormatTrackProgress(track, GetLongTermProgress(state, discipleId, track))}")
            .ToArray();
        return string.Join(" · ", parts);
    }

    /// <summary>
    /// 构建当前主修轨的火候摘要；若未登记主修，则回退为当前最强积累。
    /// </summary>
    public static string BuildActiveTrackProgressSummary(GameState state, int discipleId)
    {
        EnsureDefaults(state);
        var assignment = GetAssignment(state, discipleId);
        if (assignment != DiscipleCultivationAssignmentType.None)
        {
            return BuildTrackProgressSummary(state, discipleId, assignment);
        }

        var dominantTrack = PersistentTrackOrder
            .Select(track => new { Track = track, Progress = GetLongTermProgress(state, discipleId, track) })
            .OrderByDescending(item => item.Progress)
            .ThenBy(item => (int)item.Track)
            .FirstOrDefault();

        return dominantTrack == null || dominantTrack.Progress <= 0.0001
            ? "当前尚无长期修炼积累。"
            : $"既有火候：{BuildTrackProgressSummary(state, discipleId, dominantTrack.Track)}";
    }

    /// <summary>
    /// 构建某条修炼轨的火候摘要。
    /// </summary>
    public static string BuildTrackProgressSummary(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType assignmentType)
    {
        if (assignmentType == DiscipleCultivationAssignmentType.None)
        {
            return "常制修行";
        }

        return $"{GetTrackDisplayName(assignmentType)}：{FormatTrackProgress(assignmentType, GetLongTermProgress(state, discipleId, assignmentType))}";
    }

    /// <summary>
    /// 获取指定修炼轨相对满火候的进度比例，供卷册进度条直接读取。
    /// </summary>
    public static double GetTrackProgressRatio(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType assignmentType)
    {
        if (assignmentType == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var progress = GetLongTermProgress(state, discipleId, assignmentType);
        return CultivationProgressCap <= 0.0001
            ? 0
            : Math.Clamp(progress / CultivationProgressCap, 0, 1);
    }

    /// <summary>
    /// 解析长期修炼对弟子个体表现的温和反哺。
    /// </summary>
    public static DiscipleCultivationFeedback ResolvePerformanceFeedback(GameState state, int discipleId)
    {
        EnsureDefaults(state);
        var skillProgress = GetLongTermProgress(state, discipleId, DiscipleCultivationAssignmentType.SkillTraining);
        var techniqueProgress = GetLongTermProgress(state, discipleId, DiscipleCultivationAssignmentType.TechniquePolish);
        var craftProgress = GetLongTermProgress(state, discipleId, DiscipleCultivationAssignmentType.CraftPractice);
        var meditationProgress = GetLongTermProgress(state, discipleId, DiscipleCultivationAssignmentType.Meditation);

        var healthBonus = Math.Clamp(
            (int)Math.Round((skillProgress * 0.20) + (meditationProgress * 0.35), MidpointRounding.AwayFromZero),
            0,
            6);
        var moodBonus = Math.Clamp(
            (int)Math.Round(meditationProgress * 0.55, MidpointRounding.AwayFromZero),
            0,
            7);
        var combatBonus = Math.Clamp(
            (int)Math.Round((techniqueProgress * 0.25) + (skillProgress * 0.15), MidpointRounding.AwayFromZero),
            0,
            5);
        var craftBonus = Math.Clamp(
            (int)Math.Round(craftProgress * 0.55, MidpointRounding.AwayFromZero),
            0,
            7);
        var insightBonus = Math.Clamp(
            (int)Math.Round(techniqueProgress * 0.60, MidpointRounding.AwayFromZero),
            0,
            7);
        var executionBonus = Math.Clamp(
            (int)Math.Round((skillProgress * 0.40) + (craftProgress * 0.20), MidpointRounding.AwayFromZero),
            0,
            6);
        var contributionBonus = Math.Clamp(
            (int)Math.Round((craftProgress * 0.18) + (skillProgress * 0.12), MidpointRounding.AwayFromZero),
            0,
            4);
        var baseFeedback = new DiscipleCultivationFeedback(
            healthBonus,
            moodBonus,
            combatBonus,
            craftBonus,
            insightBonus,
            executionBonus,
            contributionBonus);
        var specializationFeedback = ResolveSpecializationEffectFeedback(state, discipleId);
        return MergeFeedback(baseFeedback, specializationFeedback);
    }

    /// <summary>
    /// 构建长期火候反哺个体表现的摘要。
    /// </summary>
    public static string BuildPerformanceFeedbackSummary(GameState state, int discipleId)
    {
        var feedback = ResolvePerformanceFeedback(state, discipleId);
        var parts = new List<string>();
        if (feedback.InsightBonus > 0)
        {
            parts.Add($"悟性 +{feedback.InsightBonus}");
        }

        if (feedback.CraftBonus > 0)
        {
            parts.Add($"匠艺 +{feedback.CraftBonus}");
        }

        if (feedback.ExecutionBonus > 0)
        {
            parts.Add($"执行 +{feedback.ExecutionBonus}");
        }

        if (feedback.MoodBonus > 0)
        {
            parts.Add($"心境 +{feedback.MoodBonus}");
        }

        if (feedback.HealthBonus > 0)
        {
            parts.Add($"气血 +{feedback.HealthBonus}");
        }

        if (feedback.CombatBonus > 0)
        {
            parts.Add($"战修 +{feedback.CombatBonus}");
        }

        if (feedback.ContributionBonus > 0)
        {
            parts.Add($"贡献 +{feedback.ContributionBonus}");
        }

        return parts.Count <= 0
            ? "火候尚浅，个体表现反哺未显。"
            : string.Join(" · ", parts);
    }

    /// <summary>
    /// 构建专修分支已经落地成形的专精效用，强调本轮成长不只是称谓变化。
    /// </summary>
    public static string BuildSpecializationEffectSummary(GameState state, int discipleId)
    {
        var branch = ResolveSpecializationBranch(state, discipleId, GetDominantTrack(state, discipleId));
        if (branch.BranchType == CultivationBranchType.None)
        {
            return "专精效用尚未成形。";
        }

        var effect = ResolveSpecializationEffectFeedback(state, discipleId);
        var parts = new List<string>();
        if (effect.CombatBonus > 0)
        {
            parts.Add($"战修 +{effect.CombatBonus}");
        }

        if (effect.CraftBonus > 0)
        {
            parts.Add($"匠艺 +{effect.CraftBonus}");
        }

        if (effect.InsightBonus > 0)
        {
            parts.Add($"悟性 +{effect.InsightBonus}");
        }

        if (effect.ExecutionBonus > 0)
        {
            parts.Add($"执行 +{effect.ExecutionBonus}");
        }

        if (effect.HealthBonus > 0)
        {
            parts.Add($"气血 +{effect.HealthBonus}");
        }

        if (effect.MoodBonus > 0)
        {
            parts.Add($"心境 +{effect.MoodBonus}");
        }

        if (effect.ContributionBonus > 0)
        {
            parts.Add($"贡献 +{effect.ContributionBonus}");
        }

        return parts.Count <= 0
            ? "专精效用尚未成形。"
            : $"{GetBranchDisplayName(branch.BranchType)}：{string.Join(" · ", parts)}";
    }

    /// <summary>
    /// 构建适合日志/结果页拼接的专精效用摘要；若尚未成形则返回空串。
    /// </summary>
    public static string BuildSpecializationEffectLogSummary(GameState state, int discipleId)
    {
        var summary = BuildSpecializationEffectSummary(state, discipleId);
        return string.Equals(summary, "专精效用尚未成形。", StringComparison.Ordinal)
            ? string.Empty
            : $"专精效用：{summary}";
    }

    /// <summary>
    /// 解析当前弟子的主导修炼路数；若火候都很浅，则视为未成路数。
    /// </summary>
    public static DiscipleCultivationAssignmentType GetDominantTrack(GameState state, int discipleId)
    {
        EnsureDefaults(state);
        var dominantTrack = PersistentTrackOrder
            .Select(track => new { Track = track, Progress = GetLongTermProgress(state, discipleId, track) })
            .OrderByDescending(item => item.Progress)
            .ThenBy(item => (int)item.Track)
            .FirstOrDefault();
        return dominantTrack == null || dominantTrack.Progress <= 0.0001
            ? DiscipleCultivationAssignmentType.None
            : dominantTrack.Track;
    }

    /// <summary>
    /// 构建当前弟子的专精路数摘要，供卷册与按钮等短文本展示。
    /// </summary>
    public static string BuildSpecializationSummary(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return "路数未成";
        }

        var stageLabel = GetTrackStageLabel(state, discipleId, dominantTrack);
        var focusSummary = dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "执行/气血更稳",
            DiscipleCultivationAssignmentType.TechniquePolish => "悟性/战修偏盛",
            DiscipleCultivationAssignmentType.CraftPractice => "匠艺/执行见长",
            DiscipleCultivationAssignmentType.Meditation => "心境/气血更稳",
            _ => "常制修行"
        };

        return string.IsNullOrWhiteSpace(stageLabel)
            ? $"{GetTrackDisplayName(dominantTrack)}（{focusSummary}）"
            : $"{GetTrackDisplayName(dominantTrack)}·{stageLabel}（{focusSummary}）";
    }

    /// <summary>
    /// 构建当前弟子的专修分支摘要；用于十二期把同一路数继续细化为更明确的培养偏锋。
    /// </summary>
    public static string BuildSpecializationBranchSummary(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return "分支未定";
        }

        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        return branch.BranchType == CultivationBranchType.None
            ? "分支未定"
            : $"{GetBranchDisplayName(branch.BranchType)}（{GetBranchFocusSummary(branch.BranchType)}）";
    }

    /// <summary>
    /// 构建专修分支的批注，强调当前路数进一步往哪类差事与成长方向偏转。
    /// </summary>
    public static string BuildSpecializationBranchNarrative(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return "专修分支尚浅，仍待火候牵出偏锋。";
        }

        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        if (branch.BranchType == CultivationBranchType.None)
        {
            return "专修分支尚浅，仍待火候牵出偏锋。";
        }

        var intro = branch.SecondaryTrack == DiscipleCultivationAssignmentType.None ||
                    branch.SecondaryProgress < BranchFormationThreshold - 0.0001
            ? $"当前主脉先沿“{GetBranchDisplayName(branch.BranchType)}”稳步推进。"
            : $"又以{GetTrackDisplayName(branch.SecondaryTrack)}牵出偏锋，分支渐转作“{GetBranchDisplayName(branch.BranchType)}”。";
        return $"{intro}{GetBranchNarrative(branch.BranchType)}";
    }

    /// <summary>
    /// 构建当前专修分支映照出的功法偏锋名。
    /// </summary>
    public static string BuildTechniqueSpecializationLabel(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return "未定";
        }

        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        return branch.BranchType switch
        {
            CultivationBranchType.SkillFoundation => "稳根守势",
            CultivationBranchType.SkillPatrol => "巡山劲诀",
            CultivationBranchType.SkillWorkshop => "营造稳诀",
            CultivationBranchType.SkillStillness => "守心静诀",
            CultivationBranchType.TechniquePure => "正脉精修",
            CultivationBranchType.TechniqueExpedition => "锋行战诀",
            CultivationBranchType.TechniqueCraft => "器理推诀",
            CultivationBranchType.TechniqueLecture => "讲法玄诀",
            CultivationBranchType.CraftSteward => "总坊整诀",
            CultivationBranchType.CraftField => "营造砺诀",
            CultivationBranchType.CraftArtifact => "器火演诀",
            CultivationBranchType.CraftStillness => "丹火守诀",
            CultivationBranchType.MeditationPure => "澄心养气",
            CultivationBranchType.MeditationGuard => "静值守山",
            CultivationBranchType.MeditationLecture => "澄悟讲法",
            CultivationBranchType.MeditationCraft => "养息调工",
            _ => "未定"
        };
    }

    /// <summary>
    /// 构建当前专修分支映照出的主修技艺名。
    /// </summary>
    public static string BuildPrimaryCraftSpecializationLabel(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return "未定";
        }

        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        return branch.BranchType switch
        {
            CultivationBranchType.SkillFoundation => "田务",
            CultivationBranchType.SkillPatrol => "巡检",
            CultivationBranchType.SkillWorkshop => "营造",
            CultivationBranchType.SkillStillness => "守阵",
            CultivationBranchType.TechniquePure => "推演",
            CultivationBranchType.TechniqueExpedition => "护山",
            CultivationBranchType.TechniqueCraft => "炼器",
            CultivationBranchType.TechniqueLecture => "讲法",
            CultivationBranchType.CraftSteward => "庶务",
            CultivationBranchType.CraftField => "营造",
            CultivationBranchType.CraftArtifact => "炼器",
            CultivationBranchType.CraftStillness => "丹火",
            CultivationBranchType.MeditationPure => "校勘",
            CultivationBranchType.MeditationGuard => "守山",
            CultivationBranchType.MeditationLecture => "讲法",
            CultivationBranchType.MeditationCraft => "细工",
            _ => "未定"
        };
    }

    /// <summary>
    /// 构建当前专修分支映照出的辅修技艺名。
    /// </summary>
    public static string BuildSecondaryCraftSpecializationLabel(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return "未定";
        }

        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        return branch.BranchType switch
        {
            CultivationBranchType.SkillFoundation => "护山值守",
            CultivationBranchType.SkillPatrol => "外勤压线",
            CultivationBranchType.SkillWorkshop => "工序统筹",
            CultivationBranchType.SkillStillness => "讲法旁听",
            CultivationBranchType.TechniquePure => "经卷推演",
            CultivationBranchType.TechniqueExpedition => "巡山步势",
            CultivationBranchType.TechniqueCraft => "行装整备",
            CultivationBranchType.TechniqueLecture => "经卷校勘",
            CultivationBranchType.CraftSteward => "账务统筹",
            CultivationBranchType.CraftField => "器用轮值",
            CultivationBranchType.CraftArtifact => "阵枢营造",
            CultivationBranchType.CraftStillness => "总坊细整",
            CultivationBranchType.MeditationPure => "心法复盘",
            CultivationBranchType.MeditationGuard => "轮值巡看",
            CultivationBranchType.MeditationLecture => "典册校勘",
            CultivationBranchType.MeditationCraft => "慢火整备",
            _ => "未定"
        };
    }

    /// <summary>
    /// 基于当前专修分支，为基础功法名追加偏锋映照。
    /// </summary>
    public static string DecorateTechniqueDisplayName(GameState state, int discipleId, string baseTechnique)
    {
        if (string.IsNullOrWhiteSpace(baseTechnique))
        {
            return baseTechnique;
        }

        var specialization = BuildTechniqueSpecializationLabel(state, discipleId);
        return specialization == "未定"
            ? baseTechnique
            : $"{baseTechnique}·{specialization}";
    }

    /// <summary>
    /// 基于当前专修分支，为主修技艺追加偏锋映照。
    /// </summary>
    public static string DecoratePrimarySkillDisplayName(GameState state, int discipleId, string baseSkill)
    {
        if (string.IsNullOrWhiteSpace(baseSkill))
        {
            return baseSkill;
        }

        var specialization = BuildPrimaryCraftSpecializationLabel(state, discipleId);
        return specialization == "未定"
            ? baseSkill
            : $"{baseSkill}·{specialization}";
    }

    /// <summary>
    /// 用分支映照更新辅修技艺名；若分支尚浅则回退原有称谓。
    /// </summary>
    public static string DecorateSecondarySkillDisplayName(GameState state, int discipleId, string fallbackSkill)
    {
        var specialization = BuildSecondaryCraftSpecializationLabel(state, discipleId);
        return specialization == "未定" ? fallbackSkill : specialization;
    }

    /// <summary>
    /// 汇总当前弟子的功法/技艺专精映照，供卷册状态区直接显示。
    /// </summary>
    public static string BuildTechniqueCraftSpecializationSummary(GameState state, int discipleId)
    {
        var technique = BuildTechniqueSpecializationLabel(state, discipleId);
        var primarySkill = BuildPrimaryCraftSpecializationLabel(state, discipleId);
        var secondarySkill = BuildSecondaryCraftSpecializationLabel(state, discipleId);
        if (technique == "未定" && primarySkill == "未定" && secondarySkill == "未定")
        {
            return "功法技艺专精尚待映照。";
        }

        return $"功法偏锋：{technique} · 主艺偏锋：{primarySkill} · 辅艺映照：{secondarySkill}";
    }

    /// <summary>
    /// 若专修分支首次成形或发生转向，则补记一条专名成形履历。
    /// </summary>
    public static bool TryBuildBranchIdentityLog(
        GameState state,
        int discipleId,
        string discipleName,
        string previousBranchSummary,
        out string log,
        out string historyEntry)
    {
        log = string.Empty;
        historyEntry = string.Empty;

        if (discipleId <= 0 || string.IsNullOrWhiteSpace(discipleName))
        {
            return false;
        }

        var currentBranchSummary = BuildSpecializationBranchSummary(state, discipleId);
        if (currentBranchSummary == "分支未定" ||
            string.Equals(currentBranchSummary, previousBranchSummary, StringComparison.Ordinal))
        {
            return false;
        }

        var techniqueLabel = BuildTechniqueSpecializationLabel(state, discipleId);
        var primarySkillLabel = BuildPrimaryCraftSpecializationLabel(state, discipleId);
        var actionText = string.Equals(previousBranchSummary, "分支未定", StringComparison.Ordinal)
            ? "定作"
            : "转作";

        log =
            $"“{discipleName}”的专修偏锋{actionText}「{currentBranchSummary}」，" +
            $"功法偏锋「{techniqueLabel}」，主艺偏锋「{primarySkillLabel}」。";
        historyEntry =
            $"专名：分支{actionText}「{currentBranchSummary}」，" +
            $"功法偏锋「{techniqueLabel}」，主艺偏锋「{primarySkillLabel}」。";
        return true;
    }

    /// <summary>
    /// 构建当前专精路数的叙述性批注，适合卷中批语或弟子备注。
    /// </summary>
    public static string BuildSpecializationNarrative(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        return dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "根基磨砺见效，行事与护体都更稳。",
            DiscipleCultivationAssignmentType.TechniquePolish => "功法火候渐开，悟性与战修更显锋芒。",
            DiscipleCultivationAssignmentType.CraftPractice => "技艺手感渐熟，匠艺与执事手段更见章法。",
            DiscipleCultivationAssignmentType.Meditation => "静修积气渐稳，心境与气血更易守成。",
            _ => "专精路数尚浅，仍待长期磨砺。"
        };
    }

    /// <summary>
    /// 构建当前弟子的差事相性摘要，便于玩家判断更适合投向哪类职责。
    /// </summary>
    public static string BuildDutyAffinitySummary(GameState state, DiscipleProfile profile)
    {
        var dominantTrack = GetDominantTrack(state, profile.Id);
        return dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "田务轮值 / 巡检补位更稳",
            DiscipleCultivationAssignmentType.TechniquePolish => "外务历练 / 护山巡检更强",
            DiscipleCultivationAssignmentType.CraftPractice => "工坊营造 / 执事补位更顺手",
            DiscipleCultivationAssignmentType.Meditation => "长线值守 / 讲法校勘更稳",
            _ => "尚在积累"
        };
    }

    /// <summary>
    /// 构建当前弟子的差事相性批注，说明这条修炼路数为何会反哺具体职责。
    /// </summary>
    public static string BuildDutyAffinityNarrative(GameState state, DiscipleProfile profile)
    {
        var dominantTrack = GetDominantTrack(state, profile.Id);
        var baseNarrative = dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住轮值与补位节奏，田务长线、巡山备勤与稳态执事差事更容易站住。",
            DiscipleCultivationAssignmentType.TechniquePolish => "功法路数利于压线与应敌，外务历练、护山巡检与推演差事更容易出锋芒。",
            DiscipleCultivationAssignmentType.CraftPractice => "技艺路数擅长工序衔接，工坊营造、总坊整备与执事补位会更顺手。",
            DiscipleCultivationAssignmentType.Meditation => "静修路数利于长线守成，讲法校勘、久班值守与田务轮值更能稳住节奏。",
            _ => "修炼火候仍浅，差事相性尚未稳定成形。"
        };
        var branchSuffix = BuildBranchNarrativeSuffix(state, profile.Id);
        return string.IsNullOrWhiteSpace(branchSuffix)
            ? baseNarrative
            : $"{baseNarrative} {branchSuffix}";
    }

    /// <summary>
    /// 构建修炼路数对重点名册的说明，供弟子谱与日志展示。
    /// </summary>
    public static string BuildDirectiveAffinityNarrative(
        GameState state,
        int discipleId,
        DiscipleDirectiveType directiveType)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        var baseNarrative = directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => "功法路数与外务线最相宜，历练战力与护送压线会额外吃到路数加成。",
                DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住外务行程与近身应对，外勤压线会额外受益。",
                DiscipleCultivationAssignmentType.CraftPractice => "技艺路数可补强行装整备与回流折算，外务收益会额外受益。",
                DiscipleCultivationAssignmentType.Meditation => "静修路数可收束长线消耗，久行外务时更能守住节奏。",
                _ => "当前修炼路数尚浅，外务线仍主要依赖基础战力与执行。"
            },
            DiscipleDirectiveType.StewardCandidate => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.CraftPractice => "技艺路数与执事线最合手，内务排程、工序衔接与补位执行会额外吃到路数加成。",
                DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住庶务轮值与补位节奏，稳态执事线会额外受益。",
                DiscipleCultivationAssignmentType.Meditation => "静修路数能压住久班心气，长线内务与讲法校勘会更稳。",
                DiscipleCultivationAssignmentType.TechniquePolish => "功法路数也能反哺研修与巡检类庶务，但当前更偏副向助力。",
                _ => "当前修炼路数尚浅，执事线仍主要依赖基础执行、悟性与贡献。"
            },
            _ => "当前未纳入重点名册，差事相性暂只作为后续观察。"
        };
        var branchSuffix = BuildBranchNarrativeSuffix(state, discipleId);
        return string.IsNullOrWhiteSpace(branchSuffix)
            ? baseNarrative
            : $"{baseNarrative} {branchSuffix}";
    }

    /// <summary>
    /// 构建修炼路数对具体内务条目的相性批注，供执事补位说明使用。
    /// </summary>
    public static string BuildTaskAffinityNarrative(GameState state, int discipleId, SectTaskType taskType)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        var baseNarrative = taskType switch
        {
            SectTaskType.FieldDuty => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住轮值与奔走，本条阵材采炼会额外吃到稳态加成。",
                DiscipleCultivationAssignmentType.Meditation => "静修路数能压住长线消耗，本条阵材采炼会额外吃到守成加成。",
                _ => "这条田务差事主要靠基础岗位能力支撑，路数加成较轻。"
            },
            SectTaskType.WorkshopDuty => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.CraftPractice => "技艺路数与阵枢营造最合手，本条营造差事会额外吃到工序熟手加成。",
                DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住长线营造节奏，本条营造差事会得到稳态执行加成。",
                _ => "这条工坊差事仍以岗位基础与工务经验为主。"
            },
            SectTaskType.LogisticsPatrol => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => "功法路数与巡山警戒最相宜，本条巡检差事会额外吃到压线与应敌加成。",
                DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住巡山轮值，本条巡检差事会得到补位稳态加成。",
                _ => "这条巡检差事仍主要依赖岗位基础与当下战修。"
            },
            SectTaskType.ScriptureStudy => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => "功法路数能推高推演锋度，本条研修差事会额外吃到悟性加成。",
                DiscipleCultivationAssignmentType.Meditation => "静修路数能稳住久坐与讲法节奏，本条研修差事会额外吃到心境加成。",
                _ => "这条研修差事仍主要依赖基础悟性与学识。"
            },
            SectTaskType.SectCommerce => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.CraftPractice => "技艺路数擅长整备与衔接，本条总坊差事会额外吃到工序加成。",
                DiscipleCultivationAssignmentType.SkillTraining => "根基路数能稳住账务轮值节奏，本条总坊差事会额外吃到稳态加成。",
                DiscipleCultivationAssignmentType.Meditation => "静修路数利于久班值守，本条总坊差事会额外吃到守成加成。",
                _ => "这条总坊差事仍主要依赖基础执行与贡献。"
            },
            SectTaskType.OuterTrade => dominantTrack switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => "功法路数能稳住外事压线，本条外事行商会额外吃到应变加成。",
                DiscipleCultivationAssignmentType.CraftPractice => "技艺路数能补强行装整备，本条外事行商会额外吃到筹备加成。",
                _ => "这条外事差事仍主要依赖基础贡献与外务经验。"
            },
            _ => "当前差事暂未建立额外路数批注。"
        };
        var branchSuffix = BuildBranchNarrativeSuffix(state, discipleId);
        return string.IsNullOrWhiteSpace(branchSuffix)
            ? baseNarrative
            : $"{baseNarrative} {branchSuffix}";
    }

    /// <summary>
    /// 尝试构建一条修炼感悟札记，用于主日志与弟子个人履历回看。
    /// 当前采用确定性节律触发，避免无状态下同小时内刷出过多札记。
    /// </summary>
    public static bool TryBuildInsightChronicle(
        GameState state,
        DiscipleProfile profile,
        out string chronicleLog,
        out string historyEntry)
    {
        chronicleLog = string.Empty;
        historyEntry = string.Empty;

        var dominantTrack = GetDominantTrack(state, profile.Id);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return false;
        }

        var dominantProgress = GetLongTermProgress(state, profile.Id, dominantTrack);
        var milestoneTier = ResolveMilestoneTier(dominantProgress);
        if (milestoneTier <= 0)
        {
            return false;
        }

        var triggerCycle = dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => 8,
            DiscipleCultivationAssignmentType.TechniquePolish => 9,
            DiscipleCultivationAssignmentType.CraftPractice => 10,
            DiscipleCultivationAssignmentType.Meditation => 12,
            _ => 0
        };
        if (triggerCycle <= 0)
        {
            return false;
        }

        // 使用时辰结算数 + 弟子编号形成固定节律，保证偶发但可复现。
        var triggerValue = (state.HourSettlements + (profile.Id * 3) + ((int)dominantTrack + 1) * 5 + milestoneTier) % triggerCycle;
        if (triggerValue != 0)
        {
            return false;
        }

        var stageLabel = GetTrackStageLabel(state, profile.Id, dominantTrack);
        var insightText = BuildInsightNarrative(state, profile, dominantTrack);
        if (string.IsNullOrWhiteSpace(insightText))
        {
            return false;
        }

        var routeSummary = BuildSpecializationSummary(state, profile.Id);
        var branchSummary = BuildSpecializationBranchSummary(state, profile.Id);
        chronicleLog =
            $"修炼札记：{profile.Name}于{GetTrackDisplayName(dominantTrack)}" +
            $"{(string.IsNullOrWhiteSpace(stageLabel) ? string.Empty : $"「{stageLabel}」")}之际，{insightText} 当前路数：{routeSummary} · 分支：{branchSummary}。";
        historyEntry = $"札记：{insightText}";
        return true;
    }

    /// <summary>
    /// 当修炼札记触发时，顺带结出一缕轻量机缘，用作九期的可见奖励反馈。
    /// </summary>
    public static bool TryResolveInsightBoon(
        GameState state,
        DiscipleProfile profile,
        out DiscipleCultivationInsightBoon boon)
    {
        boon = default;

        var dominantTrack = GetDominantTrack(state, profile.Id);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return false;
        }

        var dominantProgress = GetLongTermProgress(state, profile.Id, dominantTrack);
        var milestoneTier = ResolveMilestoneTier(dominantProgress);
        if (milestoneTier <= 0)
        {
            return false;
        }

        switch (dominantTrack)
        {
            case DiscipleCultivationAssignmentType.SkillTraining:
                boon = new DiscipleCultivationInsightBoon(
                    0,
                    1,
                    0,
                    0,
                    0,
                    $"并得机缘：吐纳与步法在轮值间暗合，{MaterialSemanticRules.FormatDelta(nameof(GameState.ContributionPoints), 1)}。",
                    $"机缘：吐纳与步法暗合，{MaterialSemanticRules.FormatDelta(nameof(GameState.ContributionPoints), 1)}。");
                return true;

            case DiscipleCultivationAssignmentType.TechniquePolish:
                var researchGain = 0.16 + (milestoneTier * 0.10);
                boon = new DiscipleCultivationInsightBoon(
                    researchGain,
                    0,
                    0,
                    0,
                    0,
                    $"并得机缘：法诀转折忽然贯通，传承研修 +{researchGain:0.0#}。",
                    $"机缘：法诀转折忽通，传承研修 +{researchGain:0.0#}。");
                return true;

            case DiscipleCultivationAssignmentType.CraftPractice:
                boon = new DiscipleCultivationInsightBoon(
                    0,
                    0,
                    1,
                    0,
                    0,
                    $"并得机缘：工序顺手成式，{MaterialSemanticRules.FormatDelta(nameof(GameState.IndustryTools), 1)}。",
                    $"机缘：工序顺手成式，{MaterialSemanticRules.FormatDelta(nameof(GameState.IndustryTools), 1)}。");
                return true;

            case DiscipleCultivationAssignmentType.Meditation:
                var happinessGain = 0.18 + (milestoneTier * 0.08);
                var threatReduction = 0.06 + (milestoneTier * 0.04);
                boon = new DiscipleCultivationInsightBoon(
                    0,
                    0,
                    0,
                    happinessGain,
                    threatReduction,
                    $"并得机缘：心息自调，民心 +{happinessGain:0.0#}、危兆 -{threatReduction:0.0#}。",
                    $"机缘：心息自调，民心 +{happinessGain:0.0#}、危兆 -{threatReduction:0.0#}。");
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 重点名册筛选时，修炼路数可提供额外相性评分。
    /// </summary>
    public static double GetDirectiveCandidateScoreBonus(
        GameState state,
        int discipleId,
        DiscipleDirectiveType directiveType)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var strengthRatio = GetTrackStrengthRatio(state, discipleId, dominantTrack);
        var fitWeight = GetDirectiveFitWeight(dominantTrack, directiveType);
        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        var branchStrengthRatio = GetBranchStrengthRatio(branch);
        var branchFitWeight = GetBranchDirectiveFitWeight(branch.BranchType, directiveType);
        var maxBonus = directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => 18.0,
            DiscipleDirectiveType.StewardCandidate => 16.0,
            _ => 0.0
        };

        return (fitWeight * strengthRatio * maxBonus) +
               (branchFitWeight * branchStrengthRatio * 6.0);
    }

    /// <summary>
    /// 外务候补对队伍战力的额外路数加成。
    /// </summary>
    public static double GetOuterMissionPowerBonus(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var strengthRatio = GetTrackStrengthRatio(state, discipleId, dominantTrack);
        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        var branchStrengthRatio = GetBranchStrengthRatio(branch);
        return
            (GetDirectiveFitWeight(dominantTrack, DiscipleDirectiveType.OuterMissionCandidate) * strengthRatio * 0.65) +
            (GetBranchDirectiveFitWeight(branch.BranchType, DiscipleDirectiveType.OuterMissionCandidate) * branchStrengthRatio * 0.12);
    }

    /// <summary>
    /// 外务候补对收益回流的额外路数加成。
    /// </summary>
    public static double GetOuterMissionLootBonus(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var strengthRatio = GetTrackStrengthRatio(state, discipleId, dominantTrack);
        var fitWeight = dominantTrack switch
        {
            DiscipleCultivationAssignmentType.CraftPractice => 1.0,
            DiscipleCultivationAssignmentType.TechniquePolish => 0.75,
            DiscipleCultivationAssignmentType.SkillTraining => 0.55,
            DiscipleCultivationAssignmentType.Meditation => 0.40,
            _ => 0
        };
        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        var branchStrengthRatio = GetBranchStrengthRatio(branch);
        var branchFitWeight = GetBranchOuterMissionLootWeight(branch.BranchType);

        return (fitWeight * strengthRatio * 0.018) +
               (branchFitWeight * branchStrengthRatio * 0.006);
    }

    /// <summary>
    /// 执事培养对贡献回流的额外路数加成。
    /// </summary>
    public static double GetStewardContributionBonus(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var strengthRatio = GetTrackStrengthRatio(state, discipleId, dominantTrack);
        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        var branchStrengthRatio = GetBranchStrengthRatio(branch);
        return
            (GetDirectiveFitWeight(dominantTrack, DiscipleDirectiveType.StewardCandidate) * strengthRatio * 0.015) +
            (GetBranchDirectiveFitWeight(branch.BranchType, DiscipleDirectiveType.StewardCandidate) * branchStrengthRatio * 0.005);
    }

    /// <summary>
    /// 具体内务挑选执事时，修炼路数可提供额外相性评分。
    /// </summary>
    public static double GetTaskCandidateScoreBonus(GameState state, int discipleId, SectTaskType taskType)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var strengthRatio = GetTrackStrengthRatio(state, discipleId, dominantTrack);
        var fitWeight = GetTaskFitWeight(dominantTrack, taskType);
        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        var branchStrengthRatio = GetBranchStrengthRatio(branch);
        var branchFitWeight = GetBranchTaskFitWeight(branch.BranchType, taskType);
        return (fitWeight * strengthRatio * 18.0) +
               (branchFitWeight * branchStrengthRatio * 6.0);
    }

    /// <summary>
    /// 具体内务执行时，修炼路数可额外提供轻量执行修正。
    /// </summary>
    public static double GetTaskExecutionBonus(GameState state, int discipleId, SectTaskType taskType)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return 0;
        }

        var strengthRatio = GetTrackStrengthRatio(state, discipleId, dominantTrack);
        var fitWeight = GetTaskFitWeight(dominantTrack, taskType);
        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        var branchStrengthRatio = GetBranchStrengthRatio(branch);
        var branchFitWeight = GetBranchTaskFitWeight(branch.BranchType, taskType);
        return (fitWeight * strengthRatio * 0.020) +
               (branchFitWeight * branchStrengthRatio * 0.008);
    }

    /// <summary>
    /// 获取修炼轨当前阶段名，用于标签或术语拼接。
    /// </summary>
    public static string GetTrackStageLabel(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType assignmentType)
    {
        var progress = GetLongTermProgress(state, discipleId, assignmentType);
        return progress <= 0.0001 ? string.Empty : GetTrackStageName(assignmentType, progress);
    }

    /// <summary>
    /// 为弟子追加一条修炼履历，默认仅保留最近若干条。
    /// </summary>
    public static void AppendHistoryEntry(GameState state, int discipleId, string entry)
    {
        EnsureDefaults(state);
        if (discipleId <= 0 || string.IsNullOrWhiteSpace(entry))
        {
            return;
        }

        var calendarInfo = new GameCalendarSystem().Describe(state.GameMinutes);
        var formattedEntry = $"[{calendarInfo.DateText} · {calendarInfo.TimeOfDayName}] {entry.Trim()}";
        if (!state.DiscipleCultivationHistory.TryGetValue(discipleId, out var historyEntries) || historyEntries == null)
        {
            historyEntries = new List<string>();
            state.DiscipleCultivationHistory[discipleId] = historyEntries;
        }

        if (historyEntries.Count > 0 && string.Equals(historyEntries[^1], formattedEntry, StringComparison.Ordinal))
        {
            return;
        }

        historyEntries.Add(formattedEntry);
        if (historyEntries.Count > MaxHistoryEntries)
        {
            historyEntries.RemoveRange(0, historyEntries.Count - MaxHistoryEntries);
        }
    }

    /// <summary>
    /// 读取弟子的近时修炼履历（按最近优先返回）。
    /// </summary>
    public static IReadOnlyList<string> GetHistoryEntries(GameState state, int discipleId, int take = DefaultHistoryPreviewEntries)
    {
        EnsureDefaults(state);
        if (discipleId <= 0 ||
            take <= 0 ||
            !state.DiscipleCultivationHistory.TryGetValue(discipleId, out var historyEntries) ||
            historyEntries == null ||
            historyEntries.Count <= 0)
        {
            return Array.Empty<string>();
        }

        return historyEntries
            .TakeLast(Math.Min(take, historyEntries.Count))
            .Reverse()
            .ToArray();
    }

    /// <summary>
    /// 构建近时履历的一句话摘要。
    /// </summary>
    public static string BuildLatestHistorySummary(GameState state, int discipleId)
    {
        var latest = GetHistoryEntries(state, discipleId, 1).FirstOrDefault();
        return string.IsNullOrWhiteSpace(latest) ? "近时尚无修炼记载。" : latest;
    }

    /// <summary>
    /// 优先回看最近的感悟/机缘记录，供卷册显眼位置展示。
    /// </summary>
    public static string BuildLatestInsightSummary(GameState state, int discipleId)
    {
        var latestInsight = GetHistoryEntries(state, discipleId, MaxHistoryEntries)
            .FirstOrDefault(entry =>
                entry.Contains("札记：", StringComparison.Ordinal) ||
                entry.Contains("机缘：", StringComparison.Ordinal));
        return string.IsNullOrWhiteSpace(latestInsight)
            ? "近时暂无感悟机缘。"
            : latestInsight;
    }

    /// <summary>
    /// 构建多行修炼履历文本，供弟子谱详录直接展示。
    /// </summary>
    public static string BuildHistoryMultilineText(GameState state, int discipleId, int take = DefaultHistoryPreviewEntries)
    {
        var entries = GetHistoryEntries(state, discipleId, take);
        if (entries.Count <= 0)
        {
            return "近时尚无修炼记载。";
        }

        return string.Join("\n", entries.Select(entry => $"• {entry}"));
    }

    public static string BuildDiscipleAssignmentSummary(GameState state, DiscipleProfile profile)
    {
        var assignment = GetAssignment(state, profile.Id);
        if (assignment == DiscipleCultivationAssignmentType.None)
        {
            var dormantProgress = BuildActiveTrackProgressSummary(state, profile.Id);
            return profile.AgeBand == DiscipleAgeBand.Seedling
                ? "启蒙课业为主，稳固根基。"
                : dormantProgress == "当前尚无长期修炼积累。"
                    ? "尚未收入修炼卷，维持常制修行。"
                    : $"尚未收入修炼卷，维持常制修行。{dormantProgress}";
        }

        return
            $"{GetAssignmentDisplayName(assignment)}：{GetAssignmentShortEffect(assignment)} " +
            $"当前火候：{BuildTrackProgressSummary(state, profile.Id, assignment)}";
    }

    /// <summary>
    /// 获取修炼轨的长期成长显示名。
    /// </summary>
    public static string GetTrackDisplayName(DiscipleCultivationAssignmentType assignmentType)
    {
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "根基磨砺",
            DiscipleCultivationAssignmentType.TechniquePolish => "功法火候",
            DiscipleCultivationAssignmentType.CraftPractice => "技艺手感",
            DiscipleCultivationAssignmentType.Meditation => "静修积气",
            _ => "常制修行"
        };
    }

    private static string GetTrackShortDisplayName(DiscipleCultivationAssignmentType assignmentType)
    {
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "根基",
            DiscipleCultivationAssignmentType.TechniquePolish => "功法",
            DiscipleCultivationAssignmentType.CraftPractice => "技艺",
            DiscipleCultivationAssignmentType.Meditation => "静修",
            _ => "常制"
        };
    }

    private static string GetTrackStageName(DiscipleCultivationAssignmentType assignmentType, double progress)
    {
        if (progress <= 0.0001)
        {
            return "未起";
        }

        if (progress >= CultivationProgressCap - 0.0001)
        {
            return assignmentType switch
            {
                DiscipleCultivationAssignmentType.SkillTraining => "圆熟",
                DiscipleCultivationAssignmentType.TechniquePolish => "圆融",
                DiscipleCultivationAssignmentType.CraftPractice => "通明",
                DiscipleCultivationAssignmentType.Meditation => "圆定",
                _ => "圆熟"
            };
        }

        var stageIndex = Math.Clamp((int)Math.Floor(progress / CultivationStageStep), 0, 2);
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => stageIndex switch
            {
                0 => "稳根",
                1 => "融练",
                _ => "小成"
            },
            DiscipleCultivationAssignmentType.TechniquePolish => stageIndex switch
            {
                0 => "起手",
                1 => "渐熟",
                _ => "小成"
            },
            DiscipleCultivationAssignmentType.CraftPractice => stageIndex switch
            {
                0 => "试手",
                1 => "熟手",
                _ => "老练"
            },
            DiscipleCultivationAssignmentType.Meditation => stageIndex switch
            {
                0 => "凝息",
                1 => "调周",
                _ => "澄心"
            },
            _ => "稳修"
        };
    }

    private static string FormatTrackProgress(DiscipleCultivationAssignmentType assignmentType, double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, CultivationProgressCap);
        if (clampedProgress <= 0.0001)
        {
            return "未起";
        }

        if (clampedProgress >= CultivationProgressCap - 0.0001)
        {
            return GetTrackStageName(assignmentType, clampedProgress);
        }

        var stageIndex = Math.Clamp((int)Math.Floor(clampedProgress / CultivationStageStep), 0, 2);
        var stageStart = stageIndex * CultivationStageStep;
        var stagePercent = Math.Clamp((clampedProgress - stageStart) / CultivationStageStep * 100.0, 0, 99.9);
        return $"{GetTrackStageName(assignmentType, clampedProgress)} {stagePercent:0}%";
    }

    private static int ResolveMilestoneTier(double progress)
    {
        var clampedProgress = Math.Clamp(progress, 0, CultivationProgressCap);
        if (clampedProgress >= CultivationProgressCap - 0.0001)
        {
            return 3;
        }

        if (clampedProgress >= CultivationStageStep * 2 - 0.0001)
        {
            return 2;
        }

        return clampedProgress >= CultivationStageStep - 0.0001 ? 1 : 0;
    }

    private static Dictionary<int, double> NormalizeProgressMap(Dictionary<int, double>? source, int population)
    {
        var normalized = new Dictionary<int, double>();
        if (source == null || source.Count <= 0)
        {
            return normalized;
        }

        foreach (var (discipleId, rawProgress) in source)
        {
            if (discipleId <= 0 || discipleId > population)
            {
                continue;
            }

            var progress = Math.Clamp(rawProgress, 0, CultivationProgressCap);
            if (progress <= 0.0001)
            {
                continue;
            }

            normalized[discipleId] = progress;
        }

        return normalized;
    }

    private static Dictionary<int, List<string>> NormalizeHistoryMap(Dictionary<int, List<string>>? source, int population)
    {
        var normalized = new Dictionary<int, List<string>>();
        if (source == null || source.Count <= 0)
        {
            return normalized;
        }

        foreach (var (discipleId, rawEntries) in source)
        {
            if (discipleId <= 0 || discipleId > population || rawEntries == null)
            {
                continue;
            }

            var entries = rawEntries
                .Where(entry => !string.IsNullOrWhiteSpace(entry))
                .Select(entry => entry.Trim())
                .TakeLast(MaxHistoryEntries)
                .ToList();
            if (entries.Count <= 0)
            {
                continue;
            }

            normalized[discipleId] = entries;
        }

        return normalized;
    }

    private static Dictionary<int, double> GetProgressMap(
        GameState state,
        DiscipleCultivationAssignmentType assignmentType)
    {
        return assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => state.DiscipleSkillTrainingProgress,
            DiscipleCultivationAssignmentType.TechniquePolish => state.DiscipleTechniquePolishProgress,
            DiscipleCultivationAssignmentType.CraftPractice => state.DiscipleCraftPracticeProgress,
            DiscipleCultivationAssignmentType.Meditation => state.DiscipleMeditationProgress,
            _ => state.DiscipleSkillTrainingProgress
        };
    }

    private static double GetTrackStrengthRatio(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType assignmentType)
    {
        var progress = GetLongTermProgress(state, discipleId, assignmentType);
        return Math.Clamp(progress / CultivationProgressCap, 0, 1);
    }

    private static string BuildInsightNarrative(
        GameState state,
        DiscipleProfile profile,
        DiscipleCultivationAssignmentType dominantTrack)
    {
        var pool = dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => new[]
            {
                "晨课与轮值之间已能把吐纳、脚程与守势收成一线，田务与巡检时更见稳根之效。",
                "在根基磨砺中摸清了力气收放的分寸，久班补位与日常奔走都更不易散乱。",
                "行功与值守互相印证后，已能把护体与执行节奏一并稳住，长线差事更吃得住。"
            },
            DiscipleCultivationAssignmentType.TechniquePolish => new[]
            {
                "夜课推演时忽觉法门转折一线可通，外务压线与护山巡检时更能见锋芒。",
                "功法火候渐开之后，出手与悟解开始彼此照映，推演与应敌都更显章法。",
                "在法诀反复打磨里摸到了气机起落的关窍，历练与巡检时更容易压住阵脚。"
            },
            DiscipleCultivationAssignmentType.CraftPractice => new[]
            {
                "在阵枢试手时把前后工序串成一气，营造、整备与执事补位都更见顺手。",
                "技艺手感渐熟之后，已能同时照看细节与节拍，工坊与总坊差事更容易收拢成型。",
                "手底火候与事务节奏开始互相借力，营造与庶务交接时更不易失手。"
            },
            DiscipleCultivationAssignmentType.Meditation => new[]
            {
                "静坐回息时心湖一线澄明，讲法校勘与长线值守都更能守住火候。",
                "调周之后气息更匀，久班轮值与慢工细务时更能稳住心绪与耐性。",
                "静修积气渐稳后，已能把浮躁慢慢收平，守成差事与讲法复盘都更见沉着。"
            },
            _ => Array.Empty<string>()
        };

        if (pool.Length <= 0)
        {
            return string.Empty;
        }

        var index = (state.HourSettlements + profile.Id + (int)dominantTrack) % pool.Length;
        return pool[index];
    }

    private static string BuildBranchNarrativeSuffix(GameState state, int discipleId)
    {
        var branchSummary = BuildSpecializationBranchSummary(state, discipleId);
        return branchSummary == "分支未定"
            ? string.Empty
            : $"当前专修分支：{branchSummary}。";
    }

    private static DiscipleCultivationFeedback ResolveSpecializationEffectFeedback(GameState state, int discipleId)
    {
        var dominantTrack = GetDominantTrack(state, discipleId);
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return default;
        }

        var branch = ResolveSpecializationBranch(state, discipleId, dominantTrack);
        return branch.BranchType switch
        {
            CultivationBranchType.SkillFoundation => new DiscipleCultivationFeedback(1, 0, 0, 0, 0, 1, 0),
            CultivationBranchType.SkillPatrol => new DiscipleCultivationFeedback(0, 0, 1, 0, 0, 1, 0),
            CultivationBranchType.SkillWorkshop => new DiscipleCultivationFeedback(0, 0, 0, 1, 0, 1, 0),
            CultivationBranchType.SkillStillness => new DiscipleCultivationFeedback(1, 1, 0, 0, 0, 0, 0),
            CultivationBranchType.TechniquePure => new DiscipleCultivationFeedback(0, 0, 0, 0, 1, 0, 0),
            CultivationBranchType.TechniqueExpedition => new DiscipleCultivationFeedback(0, 0, 2, 0, 0, 0, 0),
            CultivationBranchType.TechniqueCraft => new DiscipleCultivationFeedback(0, 0, 0, 1, 1, 0, 0),
            CultivationBranchType.TechniqueLecture => new DiscipleCultivationFeedback(0, 0, 0, 0, 2, 0, 0),
            CultivationBranchType.CraftSteward => new DiscipleCultivationFeedback(0, 0, 0, 0, 0, 1, 1),
            CultivationBranchType.CraftField => new DiscipleCultivationFeedback(1, 0, 0, 1, 0, 0, 0),
            CultivationBranchType.CraftArtifact => new DiscipleCultivationFeedback(0, 0, 0, 2, 0, 0, 0),
            CultivationBranchType.CraftStillness => new DiscipleCultivationFeedback(0, 1, 0, 1, 0, 0, 0),
            CultivationBranchType.MeditationPure => new DiscipleCultivationFeedback(1, 1, 0, 0, 0, 0, 0),
            CultivationBranchType.MeditationGuard => new DiscipleCultivationFeedback(1, 0, 0, 0, 0, 1, 0),
            CultivationBranchType.MeditationLecture => new DiscipleCultivationFeedback(0, 1, 0, 0, 1, 0, 0),
            CultivationBranchType.MeditationCraft => new DiscipleCultivationFeedback(0, 1, 0, 1, 0, 0, 0),
            _ => default
        };
    }

    private static DiscipleCultivationFeedback MergeFeedback(
        DiscipleCultivationFeedback baseFeedback,
        DiscipleCultivationFeedback specializationFeedback)
    {
        return new DiscipleCultivationFeedback(
            Math.Clamp(baseFeedback.HealthBonus + specializationFeedback.HealthBonus, 0, 8),
            Math.Clamp(baseFeedback.MoodBonus + specializationFeedback.MoodBonus, 0, 8),
            Math.Clamp(baseFeedback.CombatBonus + specializationFeedback.CombatBonus, 0, 7),
            Math.Clamp(baseFeedback.CraftBonus + specializationFeedback.CraftBonus, 0, 8),
            Math.Clamp(baseFeedback.InsightBonus + specializationFeedback.InsightBonus, 0, 9),
            Math.Clamp(baseFeedback.ExecutionBonus + specializationFeedback.ExecutionBonus, 0, 8),
            Math.Clamp(baseFeedback.ContributionBonus + specializationFeedback.ContributionBonus, 0, 5));
    }

    private static CultivationBranchResolution ResolveSpecializationBranch(
        GameState state,
        int discipleId,
        DiscipleCultivationAssignmentType dominantTrack)
    {
        if (dominantTrack == DiscipleCultivationAssignmentType.None)
        {
            return new CultivationBranchResolution(CultivationBranchType.None, DiscipleCultivationAssignmentType.None, 0);
        }

        var dominantProgress = GetLongTermProgress(state, discipleId, dominantTrack);
        if (ResolveMilestoneTier(dominantProgress) <= 0)
        {
            return new CultivationBranchResolution(CultivationBranchType.None, DiscipleCultivationAssignmentType.None, 0);
        }

        var secondaryCandidates = PersistentTrackOrder
            .Where(track => track != dominantTrack)
            .Select(track => new
            {
                Track = track,
                Progress = GetLongTermProgress(state, discipleId, track)
            })
            .OrderByDescending(item => item.Progress)
            .ThenBy(item => (int)item.Track)
            .ToArray();
        var secondaryTrack = secondaryCandidates.Length > 0
            ? secondaryCandidates[0].Track
            : DiscipleCultivationAssignmentType.None;
        var secondaryProgress = secondaryCandidates.Length > 0
            ? secondaryCandidates[0].Progress
            : 0;
        var hasCrossBranch = secondaryTrack != DiscipleCultivationAssignmentType.None &&
                             secondaryProgress >= BranchFormationThreshold - 0.0001;

        var branchType = dominantTrack switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => hasCrossBranch
                ? secondaryTrack switch
                {
                    DiscipleCultivationAssignmentType.TechniquePolish => CultivationBranchType.SkillPatrol,
                    DiscipleCultivationAssignmentType.CraftPractice => CultivationBranchType.SkillWorkshop,
                    DiscipleCultivationAssignmentType.Meditation => CultivationBranchType.SkillStillness,
                    _ => CultivationBranchType.SkillFoundation
                }
                : CultivationBranchType.SkillFoundation,
            DiscipleCultivationAssignmentType.TechniquePolish => hasCrossBranch
                ? secondaryTrack switch
                {
                    DiscipleCultivationAssignmentType.SkillTraining => CultivationBranchType.TechniqueExpedition,
                    DiscipleCultivationAssignmentType.CraftPractice => CultivationBranchType.TechniqueCraft,
                    DiscipleCultivationAssignmentType.Meditation => CultivationBranchType.TechniqueLecture,
                    _ => CultivationBranchType.TechniquePure
                }
                : CultivationBranchType.TechniquePure,
            DiscipleCultivationAssignmentType.CraftPractice => hasCrossBranch
                ? secondaryTrack switch
                {
                    DiscipleCultivationAssignmentType.SkillTraining => CultivationBranchType.CraftField,
                    DiscipleCultivationAssignmentType.TechniquePolish => CultivationBranchType.CraftArtifact,
                    DiscipleCultivationAssignmentType.Meditation => CultivationBranchType.CraftStillness,
                    _ => CultivationBranchType.CraftSteward
                }
                : CultivationBranchType.CraftSteward,
            DiscipleCultivationAssignmentType.Meditation => hasCrossBranch
                ? secondaryTrack switch
                {
                    DiscipleCultivationAssignmentType.SkillTraining => CultivationBranchType.MeditationGuard,
                    DiscipleCultivationAssignmentType.TechniquePolish => CultivationBranchType.MeditationLecture,
                    DiscipleCultivationAssignmentType.CraftPractice => CultivationBranchType.MeditationCraft,
                    _ => CultivationBranchType.MeditationPure
                }
                : CultivationBranchType.MeditationPure,
            _ => CultivationBranchType.None
        };

        return new CultivationBranchResolution(branchType, hasCrossBranch ? secondaryTrack : DiscipleCultivationAssignmentType.None, secondaryProgress);
    }

    private static double GetBranchStrengthRatio(CultivationBranchResolution branch)
    {
        if (branch.SecondaryTrack == DiscipleCultivationAssignmentType.None ||
            branch.SecondaryProgress < BranchFormationThreshold - 0.0001)
        {
            return 0;
        }

        return CultivationProgressCap <= BranchFormationThreshold + 0.0001
            ? 0
            : Math.Clamp(
                (branch.SecondaryProgress - BranchFormationThreshold) /
                (CultivationProgressCap - BranchFormationThreshold),
                0,
                1);
    }

    private static string GetBranchDisplayName(CultivationBranchType branchType)
    {
        return branchType switch
        {
            CultivationBranchType.SkillFoundation => "田务固本",
            CultivationBranchType.SkillPatrol => "巡山劲行",
            CultivationBranchType.SkillWorkshop => "营造固本",
            CultivationBranchType.SkillStillness => "守心稳阵",
            CultivationBranchType.TechniquePure => "法门专修",
            CultivationBranchType.TechniqueExpedition => "外务锋行",
            CultivationBranchType.TechniqueCraft => "器理推演",
            CultivationBranchType.TechniqueLecture => "讲法悟玄",
            CultivationBranchType.CraftSteward => "总坊整备",
            CultivationBranchType.CraftField => "营造砺身",
            CultivationBranchType.CraftArtifact => "炼器演法",
            CultivationBranchType.CraftStillness => "丹火守息",
            CultivationBranchType.MeditationPure => "澄心守气",
            CultivationBranchType.MeditationGuard => "守山静值",
            CultivationBranchType.MeditationLecture => "讲法澄悟",
            CultivationBranchType.MeditationCraft => "养息调工",
            _ => "分支未定"
        };
    }

    private static string GetBranchFocusSummary(CultivationBranchType branchType)
    {
        return branchType switch
        {
            CultivationBranchType.SkillFoundation => "田务/守成",
            CultivationBranchType.SkillPatrol => "巡检/外务",
            CultivationBranchType.SkillWorkshop => "营造/整备",
            CultivationBranchType.SkillStillness => "守阵/长值",
            CultivationBranchType.TechniquePure => "悟法/正修",
            CultivationBranchType.TechniqueExpedition => "外务/护山",
            CultivationBranchType.TechniqueCraft => "炼器/筹备",
            CultivationBranchType.TechniqueLecture => "讲法/推演",
            CultivationBranchType.CraftSteward => "总坊/执事",
            CultivationBranchType.CraftField => "营造/田务",
            CultivationBranchType.CraftArtifact => "炼器/阵务",
            CultivationBranchType.CraftStillness => "细工/整备",
            CultivationBranchType.MeditationPure => "守心/校勘",
            CultivationBranchType.MeditationGuard => "值守/护山",
            CultivationBranchType.MeditationLecture => "讲法/澄悟",
            CultivationBranchType.MeditationCraft => "调息/细工",
            _ => "尚在凝线"
        };
    }

    private static string GetBranchNarrative(CultivationBranchType branchType)
    {
        return branchType switch
        {
            CultivationBranchType.SkillFoundation => "更偏稳根轮值，适合田务、守成与久班补位。",
            CultivationBranchType.SkillPatrol => "把筋骨与步势牵到巡线一侧，更偏巡检、护山与外务压线。",
            CultivationBranchType.SkillWorkshop => "把稳根火候带入工序节拍，更偏营造、总坊整备与稳态执事。",
            CultivationBranchType.SkillStillness => "在稳根中兼带静修收束，更偏守阵、讲法旁听与长线值守。",
            CultivationBranchType.TechniquePure => "法门仍沿正脉精修，偏重悟法与正面推演。",
            CultivationBranchType.TechniqueExpedition => "法诀锋线更外放，偏向外务、护山与临敌应变。",
            CultivationBranchType.TechniqueCraft => "法门与器理互证，偏向炼器、营造推演与行装筹备。",
            CultivationBranchType.TechniqueLecture => "法诀转作讲法悟玄，偏向经卷推演、讲法校勘与内院研修。",
            CultivationBranchType.CraftSteward => "技艺沿总坊正脉熟成，偏向整备、庶务与执事衔接。",
            CultivationBranchType.CraftField => "把工序带到场务一线，偏向营造、田务器用与轮值补给。",
            CultivationBranchType.CraftArtifact => "器理与手感互证，偏向炼器、阵枢营造与外务行装。",
            CultivationBranchType.CraftStillness => "技艺中兼带守息，偏向丹火细工、总坊账务与久班整备。",
            CultivationBranchType.MeditationPure => "静修仍沿澄心守气一路推进，偏向守成、校勘与心境稳持。",
            CultivationBranchType.MeditationGuard => "静修里牵出守山静值之意，偏向护山值守、轮值巡看与久班耐守。",
            CultivationBranchType.MeditationLecture => "澄息化入讲法悟玄，偏向经卷校勘、讲法复盘与心法推演。",
            CultivationBranchType.MeditationCraft => "以调息养工为主，偏向慢火细务、总坊整备与细工久持。",
            _ => "专修分支尚未成形。"
        };
    }

    private static double GetBranchDirectiveFitWeight(
        CultivationBranchType branchType,
        DiscipleDirectiveType directiveType)
    {
        return directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => branchType switch
            {
                CultivationBranchType.SkillPatrol => 0.80,
                CultivationBranchType.TechniqueExpedition => 1.00,
                CultivationBranchType.TechniqueCraft => 0.45,
                CultivationBranchType.CraftArtifact => 0.30,
                CultivationBranchType.MeditationGuard => 0.40,
                _ => 0
            },
            DiscipleDirectiveType.StewardCandidate => branchType switch
            {
                CultivationBranchType.SkillWorkshop => 0.55,
                CultivationBranchType.SkillStillness => 0.45,
                CultivationBranchType.TechniqueLecture => 0.40,
                CultivationBranchType.CraftField => 0.50,
                CultivationBranchType.CraftArtifact => 0.45,
                CultivationBranchType.CraftStillness => 0.65,
                CultivationBranchType.MeditationGuard => 0.25,
                CultivationBranchType.MeditationLecture => 0.35,
                CultivationBranchType.MeditationCraft => 0.45,
                _ => 0
            },
            _ => 0
        };
    }

    private static double GetBranchOuterMissionLootWeight(CultivationBranchType branchType)
    {
        return branchType switch
        {
            CultivationBranchType.SkillPatrol => 0.35,
            CultivationBranchType.TechniqueExpedition => 0.50,
            CultivationBranchType.TechniqueCraft => 0.45,
            CultivationBranchType.CraftArtifact => 0.80,
            CultivationBranchType.MeditationGuard => 0.15,
            _ => 0
        };
    }

    private static double GetBranchTaskFitWeight(
        CultivationBranchType branchType,
        SectTaskType taskType)
    {
        return taskType switch
        {
            SectTaskType.FieldDuty => branchType switch
            {
                CultivationBranchType.SkillPatrol => 0.25,
                CultivationBranchType.SkillStillness => 0.35,
                CultivationBranchType.CraftField => 0.70,
                CultivationBranchType.MeditationGuard => 0.85,
                _ => 0
            },
            SectTaskType.WorkshopDuty => branchType switch
            {
                CultivationBranchType.SkillWorkshop => 0.70,
                CultivationBranchType.TechniqueCraft => 0.60,
                CultivationBranchType.CraftField => 0.45,
                CultivationBranchType.CraftArtifact => 1.00,
                CultivationBranchType.CraftStillness => 0.70,
                CultivationBranchType.MeditationCraft => 0.55,
                _ => 0
            },
            SectTaskType.LogisticsPatrol => branchType switch
            {
                CultivationBranchType.SkillPatrol => 0.75,
                CultivationBranchType.TechniqueExpedition => 1.00,
                CultivationBranchType.MeditationGuard => 0.55,
                _ => 0
            },
            SectTaskType.ScriptureStudy => branchType switch
            {
                CultivationBranchType.SkillStillness => 0.30,
                CultivationBranchType.TechniqueCraft => 0.30,
                CultivationBranchType.TechniqueLecture => 1.00,
                CultivationBranchType.CraftArtifact => 0.20,
                CultivationBranchType.MeditationLecture => 0.90,
                _ => 0
            },
            SectTaskType.SectCommerce => branchType switch
            {
                CultivationBranchType.SkillWorkshop => 0.35,
                CultivationBranchType.CraftField => 0.25,
                CultivationBranchType.CraftStillness => 0.75,
                CultivationBranchType.MeditationCraft => 0.55,
                _ => 0
            },
            SectTaskType.OuterTrade => branchType switch
            {
                CultivationBranchType.SkillPatrol => 0.45,
                CultivationBranchType.TechniqueExpedition => 0.70,
                CultivationBranchType.TechniqueCraft => 0.55,
                CultivationBranchType.CraftArtifact => 0.65,
                CultivationBranchType.MeditationGuard => 0.20,
                _ => 0
            },
            _ => 0
        };
    }

    private static double GetDirectiveFitWeight(
        DiscipleCultivationAssignmentType assignmentType,
        DiscipleDirectiveType directiveType)
    {
        return directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => assignmentType switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => 1.00,
                DiscipleCultivationAssignmentType.SkillTraining => 0.75,
                DiscipleCultivationAssignmentType.CraftPractice => 0.45,
                DiscipleCultivationAssignmentType.Meditation => 0.35,
                _ => 0
            },
            DiscipleDirectiveType.StewardCandidate => assignmentType switch
            {
                DiscipleCultivationAssignmentType.CraftPractice => 1.00,
                DiscipleCultivationAssignmentType.SkillTraining => 0.75,
                DiscipleCultivationAssignmentType.Meditation => 0.65,
                DiscipleCultivationAssignmentType.TechniquePolish => 0.45,
                _ => 0
            },
            _ => 0
        };
    }

    private static double GetTaskFitWeight(
        DiscipleCultivationAssignmentType assignmentType,
        SectTaskType taskType)
    {
        return taskType switch
        {
            SectTaskType.FieldDuty => assignmentType switch
            {
                DiscipleCultivationAssignmentType.SkillTraining => 1.00,
                DiscipleCultivationAssignmentType.Meditation => 0.75,
                DiscipleCultivationAssignmentType.CraftPractice => 0.35,
                DiscipleCultivationAssignmentType.TechniquePolish => 0.25,
                _ => 0
            },
            SectTaskType.WorkshopDuty => assignmentType switch
            {
                DiscipleCultivationAssignmentType.CraftPractice => 1.00,
                DiscipleCultivationAssignmentType.SkillTraining => 0.55,
                DiscipleCultivationAssignmentType.Meditation => 0.20,
                DiscipleCultivationAssignmentType.TechniquePolish => 0.15,
                _ => 0
            },
            SectTaskType.LogisticsPatrol => assignmentType switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => 1.00,
                DiscipleCultivationAssignmentType.SkillTraining => 0.80,
                DiscipleCultivationAssignmentType.Meditation => 0.35,
                DiscipleCultivationAssignmentType.CraftPractice => 0.15,
                _ => 0
            },
            SectTaskType.ScriptureStudy => assignmentType switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => 1.00,
                DiscipleCultivationAssignmentType.Meditation => 0.70,
                DiscipleCultivationAssignmentType.SkillTraining => 0.30,
                DiscipleCultivationAssignmentType.CraftPractice => 0.20,
                _ => 0
            },
            SectTaskType.SectCommerce => assignmentType switch
            {
                DiscipleCultivationAssignmentType.CraftPractice => 0.80,
                DiscipleCultivationAssignmentType.SkillTraining => 0.60,
                DiscipleCultivationAssignmentType.Meditation => 0.45,
                DiscipleCultivationAssignmentType.TechniquePolish => 0.30,
                _ => 0
            },
            SectTaskType.OuterTrade => assignmentType switch
            {
                DiscipleCultivationAssignmentType.TechniquePolish => 0.75,
                DiscipleCultivationAssignmentType.CraftPractice => 0.65,
                DiscipleCultivationAssignmentType.SkillTraining => 0.50,
                DiscipleCultivationAssignmentType.Meditation => 0.40,
                _ => 0
            },
            _ => 0
        };
    }

    private static DiscipleCultivationAssignmentType NormalizeAssignment(string? rawAssignment)
    {
        return Enum.TryParse<DiscipleCultivationAssignmentType>(rawAssignment, out var parsed)
            ? parsed
            : DiscipleCultivationAssignmentType.None;
    }
}
