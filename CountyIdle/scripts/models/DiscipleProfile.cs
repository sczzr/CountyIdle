using System;

namespace CountyIdle.Models;

/// <summary>
/// 弟子年龄阶段枚举（用于展示与筛选）。
/// </summary>
public enum DiscipleAgeBand
{
    /// <summary>
    /// 新苗期
    /// </summary>
    Seedling,
    /// <summary>
    /// 青年期
    /// </summary>
    Young,
    /// <summary>
    /// 盛年期
    /// </summary>
    Prime,
    /// <summary>
    /// 守峰期
    /// </summary>
    Elder
}

/// <summary>
/// 弟子档案信息（用于弟子谱展示与调度）。
/// </summary>
public sealed record DiscipleProfile(
    int Id,
    string Name,
    string RankName,
    DiscipleDirectiveType DirectiveType,
    JobType? JobType,
    string DutyDisplayName,
    string RealmName,
    int RealmTier,
    DiscipleAgeBand AgeBand,
    int Age,
    bool IsElite,
    int Health,
    int Mood,
    int Potential,
    int Combat,
    int Craft,
    int Insight,
    int Execution,
    int Contribution,
    string CurrentAssignment,
    string ResidenceName,
    string LinkedPeakSummary,
    string TraitSummary,
    DiscipleEquipmentProfile EquipmentProfile,
    string Note)
{
    /// <summary>
    /// 年龄显示文本（带年龄阶段）。
    /// </summary>
    public string AgeText => $"{Age} 岁 · {GetAgeBandDisplayName(AgeBand)}";

    /// <summary>
    /// 获取年龄阶段的中文显示名。
    /// </summary>
    public static string GetAgeBandDisplayName(DiscipleAgeBand ageBand)
    {
        return ageBand switch
        {
            DiscipleAgeBand.Seedling => "新苗期",
            DiscipleAgeBand.Young => "青年期",
            DiscipleAgeBand.Prime => "盛年期",
            DiscipleAgeBand.Elder => "守峰期",
            _ => "门人"
        };
    }
}
