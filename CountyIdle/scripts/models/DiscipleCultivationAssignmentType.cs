namespace CountyIdle.Models;

/// <summary>
/// 弟子修炼安排类型（用于修炼卷登记）。
/// </summary>
public enum DiscipleCultivationAssignmentType
{
    /// <summary>
    /// 不额外指定修炼安排。
    /// </summary>
    None,

    /// <summary>
    /// 技能修炼。
    /// </summary>
    SkillTraining,

    /// <summary>
    /// 功法打磨。
    /// </summary>
    TechniquePolish,

    /// <summary>
    /// 技艺练习。
    /// </summary>
    CraftPractice,

    /// <summary>
    /// 打坐修炼。
    /// </summary>
    Meditation
}
