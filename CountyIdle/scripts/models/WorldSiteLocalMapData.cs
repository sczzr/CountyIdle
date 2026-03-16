using Godot;

namespace CountyIdle.Models;

// 世界站点局部地图地块类型
public enum WorldSiteLocalTileType
{
    Ground, // 普通地面
    Path, // 路径
    Water, // 水域
    Forest, // 林地
    Ridge, // 山脊
    Settlement, // 聚落
    Ruin, // 遗迹
    Spirit, // 灵气
    Hazard // 危险区域
}

// 单个局部地块数据
public sealed class WorldSiteLocalTileData
{
    // 创建地块数据
    public WorldSiteLocalTileData(Vector2I cell, WorldSiteLocalTileType tileType)
    {
        Cell = cell;
        TileType = tileType;
    }

    // 地块坐标
    public Vector2I Cell { get; }
    // 地块类型
    public WorldSiteLocalTileType TileType { get; }
}

// 世界站点局部地图数据
public sealed class WorldSiteLocalMapData
{
    // 创建局部地图并保证标题/提示文本可用
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

    // 地图宽度
    public int Width { get; }
    // 地图高度
    public int Height { get; }
    // 标题文本
    public string TitleText { get; }
    // 提示文本
    public string HintText { get; }
    // 地块列表
    public WorldSiteLocalTileData[] Tiles { get; }
}
