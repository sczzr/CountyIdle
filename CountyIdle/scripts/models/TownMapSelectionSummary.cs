using CountyIdle.Models;

namespace CountyIdle.Models;

public sealed class TownMapSelectionSummary
{
    public TownMapSelectionSummary(
        bool hasSelection,
        TownActivityAnchorType? anchorType,
        string title,
        string subtitle,
        string statusText,
        string residentText,
        string transitText,
        string locationText,
        string descriptionText)
    {
        HasSelection = hasSelection;
        AnchorType = anchorType;
        Title = title;
        Subtitle = subtitle;
        StatusText = statusText;
        ResidentText = residentText;
        TransitText = transitText;
        LocationText = locationText;
        DescriptionText = descriptionText;
    }

    public bool HasSelection { get; }
    public TownActivityAnchorType? AnchorType { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string StatusText { get; }
    public string ResidentText { get; }
    public string TransitText { get; }
    public string LocationText { get; }
    public string DescriptionText { get; }

    public static TownMapSelectionSummary CreateDefault()
    {
        return new TownMapSelectionSummary(
            false,
            null,
            "浮云宗·天衍峰",
            "请选择一个六边形地块",
            "待检视",
            "--",
            "--",
            "Hex 坐标待定",
            "左键点选中央 hex tile，左侧会出现该地块的状态、驻守信息与可执行操作。");
    }
}
