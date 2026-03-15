using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class SectMapSemanticRules
{
    public static string GetSettlementName(GameState state)
    {
        return GetSettlementName(state?.SectNameMap);
    }

    public static string GetSettlementName(IReadOnlyDictionary<string, string>? nameMap = null)
    {
        return SectNamingRules.GetName(nameMap, SectNamingRules.SectNameKey);
    }

    public static string GetPrimaryPeakName(GameState state)
    {
        return GetPrimaryPeakName(state?.SectNameMap);
    }

    public static string GetPrimaryPeakName(IReadOnlyDictionary<string, string>? nameMap = null)
    {
        return SectNamingRules.GetName(nameMap, SectNamingRules.PeakTianyanKey);
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

    public static string GetTechnologyTrackName(GameState state)
    {
        return GetTechnologyTrackName(state?.SectNameMap);
    }

    public static string GetTechnologyTrackName(IReadOnlyDictionary<string, string>? nameMap = null)
    {
        return SectNamingRules.GetName(nameMap, SectNamingRules.HallAcademyKey);
    }

    public static string GetTechnologyLevelLabel(int techLevel, IReadOnlyDictionary<string, string>? nameMap = null)
    {
        var trackName = GetTechnologyTrackName(nameMap);
        return techLevel <= 0 ? $"{trackName}未悟道" : $"{trackName} T{techLevel}";
    }

    public static string GetBuildingDisplayName(
        IndustryBuildingType buildingType,
        bool compact = false,
        IReadOnlyDictionary<string, string>? nameMap = null)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture => SectNamingRules.GetName(nameMap, SectNamingRules.HallFormationFieldKey),
            IndustryBuildingType.Workshop => SectNamingRules.GetName(nameMap, SectNamingRules.HallPuppetWorkshopKey),
            IndustryBuildingType.Research => GetTechnologyTrackName(nameMap),
            IndustryBuildingType.Trade => compact
                ? SectNamingRules.GetCompactName(nameMap, SectNamingRules.HallMarketKey, "总坊")
                : SectNamingRules.GetName(nameMap, SectNamingRules.HallMarketKey),
            IndustryBuildingType.Administration => SectNamingRules.GetName(nameMap, SectNamingRules.HallAffairsKey),
            _ => "建筑"
        };
    }

    public static string GetAnchorLabelPrefix(TownActivityAnchorType anchorType, IReadOnlyDictionary<string, string>? nameMap = null)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => SectNamingRules.GetName(nameMap, SectNamingRules.HallFormationFieldKey),
            TownActivityAnchorType.Workshop => SectNamingRules.GetName(nameMap, SectNamingRules.HallPuppetWorkshopKey),
            TownActivityAnchorType.Market => SectNamingRules.GetName(nameMap, SectNamingRules.HallMarketKey),
            TownActivityAnchorType.Academy => SectNamingRules.GetName(nameMap, SectNamingRules.HallAcademyKey),
            TownActivityAnchorType.Administration => SectNamingRules.GetName(nameMap, SectNamingRules.HallAffairsKey),
            TownActivityAnchorType.Leisure => SectNamingRules.GetName(nameMap, SectNamingRules.HallLeisureKey),
            _ => $"{GetSettlementName(nameMap)}场所"
        };
    }

    public static string GetAnchorTypeText(TownActivityAnchorType anchorType, IReadOnlyDictionary<string, string>? nameMap = null)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => SectNamingRules.GetName(nameMap, SectNamingRules.HallFormationFieldKey),
            TownActivityAnchorType.Workshop => SectNamingRules.GetName(nameMap, SectNamingRules.HallPuppetWorkshopKey),
            TownActivityAnchorType.Market => SectNamingRules.GetCompactName(nameMap, SectNamingRules.HallMarketKey, "总坊"),
            TownActivityAnchorType.Academy => SectNamingRules.GetName(nameMap, SectNamingRules.HallAcademyKey),
            TownActivityAnchorType.Administration => SectNamingRules.GetName(nameMap, SectNamingRules.HallAffairsKey),
            TownActivityAnchorType.Leisure => SectNamingRules.GetName(nameMap, SectNamingRules.HallLeisureKey),
            _ => $"{GetSettlementName(nameMap)}场所"
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
        return "左键检视任意院域，右键清除选中；当前先展示院域底盘、灵气与推荐坊局，不接弟子实体联动";
    }
}
