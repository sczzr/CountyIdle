namespace CountyIdle.Models;

/// <summary>
/// 八大修仙技艺面板信息
/// </summary>
public sealed class SkillPanelInfo
{
    /// <summary>
    /// 构造技艺面板展示信息。
    /// </summary>
    public SkillPanelInfo(
        CraftSkillType skillType,
        string activeSkillName,
        string titleText,
        string summaryText,
        string detailText,
        string defaultPriorityText)
    {
        SkillType = skillType;
        ActiveSkillName = activeSkillName;
        TitleText = titleText;
        SummaryText = summaryText;
        DetailText = detailText;
        DefaultPriorityText = defaultPriorityText;
    }

    // 技艺类型
    public CraftSkillType SkillType { get; }

    // 当前激活的技艺名称
    public string ActiveSkillName { get; }

    // 标题文本
    public string TitleText { get; }

    // 摘要文本
    public string SummaryText { get; }

    // 详情文本
    public string DetailText { get; }

    // 默认优先级提示
    public string DefaultPriorityText { get; }
}
