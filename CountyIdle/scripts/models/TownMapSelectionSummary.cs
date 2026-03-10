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
            "当前仅展示地貌",
            "无场所可检视",
            "--",
            "--",
            "Hex 坐标待定",
            "天衍峰山门图当前仅保留地貌与道路视图，场所与门人可视化已暂时停用。");
    }
}
