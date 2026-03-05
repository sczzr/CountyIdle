using System;
using System.Collections.Generic;
using Godot;

namespace CountyIdle.Models;

public enum TownTerrainType
{
    Ground,
    Road,
    Courtyard,
    Water
}

public enum TownFacing
{
    North,
    South,
    East,
    West
}

public sealed class TownBuildingData
{
    public TownBuildingData(Vector2I cell, TownFacing facing, int floors, bool hasMoonGate)
    {
        Cell = cell;
        Facing = facing;
        Floors = Math.Max(floors, 1);
        HasMoonGate = hasMoonGate;
    }

    public Vector2I Cell { get; }
    public TownFacing Facing { get; }
    public int Floors { get; }
    public bool HasMoonGate { get; }
}

public sealed class TownMapData
{
    private readonly TownTerrainType[,] _terrain;

    public TownMapData(int width, int height)
    {
        Width = Math.Max(width, 1);
        Height = Math.Max(height, 1);
        _terrain = new TownTerrainType[Width, Height];
        Buildings = new List<TownBuildingData>();

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                _terrain[x, y] = TownTerrainType.Ground;
            }
        }
    }

    public int Width { get; }
    public int Height { get; }
    public List<TownBuildingData> Buildings { get; }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool IsInside(Vector2I cell)
    {
        return IsInside(cell.X, cell.Y);
    }

    public TownTerrainType GetTerrain(int x, int y)
    {
        return IsInside(x, y) ? _terrain[x, y] : TownTerrainType.Ground;
    }

    public void SetTerrain(int x, int y, TownTerrainType terrain)
    {
        if (!IsInside(x, y))
        {
            return;
        }

        _terrain[x, y] = terrain;
    }

    public void AddBuilding(TownBuildingData building)
    {
        if (!IsInside(building.Cell))
        {
            return;
        }

        Buildings.Add(building);
    }

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
}
