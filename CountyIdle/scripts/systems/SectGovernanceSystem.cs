using CountyIdle.Models;

namespace CountyIdle.Systems;

public class SectGovernanceSystem
{
    public void EnsureDefaults(GameState state)
    {
        SectGovernanceRules.EnsureDefaults(state);
    }

    public bool ShiftDevelopmentDirection(GameState state, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主未调整发展方向。";
            return false;
        }

        var definition = SectGovernanceRules.CycleDevelopmentDirection(state, delta);
        SectTaskRules.ResetToRecommended(state);
        log = $"宗主已改定发展方向为“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public bool ShiftLaw(GameState state, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主未调整宗门法令。";
            return false;
        }

        var definition = SectGovernanceRules.CycleLaw(state, delta);
        log = $"宗主已颁布“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public bool ShiftTalentPlan(GameState state, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主未调整育才方略。";
            return false;
        }

        var definition = SectGovernanceRules.CycleTalentPlan(state, delta);
        log = $"宗主已改定育才方略为“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public bool ShiftQuarterDecree(GameState state, int currentQuarterIndex, out string log, int delta)
    {
        if (delta == 0)
        {
            log = "宗主未调整季度法令。";
            return false;
        }

        var definition = SectGovernanceRules.CycleQuarterDecree(state, currentQuarterIndex, delta);
        log = definition.DecreeType == SectQuarterDecreeType.None
            ? "宗主已撤销本季专项法令，恢复常态章程。"
            : $"宗主已颁下本季法令“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public bool HandleQuarterTransition(GameState state, int currentQuarterIndex, out string? log)
    {
        if (!SectGovernanceRules.IsQuarterDecreeExpired(state, currentQuarterIndex))
        {
            log = null;
            return false;
        }

        var previousDefinition = SectGovernanceRules.GetActiveQuarterDecreeDefinition(state);
        SectGovernanceRules.ClearQuarterDecree(state);
        log = $"季度轮转：上一季法令“{previousDefinition.DisplayName}”已期满，待宗主颁布新令。";
        return true;
    }

    public bool ResetGovernance(GameState state, out string log)
    {
        SectGovernanceRules.ResetToDefaults(state);
        SectTaskRules.ResetToRecommended(state);
        log = "已恢复默认治宗格局：均衡发展、宽和养民、广纳新徒，并撤销本季专项法令。";
        return true;
    }
}
