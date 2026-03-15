using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

/// <summary>
/// 旧版职业系统兼容层 - 保留以支持旧代码
/// 实际逻辑已迁移到 SkillProgressionRules
/// </summary>
public static class JobProgressionRules
{
    // JobType 到 CraftSkillType 的映射（迁移规则）
    private static CraftSkillType MapJobToSkill(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => CraftSkillType.SpiritPlant,  // 农 → 灵植
            JobType.Worker => CraftSkillType.Forging,       // 工 → 炼器
            JobType.Merchant => CraftSkillType.Golem,       // 商 → 傀儡
            JobType.Scholar => CraftSkillType.Arcane,       // 学 → 天机
            _ => CraftSkillType.SpiritPlant
        };
    }

    /// <summary>
    /// 获取面板信息 - 兼容旧接口，内部调用 SkillProgressionRules
    /// </summary>
    public static JobPanelInfo GetPanelInfo(GameState state, JobType jobType)
    {
        var skillType = MapJobToSkill(jobType);
        var skillPanel = SkillProgressionRules.GetPanelInfo(state, skillType);
        
        return new JobPanelInfo(
            jobType,
            skillPanel.ActiveSkillName,
            skillPanel.TitleText,
            skillPanel.SummaryText,
            skillPanel.DetailText,
            skillPanel.DefaultPriorityText);
    }

    /// <summary>
    /// 获取当前激活的职业名称 - 兼容旧接口
    /// </summary>
    public static string GetActiveRoleName(GameState state, JobType jobType)
    {
        var skillType = MapJobToSkill(jobType);
        return SkillProgressionRules.GetActiveSkillName(state, skillType);
    }

    /// <summary>
    /// 获取默认优先级 - 兼容旧接口
    /// </summary>
    public static string GetDefaultPriorityText(JobType jobType)
    {
        var skillType = MapJobToSkill(jobType);
        return SkillProgressionRules.GetDefaultPriorityText(skillType);
    }
}