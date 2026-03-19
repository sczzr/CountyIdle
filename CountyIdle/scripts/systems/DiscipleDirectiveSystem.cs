using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 弟子指令业务流程
public sealed class DiscipleDirectiveSystem
{
    // 确保指令数据结构初始化
    public void EnsureDefaults(GameState state)
    {
        DiscipleDirectiveRules.EnsureDefaults(state);
    }

    // 设置弟子指令并输出日志
    public bool SetDirective(GameState state, int discipleId, DiscipleDirectiveType directiveType, out string log)
    {
        DiscipleDirectiveRules.EnsureDefaults(state);

        var profile = DiscipleRosterSystem.BuildRoster(state)
            .FirstOrDefault(candidate => candidate.Id == discipleId);

        if (profile == null)
        {
            log = "未找到对应弟子，批注未能入卷。";
            return false;
        }

        if (profile.AgeBand == DiscipleAgeBand.Seedling && directiveType != DiscipleDirectiveType.None)
        {
            log = $"“{profile.Name}”仍属启蒙新苗，暂不可列入外务或执事重点名册。";
            return false;
        }

        var currentDirective = DiscipleDirectiveRules.GetDirective(state, discipleId);
        if (currentDirective == directiveType)
        {
            log = $"“{profile.Name}”当前已标记为“{DiscipleDirectiveRules.GetDirectiveDisplayName(directiveType)}”。";
            return false;
        }

        DiscipleDirectiveRules.SetDirective(state, discipleId, directiveType);
        var dutyAffinitySummary = DiscipleCultivationRules.BuildDutyAffinitySummary(state, profile);
        var specializationEffectSummary = DiscipleCultivationRules.BuildSpecializationEffectLogSummary(state, profile.Id);
        var specializationEffectSuffix = string.IsNullOrWhiteSpace(specializationEffectSummary)
            ? string.Empty
            : $" {specializationEffectSummary}。";
        log = directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate =>
                $"已将“{profile.Name}”纳入外务候补：后续历练将优先吸纳其战力与执行表现。当前差事相性：{dutyAffinitySummary}。{specializationEffectSuffix}",
            DiscipleDirectiveType.StewardCandidate =>
                $"已将“{profile.Name}”纳入执事培养：当前内务执行效率约 +{(DiscipleDirectiveRules.GetStewardExecutionModifier(state) - 1.0) * 100.0:0.#}%，贡献回流约 +{(DiscipleDirectiveRules.GetStewardContributionModifier(state) - 1.0) * 100.0:0.#}%。当前差事相性：{dutyAffinitySummary}。{specializationEffectSuffix}",
            _ =>
                $"已将“{profile.Name}”恢复为常制观察，不再额外纳入外务/执事重点名册。"
        };
        return true;
    }
}
