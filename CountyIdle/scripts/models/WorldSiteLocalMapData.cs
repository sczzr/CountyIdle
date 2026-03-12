using Godot;

namespace CountyIdle.Models;

public enum WorldSiteLocalTileType
{
    Ground,
    Path,
    Water,
    Forest,
    Ridge,
    Settlement,
    Ruin,
    Spirit,
    Hazard
}

public sealed class WorldSiteLocalTileData
{
    public WorldSiteLocalTileData(Vector2I cell, WorldSiteLocalTileType tileType)
    {
        Cell = cell;
        TileType = tileType;
    }

    public Vector2I Cell { get; }
    public WorldSiteLocalTileType TileType { get; }
}

public sealed class WorldSiteLocalMapData
{
    public WorldSiteLocalMapData(
        int width,
        int height,
        string titleText,
        string hintText,
        WorldSiteLocalTileData[] tiles)
    {
        Width = width;
        Height = height;
        TitleText = string.IsNullOrWhiteSpace(titleText) ? "局部地势图" : titleText;
        HintText = string.IsNullOrWhiteSpace(hintText) ? "依据当前 world hex 语义生成的下层地图。" : hintText;
        Tiles = tiles ?? [];
    }

    public int Width { get; }
    public int Height { get; }
    public string TitleText { get; }
    public string HintText { get; }
    public WorldSiteLocalTileData[] Tiles { get; }
}
