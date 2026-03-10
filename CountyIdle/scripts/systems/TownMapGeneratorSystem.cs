using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class TownMapGeneratorSystem
{
    private const int MapWidth = 22;
    private const int MapHeight = 16;

    private static readonly Vector2I[] CardinalDirections =
    {
        Vector2I.Right,
        Vector2I.Left,
        Vector2I.Down,
        Vector2I.Up
    };

    public TownMapData Generate(int populationHint, int housingHint, int eliteHint, int layoutSeed)
    {
        var seed = layoutSeed == 0 ? 20260306 : layoutSeed;
        var random = new Random(seed);
        var map = new TownMapData(MapWidth, MapHeight);

        CarveHorizontalMainRoad(map, random);
        CarveVerticalMainRoad(map, random);
        CarveBranchRoads(map, random);
        PaintRoadShoulders(map, random);
        PaintWaterPockets(map, random);

        return map;
    }

    private static void CarveHorizontalMainRoad(TownMapData map, Random random)
    {
        var y = (map.Height / 2) + random.Next(-1, 2);

        for (var x = 1; x < map.Width - 1; x++)
        {
            map.SetTerrain(x, y, TownTerrainType.Road);

            if (random.NextDouble() < 0.12)
            {
                map.SetTerrain(x, Math.Clamp(y + 1, 1, map.Height - 2), TownTerrainType.Road);
            }

            if (random.NextDouble() < 0.26)
            {
                y = Math.Clamp(y + random.Next(-1, 2), 2, map.Height - 3);
            }
        }
    }

    private static void CarveVerticalMainRoad(TownMapData map, Random random)
    {
        var x = (map.Width / 2) + random.Next(-1, 2);

        for (var y = 1; y < map.Height - 1; y++)
        {
            map.SetTerrain(x, y, TownTerrainType.Road);

            if (random.NextDouble() < 0.11)
            {
                map.SetTerrain(Math.Clamp(x + 1, 1, map.Width - 2), y, TownTerrainType.Road);
            }

            if (random.NextDouble() < 0.24)
            {
                x = Math.Clamp(x + random.Next(-1, 2), 2, map.Width - 3);
            }
        }
    }

    private static void CarveBranchRoads(TownMapData map, Random random)
    {
        var roadCells = new List<Vector2I>(map.EnumerateCellsByTerrain(TownTerrainType.Road));
        if (roadCells.Count == 0)
        {
            return;
        }

        var branchCount = 4 + random.Next(0, 4);
        for (var i = 0; i < branchCount; i++)
        {
            var start = roadCells[random.Next(roadCells.Count)];
            var direction = CardinalDirections[random.Next(CardinalDirections.Length)];
            var cursor = start;
            var branchLength = random.Next(4, 10);

            for (var step = 0; step < branchLength; step++)
            {
                cursor += direction;
                if (!map.IsInside(cursor))
                {
                    break;
                }

                map.SetTerrain(cursor.X, cursor.Y, TownTerrainType.Road);
                if (random.NextDouble() < 0.22)
                {
                    direction = TurnPerpendicular(direction, random);
                }
            }
        }
    }

    private static Vector2I TurnPerpendicular(Vector2I direction, Random random)
    {
        if (direction.X != 0)
        {
            return random.NextDouble() < 0.5 ? Vector2I.Up : Vector2I.Down;
        }

        return random.NextDouble() < 0.5 ? Vector2I.Left : Vector2I.Right;
    }

    private static void PaintRoadShoulders(TownMapData map, Random random)
    {
        foreach (var roadCell in map.EnumerateCellsByTerrain(TownTerrainType.Road))
        {
            foreach (var direction in CardinalDirections)
            {
                var shoulder = roadCell + direction;
                if (!map.IsInside(shoulder))
                {
                    continue;
                }

                if (map.GetTerrain(shoulder.X, shoulder.Y) != TownTerrainType.Ground)
                {
                    continue;
                }

                if (random.NextDouble() < 0.16)
                {
                    map.SetTerrain(shoulder.X, shoulder.Y, TownTerrainType.Courtyard);
                }
            }
        }
    }

    private static void PaintWaterPockets(TownMapData map, Random random)
    {
        var attempts = 24;
        var pocketCount = 1 + random.Next(0, 2);
        while (pocketCount > 0 && attempts > 0)
        {
            attempts--;
            var center = new Vector2I(random.Next(2, map.Width - 2), random.Next(2, map.Height - 2));
            if (IsRoadNearby(map, center, 2))
            {
                continue;
            }

            var painted = false;
            foreach (var offset in CardinalDirections)
            {
                var target = center + offset;
                if (!map.IsInside(target))
                {
                    continue;
                }

                if (map.GetTerrain(target.X, target.Y) == TownTerrainType.Road)
                {
                    continue;
                }

                map.SetTerrain(target.X, target.Y, TownTerrainType.Water);
                painted = true;
            }

            map.SetTerrain(center.X, center.Y, TownTerrainType.Water);
            if (painted)
            {
                pocketCount--;
            }
        }
    }

    private static bool IsRoadNearby(TownMapData map, Vector2I center, int radius)
    {
        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var cell = center + new Vector2I(x, y);
                if (!map.IsInside(cell))
                {
                    continue;
                }

                if (map.GetTerrain(cell.X, cell.Y) == TownTerrainType.Road)
                {
                    return true;
                }
            }
        }

        return false;
    }

}
