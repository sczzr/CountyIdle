using System;
using System.Collections.Generic;
using CountyIdle.Models;
using Godot;

namespace CountyIdle.Systems;

public sealed class WorldSiteLocalMapGeneratorSystem
{
    private static readonly HexDirectionMask[] MaskIterationOrder =
    [
        HexDirectionMask.East,
        HexDirectionMask.NorthEast,
        HexDirectionMask.NorthWest,
        HexDirectionMask.West,
        HexDirectionMask.SouthWest,
        HexDirectionMask.SouthEast
    ];

    private static readonly HexDirectionMask[] EntryPreferenceOrder =
    [
        HexDirectionMask.West,
        HexDirectionMask.NorthWest,
        HexDirectionMask.SouthWest,
        HexDirectionMask.East,
        HexDirectionMask.NorthEast,
        HexDirectionMask.SouthEast
    ];

    public TownMapData GenerateSandboxMap(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        var localMap = Generate(site, sourceCell);
        var townMap = new TownMapData(localMap.Width, localMap.Height);
        var coreCell = new Vector2I(localMap.Width / 2, localMap.Height / 2);
        var entryDirection = ResolvePrimaryEntryDirection(site, sourceCell);
        var entryCell = ResolveBoundaryCell(entryDirection, localMap.Width, localMap.Height, coreCell);
        var entryFacing = ResolveFacingTowardCore(entryDirection);

        foreach (var tile in localMap.Tiles)
        {
            townMap.SetTerrain(tile.Cell.X, tile.Cell.Y, ResolveTownTerrain(tile.TileType));
            townMap.SetTerrainVisualFamily(tile.Cell.X, tile.Cell.Y, ResolveTerrainVisualFamily(sourceCell, tile.TileType));
            townMap.SetCellCompound(CreateCompound(site, sourceCell, tile, coreCell));
        }

        if (site.PrimaryType != "Ruin")
        {
            townMap.AddActivityAnchor(new TownActivityAnchorData(
                ResolveAnchorType(site.PrimaryType),
                entryCell,
                coreCell,
                entryFacing,
                site.PrimaryType is "Sect" or "ImmortalCity" ? 2 : 1,
                0,
                ResolveAnchorLabel(site)));
        }

        if (site.PrimaryType is "Sect" or "MortalRealm" or "Market" or "CultivatorClan" or "ImmortalCity")
        {
            townMap.AddBuilding(new TownBuildingData(
                coreCell,
                TownFacing.South,
                site.PrimaryType is "Sect" or "ImmortalCity" ? 2 : 1,
                true));
        }

        return townMap;
    }

