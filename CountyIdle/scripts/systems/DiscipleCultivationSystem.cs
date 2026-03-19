using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

/// <summary>
/// 修炼卷业务流程。
/// </summary>
public sealed class DiscipleCultivationSystem
{
    public void EnsureDefaults(GameState state)
    {
        DiscipleCultivationRules.EnsureDefaults(state);
    }

    public bool SetAssignment(GameState state, int discipleId, DiscipleCultivationAssignmentType assignmentType, out string log)
    {
        DiscipleCultivationRules.EnsureDefaults(state);

        var profile = DiscipleRosterSystem.BuildRoster(state)
            .FirstOrDefault(candidate => candidate.Id == discipleId);

        if (profile == null)
        {
            log = "未找到对应弟子，修炼安排未能入卷。";
            return false;
        }

        var currentAssignment = DiscipleCultivationRules.GetAssignment(state, discipleId);
        if (currentAssignment == assignmentType)
        {
            log = assignmentType == DiscipleCultivationAssignmentType.None
                ? $"“{profile.Name}”当前未登记额外修炼安排。"
                : $"“{profile.Name}”当前已登记“{DiscipleCultivationRules.GetAssignmentDisplayName(assignmentType)}”。";
            return false;
        }

        DiscipleCultivationRules.SetAssignment(state, discipleId, assignmentType);
        var historyEntry = assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining => "改定主修为技能修炼。",
            DiscipleCultivationAssignmentType.TechniquePolish => "改定主修为功法打磨。",
            DiscipleCultivationAssignmentType.CraftPractice => "改定主修为技艺练习。",
            DiscipleCultivationAssignmentType.Meditation => "改定主修为打坐修炼。",
            _ => "撤下修炼卷安排，恢复常制修行。"
        };
        DiscipleCultivationRules.AppendHistoryEntry(state, discipleId, historyEntry);
        log = assignmentType switch
        {
            DiscipleCultivationAssignmentType.SkillTraining =>
                $"已将“{profile.Name}”收录入修炼卷，当前安排为“技能修炼”。",
            DiscipleCultivationAssignmentType.TechniquePolish =>
                $"已将“{profile.Name}”收录入修炼卷，当前安排为“功法打磨”。",
            DiscipleCultivationAssignmentType.CraftPractice =>
                $"已将“{profile.Name}”收录入修炼卷，当前安排为“技艺练习”。",
            DiscipleCultivationAssignmentType.Meditation =>
                $"已将“{profile.Name}”收录入修炼卷，当前安排为“打坐修炼”。",
            _ =>
                $"已将“{profile.Name}”从修炼卷安排中撤下，恢复常制修行。"
        };
        return true;
    }
}
