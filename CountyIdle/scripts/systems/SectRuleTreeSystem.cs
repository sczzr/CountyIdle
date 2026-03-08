using CountyIdle.Models;

namespace CountyIdle.Systems;

public class SectRuleTreeSystem
{
    public void EnsureDefaults(GameState state)
    {
        SectRuleTreeRules.EnsureDefaults(state);
    }

    public bool ShiftAffairsRule(GameState state, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主未调整庶务门规。";
            return false;
        }

        var definition = SectRuleTreeRules.CycleAffairsRule(state, delta);
        log = $"宗主已定庶务门规“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public bool ShiftDoctrineRule(GameState state, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主未调整传功门规。";
            return false;
        }

        var definition = SectRuleTreeRules.CycleDoctrineRule(state, delta);
        log = $"宗主已定传功门规“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public bool ShiftDisciplineRule(GameState state, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主未调整巡山门规。";
            return false;
        }

        var definition = SectRuleTreeRules.CycleDisciplineRule(state, delta);
        log = $"宗主已定巡山门规“{definition.DisplayName}”：{definition.ShortEffect}。";
        return true;
    }

    public void ResetRules(GameState state)
    {
        SectRuleTreeRules.ResetToDefaults(state);
    }
}
