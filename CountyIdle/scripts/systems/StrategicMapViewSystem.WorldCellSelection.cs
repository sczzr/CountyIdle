using System;
using System.Linq;
using CountyIdle.Models;
using Godot;

namespace CountyIdle.Systems;

public partial class StrategicMapViewSystem
{
    private XianxiaSiteData? TryPickWorldSite(Vector2 mousePosition, Vector2 center, float unit)
    {
        if (_xianxiaWorldMap == null)
        {
            return null;
        }

        XianxiaSiteData? bestSite = null;
        var bestDistance = float.MaxValue;

        foreach (var site in _xianxiaWorldMap.Sites)
        {
            if (!_xianxiaWorldCenters.TryGetValue((site.Coord.Q, site.Coord.R), out var normalizedCenter))
            {
                continue;
            }

            var canvasPoint = ToCanvas(center, unit, normalizedCenter.X, normalizedCenter.Y);
            var hitRadius = Math.Max(ResolveWorldSiteHitRadius(site) + 6f, 10f);
            var distance = mousePosition.DistanceTo(canvasPoint);
            if (distance > hitRadius || distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestSite = site;
        }

        return bestSite;
    }

    private XianxiaSiteData? TryBuildWorldCellSite(Vector2 mousePosition, Vector2 center, float unit)
    {
        if (_xianxiaWorldMap == null)
        {
            return null;
        }

        var cell = TryPickWorldCell(mousePosition, center, unit);
        if (cell == null)
        {
            return null;
        }

        var anchoredSite = _xianxiaWorldMap.Sites.FirstOrDefault(site =>
            site.Coord.Q == cell.Coord.Q &&
            site.Coord.R == cell.Coord.R);
        if (anchoredSite != null)
        {
            return anchoredSite;
        }

        var primaryType = ResolveWorldCellPrimaryType(cell);
        var secondaryTag = ResolveWorldCellSecondaryTag(cell, primaryType);

        return new XianxiaSiteData
        {
            Role = primaryType == "Ruin" ? XianxiaSiteRoleType.Ruin : XianxiaSiteRoleType.ResourceHub,
            Coord = new HexAxialCoordData
            {
                Q = cell.Coord.Q,
                R = cell.Coord.R
            },
            Structure = cell.Structure,
            Label = ResolveWorldCellLabel(cell, primaryType, secondaryTag),
            Importance = ResolveWorldCellImportance(cell, primaryType),
            PrimaryType = primaryType,
            SecondaryTag = secondaryTag,
            RegionId = ResolveWorldCellRegionId(cell),
            RarityTier = ResolveWorldCellRarityTier(cell, primaryType),
            UnlockTier = ResolveWorldCellUnlockTier(cell, primaryType)
        };
    }

    private XianxiaHexCellData? TryPickWorldCell(Vector2 mousePosition, Vector2 center, float unit)
    {
        if (_xianxiaWorldMap == null || _xianxiaWorldCenters.Count == 0)
        {
            return null;
        }

        var hexRadius = Math.Max(_xianxiaWorldHexRadius * unit, 2f);
        XianxiaHexCellData? nearestCell = null;
        var bestDistance = float.MaxValue;

        foreach (var cell in _xianxiaWorldMap.Cells)
        {
            if (!_xianxiaWorldCenters.TryGetValue((cell.Coord.Q, cell.Coord.R), out var normalizedCenter))
            {
                continue;
            }

            var canvasCenter = ToCanvas(center, unit, normalizedCenter.X, normalizedCenter.Y);
            var hex = BuildHexPolygon(canvasCenter, hexRadius);
            if (Geometry2D.IsPointInPolygon(mousePosition, hex))
            {
                return cell;
            }

            var distance = mousePosition.DistanceTo(canvasCenter);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestCell = cell;
            }
        }

        return bestDistance <= hexRadius * 0.94f ? nearestCell : null;
    }

