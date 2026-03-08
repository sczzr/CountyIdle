using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record SectAffairsRuleDefinition(
    SectAffairsRuleType RuleType,
    string DisplayName,
    string ShortEffect,
    string Description);

public sealed record SectDoctrineRuleDefinition(
    SectDoctrineRuleType RuleType,
    string DisplayName,
    string ShortEffect,
    string Description);

public sealed record SectDisciplineRuleDefinition(
    SectDisciplineRuleType RuleType,
    string DisplayName,
    string ShortEffect,
    string Description);

public static class SectRuleTreeRules
{
    private const SectAffairsRuleType DefaultAffairsRule = SectAffairsRuleType.Balanced;
    private const SectDoctrineRuleType DefaultDoctrineRule = SectDoctrineRuleType.Balanced;
    private const SectDisciplineRuleType DefaultDisciplineRule = SectDisciplineRuleType.Balanced;

    private static readonly IReadOnlyList<SectAffairsRuleDefinition> AffairsRules =
    [
        new(SectAffairsRuleType.Balanced, "庶务常制", "庶务按常规执行", "不额外偏置庶务门规，按宗门常制运转。"),
        new(SectAffairsRuleType.NewDiscipleCare, "抚恤新徒", "供养与新徒更稳", "优先照看新入门弟子、恢复链路与基础供养。"),
        new(SectAffairsRuleType.MeritLedger, "尚功明录", "功过记载更清", "细化功过记载与赏罚明录，强化贡献激励。"),
        new(SectAffairsRuleType.HundredWorksAudit, "百工验收", "工坊验收更严", "强化工坊验收与交付标准，提升器械与营造的可用性。")
    ];

    private static readonly IReadOnlyList<SectDoctrineRuleDefinition> DoctrineRules =
    [
        new(SectDoctrineRuleType.Balanced, "传功常制", "传功按常规推进", "不额外偏置传功门规，按常态讲授与修习安排。"),
        new(SectDoctrineRuleType.QuietCultivation, "静室修行", "闭关修习更稳", "鼓励静室修行与专注研读，稳步提升悟道效率。"),
        new(SectDoctrineRuleType.MonthlyLectureExam, "月考讲评", "课业节奏更紧", "通过月考、讲评和答疑强化学习节奏与讲法反馈。"),
        new(SectDoctrineRuleType.OpenArchiveStudy, "开阁阅卷", "典籍开放更广", "扩大典籍、图录与课卷开放范围，促进知识流动。")
    ];

    private static readonly IReadOnlyList<SectDisciplineRuleDefinition> DisciplineRules =
    [
        new(SectDisciplineRuleType.Balanced, "巡山常制", "巡山按常规执行", "不额外偏置巡山门规，维持平时护山与风纪尺度。"),
        new(SectDisciplineRuleType.NightPatrolTokens, "夜巡验符", "夜巡更严密", "加强夜巡与出入验符，压低山门潜在风险。"),
        new(SectDisciplineRuleType.LawHallInspection, "执法先行", "执法响应更快", "强化执法总堂前置介入，提升风险压制与秩序响应。"),
        new(SectDisciplineRuleType.PeakGateMutualWatch, "峰门互保", "峰门联防更强", "通过峰门互保与值守轮换，稳住秩序与同门信任。")
    ];

    private static readonly IReadOnlyDictionary<SectAffairsRuleType, SectAffairsRuleDefinition> AffairsLookup =
        AffairsRules.ToDictionary(static item => item.RuleType);

    private static readonly IReadOnlyDictionary<SectDoctrineRuleType, SectDoctrineRuleDefinition> DoctrineLookup =
        DoctrineRules.ToDictionary(static item => item.RuleType);

    private static readonly IReadOnlyDictionary<SectDisciplineRuleType, SectDisciplineRuleDefinition> DisciplineLookup =
        DisciplineRules.ToDictionary(static item => item.RuleType);

    public static void EnsureDefaults(GameState state)
    {
        state.ActiveAffairsRule = NormalizeEnumValue(state.ActiveAffairsRule, DefaultAffairsRule);
        state.ActiveDoctrineRule = NormalizeEnumValue(state.ActiveDoctrineRule, DefaultDoctrineRule);
        state.ActiveDisciplineRule = NormalizeEnumValue(state.ActiveDisciplineRule, DefaultDisciplineRule);
    }

