using CountyIdle.Models;

namespace CountyIdle.Systems;

public class SectPeakSupportSystem
{
    public void EnsureDefaults(GameState state)
    {
        SectPeakSupportRules.EnsureDefaults(state);
    }

    public bool SetPeakSupport(GameState state, SectPeakSupportType supportType, out string log)
    {
        var current = SectPeakSupportRules.GetActiveSupport(state);
        if (current == supportType)
        {
            log = $"当前已由“{SectPeakSupportRules.GetDefinition(supportType).DisplayName}”担任协同峰。";
            return false;
        }

        var definition = SectPeakSupportRules.SetActiveSupport(state, supportType);
        log = supportType == SectPeakSupportType.Balanced
            ? "已恢复诸峰均衡轮转，不再额外偏向单峰协同。"
            : $"宗主已令“{definition.DisplayName}”担任本季协同峰：{definition.ShortEffect}。";
        return true;
    }

    public bool ResetPeakSupport(GameState state, out string log)
    {
        return SetPeakSupport(state, SectPeakSupportType.Balanced, out log);
    }
}