    private static string ResolveWorldCellPrimaryType(XianxiaHexCellData cell)
    {
        if (cell.Wonder != XianxiaWonderType.None ||
            cell.Structure is XianxiaStructureType.RuinsPlatform or XianxiaStructureType.AncientCityRuins or XianxiaStructureType.AncientShrine ||
            cell.Biome == XianxiaBiomeType.AncientRuinsLand ||
            (cell.Corruption > 0.62f && cell.QiDensity > 0.48f))
        {
            return "Ruin";
        }

        if (cell.Structure is XianxiaStructureType.SectFoundation or XianxiaStructureType.SectMainHall or XianxiaStructureType.SectTrainingGround or
            XianxiaStructureType.TempleFoundation or XianxiaStructureType.TempleComplex or XianxiaStructureType.CultivationPlatform or
            XianxiaStructureType.MeditationPlatform or XianxiaStructureType.HeavenlyGate ||
            cell.IsSectCandidate)
        {
            return "Sect";
        }

        if (cell.Structure == XianxiaStructureType.MarketSquare)
        {
            return "Market";
        }

        if (cell.Structure is XianxiaStructureType.ImmortalPavilion or XianxiaStructureType.DragonStatue ||
            (cell.QiDensity > 0.72f && cell.FactionInfluence >= 2 && cell.MonsterThreat < 0.52f))
        {
            return "ImmortalCity";
        }

        if (cell.Structure is XianxiaStructureType.FortressBase or XianxiaStructureType.Watchtower or XianxiaStructureType.MartialArena ||
            (cell.QiDensity > 0.56f && cell.FactionInfluence >= 1 && cell.Corruption < 0.58f))
        {
            return "CultivatorClan";
        }

        if (cell.Structure is XianxiaStructureType.VillageBase or XianxiaStructureType.BridgeFoundation ||
            (cell.Fertility > 0.62f && cell.MonsterThreat < 0.56f))
        {
            return "MortalRealm";
        }

        if (cell.RoadMask != HexDirectionMask.None && (cell.Water != XianxiaWaterType.None || cell.FactionInfluence > 0))
        {
            return "Market";
        }

        return "Wilderness";
    }

    private static string ResolveWorldCellSecondaryTag(XianxiaHexCellData cell, string primaryType)
    {
        return primaryType switch
        {
            "Sect" => cell.Structure is XianxiaStructureType.SectMainHall or XianxiaStructureType.HeavenlyGate
                ? "MountainGate"
                : cell.Height >= 76
                    ? "BranchPeak"
                    : "OuterCourtyard",
            "MortalRealm" => cell.Water != XianxiaWaterType.None
                ? "RiverTown"
                : cell.Fertility > 0.72f
                    ? "FarmVillage"
                    : "CountySeat",
            "Market" => cell.Water != XianxiaWaterType.None
                ? "RoadsideMarket"
                : cell.QiDensity > 0.64f
                    ? "SectMarket"
                    : "LooseCultivatorBazaar",
            "CultivatorClan" => cell.ElementAffinity switch
            {
                XianxiaElementType.Metal or XianxiaElementType.Fire => "ForgeLineage",
                XianxiaElementType.Wood or XianxiaElementType.Water => "MedicineLineage",
                _ when cell.Fertility > 0.66f && cell.Water != XianxiaWaterType.None => "SpiritFieldManor",
                _ when cell.RoadMask != HexDirectionMask.None => "GuestHall",
                _ => "AncestralEstate"
            },
            "ImmortalCity" => cell.Water != XianxiaWaterType.None
                ? "HarborCity"
                : cell.MonsterThreat > 0.56f
                    ? "FrontierCity"
                    : cell.QiDensity > 0.70f && cell.Corruption < 0.18f
                        ? "ImperialCultCity"
                        : cell.RoadMask != HexDirectionMask.None
                            ? "TransitHub"
                            : "GrandCity",
            "Ruin" => cell.Corruption > 0.72f
                ? "SealedDungeon"
                : cell.QiDensity > 0.68f
                    ? "TrialRealm"
                    : cell.Structure is XianxiaStructureType.AncientCityRuins or XianxiaStructureType.FortressBase
                        ? "BattlefieldRemnant"
                        : "AncientCave",
            _ => cell.Biome switch
            {
                XianxiaBiomeType.MistyMountains or XianxiaBiomeType.JadeHighlands or XianxiaBiomeType.SnowPeaks => "SpiritMountainWilds",
                XianxiaBiomeType.BambooValley or XianxiaBiomeType.SacredForest => "ForestWilds",
                XianxiaBiomeType.SpiritSwamps => "SwampWilds",
                XianxiaBiomeType.CrystalFields => "CrystalWilds",
                XianxiaBiomeType.FloatingIsles => "SkyWilds",
                XianxiaBiomeType.DesertBadlands => "DesertWilds",
                _ when cell.Water != XianxiaWaterType.None => "RiverWilds",
                _ => "OpenWilds"
            }
        };
    }

