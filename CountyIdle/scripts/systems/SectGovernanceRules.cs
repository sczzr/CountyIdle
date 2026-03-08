using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record SectDevelopmentDirectionDefinition(
    SectDevelopmentDirectionType DirectionType,
    string DisplayName,
    string ShortEffect,
    string Description);

public sealed record SectLawDefinition(
    SectLawType LawType,
    string DisplayName,
    string ShortEffect,
    string Description);

public sealed record SectTalentPlanDefinition(
    SectTalentPlanType PlanType,
    string DisplayName,
    string ShortEffect,
    string Description);

public sealed record SectQuarterDecreeDefinition(
    SectQuarterDecreeType DecreeType,
    string DisplayName,
    string ShortEffect,
    string Description);

public static class SectGovernanceRules
{
    private const SectDevelopmentDirectionType DefaultDirection = SectDevelopmentDirectionType.Balanced;
    private const SectLawType DefaultLaw = SectLawType.Benevolent;
    private const SectTalentPlanType DefaultTalentPlan = SectTalentPlanType.RecruitDisciples;
    private const SectQuarterDecreeType DefaultQuarterDecree = SectQuarterDecreeType.None;

    private static readonly IReadOnlyList<SectDevelopmentDirectionDefinition> DevelopmentDirections =
    [
        new(SectDevelopmentDirectionType.Balanced, "均衡发展", "各堂口稳步并进", "内务、传承、护山与外务保持均衡，适合作为常态治宗方略。"),
        new(SectDevelopmentDirectionType.SupplyFirst, "供养优先", "供养增益更高", "优先保障阵材、营造与基础供给，为宗门扩建和日常运转夯实底盘。"),
        new(SectDevelopmentDirectionType.DoctrineFirst, "研修优先", "传承研修增益更高", "优先扶持传法院与阵堂推演，加快技艺突破和传承积累。"),
        new(SectDevelopmentDirectionType.DefenseFirst, "护山优先", "戒备与贡献更高", "优先巡山、营造与护山秩序，降低外部威胁并提升内务响应。"),
        new(SectDevelopmentDirectionType.OutreachFirst, "外务优先", "灵石回流更高", "优先总坊与外事链路，增强灵石回流与附庸往来。")
    ];

    private static readonly IReadOnlyList<SectLawDefinition> Laws =
    [
        new(SectLawType.Benevolent, "宽和养民", "门人安稳度提升", "减轻门人压力，提升民心与生育意愿，适合恢复期与扩张期。"),
        new(SectLawType.Discipline, "严整戒律", "威胁压制更强", "强调巡山、戒律与秩序执行，可快速压低威胁，但会带来些许压迫感。"),
        new(SectLawType.Merit, "尚功奖绩", "贡献回流更高", "以功绩和赏罚驱动执行，提升贡献点回流，适合扩建与内务强化。"),
        new(SectLawType.OpenLectures, "开坛传习", "研修积累更快", "鼓励讲法、研修与开坛交流，提升研修效率并略微改善门人情绪。")
    ];

    private static readonly IReadOnlyList<SectTalentPlanDefinition> TalentPlans =
    [
        new(SectTalentPlanType.RecruitDisciples, "广纳新徒", "门人增长更快", "优先扩充弟子来源与后备门人，适合长线扩张。"),
        new(SectTalentPlanType.ArrayScholarship, "阵师深造", "研修与悟道更快", "集中资源培养阵师、推演弟子与研修骨干。"),
        new(SectTalentPlanType.StewardTraining, "执事磨砺", "内务执行更稳", "强化执事层与庶务骨干，使宗门内务落实更顺畅。"),
        new(SectTalentPlanType.OuterMissions, "外务历练", "外务回流更高", "让门人更多参与外事试炼与商路往返，提升对外回流。")
    ];

    private static readonly IReadOnlyList<SectQuarterDecreeDefinition> QuarterDecrees =
    [
        new(SectQuarterDecreeType.None, "本季无加令", "待宗主颁令", "尚未颁布本季度专项法令，诸堂按常态章程运转。"),
        new(SectQuarterDecreeType.OpenGranaries, "开库赈济", "供养与民心更稳", "优先开库、赈济与后勤调拨，确保门人吃穿与恢复链路稳定。"),
        new(SectQuarterDecreeType.GrandLecture, "开坛季讲", "传承讲法更盛", "本季重点开坛讲法、公开讲授与试炼答疑，强化阵道传承氛围。"),
        new(SectQuarterDecreeType.FortifyMountain, "护山检阅", "巡山与戒备更严", "本季集中整肃巡山、检修阵眼与战备秩序，优先压低山门威胁。"),
        new(SectQuarterDecreeType.MarketDrive, "坊市开榷", "商路与账务更活", "本季鼓励总坊、外事与附庸流通，推动坊市活跃与灵石回笼。"),
        new(SectQuarterDecreeType.HundredWorks, "百工会炼", "工坊与锻器更强", "本季集中资源支持铸机阁与阵基殿，优先保障营造、锻器与器械维护。")
    ];

