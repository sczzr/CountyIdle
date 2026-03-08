using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class SectMapSemanticRules
{
    public static string GetSettlementName()
    {
        return "宗门";
    }

    public static string GetWorldMapTitle()
    {
        return "世界地图";
    }

    public static string GetLegacyPrefectureMapTitle()
    {
        return "外域态势";
    }

    public static string GetOuterRegionRoadName()
    {
        return "灵道";
    }

    public static string GetOuterRegionSettlementName()
    {
        return "聚落";
    }

    public static string GetOuterRegionReliefActionName()
    {
        return "抚恤聚落";
    }

    public static string GetWildernessGatheringLabel()
    {
        return "山野采集";
    }

    public static string GetTechnologyTrackName()
    {
        return "藏经阁";
    }

    public static string GetTechnologyLevelLabel(int techLevel)
    {
        var trackName = GetTechnologyTrackName();
        return techLevel <= 0 ? $"{trackName}未悟道" : $"{trackName} T{techLevel}";
    }

    public static string GetBuildingDisplayName(IndustryBuildingType buildingType, bool compact = false)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture => "灵田",
            IndustryBuildingType.Workshop => "炼器坊",
            IndustryBuildingType.Research => GetTechnologyTrackName(),
            IndustryBuildingType.Trade => compact ? "坊市" : "山门坊市",
            IndustryBuildingType.Administration => "宗务殿",
            _ => "建筑"
        };
    }

    public static string GetAnchorLabelPrefix(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => "灵田",
            TownActivityAnchorType.Workshop => "炼器坊",
            TownActivityAnchorType.Market => "山门坊市",
            TownActivityAnchorType.Academy => "藏经阁",
            TownActivityAnchorType.Administration => "宗务殿",
            TownActivityAnchorType.Leisure => "论道亭",
            _ => "宗门场所"
        };
    }

    public static string GetAnchorTypeText(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => "灵田",
            TownActivityAnchorType.Workshop => "炼器坊",
            TownActivityAnchorType.Market => "坊市",
            TownActivityAnchorType.Academy => "藏经阁",
            TownActivityAnchorType.Administration => "宗务殿",
            TownActivityAnchorType.Leisure => "论道亭",
            _ => "宗门场所"
        };
    }

    public static string GetAdministrationStatusText()
    {
        return "议事中";
    }

    public static string GetLeisureIdleStatusText()
    {
        return "静修中";
    }

    public static string GetLeisureBusyStatusText()
    {
        return "论道中";
    }

    public static string GetLeisureInboundStatusText()
    {
        return "有人前往";
    }

    public static string GetWorkBusyStatusText()
    {
        return "运转中";
    }

    public static string GetWorkInboundStatusText()
    {
        return "前往中";
    }

    public static string GetWorkIdleStatusText()
    {
        return "暂歇中";
    }

    public static string GetEmptyResidentStatusText(TownActivityAnchorType anchorType)
    {
        return anchorType == TownActivityAnchorType.Leisure ? GetLeisureIdleStatusText() : "暂无可视常驻弟子";
    }

    public static string GetMapInteractionHint()
    {
        return "左键选中宗门场所查看状态 · 右键取消选中";
    }
}