    private static string ResolveWorldCellLabel(XianxiaHexCellData cell, string primaryType, string secondaryTag)
    {
        var prefix = primaryType switch
        {
            "Sect" => secondaryTag switch
            {
                "MountainGate" => "山门外缘",
                "BranchPeak" => "分峰台地",
                _ => "外门院地"
            },
            "MortalRealm" => secondaryTag switch
            {
                "RiverTown" => "河埠乡镇",
                "FarmVillage" => "灵田乡里",
                _ => "府县治所"
            },
            "Market" => secondaryTag switch
            {
                "SectMarket" => "山门坊市",
                "RoadsideMarket" => "路驿坊口",
                _ => "散修集市"
            },
            "CultivatorClan" => secondaryTag switch
            {
                "SpiritFieldManor" => "灵田庄园",
                "ForgeLineage" => "铸器别脉",
                "MedicineLineage" => "丹药别脉",
                "GuestHall" => "客卿别馆",
                _ => "世家祖庭"
            },
            "ImmortalCity" => secondaryTag switch
            {
                "HarborCity" => "河港仙城",
                "FrontierCity" => "边陲仙城",
                "ImperialCultCity" => "修士都城",
                "TransitHub" => "通衢驿城",
                _ => "云上大城"
            },
            "Ruin" => secondaryTag switch
            {
                "SealedDungeon" => "封印地宫",
                "TrialRealm" => "试炼秘境",
                "BattlefieldRemnant" => "古战场遗址",
                _ => "古修洞府"
            },
            _ => secondaryTag switch
            {
                "SpiritMountainWilds" => "云岭山野",
                "ForestWilds" => "古木幽野",
                "SwampWilds" => "灵沼荒野",
                "CrystalWilds" => "晶砂荒原",
                "SkyWilds" => "浮天野境",
                "DesertWilds" => "流沙野境",
                "RiverWilds" => "河谷野径",
                _ => "外野地界"
            }
        };

        return $"{prefix} [{cell.Coord.Q},{cell.Coord.R}]";
    }

    private static int ResolveWorldCellImportance(XianxiaHexCellData cell, string primaryType)
    {
        return primaryType switch
        {
            "Sect" or "ImmortalCity" => 3,
            "Ruin" when cell.Wonder != XianxiaWonderType.None => 3,
            "Market" or "CultivatorClan" => 2,
            _ => 1
        };
    }

    private static string ResolveWorldCellRegionId(XianxiaHexCellData cell)
    {
        var height01 = Mathf.Clamp(cell.Height / 100f, 0f, 1f);

        if (cell.Biome == XianxiaBiomeType.AncientRuinsLand || (cell.Corruption > 0.62f && cell.QiDensity < 0.46f))
        {
            return "BrokenVeinRuins";
        }

        if (height01 > 0.66f || cell.Biome is XianxiaBiomeType.MistyMountains or XianxiaBiomeType.JadeHighlands or XianxiaBiomeType.SnowPeaks)
        {
            return "SpiritMountain";
        }

        if (cell.MonsterThreat > 0.58f || cell.Biome is XianxiaBiomeType.DesertBadlands or XianxiaBiomeType.SpiritSwamps)
        {
            return "FrontierWilds";
        }

        if (cell.Fertility > 0.62f && cell.Moisture > 0.48f)
        {
            return "MortalHeartland";
        }

        return "TradeCorridor";
    }

    private static string ResolveWorldCellRarityTier(XianxiaHexCellData cell, string primaryType)
    {
        if (cell.Wonder != XianxiaWonderType.None || (primaryType == "Ruin" && cell.QiDensity > 0.80f))
        {
            return "Legendary";
        }

        if (cell.QiDensity > 0.68f || cell.MonsterThreat > 0.66f || cell.Corruption > 0.68f)
        {
            return "Rare";
        }

        if (cell.Structure != XianxiaStructureType.None ||
            cell.Resource != XianxiaResourceType.None ||
            primaryType is "Market" or "CultivatorClan" or "ImmortalCity")
        {
            return "Uncommon";
        }

        return "Common";
    }

    private static int ResolveWorldCellUnlockTier(XianxiaHexCellData cell, string primaryType)
    {
        return primaryType switch
        {
            "Ruin" when cell.Wonder != XianxiaWonderType.None || cell.MonsterThreat > 0.72f => 2,
            "ImmortalCity" => 1,
            "CultivatorClan" when cell.QiDensity > 0.62f => 1,
            "Wilderness" when cell.MonsterThreat > 0.64f => 1,
            _ => 0
        };
    }

    private bool IsSelectedWorldCell(XianxiaHexCellData cell)
    {
        return _selectedWorldSite != null &&
               _selectedWorldSite.Coord.Q == cell.Coord.Q &&
               _selectedWorldSite.Coord.R == cell.Coord.R;
    }
}
