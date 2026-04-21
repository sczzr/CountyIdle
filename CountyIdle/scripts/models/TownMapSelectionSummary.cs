using System;
using CountyIdle.Models;

namespace CountyIdle.Models;

// 小镇地图选中信息摘要（用于 UI 展示）
public sealed class TownMapSelectionSummary
{
    // 构造选中摘要数据（确保列表不为空引用）
    public TownMapSelectionSummary(
        bool hasSelection,
        TownActivityAnchorType? anchorType,
        TownCellContentKind? contentKind,
        IndustryBuildingType? suggestedBuildType,
        int buildSlotCount,
        int occupiedBuildSlotCount,
        string badgeText,
        string title,
        string subtitle,
        string buildingLabel,
        string buildingText,
        string[] buildingList,
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
        BuildSlotCount = Math.Max(buildSlotCount, 0);
        OccupiedBuildSlotCount = Math.Max(occupiedBuildSlotCount, 0);
        BadgeText = badgeText;
        Title = title;
        Subtitle = subtitle;
        BuildingLabel = buildingLabel;
        BuildingText = buildingText;
        BuildingList = buildingList ?? Array.Empty<string>();
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

    // 是否已有选中
    public bool HasSelection { get; }
    // 选中格子所属锚点类型
    public TownActivityAnchorType? AnchorType { get; }
    // 选中格子的内容类型
    public TownCellContentKind? ContentKind { get; }
    // 推荐建设类型
    public IndustryBuildingType? SuggestedBuildType { get; }
    // 当前地块总坊位数
    public int BuildSlotCount { get; }
    // 当前地块已占用坊位数
    public int OccupiedBuildSlotCount { get; }
    // 当前地块是否仍可继续营建
    public bool HasBuildCapacity => HasSelection && BuildSlotCount > OccupiedBuildSlotCount;
    // 徽章文本
    public string BadgeText { get; }
    // 标题文本
    public string Title { get; }
    // 副标题文本
    public string Subtitle { get; }
    // 建筑标签
    public string BuildingLabel { get; }
    // 建筑说明文本
    public string BuildingText { get; }
    // 建筑列表
    public string[] BuildingList { get; }
    // 状态标签
    public string StatusLabel { get; }
    // 状态说明文本
    public string StatusText { get; }
    // 居民标签
    public string ResidentLabel { get; }
    // 居民说明文本
    public string ResidentText { get; }
    // 交通标签
    public string TransitLabel { get; }
    // 交通说明文本
    public string TransitText { get; }
    // 坐标标签
    public string LocationLabel { get; }
    // 坐标说明文本
    public string LocationText { get; }
    // 描述文本
    public string DescriptionText { get; }

    // 创建默认未选中摘要（使用默认宗门/峰名）
    public static TownMapSelectionSummary CreateDefault()
    {
        return CreateDefault("浮云宗", "天衍峰");
    }

    // 创建默认未选中摘要（支持自定义宗门/峰名）
    public static TownMapSelectionSummary CreateDefault(string sectName, string peakName)
    {
        var safeSectName = string.IsNullOrWhiteSpace(sectName) ? "浮云宗" : sectName;
        var safePeakName = string.IsNullOrWhiteSpace(peakName) ? "天衍峰" : peakName;
        return new TownMapSelectionSummary(
            false,
            null,
            null,
            null,
            0,
            0,
            "未选中地块",
            $"{safeSectName}·{safePeakName}",
            "点击任意院域检视地块详情",
            "建筑列表",
            "待选中",
            Array.Empty<string>(),
            "当前态势",
            "未检视",
            "坊位格局",
            "--",
            "地脉灵气",
            "--",
            "地气坐标",
            "Hex 坐标待定",
            $"{safePeakName}山门图现已支持全格检视，左键点选任意六角地块后可查看其院域底盘、灵气和推荐坊局。");
    }
}