    private static readonly IReadOnlyDictionary<SectDevelopmentDirectionType, SectDevelopmentDirectionDefinition> DevelopmentLookup =
        DevelopmentDirections.ToDictionary(static item => item.DirectionType);

    private static readonly IReadOnlyDictionary<SectLawType, SectLawDefinition> LawLookup =
        Laws.ToDictionary(static item => item.LawType);

    private static readonly IReadOnlyDictionary<SectTalentPlanType, SectTalentPlanDefinition> TalentLookup =
        TalentPlans.ToDictionary(static item => item.PlanType);

    private static readonly IReadOnlyDictionary<SectQuarterDecreeType, SectQuarterDecreeDefinition> QuarterDecreeLookup =
        QuarterDecrees.ToDictionary(static item => item.DecreeType);

    public static void EnsureDefaults(GameState state)
    {
        state.ActiveDevelopmentDirection = NormalizeEnumValue(state.ActiveDevelopmentDirection, DefaultDirection);
        state.ActiveSectLaw = NormalizeEnumValue(state.ActiveSectLaw, DefaultLaw);
        state.ActiveTalentPlan = NormalizeEnumValue(state.ActiveTalentPlan, DefaultTalentPlan);
        state.ActiveQuarterDecree = NormalizeEnumValue(state.ActiveQuarterDecree, DefaultQuarterDecree);
    }

