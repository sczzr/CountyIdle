namespace CountyIdle.Models;

public sealed class TownBuildingPlacement
{
    public IndustryBuildingType BuildingType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public TownBuildingPlacement()
    {
    }

    public TownBuildingPlacement(IndustryBuildingType buildingType, int x, int y)
    {
        BuildingType = buildingType;
        X = x;
        Y = y;
    }
}
