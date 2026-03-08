using CountyIdle.Models;

namespace CountyIdle.Systems;

public class SectTaskSystem
{
    public void EnsureDefaults(GameState state)
    {
        SectTaskRules.EnsureDefaults(state);
    }

    public bool AdjustOrder(GameState state, SectTaskType taskType, int delta, out string log)
    {
        if (delta == 0)
        {
            log = "宗主中枢未作调整。";
            return false;
        }

        SectTaskRules.EnsureDefaults(state);

        var currentOrders = SectTaskRules.GetOrderUnits(state, taskType);
        var nextOrders = SectTaskRules.SetOrderUnits(state, taskType, currentOrders + delta);
        if (nextOrders == currentOrders)
        {
            log = delta > 0 ? "该治理条目已达当前最高关注。" : "该治理条目已降至最低关注。";
            return false;
        }

        var snapshot = SectTaskRules.BuildResolutionSnapshot(state);
        SectTaskRules.ApplyResolvedSnapshot(state, snapshot);

        var definition = SectTaskRules.GetDefinition(taskType);
        var levelText = SectTaskRules.GetDirectiveLevelSummary(state, taskType);
        var executionText = SectTaskRules.GetDirectiveExecutionSummary(state, taskType);
        log =
            $"已调整“{definition.DisplayName}”的治理力度，当前为{levelText}，{executionText}。";

        return true;
    }

    public bool ResetOrders(GameState state, out string log)
    {
        SectTaskRules.ResetToRecommended(state);
        log = $"已恢复均衡治宗方略：{SectTaskRules.BuildGovernanceHeadline(state)}，{SectTaskRules.BuildGovernanceExecutionSummary(state)}。";
        return true;
    }
}