    public static SectDevelopmentDirectionDefinition GetActiveDevelopmentDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectDevelopmentDirectionType>(state.ActiveDevelopmentDirection, out var directionType);
        return DevelopmentLookup[directionType];
    }

    public static SectLawDefinition GetActiveLawDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectLawType>(state.ActiveSectLaw, out var lawType);
        return LawLookup[lawType];
    }

    public static SectTalentPlanDefinition GetActiveTalentPlanDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectTalentPlanType>(state.ActiveTalentPlan, out var planType);
        return TalentLookup[planType];
    }

    public static SectDevelopmentDirectionType GetActiveDevelopmentDirection(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectDevelopmentDirectionType>(state.ActiveDevelopmentDirection, out var directionType);
        return directionType;
    }

    public static SectLawType GetActiveLaw(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectLawType>(state.ActiveSectLaw, out var lawType);
        return lawType;
    }

    public static SectTalentPlanType GetActiveTalentPlan(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectTalentPlanType>(state.ActiveTalentPlan, out var planType);
        return planType;
    }

    public static SectQuarterDecreeDefinition GetActiveQuarterDecreeDefinition(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectQuarterDecreeType>(state.ActiveQuarterDecree, out var decreeType);
        return QuarterDecreeLookup[decreeType];
    }

    public static SectQuarterDecreeType GetActiveQuarterDecree(GameState state)
    {
        EnsureDefaults(state);
        Enum.TryParse<SectQuarterDecreeType>(state.ActiveQuarterDecree, out var decreeType);
        return decreeType;
    }

    public static SectDevelopmentDirectionDefinition CycleDevelopmentDirection(GameState state, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(DevelopmentDirections.Select(static item => item.DirectionType).ToArray(), GetActiveDevelopmentDirection(state), delta);
        state.ActiveDevelopmentDirection = next.ToString();
        return DevelopmentLookup[next];
    }

    public static SectLawDefinition CycleLaw(GameState state, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(Laws.Select(static item => item.LawType).ToArray(), GetActiveLaw(state), delta);
        state.ActiveSectLaw = next.ToString();
        return LawLookup[next];
    }

    public static SectTalentPlanDefinition CycleTalentPlan(GameState state, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(TalentPlans.Select(static item => item.PlanType).ToArray(), GetActiveTalentPlan(state), delta);
        state.ActiveTalentPlan = next.ToString();
        return TalentLookup[next];
    }

    public static SectQuarterDecreeDefinition CycleQuarterDecree(GameState state, int currentQuarterIndex, int delta)
    {
        EnsureDefaults(state);
        var next = CycleValue(QuarterDecrees.Select(static item => item.DecreeType).ToArray(), GetActiveQuarterDecree(state), delta);
        state.ActiveQuarterDecree = next.ToString();
        state.QuarterDecreeIssuedQuarterIndex = currentQuarterIndex;
        return QuarterDecreeLookup[next];
    }

    public static void ResetToDefaults(GameState state)
    {
        state.ActiveDevelopmentDirection = DefaultDirection.ToString();
        state.ActiveSectLaw = DefaultLaw.ToString();
        state.ActiveTalentPlan = DefaultTalentPlan.ToString();
        state.ActiveQuarterDecree = DefaultQuarterDecree.ToString();
        state.QuarterDecreeIssuedQuarterIndex = -1;
    }

    public static double GetFoodYieldModifier(GameState state)
    {
        var modifier = 1.0;
        if (GetActiveDevelopmentDirection(state) == SectDevelopmentDirectionType.SupplyFirst)
        {
            modifier += 0.08;
        }

        return modifier;
    }

    public static double GetGoldYieldModifier(GameState state)
    {
        var modifier = 1.0;
        if (GetActiveDevelopmentDirection(state) == SectDevelopmentDirectionType.OutreachFirst)
        {
            modifier += 0.12;
        }

        if (GetActiveTalentPlan(state) == SectTalentPlanType.OuterMissions)
        {
            modifier += 0.10;
        }

        return modifier;
    }

    public static double GetContributionYieldModifier(GameState state)
    {
        var modifier = 1.0;
        if (GetActiveDevelopmentDirection(state) == SectDevelopmentDirectionType.DefenseFirst)
        {
            modifier += 0.08;
        }

        if (GetActiveLaw(state) == SectLawType.Merit)
        {
            modifier += 0.15;
        }

        if (GetActiveTalentPlan(state) == SectTalentPlanType.StewardTraining)
        {
            modifier += 0.10;
        }

        return modifier;
    }

    public static double GetResearchYieldModifier(GameState state)
    {
        var modifier = 1.0;
        if (GetActiveDevelopmentDirection(state) == SectDevelopmentDirectionType.DoctrineFirst)
        {
            modifier += 0.12;
        }

        if (GetActiveLaw(state) == SectLawType.OpenLectures)
        {
            modifier += 0.10;
        }

        if (GetActiveTalentPlan(state) == SectTalentPlanType.ArrayScholarship)
        {
            modifier += 0.12;
        }

        return modifier;
    }

    public static double GetPopulationGrowthModifier(GameState state)
    {
        var modifier = 1.0;
        if (GetActiveLaw(state) == SectLawType.Benevolent)
        {
            modifier += 0.05;
        }

        if (GetActiveTalentPlan(state) == SectTalentPlanType.RecruitDisciples)
        {
            modifier += 0.12;
        }

        return modifier;
    }

    public static double GetQuarterFoodYieldModifier(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.OpenGranaries => 1.12,
            _ => 1.0
        };
    }

    public static double GetQuarterGoldYieldModifier(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.MarketDrive => 1.10,
            _ => 1.0
        };
    }

    public static double GetQuarterContributionYieldModifier(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.FortifyMountain => 1.08,
            SectQuarterDecreeType.MarketDrive => 1.05,
            SectQuarterDecreeType.HundredWorks => 1.06,
            _ => 1.0
        };
    }

    public static double GetQuarterResearchYieldModifier(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.GrandLecture => 1.14,
            SectQuarterDecreeType.HundredWorks => 1.04,
            _ => 1.0
        };
    }

    public static double GetQuarterPopulationGrowthModifier(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.OpenGranaries => 1.06,
            _ => 1.0
        };
    }

    public static double GetHourlyHappinessDelta(GameState state)
    {
        return GetActiveLaw(state) switch
        {
            SectLawType.Benevolent => 0.50,
            SectLawType.Discipline => -0.20,
            SectLawType.OpenLectures => 0.15,
            _ => 0.0
        };
    }

    public static double GetHourlyThreatDelta(GameState state)
    {
        var delta = 0.0;
        if (GetActiveDevelopmentDirection(state) == SectDevelopmentDirectionType.DefenseFirst)
        {
            delta -= 0.40;
        }

        if (GetActiveLaw(state) == SectLawType.Discipline)
        {
            delta -= 0.90;
        }

        return delta;
    }

    public static double GetQuarterHappinessDelta(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.OpenGranaries => 0.25,
            SectQuarterDecreeType.GrandLecture => 0.08,
            _ => 0.0
        };
    }

    public static double GetQuarterThreatDelta(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.FortifyMountain => -0.55,
            _ => 0.0
        };
    }

    public static double GetQuarterToolCraftModifier(GameState state)
    {
        return GetActiveQuarterDecree(state) switch
        {
            SectQuarterDecreeType.HundredWorks => 1.15,
            _ => 1.0
        };
    }

    public static bool IsQuarterDecreeExpired(GameState state, int currentQuarterIndex)
    {
        EnsureDefaults(state);
        var active = GetActiveQuarterDecree(state);
        return active != SectQuarterDecreeType.None &&
               state.QuarterDecreeIssuedQuarterIndex >= 0 &&
               state.QuarterDecreeIssuedQuarterIndex != currentQuarterIndex;
    }

    public static void ClearQuarterDecree(GameState state)
    {
        state.ActiveQuarterDecree = DefaultQuarterDecree.ToString();
        state.QuarterDecreeIssuedQuarterIndex = -1;
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
