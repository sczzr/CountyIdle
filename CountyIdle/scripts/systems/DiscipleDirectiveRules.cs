using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 执事任务指派信息
public sealed record StewardTaskAppointment(
    int DiscipleId,
    string DiscipleName,
    SectTaskType TaskType,
    double ExecutionModifier,
    int Execution,
    int Insight,
    int Contribution);

// 执事候补与指派快照
public sealed class StewardAppointmentSnapshot
{
    public StewardAppointmentSnapshot(
        IReadOnlyDictionary<SectTaskType, StewardTaskAppointment> appointmentsByTask,
        IReadOnlyDictionary<int, StewardTaskAppointment> appointmentsByDiscipleId,
        int totalCandidates,
        int totalAssigned,
        double averageExecutionModifier)
    {
        AppointmentsByTask = appointmentsByTask;
        AppointmentsByDiscipleId = appointmentsByDiscipleId;
        TotalCandidates = totalCandidates;
        TotalAssigned = totalAssigned;
        AverageExecutionModifier = averageExecutionModifier;
    }

    // 按任务索引的指派
    public IReadOnlyDictionary<SectTaskType, StewardTaskAppointment> AppointmentsByTask { get; }

    // 按弟子 ID 索引的指派
    public IReadOnlyDictionary<int, StewardTaskAppointment> AppointmentsByDiscipleId { get; }

    // 候补总数
    public int TotalCandidates { get; }

    // 已指派总数
    public int TotalAssigned { get; }

    // 平均执行修正
    public double AverageExecutionModifier { get; }
}

// 弟子指令规则（外务候补/执事培养）
public static class DiscipleDirectiveRules
{
    // 确保指令字典有效且规范化
    public static void EnsureDefaults(GameState state)
    {
        state.DiscipleDirectives ??= new Dictionary<int, string>();

        var normalized = new Dictionary<int, string>();
        foreach (var (discipleId, rawDirective) in state.DiscipleDirectives)
        {
            if (discipleId <= 0 || discipleId > Math.Max(state.Population, 0))
            {
                continue;
            }

            var directiveType = NormalizeDirective(rawDirective);
            if (directiveType == DiscipleDirectiveType.None)
            {
                continue;
            }

            normalized[discipleId] = directiveType.ToString();
        }

        state.DiscipleDirectives = normalized;
    }

    // 获取指定弟子的指令类型
    public static DiscipleDirectiveType GetDirective(GameState state, int discipleId)
    {
        EnsureDefaults(state);
        if (discipleId <= 0 || !state.DiscipleDirectives.TryGetValue(discipleId, out var rawDirective))
        {
            return DiscipleDirectiveType.None;
        }

        return NormalizeDirective(rawDirective);
    }

    // 设置指定弟子的指令类型
    public static void SetDirective(GameState state, int discipleId, DiscipleDirectiveType directiveType)
    {
        EnsureDefaults(state);
        if (discipleId <= 0)
        {
            return;
        }

        if (directiveType == DiscipleDirectiveType.None)
        {
            state.DiscipleDirectives.Remove(discipleId);
            return;
        }

        state.DiscipleDirectives[discipleId] = directiveType.ToString();
    }

    // 统计某类指令的弟子数量
    public static int GetDirectiveCount(GameState state, DiscipleDirectiveType directiveType)
    {
        if (directiveType == DiscipleDirectiveType.None)
        {
            return 0;
        }

        EnsureDefaults(state);
        return state.DiscipleDirectives.Count(item => NormalizeDirective(item.Value) == directiveType);
    }

    // 构建指令概览文本
    public static string BuildDirectiveSummary(GameState state)
    {
        EnsureDefaults(state);
        var outerCount = GetDirectiveCount(state, DiscipleDirectiveType.OuterMissionCandidate);
        var stewardCount = GetDirectiveCount(state, DiscipleDirectiveType.StewardCandidate);
        return $"外务候补 {outerCount} · 执事培养 {stewardCount}";
    }

