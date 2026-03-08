using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class TownMapGeneratorSystem
{
    private const int MapWidth = 22;
    private const int MapHeight = 16;
    private const int MinHouseCount = 18;
    private const int MaxHouseCount = 72;

    private readonly record struct ActivityAnchorPlacement(
        Vector2I RoadCell,
        Vector2I LotCell,
        TownFacing Facing);

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

        var targetHouseCount = CalculateTargetHouseCount(populationHint, housingHint, eliteHint);
        PlaceBuildings(map, random, targetHouseCount, eliteHint);
        PaintWaterPockets(map, random);
        CreateActivityAnchors(map, random, populationHint, housingHint, eliteHint);

        return map;
    }

    private static int CalculateTargetHouseCount(int populationHint, int housingHint, int eliteHint)
    {
        var target = (populationHint * 0.22) + (housingHint * 0.05) + (eliteHint * 0.6);
        return Math.Clamp((int)Math.Round(target), MinHouseCount, MaxHouseCount);
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

    private static void PlaceBuildings(TownMapData map, Random random, int targetHouseCount, int eliteHint)
    {
        var occupied = new HashSet<Vector2I>();
        var roadCells = new List<Vector2I>(map.EnumerateCellsByTerrain(TownTerrainType.Road));
        Shuffle(roadCells, random);

        foreach (var roadCell in roadCells)
        {
            var directions = new List<Vector2I>(CardinalDirections);
            Shuffle(directions, random);

            foreach (var direction in directions)
            {
                if (map.Buildings.Count >= targetHouseCount)
                {
                    return;
                }

                var lotCell = roadCell + direction;
                if (!map.IsInside(lotCell))
                {
                    continue;
                }

                if (map.GetTerrain(lotCell.X, lotCell.Y) == TownTerrainType.Road || occupied.Contains(lotCell))
                {
                    continue;
                }

                if (CountNearbyBuildings(occupied, lotCell) > 2)
                {
                    continue;
                }

                var floorChance = Math.Clamp(0.14 + (eliteHint * 0.015), 0.14, 0.45);
                var floors = random.NextDouble() < floorChance ? 2 : 1;
                var hasMoonGate = random.NextDouble() < 0.36;
                var facing = FacingTowardsRoad(direction);

                map.AddBuilding(new TownBuildingData(lotCell, facing, floors, hasMoonGate));
                occupied.Add(lotCell);

                if (random.NextDouble() < 0.44)
                {
                    var backLot = lotCell + direction;
                    if (map.IsInside(backLot) &&
                        map.GetTerrain(backLot.X, backLot.Y) == TownTerrainType.Ground &&
                        !occupied.Contains(backLot))
                    {
                        map.SetTerrain(backLot.X, backLot.Y, TownTerrainType.Courtyard);
                    }
                }
            }
        }
    }

    private static int CountNearbyBuildings(HashSet<Vector2I> occupied, Vector2I center)
    {
        var count = 0;
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                if (occupied.Contains(center + new Vector2I(x, y)))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static TownFacing FacingTowardsRoad(Vector2I roadToLotDirection)
    {
        if (roadToLotDirection == Vector2I.Right)
        {
            return TownFacing.West;
        }

        if (roadToLotDirection == Vector2I.Left)
        {
            return TownFacing.East;
        }

        if (roadToLotDirection == Vector2I.Up)
        {
            return TownFacing.South;
        }

        return TownFacing.North;
    }

    private static void PaintWaterPockets(TownMapData map, Random random)
    {
        var attempts = 24;
        var pocketCount = 1 + random.Next(0, 2);
        var buildingCells = new HashSet<Vector2I>();
        foreach (var building in map.Buildings)
        {
            buildingCells.Add(building.Cell);
        }

        while (pocketCount > 0 && attempts > 0)
        {
            attempts--;
            var center = new Vector2I(random.Next(2, map.Width - 2), random.Next(2, map.Height - 2));
            if (buildingCells.Contains(center) || IsRoadNearby(map, center, 2))
            {
                continue;
            }

            var painted = false;
            foreach (var offset in CardinalDirections)
            {
                var target = center + offset;
                if (!map.IsInside(target) || buildingCells.Contains(target))
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

    private static void CreateActivityAnchors(TownMapData map, Random random, int populationHint, int housingHint, int eliteHint)
    {
        var roadCells = new List<Vector2I>(map.EnumerateCellsByTerrain(TownTerrainType.Road));
        if (roadCells.Count == 0)
        {
            return;
        }

        var occupiedRoadCells = new HashSet<Vector2I>();
        var occupiedLotCells = new HashSet<Vector2I>();
        foreach (var building in map.Buildings)
        {
            occupiedLotCells.Add(building.Cell);
        }

        AddAnchorGroup(map, random, roadCells, occupiedRoadCells, occupiedLotCells, TownActivityAnchorType.Farmstead, SectMapSemanticRules.GetAnchorLabelPrefix(TownActivityAnchorType.Farmstead), Math.Clamp(populationHint / 120, 1, 3), eliteHint);
        AddAnchorGroup(map, random, roadCells, occupiedRoadCells, occupiedLotCells, TownActivityAnchorType.Workshop, SectMapSemanticRules.GetAnchorLabelPrefix(TownActivityAnchorType.Workshop), Math.Clamp(populationHint / 150, 1, 2), eliteHint);
        AddAnchorGroup(map, random, roadCells, occupiedRoadCells, occupiedLotCells, TownActivityAnchorType.Market, SectMapSemanticRules.GetAnchorLabelPrefix(TownActivityAnchorType.Market), Math.Clamp(populationHint / 180, 1, 2), eliteHint);
        AddAnchorGroup(map, random, roadCells, occupiedRoadCells, occupiedLotCells, TownActivityAnchorType.Academy, SectMapSemanticRules.GetAnchorLabelPrefix(TownActivityAnchorType.Academy), Math.Clamp(1 + (eliteHint / 16), 1, 2), eliteHint);
        AddAnchorGroup(map, random, roadCells, occupiedRoadCells, occupiedLotCells, TownActivityAnchorType.Administration, SectMapSemanticRules.GetAnchorLabelPrefix(TownActivityAnchorType.Administration), 1, eliteHint);
        AddAnchorGroup(map, random, roadCells, occupiedRoadCells, occupiedLotCells, TownActivityAnchorType.Leisure, SectMapSemanticRules.GetAnchorLabelPrefix(TownActivityAnchorType.Leisure), Math.Clamp(housingHint / 180, 1, 3), eliteHint);
    }

    private static void AddAnchorGroup(
        TownMapData map,
        Random random,
        List<Vector2I> roadCells,
        HashSet<Vector2I> occupiedRoadCells,
        HashSet<Vector2I> occupiedLotCells,
        TownActivityAnchorType anchorType,
        string labelPrefix,
        int count,
        int eliteHint)
    {
        for (var index = 0; index < count; index++)
        {
            var placement = SelectAnchorPlacement(map, random, roadCells, occupiedRoadCells, occupiedLotCells, anchorType, index);
            if (!placement.HasValue)
            {
                continue;
            }

            occupiedRoadCells.Add(placement.Value.RoadCell);
            occupiedLotCells.Add(placement.Value.LotCell);

            var label = count > 1 ? $"{labelPrefix}{index + 1}" : labelPrefix;
            var visualVariant = Math.Abs((placement.Value.LotCell.X * 17) + (placement.Value.LotCell.Y * 31) + index) % 3;
            var floors = GetAnchorFloors(anchorType, eliteHint, visualVariant);
            map.AddActivityAnchor(new TownActivityAnchorData(
                anchorType,
                placement.Value.RoadCell,
                placement.Value.LotCell,
                placement.Value.Facing,
                floors,
                visualVariant,
                label));
        }
    }

    private static ActivityAnchorPlacement? SelectAnchorPlacement(
        TownMapData map,
        Random random,
        List<Vector2I> roadCells,
        HashSet<Vector2I> occupiedRoadCells,
        HashSet<Vector2I> occupiedLotCells,
        TownActivityAnchorType anchorType,
        int variantIndex)
    {
        var center = new Vector2((map.Width - 1) * 0.5f, (map.Height - 1) * 0.5f);
        var maxDistance = Math.Max(center.Length(), 1f);
        ActivityAnchorPlacement? bestPlacement = null;
        var bestScore = float.MaxValue;

        foreach (var roadCell in roadCells)
        {
            if (occupiedRoadCells.Contains(roadCell))
            {
                continue;
            }

            if (IsAnchorTooClose(occupiedRoadCells, roadCell))
            {
                continue;
            }

            if (!TrySelectAnchorLot(map, roadCell, occupiedLotCells, anchorType, false, out var lotCell, out var facing))
            {
                continue;
            }

            var delta = new Vector2(roadCell.X, roadCell.Y) - center;
            var normalizedDistance = delta.Length() / maxDistance;
            var verticalBias = roadCell.Y / (float)Math.Max(map.Height - 1, 1);
            var horizontalBias = Math.Abs(roadCell.X - center.X) / Math.Max(center.X, 1f);
            var courtyardBonus = CountNearbyTerrain(map, roadCell, TownTerrainType.Courtyard, 1) * 0.06f;
            var waterBonus = CountNearbyTerrain(map, roadCell, TownTerrainType.Water, 2) * 0.08f;
            var lotDensityPenalty = CountNearbyBuildings(occupiedLotCells, lotCell) * 0.05f;
            var jitter = (float)(random.NextDouble() * 0.12) + (variantIndex * 0.03f);

            float score = anchorType switch
            {
                TownActivityAnchorType.Farmstead => (1f - normalizedDistance) * 0.65f + Math.Abs(verticalBias - 0.78f) * 0.65f + horizontalBias * 0.15f + lotDensityPenalty + jitter,
                TownActivityAnchorType.Workshop => Math.Abs(verticalBias - 0.60f) * 0.80f + normalizedDistance * 0.30f + courtyardBonus + lotDensityPenalty + jitter,
                TownActivityAnchorType.Market => normalizedDistance * 0.55f + Math.Abs(verticalBias - 0.52f) * 0.30f + horizontalBias * 0.18f + (lotDensityPenalty * 0.7f) + jitter,
                TownActivityAnchorType.Academy => Math.Abs(verticalBias - 0.24f) * 0.82f + normalizedDistance * 0.25f + courtyardBonus + lotDensityPenalty + jitter,
                TownActivityAnchorType.Administration => Math.Abs(verticalBias - 0.36f) * 0.78f + horizontalBias * 0.32f + normalizedDistance * 0.22f + lotDensityPenalty + jitter,
                TownActivityAnchorType.Leisure => normalizedDistance * 0.42f + Math.Abs(verticalBias - 0.50f) * 0.24f - waterBonus - courtyardBonus + (lotDensityPenalty * 0.65f) + jitter,
                _ => normalizedDistance + jitter
            };

            if (score < bestScore)
            {
                bestScore = score;
                bestPlacement = new ActivityAnchorPlacement(roadCell, lotCell, facing);
            }
        }

        if (bestPlacement.HasValue)
        {
            return bestPlacement;
        }

        foreach (var roadCell in roadCells)
        {
            if (occupiedRoadCells.Contains(roadCell))
            {
                continue;
            }

            if (TrySelectAnchorLot(map, roadCell, occupiedLotCells, anchorType, true, out var lotCell, out var facing))
            {
                return new ActivityAnchorPlacement(roadCell, lotCell, facing);
            }
        }

        return null;
    }

    private static bool TrySelectAnchorLot(
        TownMapData map,
        Vector2I roadCell,
        HashSet<Vector2I> occupiedLotCells,
        TownActivityAnchorType anchorType,
        bool allowDenseNeighbors,
        out Vector2I lotCell,
        out TownFacing facing)
    {
        foreach (var direction in GetPreferredLotDirections(anchorType))
        {
            var candidate = roadCell + direction;
            if (!IsAnchorLotValid(map, candidate, occupiedLotCells, allowDenseNeighbors))
            {
                continue;
            }

            lotCell = candidate;
            facing = FacingTowardsRoad(direction);
            return true;
        }

        foreach (var direction in CardinalDirections)
        {
            var candidate = roadCell + direction;
            if (!IsAnchorLotValid(map, candidate, occupiedLotCells, true))
            {
                continue;
            }

            lotCell = candidate;
            facing = FacingTowardsRoad(direction);
            return true;
        }

        lotCell = roadCell;
        facing = TownFacing.South;
        return false;
    }

    private static bool IsAnchorLotValid(TownMapData map, Vector2I lotCell, HashSet<Vector2I> occupiedLotCells, bool allowDenseNeighbors)
    {
        if (!map.IsInside(lotCell))
        {
            return false;
        }

        var terrain = map.GetTerrain(lotCell.X, lotCell.Y);
        if (terrain == TownTerrainType.Road || terrain == TownTerrainType.Water)
        {
            return false;
        }

        if (occupiedLotCells.Contains(lotCell))
        {
            return false;
        }

        if (!allowDenseNeighbors && CountNearbyBuildings(occupiedLotCells, lotCell) > 3)
        {
            return false;
        }

        return true;
    }

    private static Vector2I[] GetPreferredLotDirections(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => [Vector2I.Down, Vector2I.Left, Vector2I.Right, Vector2I.Up],
            TownActivityAnchorType.Workshop => [Vector2I.Right, Vector2I.Down, Vector2I.Left, Vector2I.Up],
            TownActivityAnchorType.Market => [Vector2I.Left, Vector2I.Right, Vector2I.Down, Vector2I.Up],
            TownActivityAnchorType.Academy => [Vector2I.Up, Vector2I.Left, Vector2I.Right, Vector2I.Down],
            TownActivityAnchorType.Administration => [Vector2I.Up, Vector2I.Right, Vector2I.Left, Vector2I.Down],
            TownActivityAnchorType.Leisure => [Vector2I.Left, Vector2I.Down, Vector2I.Right, Vector2I.Up],
            _ => CardinalDirections
        };
    }

    private static int GetAnchorFloors(TownActivityAnchorType anchorType, int eliteHint, int visualVariant)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Administration => 2,
            TownActivityAnchorType.Academy => 2,
            TownActivityAnchorType.Market when eliteHint >= 24 && visualVariant == 0 => 2,
            TownActivityAnchorType.Workshop when eliteHint >= 18 && visualVariant == 2 => 2,
            _ => 1
        };
    }

    private static bool IsAnchorTooClose(HashSet<Vector2I> occupiedRoadCells, Vector2I roadCell)
    {
        foreach (var occupiedCell in occupiedRoadCells)
        {
            var distance = Math.Abs(occupiedCell.X - roadCell.X) + Math.Abs(occupiedCell.Y - roadCell.Y);
            if (distance < 4)
            {
                return true;
            }
        }

        return false;
    }

    private static int CountNearbyTerrain(TownMapData map, Vector2I center, TownTerrainType terrainType, int radius)
    {
        var count = 0;
        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var cell = center + new Vector2I(x, y);
                if (!map.IsInside(cell))
                {
                    continue;
                }

                if (map.GetTerrain(cell.X, cell.Y) == terrainType)
                {
                    count++;
                }
            }
        }

        return count;
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

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var swapIndex = random.Next(i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
