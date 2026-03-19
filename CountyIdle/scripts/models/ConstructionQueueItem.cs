namespace CountyIdle.Models;

/// <summary>
/// 营建队列条目：记录建造类型、时辰进度与可退回成本。
/// </summary>
public sealed class ConstructionQueueItem
{
    /// <summary>
    /// 建筑类型。
    /// </summary>
    public IndustryBuildingType BuildingType { get; set; }

    /// <summary>
    /// 总耗时（时辰）。
    /// </summary>
    public int TotalHours { get; set; }

    /// <summary>
    /// 剩余耗时（时辰）。
    /// </summary>
    public int RemainingHours { get; set; }

    /// <summary>
    /// 木材成本（量化后的整数）。
    /// </summary>
    public int WoodCost { get; set; }

    /// <summary>
    /// 石材成本（量化后的整数）。
    /// </summary>
    public int StoneCost { get; set; }

    /// <summary>
    /// 灵石成本（量化后的整数）。
    /// </summary>
    public int GoldCost { get; set; }

    /// <summary>
    /// 贡献成本（量化后的整数）。
    /// </summary>
    public int ContributionCost { get; set; }

    /// <summary>
    /// 建材成本（量化后的整数）。
    /// </summary>
    public int ConstructionCost { get; set; }

    public ConstructionQueueItem()
    {
    }

    public ConstructionQueueItem(
        IndustryBuildingType buildingType,
        int totalHours,
        int remainingHours,
        int woodCost,
        int stoneCost,
        int goldCost,
        int contributionCost,
        int constructionCost)
    {
        BuildingType = buildingType;
        TotalHours = totalHours;
        RemainingHours = remainingHours;
        WoodCost = woodCost;
        StoneCost = stoneCost;
        GoldCost = goldCost;
        ContributionCost = contributionCost;
        ConstructionCost = constructionCost;
    }
}