    public WorldSiteLocalMapData Generate(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        var width = ResolveWidth(site.PrimaryType);
        var height = ResolveHeight(site.PrimaryType);
        var coreCell = new Vector2I(width / 2, height / 2);
        var tiles = new Dictionary<Vector2I, WorldSiteLocalTileType>(width * height);
        var seed = Hash(site.Coord.Q, site.Coord.R, site.PrimaryType.GetHashCode() ^ site.SecondaryTag.GetHashCode());
        var random = new Random(seed);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cell = new Vector2I(x, y);
                tiles[cell] = ResolveBaseTileType(site, sourceCell, cell, coreCell, random);
            }
        }

        PaintDirectionalWaterFeatures(site, sourceCell, tiles, coreCell, width, height, random);
        PaintDirectionalRidges(site, sourceCell, tiles, coreCell, width, height, random);
        PaintCoreArea(site, sourceCell, tiles, coreCell);
        PaintApproachPath(site, sourceCell, tiles, coreCell, width, height, random);
        PaintRegionalFeatures(site, sourceCell, tiles, coreCell, random);

        var tileList = new WorldSiteLocalTileData[tiles.Count];
        var index = 0;
        foreach (var pair in tiles)
        {
            tileList[index++] = new WorldSiteLocalTileData(pair.Key, pair.Value);
        }

        return new WorldSiteLocalMapData(
            width,
            height,
            ResolveLocalMapTitle(site),
            BuildLocalMapHint(site, sourceCell),
            tileList);
    }

    private static int ResolveWidth(string primaryType)
    {
        return primaryType switch
        {
            "Sect" => 13,
            "MortalRealm" => 13,
            "Market" => 12,
            "CultivatorClan" => 12,
            "ImmortalCity" => 14,
            "Ruin" => 11,
            _ => 12
        };
    }

    private static int ResolveHeight(string primaryType)
    {
        return primaryType switch
        {
            "Sect" => 9,
            "MortalRealm" => 9,
            "Market" => 8,
            "CultivatorClan" => 8,
            "ImmortalCity" => 9,
            "Ruin" => 8,
            _ => 8
        };
    }

    private static WorldSiteLocalTileType ResolveBaseTileType(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Vector2I cell,
        Vector2I coreCell,
        Random random)
    {
        var distance = GetDistance(cell, coreCell);
        var inheritedVisualFamily = WorldTerrainVisualRules.ResolveWorldVisualFamily(sourceCell);

        if (sourceCell != null &&
            sourceCell.Biome is XianxiaBiomeType.BambooValley or XianxiaBiomeType.SacredForest or XianxiaBiomeType.SpiritSwamps &&
            random.NextDouble() < 0.28d)
        {
            return WorldSiteLocalTileType.Forest;
        }

        if (sourceCell != null &&
            inheritedVisualFamily == TownTerrainVisualFamily.Spirit &&
            distance <= 2.8f &&
            random.NextDouble() < 0.18d)
        {
            return WorldSiteLocalTileType.Spirit;
        }

        if (sourceCell != null &&
            inheritedVisualFamily == TownTerrainVisualFamily.Rugged &&
            distance > 2.7f &&
            random.NextDouble() < 0.08d)
        {
            return site.PrimaryType == "Ruin"
                ? WorldSiteLocalTileType.Hazard
                : WorldSiteLocalTileType.Ridge;
        }

        if (site.PrimaryType == "Wilderness" && distance > 2.6f && random.NextDouble() < 0.12d)
        {
            return WorldSiteLocalTileType.Hazard;
        }

        return WorldSiteLocalTileType.Ground;
    }

    private static void PaintCoreArea(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I coreCell)
    {
        foreach (var pair in tiles)
        {
            if (GetDistance(pair.Key, coreCell) > 1.45f)
            {
                continue;
            }

            tiles[pair.Key] = site.PrimaryType switch
            {
                "Sect" or "MortalRealm" or "Market" or "CultivatorClan" or "ImmortalCity" => WorldSiteLocalTileType.Settlement,
                "Ruin" => WorldSiteLocalTileType.Ruin,
                _ when sourceCell?.QiDensity > 0.72f => WorldSiteLocalTileType.Spirit,
                _ => WorldSiteLocalTileType.Ground
            };
        }

        if (sourceCell?.QiDensity > 0.74f)
        {
            tiles[coreCell] = WorldSiteLocalTileType.Spirit;
        }
    }

    private static void PaintApproachPath(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I coreCell,
        int width,
        int height,
        Random random)
    {
        var shouldPaintPath =
            site.PrimaryType != "Ruin" &&
            (sourceCell?.RoadMask != HexDirectionMask.None ||
             site.PrimaryType is "Sect" or "MortalRealm" or "Market" or "CultivatorClan" or "ImmortalCity");

        if (!shouldPaintPath)
        {
            return;
        }

        var approachDirections = ExtractDirections(sourceCell?.RoadMask ?? HexDirectionMask.None);
        if (approachDirections.Count == 0)
        {
            approachDirections.Add(ResolvePrimaryEntryDirection(site, sourceCell));
        }

        foreach (var direction in approachDirections)
        {
            var boundaryCell = ResolveBoundaryCell(direction, width, height, coreCell);
            var pathTarget = ResolvePathTarget(direction, width, height, coreCell, random);
            PaintFeatureLine(tiles, boundaryCell, pathTarget, WorldSiteLocalTileType.Path, 0, CanOverrideWithPath);
        }
    }

    private static void PaintDirectionalWaterFeatures(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I coreCell,
        int width,
        int height,
        Random random)
    {
        if (sourceCell == null || (sourceCell.Water == XianxiaWaterType.None && sourceCell.RiverMask == HexDirectionMask.None))
        {
            return;
        }

        var waterDirections = ExtractDirections(sourceCell.RiverMask);
        if (waterDirections.Count == 0)
        {
            waterDirections.Add(ResolveStableFallbackDirection(sourceCell, 41));
        }

        var basinCenters = new List<Vector2I>();
        foreach (var direction in waterDirections)
        {
            var boundaryCell = ResolveBoundaryCell(direction, width, height, coreCell);
            var waterFocus = ResolveWaterFocus(direction, width, height, coreCell, random);
            PaintFeatureLine(tiles, boundaryCell, waterFocus, WorldSiteLocalTileType.Water, sourceCell.Water != XianxiaWaterType.None ? 1 : 0);
            basinCenters.Add(waterFocus);
        }

        if (sourceCell.Water != XianxiaWaterType.None)
        {
            foreach (var basin in basinCenters)
            {
                PaintFeatureBlob(tiles, basin, 1, WorldSiteLocalTileType.Water);
            }

            if (basinCenters.Count >= 2)
            {
                var averageX = 0f;
                var averageY = 0f;
                foreach (var basin in basinCenters)
                {
                    averageX += basin.X;
                    averageY += basin.Y;
                }

                var pooledCenter = new Vector2I(
                    Mathf.RoundToInt(averageX / basinCenters.Count),
                    Mathf.RoundToInt(averageY / basinCenters.Count));
                PaintFeatureBlob(tiles, pooledCenter, site.PrimaryType == "Wilderness" ? 2 : 1, WorldSiteLocalTileType.Water);
            }
        }
    }

    private static void PaintDirectionalRidges(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I coreCell,
        int width,
        int height,
        Random random)
    {
        if (sourceCell == null)
        {
            return;
        }

        var shouldPaintRidges =
            sourceCell.CliffMask != HexDirectionMask.None ||
            sourceCell.Biome is XianxiaBiomeType.MistyMountains or XianxiaBiomeType.JadeHighlands or XianxiaBiomeType.SnowPeaks ||
            sourceCell.Height >= 72 ||
            WorldTerrainVisualRules.ResolveWorldVisualFamily(sourceCell) is TownTerrainVisualFamily.Rugged or TownTerrainVisualFamily.Snow;
        if (!shouldPaintRidges)
        {
            return;
        }

        var ridgeDirections = ExtractDirections(sourceCell.CliffMask);
        if (ridgeDirections.Count == 0)
        {
            ridgeDirections.Add(ResolveStableFallbackDirection(sourceCell, 67));
        }

        var ridgeTileType = site.PrimaryType == "Ruin" && (sourceCell.Corruption > 0.58f || sourceCell.MonsterThreat > 0.56f)
            ? WorldSiteLocalTileType.Hazard
            : WorldSiteLocalTileType.Ridge;
        foreach (var direction in ridgeDirections)
        {
            var boundaryCell = ResolveBoundaryCell(direction, width, height, coreCell);
            var ridgeFocus = ResolveRidgeFocus(direction, width, height, coreCell, random);
            PaintFeatureLine(tiles, boundaryCell, ridgeFocus, ridgeTileType, 1);
            PaintFeatureBlob(tiles, ridgeFocus, 1, ridgeTileType);
        }
    }

    private static void PaintRegionalFeatures(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I coreCell,
        Random random)
    {
        var inheritedVisualFamily = WorldTerrainVisualRules.ResolveWorldVisualFamily(sourceCell);
        foreach (var pair in tiles)
        {
            if (pair.Value != WorldSiteLocalTileType.Ground)
            {
                continue;
            }

            if (site.PrimaryType == "Ruin" && (sourceCell?.Corruption > 0.56f || sourceCell?.MonsterThreat > 0.56f) && random.NextDouble() < 0.22d)
            {
                tiles[pair.Key] = WorldSiteLocalTileType.Hazard;
                continue;
            }

            if (sourceCell != null &&
                inheritedVisualFamily == TownTerrainVisualFamily.Rugged &&
                random.NextDouble() < 0.15d)
            {
                tiles[pair.Key] = site.PrimaryType == "Ruin"
                    ? WorldSiteLocalTileType.Hazard
                    : WorldSiteLocalTileType.Ridge;
                continue;
            }

            if (sourceCell != null &&
                inheritedVisualFamily == TownTerrainVisualFamily.Snow &&
                random.NextDouble() < 0.16d)
            {
                tiles[pair.Key] = WorldSiteLocalTileType.Ridge;
                continue;
            }

            if (site.PrimaryType == "Wilderness" && sourceCell?.QiDensity > 0.68f && GetDistance(pair.Key, coreCell) <= 2.2f && random.NextDouble() < 0.18d)
            {
                tiles[pair.Key] = WorldSiteLocalTileType.Spirit;
                continue;
            }

            if (sourceCell != null &&
                sourceCell.Biome is XianxiaBiomeType.BambooValley or XianxiaBiomeType.SacredForest &&
                random.NextDouble() < 0.22d)
            {
                tiles[pair.Key] = WorldSiteLocalTileType.Forest;
                continue;
            }

            if (sourceCell != null &&
                inheritedVisualFamily == TownTerrainVisualFamily.Spirit &&
                GetDistance(pair.Key, coreCell) <= 2.5f &&
                random.NextDouble() < 0.16d)
            {
                tiles[pair.Key] = WorldSiteLocalTileType.Spirit;
            }
        }
    }

    private static string ResolveLocalMapTitle(XianxiaSiteData site)
    {
        return site.PrimaryType switch
        {
            "Sect" => "山门局部地势图",
            "MortalRealm" => "凡俗据点地势图",
            "Market" => "坊市局部流转图",
            "Wilderness" => "野外局部踏勘图",
            "CultivatorClan" => "世家局部地势图",
            "ImmortalCity" => "仙城局部地势图",
            "Ruin" => "遗迹局部踏勘图",
            _ => "二级地图局部地势图"
        };
    }

    private static string BuildLocalMapHint(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        var terrainHint = sourceCell == null
            ? "当前使用点位基础语义生成。"
            : $"基于 {sourceCell.Biome} / {sourceCell.Terrain} / {sourceCell.Water} 的 world hex 语义生成，并继承道路 / 水体 / 高差方向。";
        var focusHint = site.PrimaryType switch
        {
            "Wilderness" => "当前地图偏向探路、采集与遭遇事件。",
            "Ruin" => "当前地图偏向试炼、破阵与高风险探索。",
            "Market" => "当前地图偏向流转节点与短期机会。",
            "Sect" => "当前地图偏向山门访问与驻点往来。",
            _ => "当前地图用于承接该格的下一层玩法。"
        };
        return $"{terrainHint}{focusHint}";
    }

    private static TownTerrainType ResolveTownTerrain(WorldSiteLocalTileType tileType)
    {
        return tileType switch
        {
            WorldSiteLocalTileType.Path => TownTerrainType.Road,
            WorldSiteLocalTileType.Water => TownTerrainType.Water,
            WorldSiteLocalTileType.Settlement or WorldSiteLocalTileType.Ruin or WorldSiteLocalTileType.Spirit => TownTerrainType.Courtyard,
            _ => TownTerrainType.Ground
        };
    }

    private static TownTerrainVisualFamily ResolveTerrainVisualFamily(
        XianxiaHexCellData? sourceCell,
        WorldSiteLocalTileType tileType)
    {
        return WorldTerrainVisualRules.ResolveSecondaryMapVisualFamily(sourceCell, tileType);
    }

    private static List<HexDirectionMask> ExtractDirections(HexDirectionMask mask)
    {
        var directions = new List<HexDirectionMask>(3);
        foreach (var direction in MaskIterationOrder)
        {
            if ((mask & direction) != HexDirectionMask.None)
            {
                directions.Add(direction);
            }
        }

        return directions;
    }

    private static HexDirectionMask ResolvePrimaryEntryDirection(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        if (sourceCell != null && sourceCell.RoadMask != HexDirectionMask.None)
        {
            return ResolveDominantDirection(sourceCell.RoadMask);
        }

        if (sourceCell != null && sourceCell.RiverMask != HexDirectionMask.None)
        {
            return Opposite(ResolveDominantDirection(sourceCell.RiverMask));
        }

        if (sourceCell?.Water != XianxiaWaterType.None)
        {
            return Opposite(ResolveStableFallbackDirection(sourceCell, 97));
        }

        if (sourceCell != null && sourceCell.CliffMask != HexDirectionMask.None)
        {
            return Opposite(ResolveDominantDirection(sourceCell.CliffMask));
        }

        return site.PrimaryType switch
        {
            "ImmortalCity" => HexDirectionMask.East,
            "Market" => HexDirectionMask.West,
            _ => HexDirectionMask.West
        };
    }

    private static HexDirectionMask ResolveDominantDirection(HexDirectionMask mask)
    {
        foreach (var direction in EntryPreferenceOrder)
        {
            if ((mask & direction) != HexDirectionMask.None)
            {
                return direction;
            }
        }

        return HexDirectionMask.West;
    }

    private static HexDirectionMask ResolveStableFallbackDirection(XianxiaHexCellData sourceCell, int salt)
    {
        var directions = new[]
        {
            HexDirectionMask.West,
            HexDirectionMask.NorthWest,
            HexDirectionMask.NorthEast,
            HexDirectionMask.East,
            HexDirectionMask.SouthEast,
            HexDirectionMask.SouthWest
        };
        var index = Hash(sourceCell.Coord.Q, sourceCell.Coord.R, salt) % directions.Length;
        return directions[index];
    }

    private static Vector2I ResolveBoundaryCell(HexDirectionMask direction, int width, int height, Vector2I coreCell)
    {
        var horizontalInset = Math.Max(width / 4, 1);
        return direction switch
        {
            HexDirectionMask.East => new Vector2I(width - 1, coreCell.Y),
            HexDirectionMask.NorthEast => new Vector2I(Math.Clamp(coreCell.X + horizontalInset, 0, width - 1), 0),
            HexDirectionMask.NorthWest => new Vector2I(Math.Clamp(coreCell.X - horizontalInset, 0, width - 1), 0),
            HexDirectionMask.West => new Vector2I(0, coreCell.Y),
            HexDirectionMask.SouthWest => new Vector2I(Math.Clamp(coreCell.X - horizontalInset, 0, width - 1), height - 1),
            HexDirectionMask.SouthEast => new Vector2I(Math.Clamp(coreCell.X + horizontalInset, 0, width - 1), height - 1),
            _ => new Vector2I(0, coreCell.Y)
        };
    }

    private static Vector2I ResolvePathTarget(
        HexDirectionMask direction,
        int width,
        int height,
        Vector2I coreCell,
        Random random)
    {
        var offset = direction switch
        {
            HexDirectionMask.East => new Vector2I(2, random.Next(-1, 2)),
            HexDirectionMask.NorthEast => new Vector2I(1, -2),
            HexDirectionMask.NorthWest => new Vector2I(-1, -2),
            HexDirectionMask.West => new Vector2I(-2, random.Next(-1, 2)),
            HexDirectionMask.SouthWest => new Vector2I(-1, 2),
            HexDirectionMask.SouthEast => new Vector2I(1, 2),
            _ => Vector2I.Zero
        };

        return new Vector2I(
            Math.Clamp(coreCell.X + offset.X, 0, width - 1),
            Math.Clamp(coreCell.Y + offset.Y, 0, height - 1));
    }

    private static Vector2I ResolveWaterFocus(
        HexDirectionMask direction,
        int width,
        int height,
        Vector2I coreCell,
        Random random)
    {
        var offset = direction switch
        {
            HexDirectionMask.East => new Vector2I(2, random.Next(-1, 2)),
            HexDirectionMask.NorthEast => new Vector2I(2, -2),
            HexDirectionMask.NorthWest => new Vector2I(-2, -2),
            HexDirectionMask.West => new Vector2I(-2, random.Next(-1, 2)),
            HexDirectionMask.SouthWest => new Vector2I(-2, 2),
            HexDirectionMask.SouthEast => new Vector2I(2, 2),
            _ => Vector2I.Zero
        };

        return new Vector2I(
            Math.Clamp(coreCell.X + offset.X, 0, width - 1),
            Math.Clamp(coreCell.Y + offset.Y, 0, height - 1));
    }

    private static Vector2I ResolveRidgeFocus(
        HexDirectionMask direction,
        int width,
        int height,
        Vector2I coreCell,
        Random random)
    {
        var offset = direction switch
        {
            HexDirectionMask.East => new Vector2I(3, random.Next(-1, 2)),
            HexDirectionMask.NorthEast => new Vector2I(2, -3),
            HexDirectionMask.NorthWest => new Vector2I(-2, -3),
            HexDirectionMask.West => new Vector2I(-3, random.Next(-1, 2)),
            HexDirectionMask.SouthWest => new Vector2I(-2, 3),
            HexDirectionMask.SouthEast => new Vector2I(2, 3),
            _ => Vector2I.Zero
        };

        return new Vector2I(
            Math.Clamp(coreCell.X + offset.X, 0, width - 1),
            Math.Clamp(coreCell.Y + offset.Y, 0, height - 1));
    }

    private static TownFacing ResolveFacingTowardCore(HexDirectionMask direction)
    {
        return direction switch
        {
            HexDirectionMask.East => TownFacing.West,
            HexDirectionMask.NorthEast or HexDirectionMask.NorthWest => TownFacing.South,
            HexDirectionMask.SouthWest or HexDirectionMask.SouthEast => TownFacing.North,
            _ => TownFacing.East
        };
    }

    private static HexDirectionMask Opposite(HexDirectionMask mask)
    {
        return mask switch
        {
            HexDirectionMask.East => HexDirectionMask.West,
            HexDirectionMask.NorthEast => HexDirectionMask.SouthWest,
            HexDirectionMask.NorthWest => HexDirectionMask.SouthEast,
            HexDirectionMask.West => HexDirectionMask.East,
            HexDirectionMask.SouthWest => HexDirectionMask.NorthEast,
            HexDirectionMask.SouthEast => HexDirectionMask.NorthWest,
            _ => HexDirectionMask.None
        };
    }

    private static void PaintFeatureLine(
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I start,
        Vector2I end,
        WorldSiteLocalTileType tileType,
        int radius = 0,
        Func<WorldSiteLocalTileType, bool>? canOverride = null)
    {
        var steps = Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
        if (steps <= 0)
        {
            PaintFeatureBlob(tiles, start, radius, tileType, canOverride);
            return;
        }

        for (var index = 0; index <= steps; index++)
        {
            var t = index / (float)steps;
            var cell = new Vector2I(
                Mathf.RoundToInt(Mathf.Lerp(start.X, end.X, t)),
                Mathf.RoundToInt(Mathf.Lerp(start.Y, end.Y, t)));
            PaintFeatureBlob(tiles, cell, radius, tileType, canOverride);
        }
    }

    private static void PaintFeatureBlob(
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I center,
        int radius,
        WorldSiteLocalTileType tileType,
        Func<WorldSiteLocalTileType, bool>? canOverride = null)
    {
        var effectiveRadius = Math.Max(radius, 0);
        for (var y = center.Y - effectiveRadius; y <= center.Y + effectiveRadius; y++)
        {
            for (var x = center.X - effectiveRadius; x <= center.X + effectiveRadius; x++)
            {
                var cell = new Vector2I(x, y);
                if (!tiles.TryGetValue(cell, out var current))
                {
                    continue;
                }

                if (effectiveRadius > 0 && GetDistance(cell, center) > effectiveRadius + 0.15f)
                {
                    continue;
                }

                if (canOverride != null && !canOverride(current))
                {
                    continue;
                }

                tiles[cell] = tileType;
            }
        }
    }

    private static bool CanOverrideWithPath(WorldSiteLocalTileType current)
    {
        return current != WorldSiteLocalTileType.Water;
    }

    private static TownCellCompoundData CreateCompound(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        WorldSiteLocalTileData tile,
        Vector2I coreCell)
    {
        var contentKind = ResolveContentKind(site.PrimaryType, tile.TileType);
        var suggestedBuildType = ResolveSuggestedBuildType(site.PrimaryType, tile.TileType);
        var featureTexts = BuildFeatureTexts(site, sourceCell, tile.TileType);
        var subBuildings = BuildSubBuildings(site, tile.TileType);
        var totalQiDemand = 0f;
        foreach (var plan in subBuildings)
        {
            totalQiDemand += plan.QiDemand;
        }

        var baseQiCapacity = ResolveBaseQiCapacity(site, sourceCell, tile.Cell, coreCell);
        var qiCongestion = baseQiCapacity <= 0 || totalQiDemand <= baseQiCapacity
            ? 0f
            : (totalQiDemand - baseQiCapacity) / baseQiCapacity;
        var synergyScore = ResolveSynergyScore(site, tile.TileType);
        var stability = ResolveStability(site, sourceCell, tile.TileType);

        return new TownCellCompoundData(
            tile.Cell,
            site.Label,
            contentKind,
            TownCompoundPlanStyle.Natural,
            ResolveQiAffinityText(sourceCell),
            baseQiCapacity,
            ResolveQiRecoveryPerHour(site, sourceCell),
            Math.Max(subBuildings.Length, 1),
            featureTexts,
            subBuildings,
            totalQiDemand,
            qiCongestion,
            synergyScore,
            stability,
            suggestedBuildType);
    }

    private static TownCellContentKind ResolveContentKind(string primaryType, WorldSiteLocalTileType tileType)
    {
        return tileType switch
        {
            WorldSiteLocalTileType.Path => TownCellContentKind.Infrastructure,
            WorldSiteLocalTileType.Ruin or WorldSiteLocalTileType.Hazard => TownCellContentKind.Special,
            WorldSiteLocalTileType.Settlement => primaryType switch
            {
                "Market" => TownCellContentKind.Service,
                "CultivatorClan" => TownCellContentKind.Residence,
                "MortalRealm" => TownCellContentKind.Residence,
                _ => TownCellContentKind.Service
            },
            WorldSiteLocalTileType.Spirit => TownCellContentKind.Special,
            _ => primaryType switch
            {
                "Wilderness" => TownCellContentKind.Production,
                "Ruin" => TownCellContentKind.Special,
                _ => TownCellContentKind.Production
            }
        };
    }

    private static IndustryBuildingType? ResolveSuggestedBuildType(string primaryType, WorldSiteLocalTileType tileType)
    {
        return primaryType switch
        {
            "Sect" => IndustryBuildingType.Research,
            "MortalRealm" => IndustryBuildingType.Agriculture,
            "Market" => IndustryBuildingType.Trade,
            "CultivatorClan" => tileType == WorldSiteLocalTileType.Settlement ? IndustryBuildingType.Administration : IndustryBuildingType.Workshop,
            "ImmortalCity" => IndustryBuildingType.Trade,
            "Wilderness" => IndustryBuildingType.Agriculture,
            _ => null
        };
    }

    private static string[] BuildFeatureTexts(XianxiaSiteData site, XianxiaHexCellData? sourceCell, WorldSiteLocalTileType tileType)
    {
        var features = new List<string>();
        if (sourceCell != null)
        {
            features.Add(sourceCell.Biome.ToString());
            if (sourceCell.Water != XianxiaWaterType.None)
            {
                features.Add("近水");
            }

            if (sourceCell.QiDensity > 0.68f)
            {
                features.Add("灵气偏盛");
            }

            if (sourceCell.MonsterThreat > 0.60f)
            {
                features.Add("妖患逼近");
            }
        }

        features.Add(tileType switch
        {
            WorldSiteLocalTileType.Path => "道路节点",
            WorldSiteLocalTileType.Settlement => "局部聚落",
            WorldSiteLocalTileType.Ruin => "残构核心",
            WorldSiteLocalTileType.Spirit => "灵脉节点",
            WorldSiteLocalTileType.Hazard => "险情地带",
            WorldSiteLocalTileType.Forest => "林地遮蔽",
            WorldSiteLocalTileType.Ridge => "高差地势",
            WorldSiteLocalTileType.Water => "水域边界",
            _ => "外域腹地"
        });
        return features.ToArray();
    }

    private static TownSubBuildingPlan[] BuildSubBuildings(XianxiaSiteData site, WorldSiteLocalTileType tileType)
    {
        return (site.PrimaryType, tileType) switch
        {
            (_, WorldSiteLocalTileType.Path) =>
            [
                new TownSubBuildingPlan("outer_route", "行路点", 4f, 1, ["traffic"], ["erosion"])
            ],
            ("Sect", _) =>
            [
                new TownSubBuildingPlan("outer_hall", "外院", 14f, 3, ["research", "quiet"], ["crowded"]),
                new TownSubBuildingPlan("rest_pavilion", "驻留亭", 8f, 1, ["recovery", "quiet"], ["crowded"])
            ],
            ("MortalRealm", _) =>
            [
                new TownSubBuildingPlan("hamlet", "乡坊", 10f, 3, ["food", "rest"], ["crowded"]),
                new TownSubBuildingPlan("field_shed", "田棚", 8f, 2, ["food", "storage"], ["crowded"])
            ],
            ("Market", _) =>
            [
                new TownSubBuildingPlan("trade_stall", "行商摊", 10f, 2, ["trade", "traffic"], ["crowded"]),
                new TownSubBuildingPlan("relay_shed", "转运棚", 8f, 2, ["storage", "traffic"], ["crowded"])
            ],
            ("CultivatorClan", _) =>
            [
                new TownSubBuildingPlan("guest_hall", "会客院", 12f, 2, ["governance", "quiet"], ["crowded"]),
                new TownSubBuildingPlan("lineage_store", "族藏阁", 10f, 2, ["storage", "stability"], ["crowded"])
            ],
            ("ImmortalCity", _) =>
            [
                new TownSubBuildingPlan("city_exchange", "交割亭", 12f, 3, ["trade", "traffic"], ["crowded"]),
                new TownSubBuildingPlan("supply_stack", "补给仓", 10f, 2, ["storage", "traffic"], ["crowded"])
            ],
            ("Ruin", _) =>
            [
                new TownSubBuildingPlan("sealed_gate", "封阵口", 16f, 2, ["threat_control", "stability"], ["fire_restless"]),
                new TownSubBuildingPlan("ruin_core", "残殿核", 18f, 3, ["threat_control"], ["crowded"])
            ],
            _ =>
            [
                new TownSubBuildingPlan("wild_camp", "探路营", 9f, 2, ["recovery", "safety"], ["isolated"]),
                new TownSubBuildingPlan("gather_node", "采集点", 8f, 2, ["food", "storage"], ["crowded"])
            ]
        };
    }

    private static int ResolveBaseQiCapacity(XianxiaSiteData site, XianxiaHexCellData? sourceCell, Vector2I cell, Vector2I coreCell)
    {
        var baseValue = 72 + Math.Max((int)Math.Round((sourceCell?.QiDensity ?? 0.42f) * 64f), 0);
        var distancePenalty = (int)Math.Round(GetDistance(cell, coreCell) * 6f);
        return Math.Max(baseValue - distancePenalty, 36);
    }

    private static int ResolveQiRecoveryPerHour(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        return site.PrimaryType switch
        {
            "Sect" => 8,
            "Ruin" => 4,
            _ => 5 + Math.Max((int)Math.Round((sourceCell?.QiDensity ?? 0.40f) * 4f), 0)
        };
    }

    private static float ResolveSynergyScore(XianxiaSiteData site, WorldSiteLocalTileType tileType)
    {
        return (site.PrimaryType, tileType) switch
        {
            ("Sect", WorldSiteLocalTileType.Settlement) => 0.18f,
            ("Market", WorldSiteLocalTileType.Settlement) => 0.16f,
            ("Ruin", _) => -0.04f,
            ("Wilderness", WorldSiteLocalTileType.Hazard) => -0.06f,
            _ => 0.08f
        };
    }

    private static float ResolveStability(XianxiaSiteData site, XianxiaHexCellData? sourceCell, WorldSiteLocalTileType tileType)
    {
        var value = site.PrimaryType switch
        {
            "Sect" => 1.06f,
            "MortalRealm" => 1.02f,
            "Market" => 0.98f,
            "CultivatorClan" => 1.00f,
            "ImmortalCity" => 1.04f,
            "Ruin" => 0.72f,
            _ => 0.88f
        };

        if (tileType == WorldSiteLocalTileType.Hazard)
        {
            value -= 0.16f;
        }

        if (sourceCell != null)
        {
            value -= sourceCell.MonsterThreat * 0.10f;
            value += sourceCell.QiDensity * 0.06f;
        }

        return Math.Clamp(value, 0.48f, 1.18f);
    }

    private static string ResolveQiAffinityText(XianxiaHexCellData? sourceCell)
    {
        return sourceCell?.ElementAffinity switch
        {
            XianxiaElementType.Wood => "木旺地脉",
            XianxiaElementType.Fire => "火炽地脉",
            XianxiaElementType.Earth => "土稳地脉",
            XianxiaElementType.Metal => "金锐地脉",
            XianxiaElementType.Water => "水盛地脉",
            XianxiaElementType.Yin => "阴灵地脉",
            XianxiaElementType.Yang => "阳炽地脉",
            XianxiaElementType.Chaos => "乱流地脉",
            _ => "地脉平稳"
        };
    }

    private static TownActivityAnchorType ResolveAnchorType(string primaryType)
    {
        return primaryType switch
        {
            "Sect" => TownActivityAnchorType.Academy,
            "MortalRealm" => TownActivityAnchorType.Farmstead,
            "Market" => TownActivityAnchorType.Market,
            "CultivatorClan" => TownActivityAnchorType.Administration,
            "ImmortalCity" => TownActivityAnchorType.Market,
            "Ruin" => TownActivityAnchorType.Leisure,
            _ => TownActivityAnchorType.Leisure
        };
    }

    private static string ResolveAnchorLabel(XianxiaSiteData site)
    {
        return site.PrimaryType switch
        {
            "Sect" => "外院据点",
            "MortalRealm" => "附庸据点",
            "Market" => "流转坊口",
            "CultivatorClan" => "世家接引处",
            "ImmortalCity" => "驿城接引处",
            "Ruin" => "遗迹入口",
            _ => "野外营地"
        };
    }

    private static int Hash(int q, int r, int salt)
    {
        unchecked
        {
            var hash = (q * 73856093) ^ (r * 19349663) ^ (salt * 83492791);
            return hash & int.MaxValue;
        }
    }

    private static float GetDistance(Vector2I left, Vector2I right)
    {
        var delta = left - right;
        return Mathf.Sqrt((delta.X * delta.X) + (delta.Y * delta.Y));
    }
}
