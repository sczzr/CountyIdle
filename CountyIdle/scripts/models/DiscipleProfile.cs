using System;

namespace CountyIdle.Models;

public enum DiscipleAgeBand
{
    Seedling,
    Young,
    Prime,
    Elder
}

public sealed record DiscipleProfile(
    int Id,
    string Name,
    string RankName,
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
    string Note)
{
    public string AgeText => $"{Age} 岁 · {GetAgeBandDisplayName(AgeBand)}";

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
