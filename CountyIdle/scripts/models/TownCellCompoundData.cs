using System;
using Godot;

namespace CountyIdle.Models;

public enum TownCellContentKind
{
    Empty,
    Infrastructure,
    Production,
    Service,
    Residence,
    Special
}

public enum TownCompoundPlanStyle
{
    Natural,
    Specialized,
    Synergy,
    Balanced
}

public sealed class TownSubBuildingPlan
{
    public TownSubBuildingPlan(
        string templateId,
        string displayName,
        float qiDemand,
        int laborDemand,
        string[] synergyTags,
        string[] conflictTags)
    {
        TemplateId = string.IsNullOrWhiteSpace(templateId) ? "unknown" : templateId;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "未命名坊位" : displayName;
        QiDemand = Math.Max(qiDemand, 0f);
        LaborDemand = Math.Max(laborDemand, 0);
        SynergyTags = synergyTags ?? [];
        ConflictTags = conflictTags ?? [];
    }

    public string TemplateId { get; }
    public string DisplayName { get; }
    public float QiDemand { get; }
    public int LaborDemand { get; }
    public string[] SynergyTags { get; }
    public string[] ConflictTags { get; }
}

public sealed class TownCellCompoundData
{
    public TownCellCompoundData(
        Vector2I cell,
        string regionName,
        TownCellContentKind contentKind,
        TownCompoundPlanStyle planStyle,
        string qiAffinityText,
        int baseQiCapacity,
        int qiRecoveryPerHour,
        int buildSlotCount,
        string[] featureTexts,
        TownSubBuildingPlan[] subBuildings,
        float totalQiDemand,
        float qiCongestion,
        float synergyScore,
        float stability,
        IndustryBuildingType? suggestedBuildType)
    {
        Cell = cell;
        RegionName = string.IsNullOrWhiteSpace(regionName) ? "天衍峰" : regionName;
        ContentKind = contentKind;
        PlanStyle = planStyle;
        QiAffinityText = string.IsNullOrWhiteSpace(qiAffinityText) ? "地脉平稳" : qiAffinityText;
        BaseQiCapacity = Math.Max(baseQiCapacity, 0);
        QiRecoveryPerHour = Math.Max(qiRecoveryPerHour, 0);
        BuildSlotCount = Math.Max(buildSlotCount, 0);
        FeatureTexts = featureTexts ?? [];
        SubBuildings = subBuildings ?? [];
        TotalQiDemand = Math.Max(totalQiDemand, 0f);
        QiCongestion = Math.Max(qiCongestion, 0f);
        SynergyScore = synergyScore;
        Stability = Math.Max(stability, 0f);
        SuggestedBuildType = suggestedBuildType;
    }

    public Vector2I Cell { get; }
    public string RegionName { get; }
    public TownCellContentKind ContentKind { get; }
    public TownCompoundPlanStyle PlanStyle { get; }
    public string QiAffinityText { get; }
    public int BaseQiCapacity { get; }
    public int QiRecoveryPerHour { get; }
    public int BuildSlotCount { get; }
    public string[] FeatureTexts { get; }
    public TownSubBuildingPlan[] SubBuildings { get; }
    public float TotalQiDemand { get; }
    public float QiCongestion { get; }
    public float SynergyScore { get; }
    public float Stability { get; }
    public IndustryBuildingType? SuggestedBuildType { get; }
}
