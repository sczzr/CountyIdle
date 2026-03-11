using CountyIdle.Models;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CountyIdle.Systems;

public class TownMapGeneratorSystem
{
    private const int MapWidth = 22;
    private const int MapHeight = 16;

    public TownMapData Generate(int populationHint, int housingHint, int eliteHint, int layoutSeed)
    {
        var map = new TownMapData(MapWidth, MapHeight);
        var safeSeed = layoutSeed == 0 ? 20260311 : layoutSeed;

        foreach (var cell in map.EnumerateAllCells())
        {
            map.SetCellCompound(CreateCellCompound(cell, populationHint, housingHint, eliteHint, safeSeed));
        }

        PopulateTerrain(map, safeSeed);
        return map;
    }

    private static void PopulateTerrain(TownMapData map, int layoutSeed)
    {
        PaintWaterStream(map, layoutSeed);
        PaintRoadNetwork(map);
        PaintCourtyardCells(map, layoutSeed);
    }

    private static void PaintWaterStream(TownMapData map, int layoutSeed)
    {
        var baseColumn = 13 + (layoutSeed % 3);
        for (var y = 1; y < map.Height - 1; y++)
        {
            var wobble = ((layoutSeed / 17) + (y * 5)) % 3 - 1;
            var x = Math.Clamp(baseColumn + wobble + (y >= map.Height / 2 ? 1 : 0), 11, map.Width - 3);
            map.SetTerrain(x, y, TownTerrainType.Water);

            if ((y == 5 || y == 10) && map.IsInside(x + 1, y))
            {
                map.SetTerrain(x + 1, y, TownTerrainType.Water);
            }
        }
    }

    private static void PaintRoadNetwork(TownMapData map)
    {
        PaintRoadRow(map, 7, 1, map.Width - 4);
        PaintRoadRow(map, 4, 2, 8);
        PaintRoadRow(map, 11, 8, 16);
        PaintRoadColumn(map, 4, 1, 7);
        PaintRoadColumn(map, 9, 7, map.Height - 2);
        PaintRoadColumn(map, 16, 4, 12);
    }

    private static void PaintRoadRow(TownMapData map, int row, int startX, int endX)
    {
        for (var x = startX; x <= endX; x++)
        {
            if (map.GetTerrain(x, row) != TownTerrainType.Water)
            {
                map.SetTerrain(x, row, TownTerrainType.Road);
            }
        }
    }

    private static void PaintRoadColumn(TownMapData map, int x, int startY, int endY)
    {
        for (var y = startY; y <= endY; y++)
        {
            if (map.GetTerrain(x, y) != TownTerrainType.Water)
            {
                map.SetTerrain(x, y, TownTerrainType.Road);
            }
        }
    }

    private static void PaintCourtyardCells(TownMapData map, int layoutSeed)
    {
        foreach (var cell in map.EnumerateAllCells())
        {
            if (map.GetTerrain(cell.X, cell.Y) != TownTerrainType.Ground)
            {
                continue;
            }

            var compound = map.GetCellCompound(cell);
            if (compound == null)
            {
                continue;
            }

            if (compound.ContentKind is TownCellContentKind.Service or TownCellContentKind.Special)
            {
                map.SetTerrain(cell.X, cell.Y, TownTerrainType.Courtyard);
                continue;
            }

            if (compound.ContentKind == TownCellContentKind.Residence &&
                ((GetCellHash(cell, layoutSeed) / 11) % 3) == 0)
            {
                map.SetTerrain(cell.X, cell.Y, TownTerrainType.Courtyard);
            }
        }
    }

    private static TownCellCompoundData CreateCellCompound(
        Vector2I cell,
        int populationHint,
        int housingHint,
        int eliteHint,
        int layoutSeed)
    {
        var hash = GetCellHash(cell, layoutSeed);
        var regionName = GetRegionName(cell);
        var contentKind = GetContentKind(cell, hash);
        var qiAffinityText = GetQiAffinityText(hash);
        var baseQiCapacity = 78 + (hash % 52) + ((cell.Y % 4) * 6);
        var qiRecoveryPerHour = 4 + ((hash / 7) % 5);
        var buildSlotCount = 2 + (((hash / 13) + cell.X + cell.Y) % 2);
        var suggestedBuildType = GetSuggestedBuildType(contentKind, hash);
        var features = GetFeatureTexts(cell, hash);
        var subBuildings = CreateSubBuildingPlans(contentKind, suggestedBuildType, hash, buildSlotCount);
        var totalQiDemand = subBuildings.Sum(static building => building.QiDemand);
        var synergyScore = CalculateSynergyScore(subBuildings);
        var qiCongestion = CalculateQiCongestion(baseQiCapacity, totalQiDemand);
        var stability = CalculateStability(contentKind, qiCongestion, synergyScore, hash);

        if (populationHint > housingHint && contentKind == TownCellContentKind.Residence)
        {
            qiRecoveryPerHour += 1;
        }

        if (eliteHint > 0 && contentKind == TownCellContentKind.Service)
        {
            baseQiCapacity += Math.Min(eliteHint, 8);
        }

        baseQiCapacity += Math.Clamp((int)Math.Round(synergyScore * 2f), 0, 10);

        return new TownCellCompoundData(
            cell,
            regionName,
            contentKind,
            qiAffinityText,
            baseQiCapacity,
            qiRecoveryPerHour,
            buildSlotCount,
            features,
            subBuildings,
            totalQiDemand,
            qiCongestion,
            synergyScore,
            stability,
            suggestedBuildType);
    }