    // 获取指令显示名
    public static string GetDirectiveDisplayName(DiscipleDirectiveType directiveType)
    {
        return directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => "外务候补",
            DiscipleDirectiveType.StewardCandidate => "执事培养",
            _ => "常制观察"
        };
    }

    // 获取指令短描述
    public static string GetDirectiveShortEffect(DiscipleDirectiveType directiveType)
    {
        return directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => "历练队伍会优先吸纳其战力与执行表现。",
            DiscipleDirectiveType.StewardCandidate => "内务执行效率与贡献回流会额外参考其执行、悟性与贡献。",
            _ => "仅保留卷册跟踪，不额外改变小时结算倾向。"
        };
    }

    // 获取指令效果概述
    public static string BuildDirectiveEffectSummary(GameState state, DiscipleDirectiveType directiveType)
    {
        return directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => BuildOuterMissionSummary(state),
            DiscipleDirectiveType.StewardCandidate => BuildStewardSummary(state),
            _ => "当前未纳入重点名册，仅保留卷册观察与后续跟踪。"
        };
    }

    // 外务候补带来的队伍战力加成
    public static double GetOuterMissionTeamPowerBonus(GameState state)
    {
        var candidates = GetCandidateProfiles(state, DiscipleDirectiveType.OuterMissionCandidate, 3);
        if (candidates.Count == 0)
        {
            return 0.0;
        }

        var averageCombat = candidates.Average(static profile => profile.Combat);
        var averageExecution = candidates.Average(static profile => profile.Execution);
        var bonus =
            (candidates.Count * 0.55) +
            (Math.Max(averageCombat - 50.0, 0.0) * 0.05) +
            (Math.Max(averageExecution - 50.0, 0.0) * 0.02);

        return Math.Min(bonus, 6.0);
    }

    // 外务候补带来的收益修正
    public static double GetOuterMissionLootModifier(GameState state)
    {
        var candidates = GetCandidateProfiles(state, DiscipleDirectiveType.OuterMissionCandidate, 3);
        if (candidates.Count == 0)
        {
            return 1.0;
        }

        var averageContribution = candidates.Average(static profile => profile.Contribution);
        var bonus =
            (candidates.Count * 0.015) +
            (Math.Max(averageContribution - 40.0, 0.0) * 0.0006);

        return 1.0 + Math.Min(bonus, 0.12);
    }

    // 执事候补带来的贡献修正
    public static double GetStewardContributionModifier(GameState state)
    {
        var candidates = GetCandidateProfiles(state, DiscipleDirectiveType.StewardCandidate, 4);
        if (candidates.Count == 0)
        {
            return 1.0;
        }

        var averageExecution = candidates.Average(static profile => profile.Execution);
        var averageContribution = candidates.Average(static profile => profile.Contribution);
        var bonus =
            (candidates.Count * 0.015) +
            (Math.Max(averageExecution - 55.0, 0.0) * 0.0010) +
            (Math.Max(averageContribution - 45.0, 0.0) * 0.0005);

        return 1.0 + Math.Min(bonus, 0.12);
    }

    // 执事候补带来的执行修正
    public static double GetStewardExecutionModifier(GameState state)
    {
        var appointmentSnapshot = BuildStewardAppointmentSnapshot(state);
        if (appointmentSnapshot.TotalAssigned <= 0)
        {
            return 1.0;
        }

        return appointmentSnapshot.AverageExecutionModifier;
    }

    // 构建执事指派快照
    public static StewardAppointmentSnapshot BuildStewardAppointmentSnapshot(GameState state)
    {
        var candidates = GetCandidateProfiles(state, DiscipleDirectiveType.StewardCandidate, 6);
        if (candidates.Count == 0)
        {
            return new StewardAppointmentSnapshot(
                new Dictionary<SectTaskType, StewardTaskAppointment>(),
                new Dictionary<int, StewardTaskAppointment>(),
                0,
                0,
                1.0);
        }

        var internalTasks = SectTaskRules.GetOrderedDefinitions()
            .Where(definition =>
                definition.IsInternalTask &&
                SectTaskRules.GetOrderUnits(state, definition.TaskType) > 0)
            .OrderByDescending(definition => SectTaskRules.GetOrderUnits(state, definition.TaskType))
            .ThenBy(definition => definition.PriorityOrder)
            .ToArray();

        if (internalTasks.Length == 0)
        {
            return new StewardAppointmentSnapshot(
                new Dictionary<SectTaskType, StewardTaskAppointment>(),
                new Dictionary<int, StewardTaskAppointment>(),
                candidates.Count,
                0,
                1.0);
        }

        var remainingCandidates = new List<DiscipleProfile>(candidates);
        var appointmentsByTask = new Dictionary<SectTaskType, StewardTaskAppointment>();
        var appointmentsByDiscipleId = new Dictionary<int, StewardTaskAppointment>();

        foreach (var taskDefinition in internalTasks)
        {
            if (remainingCandidates.Count <= 0)
            {
                break;
            }

            var chosenCandidate = remainingCandidates
                .OrderByDescending(candidate => ScoreStewardCandidateForTask(taskDefinition, candidate))
                .ThenByDescending(candidate => candidate.Execution)
                .ThenByDescending(candidate => candidate.Insight)
                .ThenByDescending(candidate => candidate.Contribution)
                .ThenBy(candidate => candidate.Id)
                .First();

            remainingCandidates.Remove(chosenCandidate);
            var executionModifier = ComputeStewardTaskExecutionModifier(taskDefinition, chosenCandidate);
            var appointment = new StewardTaskAppointment(
                chosenCandidate.Id,
                chosenCandidate.Name,
                taskDefinition.TaskType,
                executionModifier,
                chosenCandidate.Execution,
                chosenCandidate.Insight,
                chosenCandidate.Contribution);

            appointmentsByTask[taskDefinition.TaskType] = appointment;
            appointmentsByDiscipleId[chosenCandidate.Id] = appointment;
        }

        var averageExecutionModifier = appointmentsByTask.Count > 0
            ? appointmentsByTask.Values.Average(static appointment => appointment.ExecutionModifier)
            : 1.0;

        return new StewardAppointmentSnapshot(
            appointmentsByTask,
            appointmentsByDiscipleId,
            candidates.Count,
            appointmentsByTask.Count,
            averageExecutionModifier);
    }

    // 获取指定任务的执行修正（按当前快照）
    public static double GetStewardTaskExecutionModifier(GameState state, SectTaskType taskType)
    {
        return GetStewardTaskExecutionModifier(BuildStewardAppointmentSnapshot(state), taskType);
    }

    // 从快照中获取指定任务的执行修正
    public static double GetStewardTaskExecutionModifier(StewardAppointmentSnapshot snapshot, SectTaskType taskType)
    {
        return snapshot.AppointmentsByTask.TryGetValue(taskType, out var appointment)
            ? appointment.ExecutionModifier
            : 1.0;
    }

    // 获取指定任务的执事指派
    public static bool TryGetStewardAppointment(GameState state, SectTaskType taskType, out StewardTaskAppointment? appointment)
    {
        var snapshot = BuildStewardAppointmentSnapshot(state);
        if (snapshot.AppointmentsByTask.TryGetValue(taskType, out var foundAppointment))
        {
            appointment = foundAppointment;
            return true;
        }

        appointment = null;
        return false;
    }

    // 获取指定弟子的执事指派
    public static bool TryGetDiscipleStewardAppointment(GameState state, int discipleId, out StewardTaskAppointment? appointment)
    {
        var snapshot = BuildStewardAppointmentSnapshot(state);
        if (snapshot.AppointmentsByDiscipleId.TryGetValue(discipleId, out var foundAppointment))
        {
            appointment = foundAppointment;
            return true;
        }

        appointment = null;
        return false;
    }

    // 构建弟子指令效果摘要
    public static string BuildDiscipleDirectiveEffectSummary(GameState state, DiscipleProfile profile)
    {
        return profile.DirectiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => BuildOuterMissionDiscipleSummary(state, profile),
            DiscipleDirectiveType.StewardCandidate => BuildStewardDiscipleSummary(state, profile),
            _ => BuildDirectiveEffectSummary(state, profile.DirectiveType)
        };
    }

    // 外务候补整体效果说明
    private static string BuildOuterMissionSummary(GameState state)
    {
        var count = GetDirectiveCount(state, DiscipleDirectiveType.OuterMissionCandidate);
        if (count <= 0)
        {
            return "当前宗门尚未点定外务候补，历练仍按默认精英队列出行。";
        }

        var teamPowerBonus = GetOuterMissionTeamPowerBonus(state);
        var lootModifier = GetOuterMissionLootModifier(state);
        return $"当前外务候补 {count} 人；历练队伍战力额外 +{teamPowerBonus:0.0}，外务回流额外 +{(lootModifier - 1.0) * 100.0:0.#}%。";
    }

    // 执事候补整体效果说明
    private static string BuildStewardSummary(GameState state)
    {
        var count = GetDirectiveCount(state, DiscipleDirectiveType.StewardCandidate);
        if (count <= 0)
        {
            return "当前宗门尚未点定执事培养对象，内务仍按常制折算执行。";
        }

        var contributionModifier = GetStewardContributionModifier(state);
        var appointmentSnapshot = BuildStewardAppointmentSnapshot(state);
        if (appointmentSnapshot.TotalAssigned <= 0)
        {
            return $"当前执事培养 {count} 人；暂未轮到具体庶务补位，贡献回流额外 +{(contributionModifier - 1.0) * 100.0:0.#}%。";
        }

        var executionModifier = appointmentSnapshot.AverageExecutionModifier;
        return $"当前执事培养 {count} 人；其中 {appointmentSnapshot.TotalAssigned} 人正补位内务，执行效率额外 +{(executionModifier - 1.0) * 100.0:0.#}%，贡献回流额外 +{(contributionModifier - 1.0) * 100.0:0.#}%。";
    }

    // 外务候补弟子效果说明
    private static string BuildOuterMissionDiscipleSummary(GameState state, DiscipleProfile profile)
    {
        var activeCandidates = GetCandidateProfiles(state, DiscipleDirectiveType.OuterMissionCandidate, 3);
        var isActive = activeCandidates.Any(candidate => candidate.Id == profile.Id);
        if (isActive)
        {
            return "当前列入外务先遣名册前列，历练队伍会优先吸纳其战力与执行表现。";
        }

        return "已入外务候补名册，但当前仍在后备序列，待前列人手轮换时补入。";
    }

    // 执事候补弟子效果说明
    private static string BuildStewardDiscipleSummary(GameState state, DiscipleProfile profile)
    {
        if (TryGetDiscipleStewardAppointment(state, profile.Id, out var appointment) && appointment != null)
        {
            var taskDefinition = SectTaskRules.GetDefinition(appointment.TaskType);
            return $"当前正代行“{taskDefinition.DisplayName}”执事补位，本条内务执行效率 +{(appointment.ExecutionModifier - 1.0) * 100.0:0.#}%。";
        }

        return "已入执事培养名册，但当前尚未轮到具体庶务补位。";
    }

    // 计算候补弟子匹配任务的评分
    private static double ScoreStewardCandidateForTask(SectTaskDefinition taskDefinition, DiscipleProfile profile)
    {
        var score =
            (profile.Execution * 2.2) +
            (profile.Insight * 1.1) +
            (profile.Contribution * 0.8) +
            (profile.IsElite ? 8.0 : 0.0);

        if (profile.JobType == taskDefinition.JobType)
        {
            score += 24.0;
        }

        score += taskDefinition.TaskType switch
        {
            SectTaskType.WorkshopDuty => profile.Craft * 0.6,
            SectTaskType.LogisticsPatrol => profile.Combat * 0.5,
            SectTaskType.ScriptureStudy => profile.Insight * 0.7,
            SectTaskType.SectCommerce => profile.Contribution * 0.4,
            SectTaskType.FieldDuty => profile.Health * 0.3,
            _ => 0.0
        };

        return score;
    }

    // 计算执事任务执行修正值
    private static double ComputeStewardTaskExecutionModifier(SectTaskDefinition taskDefinition, DiscipleProfile profile)
    {
        var bonus =
            0.015 +
            Math.Max(profile.Execution - 60, 0) * 0.0008 +
            Math.Max(profile.Insight - 55, 0) * 0.0004 +
            Math.Max(profile.Contribution - 50, 0) * 0.0003;

        if (profile.JobType == taskDefinition.JobType)
        {
            bonus += 0.012;
        }

        if (profile.IsElite)
        {
            bonus += 0.008;
        }

        return 1.0 + Math.Min(bonus, 0.10);
    }

    // 获取符合条件的候补弟子列表
    private static List<DiscipleProfile> GetCandidateProfiles(GameState state, DiscipleDirectiveType directiveType, int maxCount)
    {
        if (directiveType == DiscipleDirectiveType.None || maxCount <= 0)
        {
            return new List<DiscipleProfile>();
        }

        var roster = DiscipleRosterSystem.BuildRoster(state);
        return roster
            .Where(profile =>
                profile.DirectiveType == directiveType &&
                profile.AgeBand != DiscipleAgeBand.Seedling)
            .OrderByDescending(profile => profile.Combat + profile.Execution + profile.Contribution)
            .ThenByDescending(profile => profile.Potential)
            .Take(maxCount)
            .ToList();
    }

    // 解析并规范化指令类型
    private static DiscipleDirectiveType NormalizeDirective(string? rawDirective)
    {
        return Enum.TryParse<DiscipleDirectiveType>(rawDirective, out var parsed)
            ? parsed
            : DiscipleDirectiveType.None;
    }
}
