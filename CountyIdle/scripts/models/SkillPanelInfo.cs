namespace CountyIdle.Models;

/// <summary>
/// 八大修仙技艺面板信息
/// </summary>
public sealed class SkillPanelInfo
{
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

    public CraftSkillType SkillType { get; }

    public string ActiveSkillName { get; }

    public string TitleText { get; }

    public string SummaryText { get; }

    public string DetailText { get; }

    public string DefaultPriorityText { get; }
}