namespace CountyIdle.Models;

public sealed class JobPanelInfo
{
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

    public JobType JobType { get; }

    public string ActiveRoleName { get; }

    public string TitleText { get; }

    public string SummaryText { get; }

    public string DetailText { get; }

    public string DefaultPriorityText { get; }
}
