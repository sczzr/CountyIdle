using System;
using System.Collections.Generic;
using Godot;

namespace CountyIdle.Models;

// 小镇地形逻辑类型
public enum TownTerrainType
{
    Ground, // 普通地面
    Road, // 道路
    Courtyard, // 院落
    Water // 水体
}

// 地形视觉族系（用于渲染风格分组）
public enum TownTerrainVisualFamily
{
    Auto, // 自动匹配
    Plain, // 平原
    Spirit, // 灵气地貌
    Rugged, // 粗犷地貌
    Snow, // 雪地
    ShallowWater, // 浅水
    DeepWater // 深水
}

// 建筑朝向
public enum TownFacing
{
    North, // 北
    South, // 南
    East, // 东
    West // 西
}

// 活动锚点类型（街区功能定位）
public enum TownActivityAnchorType
{
    Farmstead, // 农业
    Workshop, // 工坊
    Market, // 市集
    Academy, // 学院
    Administration, // 行政
    Leisure // 休闲
}

// 建筑基础数据
public sealed class TownBuildingData
{
    // 创建建筑数据并确保楼层数最小为 1
    public TownBuildingData(Vector2I cell, TownFacing facing, int floors, bool hasMoonGate)
    {
        Cell = cell;
        Facing = facing;
        Floors = Math.Max(floors, 1);
        HasMoonGate = hasMoonGate;
    }

    // 建筑所在格子
    public Vector2I Cell { get; }
    // 建筑朝向
    public TownFacing Facing { get; }
    // 建筑层数（最小 1）
    public int Floors { get; }
    // 是否包含月门标识
    public bool HasMoonGate { get; }
}

// 活动锚点数据，用于描述街区功能中心
public sealed class TownActivityAnchorData
{
    // 录入锚点并对楼层/变体做下限修正
    public TownActivityAnchorData(
        TownActivityAnchorType anchorType,
        Vector2I roadCell,
        Vector2I lotCell,
        TownFacing facing,
        int floors,
        int visualVariant,
        string label)
    {
        AnchorType = anchorType;
        RoadCell = roadCell;
        LotCell = lotCell;
        Facing = facing;
        Floors = Math.Max(floors, 1);
        VisualVariant = Math.Max(visualVariant, 0);
        Label = string.IsNullOrWhiteSpace(label) ? anchorType.ToString() : label;
    }

    // 锚点类型
    public TownActivityAnchorType AnchorType { get; }
    // 关联道路格
    public Vector2I RoadCell { get; }
    // 关联地块格
    public Vector2I LotCell { get; }
    // 建筑朝向
    public TownFacing Facing { get; }
    // 楼层数（最小 1）
    public int Floors { get; }
    // 视觉变体编号（最小 0）
    public int VisualVariant { get; }
    // 显示标签
    public string Label { get; }
}

// 小镇地图数据容器
public sealed class TownMapData
{
    // 地形逻辑网格
    private readonly TownTerrainType[,] _terrain;
    // 地形视觉族系网格
    private readonly TownTerrainVisualFamily[,] _terrainVisualFamilies;
    // 单元格复合信息
    private readonly Dictionary<Vector2I, TownCellCompoundData> _cellCompounds;

    // 初始化地图尺寸并填充默认地形
    public TownMapData(int width, int height)
    {
        Width = Math.Max(width, 1);
        Height = Math.Max(height, 1);
        _terrain = new TownTerrainType[Width, Height];
        _terrainVisualFamilies = new TownTerrainVisualFamily[Width, Height];
        _cellCompounds = new Dictionary<Vector2I, TownCellCompoundData>();
        Buildings = new List<TownBuildingData>();
        ActivityAnchors = new List<TownActivityAnchorData>();

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                _terrain[x, y] = TownTerrainType.Ground;
                _terrainVisualFamilies[x, y] = TownTerrainVisualFamily.Auto;
            }
        }
    }

    // 地图宽度
    public int Width { get; }
    // 地图高度
    public int Height { get; }
    // 建筑列表
    public List<TownBuildingData> Buildings { get; }
    // 活动锚点列表
    public List<TownActivityAnchorData> ActivityAnchors { get; }

    // 判断坐标是否在地图范围内
    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    // 判断格子是否在地图范围内
    public bool IsInside(Vector2I cell)
    {
        return IsInside(cell.X, cell.Y);
    }

    // 获取指定格子的地形（越界返回默认地面）
    public TownTerrainType GetTerrain(int x, int y)
    {
        return IsInside(x, y) ? _terrain[x, y] : TownTerrainType.Ground;
    }

    // 设置指定格子的地形（越界忽略）
    public void SetTerrain(int x, int y, TownTerrainType terrain)
    {
        if (!IsInside(x, y))
        {
            return;
        }

        _terrain[x, y] = terrain;
    }

    // 获取指定格子的视觉族系（越界返回自动）
    public TownTerrainVisualFamily GetTerrainVisualFamily(int x, int y)
    {
        return IsInside(x, y) ? _terrainVisualFamilies[x, y] : TownTerrainVisualFamily.Auto;
    }

    // 设置指定格子的视觉族系（越界忽略）
    public void SetTerrainVisualFamily(int x, int y, TownTerrainVisualFamily visualFamily)
    {
        if (!IsInside(x, y))
        {
            return;
        }

        _terrainVisualFamilies[x, y] = visualFamily;
    }

    // 添加建筑（越界忽略）
    public void AddBuilding(TownBuildingData building)
    {
        if (!IsInside(building.Cell))
        {
            return;
        }

        Buildings.Add(building);
    }

    // 添加活动锚点（道路/地块越界则忽略）
    public void AddActivityAnchor(TownActivityAnchorData activityAnchor)
    {
        if (!IsInside(activityAnchor.RoadCell) || !IsInside(activityAnchor.LotCell))
        {
            return;
        }

        ActivityAnchors.Add(activityAnchor);
    }

    // 记录单元格复合数据（越界忽略）
    public void SetCellCompound(TownCellCompoundData compound)
    {
        if (!IsInside(compound.Cell))
        {
            return;
        }

        _cellCompounds[compound.Cell] = compound;
    }

    // 获取单元格复合数据
    public TownCellCompoundData? GetCellCompound(Vector2I cell)
    {
        return _cellCompounds.TryGetValue(cell, out var compound) ? compound : null;
    }

    // 遍历所有格子
    public IEnumerable<Vector2I> EnumerateAllCells()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                yield return new Vector2I(x, y);
            }
        }
    }

    // 按地形筛选格子
    public IEnumerable<Vector2I> EnumerateCellsByTerrain(TownTerrainType terrain)
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (_terrain[x, y] == terrain)
                {
                    yield return new Vector2I(x, y);
                }
            }
        }
    }

    // 枚举指定类型的活动锚点
    public IEnumerable<TownActivityAnchorData> EnumerateActivityAnchors(TownActivityAnchorType anchorType)
    {
        foreach (var anchor in ActivityAnchors)
        {
            if (anchor.AnchorType == anchorType)
            {
                yield return anchor;
            }
        }
    }
}
