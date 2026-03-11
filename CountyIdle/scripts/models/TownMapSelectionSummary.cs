using CountyIdle.Models;

namespace CountyIdle.Models;

public sealed class TownMapSelectionSummary
{
    public TownMapSelectionSummary(
        bool hasSelection,
        TownActivityAnchorType? anchorType,
        TownCellContentKind? contentKind,
        IndustryBuildingType? suggestedBuildType,
        string badgeText,
        string title,
        string subtitle,
        string statusLabel,
        string statusText,
        string residentLabel,
        string residentText,
        string transitLabel,
        string transitText,
        string locationLabel,
        string locationText,
        string descriptionText)
    {
        HasSelection = hasSelection;
        AnchorType = anchorType;
        ContentKind = contentKind;
        SuggestedBuildType = suggestedBuildType;
        BadgeText = badgeText;
        Title = title;
        Subtitle = subtitle;
        StatusLabel = statusLabel;
        StatusText = statusText;
        ResidentLabel = residentLabel;
        ResidentText = residentText;
        TransitLabel = transitLabel;
        TransitText = transitText;
        LocationLabel = locationLabel;
        LocationText = locationText;
        DescriptionText = descriptionText;
    }

    public bool HasSelection { get; }
    public TownActivityAnchorType? AnchorType { get; }
    public TownCellContentKind? ContentKind { get; }
    public IndustryBuildingType? SuggestedBuildType { get; }
    public string BadgeText { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string StatusLabel { get; }
    public string StatusText { get; }
    public string ResidentLabel { get; }
    public string ResidentText { get; }
    public string TransitLabel { get; }
    public string TransitText { get; }
    public string LocationLabel { get; }
    public string LocationText { get; }
    public string DescriptionText { get; }

    public static TownMapSelectionSummary CreateDefault()
    {
        return new TownMapSelectionSummary(
            false,
            null,
            null,
            null,
            "未选中地块",
            "浮云宗·天衍峰",
            "点击任意院域检视地块详情",
            "当前态势",
            "未检视",
            "坊位格局",
            "--",
            "地脉灵气",
            "--",
            "地气坐标",
            "Hex 坐标待定",
            "天衍峰山门图现已支持全格检视，左键点选任意六角地块后可查看其院域底盘、灵气和推荐坊局。");
    }
}
