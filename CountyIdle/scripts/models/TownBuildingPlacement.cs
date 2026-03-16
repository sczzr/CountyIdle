namespace CountyIdle.Models;

/// <summary>
/// 山门沙盘建筑落点记录。
/// </summary>
public sealed class TownBuildingPlacement
{
    // 建筑类型
    public IndustryBuildingType BuildingType { get; set; }
    // 地图格坐标 X
    public int X { get; set; }
    // 地图格坐标 Y
    public int Y { get; set; }

    /// <summary>
    /// 供序列化使用的空构造。
    /// </summary>
    public TownBuildingPlacement()
    {
    }

    /// <summary>
    /// 创建落点记录。
    /// </summary>
    public TownBuildingPlacement(IndustryBuildingType buildingType, int x, int y)
    {
        BuildingType = buildingType;
        X = x;
        Y = y;
    }
}
