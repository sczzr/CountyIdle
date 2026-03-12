using System;
using System.Collections.Generic;
using CountyIdle.Models;
using Godot;

namespace CountyIdle.Systems;

public sealed class WorldSiteLocalMapGeneratorSystem
{
    public TownMapData GenerateSandboxMap(XianxiaSiteData site, XianxiaHexCellData? sourceCell)
    {
        var localMap = Generate(site, sourceCell);
        var townMap = new TownMapData(localMap.Width, localMap.Height);
        var coreCell = new Vector2I(localMap.Width / 2, localMap.Height / 2);
        var entryCell = new Vector2I(0, localMap.Height / 2);

        foreach (var tile in localMap.Tiles)
        {
            townMap.SetTerrain(tile.Cell.X, tile.Cell.Y, ResolveTownTerrain(tile.TileType));
            townMap.SetCellCompound(CreateCompound(site, sourceCell, tile, coreCell));
        }

        if (site.PrimaryType != "Ruin")
        {
            townMap.AddActivityAnchor(new TownActivityAnchorData(
                ResolveAnchorType(site.PrimaryType),
                entryCell,
                coreCell,
                TownFacing.East,
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

        PaintCoreArea(site, sourceCell, tiles, coreCell);
        PaintApproachPath(site, sourceCell, tiles, coreCell, random);
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

        if (sourceCell?.Water != XianxiaWaterType.None && (cell.X <= 1 || (cell.Y >= coreCell.Y + 2 && cell.X >= coreCell.X + 2)))
        {
            return WorldSiteLocalTileType.Water;
        }

        if (sourceCell != null &&
            (sourceCell.Biome is XianxiaBiomeType.MistyMountains or XianxiaBiomeType.JadeHighlands or XianxiaBiomeType.SnowPeaks ||
             sourceCell.Height >= 72) &&
            (cell.Y <= 1 || (cell.X >= coreCell.X + 2 && cell.Y <= coreCell.Y)))
        {
            return WorldSiteLocalTileType.Ridge;
        }

        if (sourceCell != null &&
            sourceCell.Biome is XianxiaBiomeType.BambooValley or XianxiaBiomeType.SacredForest or XianxiaBiomeType.SpiritSwamps &&
            random.NextDouble() < 0.28d)
        {
            return WorldSiteLocalTileType.Forest;
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

        var entryY = Math.Clamp(coreCell.Y + random.Next(-1, 2), 1, coreCell.Y + 1);
        for (var x = 0; x <= coreCell.X; x++)
        {
            var cell = new Vector2I(x, entryY);
            if (!tiles.TryGetValue(cell, out var current) || current == WorldSiteLocalTileType.Water)
            {
                continue;
            }

            tiles[cell] = current is WorldSiteLocalTileType.Settlement or WorldSiteLocalTileType.Spirit
                ? current
                : WorldSiteLocalTileType.Path;
        }
    }

    private static void PaintRegionalFeatures(
        XianxiaSiteData site,
        XianxiaHexCellData? sourceCell,
        Dictionary<Vector2I, WorldSiteLocalTileType> tiles,
        Vector2I coreCell,
        Random random)
    {
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
            : $"基于 {sourceCell.Biome} / {sourceCell.Terrain} / {sourceCell.Water} 的 world hex 语义生成。";
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
