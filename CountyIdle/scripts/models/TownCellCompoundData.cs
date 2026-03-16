using System;
using Godot;

namespace CountyIdle.Models;

/// <summary>
/// 山门地块内容类型。
/// </summary>
public enum TownCellContentKind
{
    /// <summary>
    /// 空地
    /// </summary>
    Empty,
    /// <summary>
    /// 基础设施
    /// </summary>
    Infrastructure,
    /// <summary>
    /// 生产建筑
    /// </summary>
    Production,
    /// <summary>
    /// 服务设施
    /// </summary>
    Service,
    /// <summary>
    /// 居住建筑
    /// </summary>
    Residence,
    /// <summary>
    /// 特殊建筑
    /// </summary>
    Special
}

/// <summary>
/// 院域坊局规划风格。
/// </summary>
public enum TownCompoundPlanStyle
{
    /// <summary>
    /// 自然生长
    /// </summary>
    Natural,
    /// <summary>
    /// 专精路线
    /// </summary>
    Specialized,
    /// <summary>
    /// 协同组合
    /// </summary>
    Synergy,
    /// <summary>
    /// 平衡布局
    /// </summary>
    Balanced
}

/// <summary>
/// 单个坊位子建筑规划。
/// </summary>
public sealed class TownSubBuildingPlan
{
    /// <summary>
    /// 构造坊位规划（会做基础校验）。
    /// </summary>
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

    // 模板标识
    public string TemplateId { get; }
    // 显示名
    public string DisplayName { get; }
    // 灵气需求
    public float QiDemand { get; }
    // 人力需求
    public int LaborDemand { get; }
    // 协同标签
    public string[] SynergyTags { get; }
    // 冲突标签
    public string[] ConflictTags { get; }
}

/// <summary>
/// 地块院域坊局聚合信息（用于检视器展示）。
/// </summary>
public sealed class TownCellCompoundData
{
    /// <summary>
    /// 构造地块坊局数据（会做基础校验）。
    /// </summary>
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

    // 地块坐标
    public Vector2I Cell { get; }
    // 所属区域名称
    public string RegionName { get; }
    // 内容类型
    public TownCellContentKind ContentKind { get; }
    // 规划风格
    public TownCompoundPlanStyle PlanStyle { get; }
    // 灵气亲和说明
    public string QiAffinityText { get; }
    // 基础灵气容量
    public int BaseQiCapacity { get; }
    // 每小时灵气恢复
    public int QiRecoveryPerHour { get; }
    // 可建坊位数量
    public int BuildSlotCount { get; }
    // 特征描述列表
    public string[] FeatureTexts { get; }
    // 子建筑规划
    public TownSubBuildingPlan[] SubBuildings { get; }
    // 总灵气需求
    public float TotalQiDemand { get; }
    // 灵气拥堵度
    public float QiCongestion { get; }
    // 协同评分
    public float SynergyScore { get; }
    // 稳定度
    public float Stability { get; }
    // 建议建筑类型
    public IndustryBuildingType? SuggestedBuildType { get; }
}
