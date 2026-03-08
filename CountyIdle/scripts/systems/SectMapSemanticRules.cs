using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class SectMapSemanticRules
{
    public static string GetSettlementName()
    {
        return "浮云宗";
    }

    public static string GetWorldMapTitle()
    {
        return "世界地图";
    }

    public static string GetLegacyPrefectureMapTitle()
    {
        return "江陵府外域";
    }

    public static string GetOuterRegionRoadName()
    {
        return "府域灵道";
    }

    public static string GetOuterRegionSettlementName()
    {
        return "附庸据点";
    }

    public static string GetOuterRegionReliefActionName()
    {
        return "抚恤附庸";
    }

    public static string GetWildernessGatheringLabel()
    {
        return "峰外采办";
    }

    public static string GetTechnologyTrackName()
    {
        return "传法院";
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
            IndustryBuildingType.Agriculture => "阵材圃",
            IndustryBuildingType.Workshop => "傀儡工坊",
            IndustryBuildingType.Research => GetTechnologyTrackName(),
            IndustryBuildingType.Trade => compact ? "总坊" : "青云总坊",
            IndustryBuildingType.Administration => "庶务殿",
            _ => "建筑"
        };
    }

    public static string GetAnchorLabelPrefix(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => "阵材圃",
            TownActivityAnchorType.Workshop => "傀儡工坊",
            TownActivityAnchorType.Market => "青云总坊",
            TownActivityAnchorType.Academy => "传法院",
            TownActivityAnchorType.Administration => "庶务殿",
            TownActivityAnchorType.Leisure => "演阵台",
            _ => "浮云宗场所"
        };
    }

    public static string GetAnchorTypeText(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => "阵材圃",
            TownActivityAnchorType.Workshop => "傀儡工坊",
            TownActivityAnchorType.Market => "总坊",
            TownActivityAnchorType.Academy => "传法院",
            TownActivityAnchorType.Administration => "庶务殿",
            TownActivityAnchorType.Leisure => "演阵台",
            _ => "浮云宗场所"
        };
    }

    public static string GetAdministrationStatusText()
    {
        return "核账中";
    }

    public static string GetLeisureIdleStatusText()
    {
        return "静悟中";
    }

    public static string GetLeisureBusyStatusText()
    {
        return "推演中";
    }

    public static string GetLeisureInboundStatusText()
    {
        return "有人前往";
    }

    public static string GetWorkBusyStatusText()
    {
        return "阵务中";
    }

    public static string GetWorkInboundStatusText()
    {
        return "前往中";
    }

    public static string GetWorkIdleStatusText()
    {
        return "轮休中";
    }

    public static string GetEmptyResidentStatusText(TownActivityAnchorType anchorType)
    {
        return anchorType == TownActivityAnchorType.Leisure ? GetLeisureIdleStatusText() : "暂无可视常驻弟子";
    }

    public static string GetMapInteractionHint()
    {
        return "左键选中浮云宗场所查看状态 · 右键取消选中";
    }
}