    private static string GetRegionName(Vector2I cell)
    {
        if (cell.Y <= 3)
        {
            return cell.X <= 10 ? "前山山门坪" : "前山东坡";
        }

        if (cell.Y <= 7)
        {
            return cell.X <= 10 ? "中坪坊区" : "东麓药谷";
        }

        if (cell.Y <= 11)
        {
            return cell.X <= 10 ? "西侧工务坡" : "后山静修台";
        }

        return cell.X <= 10 ? "后山居舍区" : "巡山外缘";
    }

    private static TownCellContentKind GetContentKind(Vector2I cell, int hash)
    {
        if (cell.Y <= 2 || cell.Y >= MapHeight - 2)
        {
            return TownCellContentKind.Infrastructure;
        }

        return (hash % 6) switch
        {
            0 => TownCellContentKind.Production,
            1 => TownCellContentKind.Service,
            2 => TownCellContentKind.Residence,
            3 => TownCellContentKind.Special,
            4 => TownCellContentKind.Production,
            _ => TownCellContentKind.Empty
        };
    }

    private static string GetQiAffinityText(int hash)
    {
        return ((hash / 5) % 5) switch
        {
            0 => "木旺地脉",
            1 => "火炽地脉",
            2 => "土稳地脉",
            3 => "金锐地脉",
            _ => "水盛地脉"
        };
    }

    private static IndustryBuildingType? GetSuggestedBuildType(TownCellContentKind contentKind, int hash)
    {
        return contentKind switch
        {
            TownCellContentKind.Production => (hash % 3) switch
            {
                0 => IndustryBuildingType.Agriculture,
                1 => IndustryBuildingType.Workshop,
                _ => IndustryBuildingType.Trade
            },
            TownCellContentKind.Service => (hash % 2) == 0
                ? IndustryBuildingType.Research
                : IndustryBuildingType.Administration,
            TownCellContentKind.Infrastructure => IndustryBuildingType.Administration,
            _ => null
        };
    }

    private static string[] GetFeatureTexts(Vector2I cell, int hash)
    {
        if (cell.Y <= 3)
        {
            return ["近山门", (hash % 2) == 0 ? "晨雾缓流" : "风口开阔"];
        }

        if (cell.Y <= 7)
        {
            return ["灵壤偏润", (hash % 3) == 0 ? "古树荫蔽" : "灵泉余泽"];
        }

        if (cell.Y <= 11)
        {
            return ["工务近路", (hash % 2) == 0 ? "潜火地脉" : "石台稳固"];
        }

        return ["夜巡要道", (hash % 2) == 0 ? "山风清肃" : "静修偏静"];
    }

