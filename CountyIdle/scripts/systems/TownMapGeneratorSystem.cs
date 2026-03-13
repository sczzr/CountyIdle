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
        PopulateStructures(map, populationHint, housingHint, eliteHint, safeSeed);
        return map;
    }

    private static void PopulateStructures(TownMapData map, int populationHint, int housingHint, int eliteHint, int layoutSeed)
    {
        var usedLots = new HashSet<Vector2I>();
        var anchorCounts = new Dictionary<TownActivityAnchorType, int>();

        var anchorSeeds = new[]
        {
            (Type: TownActivityAnchorType.Farmstead, Content: TownCellContentKind.Production, Preferred: IndustryBuildingType.Agriculture, FloorBias: 1),
            (Type: TownActivityAnchorType.Workshop, Content: TownCellContentKind.Production, Preferred: IndustryBuildingType.Workshop, FloorBias: 1),
            (Type: TownActivityAnchorType.Market, Content: TownCellContentKind.Production, Preferred: IndustryBuildingType.Trade, FloorBias: 1),
            (Type: TownActivityAnchorType.Academy, Content: TownCellContentKind.Service, Preferred: IndustryBuildingType.Research, FloorBias: eliteHint > 4 ? 2 : 1),
            (Type: TownActivityAnchorType.Administration, Content: TownCellContentKind.Service, Preferred: IndustryBuildingType.Administration, FloorBias: 2),
            (Type: TownActivityAnchorType.Leisure, Content: TownCellContentKind.Special, Preferred: (IndustryBuildingType?)null, FloorBias: 1)
        };

        foreach (var seed in anchorSeeds)
        {
            if (TryAddAnchorForSeed(map, usedLots, anchorCounts, seed.Type, seed.Content, seed.Preferred, seed.FloorBias, layoutSeed))
            {
                continue;
            }

            TryAddAnchorForSeed(map, usedLots, anchorCounts, seed.Type, seed.Content, null, seed.FloorBias, layoutSeed + 17);
        }

        var residenceBudget = Math.Clamp(Math.Max(housingHint, populationHint) / 70, 2, 5);
        var residenceCandidates = GetCandidateCells(map, TownCellContentKind.Residence, null, layoutSeed + 101);
        for (var index = 0; index < residenceCandidates.Count && residenceBudget > 0; index++)
        {
            var lotCell = residenceCandidates[index];
            if (usedLots.Contains(lotCell))
            {
                continue;
            }

            var roadCell = FindNearestRoadCell(map, lotCell);
            if (roadCell == null)
            {
                continue;
            }

            usedLots.Add(lotCell);
            map.AddBuilding(new TownBuildingData(
                lotCell,
                ResolveFacingFromRoad(lotCell, roadCell.Value),
                1,
                (GetCellHash(lotCell, layoutSeed + 203) % 3) == 0));
            residenceBudget--;
        }
    }

    private static bool TryAddAnchorForSeed(
        TownMapData map,
        HashSet<Vector2I> usedLots,
        Dictionary<TownActivityAnchorType, int> anchorCounts,
        TownActivityAnchorType anchorType,
        TownCellContentKind contentKind,
        IndustryBuildingType? preferredBuildType,
        int floorBias,
        int layoutSeed)
    {
        var candidates = GetCandidateCells(map, contentKind, preferredBuildType, layoutSeed);
        foreach (var lotCell in candidates)
        {
            if (usedLots.Contains(lotCell))
            {
                continue;
            }

            var roadCell = FindNearestRoadCell(map, lotCell);
            if (roadCell == null)
            {
                continue;
            }

            usedLots.Add(lotCell);
            var facing = ResolveFacingFromRoad(lotCell, roadCell.Value);
            var visualVariant = GetCellHash(lotCell, layoutSeed + ((int)anchorType * 31)) % 3;
            var count = anchorCounts.GetValueOrDefault(anchorType, 0) + 1;
            anchorCounts[anchorType] = count;
            var floors = Math.Max(1, floorBias + ((GetCellHash(lotCell, layoutSeed + 59) % 2 == 0 && floorBias > 1) ? 0 : 0));

            map.AddActivityAnchor(new TownActivityAnchorData(
                anchorType,
                roadCell.Value,
                lotCell,
                facing,
                floors,
                visualVariant,
                $"{SectMapSemanticRules.GetAnchorLabelPrefix(anchorType)}·{count}号"));

            map.AddBuilding(new TownBuildingData(
                lotCell,
                facing,
                floors,
                anchorType is TownActivityAnchorType.Academy or TownActivityAnchorType.Administration));
            return true;
        }

        return false;
    }

    private static List<Vector2I> GetCandidateCells(
        TownMapData map,
        TownCellContentKind contentKind,
        IndustryBuildingType? preferredBuildType,
        int layoutSeed)
    {
        var cells = new List<(Vector2I Cell, int Score)>();
        foreach (var cell in map.EnumerateAllCells())
        {
            var compound = map.GetCellCompound(cell);
            if (compound == null || compound.ContentKind != contentKind)
            {
                continue;
            }

            if (map.GetTerrain(cell.X, cell.Y) == TownTerrainType.Water)
            {
                continue;
            }

            if (preferredBuildType != null && compound.SuggestedBuildType != preferredBuildType)
            {
                continue;
            }

            var roadCell = FindNearestRoadCell(map, cell);
            if (roadCell == null)
            {
                continue;
            }

            var hash = GetCellHash(cell, layoutSeed);
            var score = 1000 - (Math.Abs(cell.X - (MapWidth / 2)) * 11) - (Math.Abs(cell.Y - (MapHeight / 2)) * 7) + (hash % 97);
            if (map.GetTerrain(cell.X, cell.Y) == TownTerrainType.Courtyard)
            {
                score += 24;
            }

            if (compound.Stability >= 1.0f)
            {
                score += 12;
            }

            cells.Add((cell, score));
        }

        cells.Sort((left, right) => right.Score.CompareTo(left.Score));
        var result = new List<Vector2I>(cells.Count);
        foreach (var cell in cells)
        {
            result.Add(cell.Cell);
        }

        return result;
    }

    private static Vector2I? FindNearestRoadCell(TownMapData map, Vector2I lotCell)
    {
        Vector2I? bestRoadCell = null;
        var bestDistance = int.MaxValue;

        foreach (var neighbor in GetHexNeighbors(lotCell))
        {
            if (!map.IsInside(neighbor) || map.GetTerrain(neighbor.X, neighbor.Y) != TownTerrainType.Road)
            {
                continue;
            }

            var distance = Math.Abs(neighbor.X - lotCell.X) + Math.Abs(neighbor.Y - lotCell.Y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoadCell = neighbor;
            }
        }

        return bestRoadCell;
    }

    private static IEnumerable<Vector2I> GetHexNeighbors(Vector2I cell)
    {
        var isOddRow = (cell.Y & 1) == 1;
        if (isOddRow)
        {
            yield return cell + new Vector2I(0, -1);
            yield return cell + new Vector2I(1, -1);
            yield return cell + Vector2I.Right;
            yield return cell + new Vector2I(1, 1);
            yield return cell + new Vector2I(0, 1);
            yield return cell + Vector2I.Left;
            yield break;
        }

        yield return cell + new Vector2I(-1, -1);
        yield return cell + new Vector2I(0, -1);
        yield return cell + Vector2I.Right;
        yield return cell + new Vector2I(0, 1);
        yield return cell + new Vector2I(-1, 1);
        yield return cell + Vector2I.Left;
    }

    private static TownFacing ResolveFacingFromRoad(Vector2I lotCell, Vector2I roadCell)
    {
        var delta = roadCell - lotCell;
        if (delta.Y < 0)
        {
            return TownFacing.North;
        }

        if (delta.Y > 0)
        {
            return TownFacing.South;
        }

        return delta.X >= 0 ? TownFacing.East : TownFacing.West;
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
            TownCompoundPlanStyle.Natural,
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

    public TownCellCompoundData ReplanCompound(TownCellCompoundData current, TownCompoundPlanStyle planStyle)
    {
        var structuralCapacity = Math.Max(
            current.BaseQiCapacity - Math.Clamp((int)Math.Round(current.SynergyScore * 2f), 0, 10),
            0);
        var subBuildings = CreatePlannedSubBuildings(
            current.ContentKind,
            current.SuggestedBuildType,
            current.BuildSlotCount,
            planStyle);
        var totalQiDemand = subBuildings.Sum(static building => building.QiDemand);
        var synergyScore = CalculateSynergyScore(subBuildings);
        var baseQiCapacity = structuralCapacity + Math.Clamp((int)Math.Round(synergyScore * 2f), 0, 10);
        var qiCongestion = CalculateQiCongestion(baseQiCapacity, totalQiDemand);
        var stability = CalculateStability(
            current.ContentKind,
            qiCongestion,
            synergyScore,
            GetCellHash(current.Cell, 20260312 + (((int)planStyle + 1) * 97)));

        return new TownCellCompoundData(
            current.Cell,
            current.RegionName,
            current.ContentKind,
            planStyle,
            current.QiAffinityText,
            baseQiCapacity,
            current.QiRecoveryPerHour,
            current.BuildSlotCount,
            current.FeatureTexts,
            subBuildings,
            totalQiDemand,
            qiCongestion,
            synergyScore,
            stability,
            current.SuggestedBuildType);
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

    private static TownSubBuildingPlan[] CreatePlannedSubBuildings(
        TownCellContentKind contentKind,
        IndustryBuildingType? suggestedBuildType,
        int buildSlotCount,
        TownCompoundPlanStyle planStyle)
    {
        var plans = contentKind switch
        {
            TownCellContentKind.Production => CreateProductionPlans(suggestedBuildType, planStyle),
            TownCellContentKind.Service => CreateServicePlans(suggestedBuildType, planStyle),
            TownCellContentKind.Residence => CreateResidencePlans(planStyle),
            TownCellContentKind.Infrastructure => CreateInfrastructurePlans(planStyle),
            TownCellContentKind.Special => CreateSpecialPlans(planStyle),
            _ => CreateReservePlans(planStyle)
        };

        return plans.Take(Math.Max(buildSlotCount, 0)).ToArray();
    }

    private static TownSubBuildingPlan[] CreateProductionPlans(
        IndustryBuildingType? suggestedBuildType,
        TownCompoundPlanStyle planStyle)
    {
        var specialization = suggestedBuildType switch
        {
            IndustryBuildingType.Workshop => "Workshop",
            IndustryBuildingType.Trade => "Trade",
            _ => "Agriculture"
        };

        return (specialization, planStyle) switch
        {
            ("Workshop", TownCompoundPlanStyle.Specialized) =>
                [
                    CreatePlan("workshop_puppet_t1", "傀儡工坊", 28f, 6, ["craft", "forge", "warehouse_link"], ["noise", "fire_restless"]),
                    CreatePlan("smithy_annex_t1", "炼材坊", 18f, 4, ["forge", "craft"], ["noise"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 10f, 3, ["warehouse_link", "storage"], ["crowded"])
                ],
            ("Workshop", TownCompoundPlanStyle.Synergy) =>
                [
                    CreatePlan("workshop_puppet_t1", "傀儡工坊", 24f, 5, ["craft", "warehouse_link"], ["noise"]),
                    CreatePlan("transfer_shed_t1", "转运棚", 10f, 2, ["warehouse_link", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 9f, 3, ["storage", "warehouse_link"], ["crowded"])
                ],
            ("Workshop", TownCompoundPlanStyle.Balanced) =>
                [
                    CreatePlan("repair_platform_t1", "修缮台", 12f, 3, ["craft", "stability"], ["noise"]),
                    CreatePlan("watch_corner_t1", "巡查点", 8f, 2, ["safety", "stability"], ["isolated"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 8f, 2, ["storage", "traffic"], ["crowded"])
                ],
            ("Trade", TownCompoundPlanStyle.Specialized) =>
                [
                    CreatePlan("trade_hall_t1", "青云总坊", 16f, 4, ["trade", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 10f, 3, ["storage", "trade"], ["crowded"]),
                    CreatePlan("relay_kiosk_t1", "转运亭", 12f, 3, ["traffic", "trade"], ["crowded"])
                ],
            ("Trade", TownCompoundPlanStyle.Synergy) =>
                [
                    CreatePlan("trade_hall_t1", "青云总坊", 14f, 4, ["trade", "traffic"], ["crowded"]),
                    CreatePlan("relay_kiosk_t1", "转运亭", 10f, 2, ["trade", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 9f, 2, ["trade", "storage"], ["crowded"])
                ],
            ("Trade", TownCompoundPlanStyle.Balanced) =>
                [
                    CreatePlan("warehouse_inner_t1", "仓阁", 8f, 2, ["storage", "loss_reduce"], ["crowded"]),
                    CreatePlan("relay_kiosk_t1", "转运亭", 9f, 2, ["traffic", "stability"], ["crowded"]),
                    CreatePlan("watch_corner_t1", "巡查点", 7f, 2, ["safety", "stability"], ["isolated"])
                ],
            ("Agriculture", TownCompoundPlanStyle.Specialized) =>
                [
                    CreatePlan("spirit_field_t1", "阵材圃", 18f, 4, ["wood", "food", "water_friendly"], ["wet_dense"]),
                    CreatePlan("herb_garden_t1", "药圃", 22f, 5, ["herb", "alchemy_feed", "water_friendly"], ["wet_dense"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 10f, 3, ["storage", "herb"], ["crowded"])
                ],
            ("Agriculture", TownCompoundPlanStyle.Synergy) =>
                [
                    CreatePlan("spirit_field_t1", "阵材圃", 16f, 4, ["wood", "water_friendly"], ["wet_dense"]),
                    CreatePlan("spring_channel_t1", "蕴灵渠", 11f, 2, ["water_friendly", "recovery"], ["crowded"]),
                    CreatePlan("herb_garden_t1", "药圃", 18f, 4, ["water_friendly", "herb"], ["wet_dense"])
                ],
            ("Agriculture", TownCompoundPlanStyle.Balanced) =>
                [
                    CreatePlan("spirit_field_t1", "阵材圃", 14f, 3, ["wood", "food"], ["wet_dense"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 8f, 2, ["storage", "loss_reduce"], ["crowded"]),
                    CreatePlan("nourish_shed_t1", "养脉棚", 9f, 2, ["recovery", "stability"], ["isolated"])
                ],
            _ => CreateProductionPlans(suggestedBuildType, TownCompoundPlanStyle.Specialized)
        };
    }

    private static TownSubBuildingPlan[] CreateServicePlans(
        IndustryBuildingType? suggestedBuildType,
        TownCompoundPlanStyle planStyle)
    {
        var specialization = suggestedBuildType == IndustryBuildingType.Research ? "Research" : "Administration";

        return (specialization, planStyle) switch
        {
            ("Research", TownCompoundPlanStyle.Specialized) =>
                [
                    CreatePlan("academy_outer_hall_t1", "传法院", 26f, 5, ["research", "teaching", "quiet"], ["noise"]),
                    CreatePlan("meditation_platform_t1", "静修位", 14f, 2, ["quiet", "recovery"], ["noise"]),
                    CreatePlan("scriptorium_t1", "抄经室", 16f, 3, ["research", "quiet"], ["noise"])
                ],
            ("Research", TownCompoundPlanStyle.Synergy) =>
                [
                    CreatePlan("academy_outer_hall_t1", "传法院", 22f, 4, ["research", "quiet"], ["noise"]),
                    CreatePlan("scriptorium_t1", "抄经室", 13f, 3, ["research", "quiet"], ["noise"]),
                    CreatePlan("meditation_platform_t1", "静修位", 11f, 2, ["quiet", "recovery"], ["noise"])
                ],
            ("Research", TownCompoundPlanStyle.Balanced) =>
                [
                    CreatePlan("meditation_platform_t1", "静修位", 10f, 2, ["quiet", "recovery"], ["noise"]),
                    CreatePlan("rest_pavilion_t1", "守静亭", 8f, 2, ["quiet", "stability"], ["crowded"]),
                    CreatePlan("scriptorium_t1", "抄经室", 12f, 2, ["research", "stability"], ["noise"])
                ],
            ("Administration", TownCompoundPlanStyle.Specialized) =>
                [
                    CreatePlan("administration_yard_t1", "庶务院", 14f, 3, ["governance", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 10f, 3, ["storage", "governance"], ["crowded"]),
                    CreatePlan("dispatch_hall_t1", "签押厅", 13f, 3, ["governance", "safety"], ["crowded"])
                ],
            ("Administration", TownCompoundPlanStyle.Synergy) =>
                [
                    CreatePlan("administration_yard_t1", "庶务院", 12f, 3, ["governance", "traffic"], ["crowded"]),
                    CreatePlan("dispatch_hall_t1", "签押厅", 11f, 2, ["governance", "safety"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "仓阁", 9f, 2, ["governance", "storage"], ["crowded"])
                ],
            ("Administration", TownCompoundPlanStyle.Balanced) =>
                [
                    CreatePlan("administration_yard_t1", "庶务院", 11f, 2, ["governance", "stability"], ["crowded"]),
                    CreatePlan("watch_corner_t1", "巡查点", 7f, 2, ["safety", "stability"], ["isolated"]),
                    CreatePlan("meditation_platform_t1", "静修位", 8f, 1, ["recovery", "quiet"], ["noise"])
                ],
            _ => CreateServicePlans(suggestedBuildType, TownCompoundPlanStyle.Specialized)
        };
    }

    private static TownSubBuildingPlan[] CreateResidencePlans(TownCompoundPlanStyle planStyle)
    {
        return planStyle switch
        {
            TownCompoundPlanStyle.Specialized =>
                [
                    CreatePlan("disciple_residence_t1", "居舍", 14f, 2, ["rest", "recovery", "stability"], ["crowded", "noise"]),
                    CreatePlan("canteen_corner_t1", "小灶房", 8f, 2, ["rest", "food"], ["crowded"]),
                    CreatePlan("wash_corner_t1", "洗尘处", 9f, 2, ["recovery", "rest"], ["crowded"])
                ],
            TownCompoundPlanStyle.Synergy =>
                [
                    CreatePlan("disciple_residence_t1", "居舍", 12f, 2, ["rest", "stability"], ["crowded", "noise"]),
                    CreatePlan("rest_pavilion_t1", "休憩角", 9f, 2, ["rest", "quiet", "stability"], ["crowded"]),
                    CreatePlan("care_corner_t1", "养息间", 10f, 2, ["rest", "recovery"], ["crowded"])
                ],
            TownCompoundPlanStyle.Balanced =>
                [
                    CreatePlan("disciple_residence_t1", "居舍", 11f, 2, ["rest", "stability"], ["crowded"]),
                    CreatePlan("rest_pavilion_t1", "守静亭", 7f, 1, ["quiet", "stability"], ["crowded"]),
                    CreatePlan("night_lamp_t1", "巡夜灯台", 6f, 1, ["safety", "stability"], ["isolated"])
                ],
            _ => CreateResidencePlans(TownCompoundPlanStyle.Specialized)
        };
    }

    private static TownSubBuildingPlan[] CreateInfrastructurePlans(TownCompoundPlanStyle planStyle)
    {
        return planStyle switch
        {
            TownCompoundPlanStyle.Specialized =>
                [
                    CreatePlan("mountain_road", "坊路", 4f, 1, ["traffic", "patrol"], ["erosion"]),
                    CreatePlan("relay_kiosk_t1", "转运亭", 9f, 2, ["traffic", "storage"], ["crowded"]),
                    CreatePlan("watch_corner_t1", "巡查点", 8f, 2, ["patrol", "safety"], ["isolated"])
                ],
            TownCompoundPlanStyle.Synergy =>
                [
                    CreatePlan("mountain_road", "坊路", 4f, 1, ["traffic", "patrol"], ["erosion"]),
                    CreatePlan("watch_corner_t1", "巡查点", 7f, 2, ["patrol", "traffic"], ["isolated"]),
                    CreatePlan("relay_kiosk_t1", "转运亭", 8f, 2, ["traffic", "patrol"], ["crowded"])
                ],
            TownCompoundPlanStyle.Balanced =>
                [
                    CreatePlan("mountain_road", "坊路", 4f, 1, ["traffic", "stability"], ["erosion"]),
                    CreatePlan("watch_corner_t1", "巡查点", 6f, 1, ["safety", "stability"], ["isolated"]),
                    CreatePlan("stone_array_t1", "石台阵位", 7f, 2, ["stability", "threat_control"], ["fire_restless"])
                ],
            _ => CreateInfrastructurePlans(TownCompoundPlanStyle.Specialized)
        };
    }

    private static TownSubBuildingPlan[] CreateSpecialPlans(TownCompoundPlanStyle planStyle)
    {
        return planStyle switch
        {
            TownCompoundPlanStyle.Specialized =>
                [
                    CreatePlan("watch_post_t1", "巡山岗", 12f, 3, ["patrol", "safety", "threat_control"], ["isolated"]),
                    CreatePlan("stone_array_t1", "石台阵位", 18f, 3, ["stability", "threat_control"], ["fire_restless"]),
                    CreatePlan("ward_altar_t1", "镇煞坛", 16f, 3, ["safety", "threat_control"], ["fire_restless"])
                ],
            TownCompoundPlanStyle.Synergy =>
                [
                    CreatePlan("watch_post_t1", "巡山岗", 11f, 3, ["patrol", "threat_control"], ["isolated"]),
                    CreatePlan("ward_altar_t1", "镇煞坛", 13f, 2, ["safety", "threat_control"], ["fire_restless"]),
                    CreatePlan("stone_array_t1", "石台阵位", 14f, 2, ["stability", "threat_control"], ["fire_restless"])
                ],
            TownCompoundPlanStyle.Balanced =>
                [
                    CreatePlan("spirit_spring_altar_t1", "灵泉坛", 10f, 2, ["water_friendly", "quiet"], ["crowded"]),
                    CreatePlan("watch_post_t1", "巡山岗", 9f, 2, ["safety", "patrol"], ["isolated"]),
                    CreatePlan("rest_pavilion_t1", "守静亭", 7f, 1, ["quiet", "stability"], ["crowded"])
                ],
            _ => CreateSpecialPlans(TownCompoundPlanStyle.Specialized)
        };
    }

    private static TownSubBuildingPlan[] CreateReservePlans(TownCompoundPlanStyle planStyle)
    {
        return planStyle switch
        {
            TownCompoundPlanStyle.Synergy =>
                [
                    CreatePlan("reserve_platform_t1", "预留台", 6f, 1, ["stability", "traffic"], ["crowded"]),
                    CreatePlan("relay_kiosk_t1", "转运亭", 8f, 2, ["traffic", "storage"], ["crowded"])
                ],
            TownCompoundPlanStyle.Balanced =>
                [
                    CreatePlan("reserve_platform_t1", "预留台", 5f, 1, ["stability"], ["crowded"]),
                    CreatePlan("watch_corner_t1", "巡查点", 6f, 1, ["safety", "stability"], ["isolated"])
                ],
            _ =>
                [
                    CreatePlan("reserve_platform_t1", "预留台", 7f, 1, ["stability", "traffic"], ["crowded"]),
                    CreatePlan("warehouse_inner_t1", "临时堆场", 7f, 2, ["storage"], ["crowded"])
                ]
        };
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
