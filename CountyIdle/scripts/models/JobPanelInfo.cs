namespace CountyIdle.Models;

/// <summary>
/// 职司面板展示信息（摘要/详情/默认权重）。
/// </summary>
public sealed class JobPanelInfo
{
    /// <summary>
    /// 构造职司面板展示信息。
    /// </summary>
    public JobPanelInfo(
        JobType jobType,
        string activeRoleName,
        string titleText,
        string summaryText,
        string detailText,
        string defaultPriorityText)
    {
        JobType = jobType;
        ActiveRoleName = activeRoleName;
        TitleText = titleText;
        SummaryText = summaryText;
        DetailText = detailText;
        DefaultPriorityText = defaultPriorityText;
    }

    // 职司类型
    public JobType JobType { get; }

    // 当前激活的岗位名称
    public string ActiveRoleName { get; }

    // 标题文本
    public string TitleText { get; }

    // 摘要文本
    public string SummaryText { get; }

    // 详情文本
    public string DetailText { get; }

    // 默认优先级提示
    public string DefaultPriorityText { get; }
}