    private static TownSubBuildingPlan[] CreateSubBuildingPlans(
        TownCellContentKind contentKind,
        IndustryBuildingType? suggestedBuildType,
        int hash,
        int buildSlotCount)
    {
        var plans = contentKind switch
        {
            TownCellContentKind.Production when suggestedBuildType == IndustryBuildingType.Agriculture
                => new[]
                {
                    CreatePlan("spirit_field_t1", "阵材圃", 18f, 4, ["wood", "food", "water_friendly"], ["wet_dense"]),
                    CreatePlan((hash % 2) == 0 ? "herb_garden_t1" : "warehouse_inner_t1", (hash % 2) == 0 ? "药圃" : "仓阁", (hash % 2) == 0 ? 22f : 10f, (hash % 2) == 0 ? 5 : 3, (hash % 2) == 0 ? ["herb", "alchemy_feed"] : ["storage", "traffic"], (hash % 2) == 0 ? ["wet_dense"] : ["crowded"])
                },
            TownCellContentKind.Production when suggestedBuildType == IndustryBuildingType.Workshop
                => new[]
                {
                    CreatePlan("workshop_puppet_t1", "傀儡工坊", 28f, 6, ["craft", "forge", "warehouse_link"], ["noise", "fire_restless"]),
                    CreatePlan((hash % 2) == 0 ? "warehouse_inner_t1" : "watch_post_t1", (hash % 2) == 0 ? "仓阁" : "巡山岗", (hash % 2) == 0 ? 10f : 12f, (hash % 2) == 0 ? 3 : 3, (hash % 2) == 0 ? ["storage", "traffic"] : ["patrol", "safety"], (hash % 2) == 0 ? ["crowded"] : ["isolated"])
                },
            TownCellContentKind.Production
                => new[]
                {
                    CreatePlan("trade_hall_t1", "青云总坊", 16f, 4, ["trade", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 10f, 3, ["storage", "loss_reduce"], ["crowded"])
                },
            TownCellContentKind.Service when suggestedBuildType == IndustryBuildingType.Research
                => new[]
                {
                    CreatePlan("academy_outer_hall_t1", "传法院", 26f, 5, ["research", "teaching", "quiet"], ["noise"]),
                    CreatePlan("meditation_platform_t1", "静修位", 14f, 2, ["quiet", "recovery"], ["noise"])
                },
            TownCellContentKind.Service
                => new[]
                {
                    CreatePlan("administration_yard_t1", "庶务院", 14f, 3, ["governance", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 10f, 3, ["storage", "loss_reduce"], ["crowded"])
                },
            TownCellContentKind.Residence
                => new[]
                {
                    CreatePlan("disciple_residence_t1", "居舍", 14f, 2, ["rest", "recovery", "stability"], ["crowded", "noise"]),
                    CreatePlan((hash % 2) == 0 ? "canteen_corner_t1" : "rest_pavilion_t1", (hash % 2) == 0 ? "小灶房" : "休憩角", (hash % 2) == 0 ? 8f : 10f, 2, (hash % 2) == 0 ? ["rest", "food"] : ["rest", "quiet"], ["crowded"])
                },
            TownCellContentKind.Infrastructure
                => new[]
                {
                    CreatePlan("mountain_road", "坊路", 4f, 1, ["traffic", "patrol"], ["erosion"]),
                    CreatePlan("watch_corner_t1", "巡查点", 8f, 2, ["patrol", "safety"], ["isolated"])
                },
            TownCellContentKind.Special
                => new[]
                {
                    CreatePlan("watch_post_t1", "巡山岗", 12f, 3, ["patrol", "safety", "threat_control"], ["isolated"]),
                    CreatePlan((hash % 2) == 0 ? "spirit_spring_altar_t1" : "stone_array_t1", (hash % 2) == 0 ? "灵泉坛" : "石台阵位", (hash % 2) == 0 ? 16f : 18f, (hash % 2) == 0 ? 2 : 3, (hash % 2) == 0 ? ["water_friendly", "quiet"] : ["stability", "threat_control"], (hash % 2) == 0 ? ["crowded"] : ["fire_restless"])
                },
            _ => Array.Empty<TownSubBuildingPlan>()
        };

        return plans.Take(Math.Max(buildSlotCount, 0)).ToArray();
    }

    private static TownSubBuildingPlan CreatePlan(
        string templateId,
        string displayName,
        float qiDemand,
        int laborDemand,
        string[] synergyTags,
        string[] conflictTags)
    {
        return new TownSubBuildingPlan(templateId, displayName, qiDemand, laborDemand, synergyTags, conflictTags);
    }

    private static float CalculateSynergyScore(IReadOnlyList<TownSubBuildingPlan> plans)
    {
        if (plans.Count <= 1)
        {
            return 0f;
        }

        var tagCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var conflictCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var plan in plans)
        {
            foreach (var tag in plan.SynergyTags)
            {
                tagCounts[tag] = tagCounts.GetValueOrDefault(tag, 0) + 1;
            }

            foreach (var tag in plan.ConflictTags)
            {
                conflictCounts[tag] = conflictCounts.GetValueOrDefault(tag, 0) + 1;
            }
        }

        float synergyScore = 0f;
        foreach (var count in tagCounts.Values)
        {
            if (count >= 2)
            {
                synergyScore += 0.12f * (count - 1);
            }
        }

        foreach (var count in conflictCounts.Values)
        {
            if (count >= 1)
            {
                synergyScore -= 0.08f * count;
            }
        }

        return synergyScore;
    }

    private static float CalculateQiCongestion(int qiAvailable, float totalQiDemand)
    {
        if (qiAvailable <= 0 || totalQiDemand <= qiAvailable)
        {
            return 0f;
        }

        return (totalQiDemand - qiAvailable) / qiAvailable;
    }

    private static float CalculateStability(TownCellContentKind contentKind, float qiCongestion, float synergyScore, int hash)
    {
        var baseStability = contentKind switch
        {
            TownCellContentKind.Residence => 1.08f,
            TownCellContentKind.Service => 1.02f,
            TownCellContentKind.Special => 0.96f,
            TownCellContentKind.Production => 0.94f,
            _ => 1.0f
        };
        var variance = ((hash / 19) % 5) * 0.02f;
        var stability = baseStability + synergyScore - (qiCongestion * 0.6f) + variance;
        return Math.Clamp(stability, 0.38f, 1.24f);
    }

    private static int GetCellHash(Vector2I cell, int layoutSeed)
    {
        unchecked
        {
            var hash = (cell.X * 73856093) ^
                       (cell.Y * 19349663) ^
                       (layoutSeed * 83492791);
            return hash & int.MaxValue;
        }
    }
}