    public static SectAffairsRuleDefinition GetActiveAffairsDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectAffairsRuleType>(state.ActiveAffairsRule, out var ruleType);
        return AffairsLookup[ruleType];
    }

    public static SectDoctrineRuleDefinition GetActiveDoctrineDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectDoctrineRuleType>(state.ActiveDoctrineRule, out var ruleType);
        return DoctrineLookup[ruleType];
    }

    public static SectDisciplineRuleDefinition GetActiveDisciplineDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectDisciplineRuleType>(state.ActiveDisciplineRule, out var ruleType);
        return DisciplineLookup[ruleType];
    }

    public static SectAffairsRuleDefinition CycleAffairsRule(GameState state, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(AffairsRules.Select(static item => item.RuleType).ToArray(), GetActiveAffairsType(state), delta);
        state.ActiveAffairsRule = next.ToString();
        return AffairsLookup[next];
    }

    public static SectDoctrineRuleDefinition CycleDoctrineRule(GameState state, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(DoctrineRules.Select(static item => item.RuleType).ToArray(), GetActiveDoctrineType(state), delta);
        state.ActiveDoctrineRule = next.ToString();
        return DoctrineLookup[next];
    }

    public static SectDisciplineRuleDefinition CycleDisciplineRule(GameState state, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(DisciplineRules.Select(static item => item.RuleType).ToArray(), GetActiveDisciplineType(state), delta);
        state.ActiveDisciplineRule = next.ToString();
        return DisciplineLookup[next];
    }

    public static void ResetToDefaults(GameState state)
    {
        state.ActiveAffairsRule = DefaultAffairsRule.ToString();
        state.ActiveDoctrineRule = DefaultDoctrineRule.ToString();
        state.ActiveDisciplineRule = DefaultDisciplineRule.ToString();
    }

    public static double GetFoodYieldModifier(GameState state)
    {
        return GetActiveAffairsType(state) switch
        {
            SectAffairsRuleType.NewDiscipleCare => 1.05,
            _ => 1.0
        };
    }

    public static double GetContributionYieldModifier(GameState state)
    {
        var modifier = 1.0;
        switch (GetActiveAffairsType(state))
        {
            case SectAffairsRuleType.MeritLedger:
                modifier *= 1.10;
                break;
            case SectAffairsRuleType.HundredWorksAudit:
                modifier *= 1.04;
                break;
        }

        switch (GetActiveDoctrineType(state))
        {
            case SectDoctrineRuleType.OpenArchiveStudy:
                modifier *= 1.03;
                break;
        }

        switch (GetActiveDisciplineType(state))
        {
            case SectDisciplineRuleType.LawHallInspection:
                modifier *= 1.04;
                break;
        }

        return modifier;
    }

    public static double GetResearchYieldModifier(GameState state)
    {
        return GetActiveDoctrineType(state) switch
        {
            SectDoctrineRuleType.QuietCultivation => 1.10,
            SectDoctrineRuleType.MonthlyLectureExam => 1.06,
            SectDoctrineRuleType.OpenArchiveStudy => 1.08,
            _ => 1.0
        };
    }

    public static double GetPopulationGrowthModifier(GameState state)
    {
        return GetActiveAffairsType(state) switch
        {
            SectAffairsRuleType.NewDiscipleCare => 1.06,
            _ => 1.0
        };
    }

    public static double GetHourlyHappinessDelta(GameState state)
    {
        var delta = 0.0;
        if (GetActiveAffairsType(state) == SectAffairsRuleType.NewDiscipleCare)
        {
            delta += 0.06;
        }

        if (GetActiveDoctrineType(state) == SectDoctrineRuleType.MonthlyLectureExam)
        {
            delta += 0.04;
        }

        if (GetActiveDisciplineType(state) == SectDisciplineRuleType.PeakGateMutualWatch)
        {
            delta += 0.03;
        }

        return delta;
    }

    public static double GetHourlyThreatDelta(GameState state)
    {
        return GetActiveDisciplineType(state) switch
        {
            SectDisciplineRuleType.NightPatrolTokens => -0.35,
            SectDisciplineRuleType.LawHallInspection => -0.55,
            SectDisciplineRuleType.PeakGateMutualWatch => -0.20,
            _ => 0.0
        };
    }

    public static double GetToolCraftModifier(GameState state)
    {
        return GetActiveAffairsType(state) switch
        {
            SectAffairsRuleType.HundredWorksAudit => 1.10,
            _ => 1.0
        };
    }

    public static string BuildActiveRuleSummary(GameState state)
    {
        var affairs = GetActiveAffairsDefinition(state);
        var doctrine = GetActiveDoctrineDefinition(state);
        var discipline = GetActiveDisciplineDefinition(state);
        return $"庶务门规：{affairs.DisplayName}｜传功门规：{doctrine.DisplayName}｜巡山门规：{discipline.DisplayName}";
    }

    private static SectAffairsRuleType GetActiveAffairsType(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectAffairsRuleType>(state.ActiveAffairsRule, out var ruleType);
        return ruleType;
    }

    private static SectDoctrineRuleType GetActiveDoctrineType(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectDoctrineRuleType>(state.ActiveDoctrineRule, out var ruleType);
        return ruleType;
    }

    private static SectDisciplineRuleType GetActiveDisciplineType(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectDisciplineRuleType>(state.ActiveDisciplineRule, out var ruleType);
        return ruleType;
    }

    private static string NormalizeEnumValue<TEnum>(string rawValue, TEnum fallback)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(rawValue, out var parsed) ? parsed.ToString() : fallback.ToString();
    }

    private static TEnum CycleValue<TEnum>(IReadOnlyList<TEnum> values, TEnum current, int delta)
        where TEnum : struct, Enum
    {
        if (values.Count == 0 || delta == 0)
        {
            return current;
        }

        var index = 0;
        for (var i = 0; i < values.Count; i++)
        {
            if (!EqualityComparer<TEnum>.Default.Equals(values[i], current))
            {
                continue;
            }

            index = i;
            break;
        }

        var normalizedDelta = delta > 0 ? 1 : -1;
        index = (index + normalizedDelta + values.Count) % values.Count;
        return values[index];
    }
}
