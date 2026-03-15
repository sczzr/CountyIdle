using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class WorldTerrainVisualRules
{
    public static TownTerrainVisualFamily ResolveWorldVisualFamily(XianxiaHexCellData? cell)
    {
        if (cell == null)
        {
            return TownTerrainVisualFamily.Plain;
        }

        if (cell.Water != XianxiaWaterType.None)
        {
            return IsDeepWater(cell.Water)
                ? TownTerrainVisualFamily.DeepWater
                : TownTerrainVisualFamily.ShallowWater;
        }

        return ResolveTerrainVisualFamily(cell.Terrain, cell.Biome, cell.QiDensity, cell.Corruption);
    }

    public static TownTerrainVisualFamily ResolveSecondaryMapVisualFamily(
        XianxiaHexCellData? sourceCell,
        WorldSiteLocalTileType tileType)
    {
        var baseFamily = ResolveWorldVisualFamily(sourceCell);
        return tileType switch
        {
            WorldSiteLocalTileType.Water => baseFamily is TownTerrainVisualFamily.DeepWater or TownTerrainVisualFamily.ShallowWater
                ? baseFamily
                : TownTerrainVisualFamily.ShallowWater,
            WorldSiteLocalTileType.Spirit => TownTerrainVisualFamily.Spirit,
            WorldSiteLocalTileType.Ridge => baseFamily == TownTerrainVisualFamily.Snow
                ? TownTerrainVisualFamily.Snow
                : TownTerrainVisualFamily.Rugged,
            WorldSiteLocalTileType.Ruin or WorldSiteLocalTileType.Hazard => baseFamily == TownTerrainVisualFamily.Snow
                ? TownTerrainVisualFamily.Snow
                : TownTerrainVisualFamily.Rugged,
            WorldSiteLocalTileType.Forest => baseFamily == TownTerrainVisualFamily.Spirit
                ? TownTerrainVisualFamily.Spirit
                : TownTerrainVisualFamily.Plain,
            _ => baseFamily
        };
    }

    public static TownTerrainVisualFamily ResolveTerrainVisualFamily(
        XianxiaTerrainType terrain,
        XianxiaBiomeType biome,
        float qiDensity,
        float corruption)
    {
        return terrain switch
        {
            XianxiaTerrainType.GrassLush or
            XianxiaTerrainType.GrassSparse or
            XianxiaTerrainType.WildflowerMeadow => TownTerrainVisualFamily.Plain,

            XianxiaTerrainType.ForestGround or
            XianxiaTerrainType.BambooGround or
            XianxiaTerrainType.AncientForestFloor or
            XianxiaTerrainType.SpiritSoil or
            XianxiaTerrainType.CrystalGround or
            XianxiaTerrainType.CloudGround => TownTerrainVisualFamily.Spirit,

            XianxiaTerrainType.SnowPlain or
            XianxiaTerrainType.SnowRock => TownTerrainVisualFamily.Snow,

            XianxiaTerrainType.MountainRock or
            XianxiaTerrainType.MountainMoss or
            XianxiaTerrainType.MountainPlateau or
            XianxiaTerrainType.DesertSand or
            XianxiaTerrainType.DesertRock or
            XianxiaTerrainType.VolcanicRock or
            XianxiaTerrainType.AshGround or
            XianxiaTerrainType.AncientStone or
            XianxiaTerrainType.RuinedGround or
            XianxiaTerrainType.FloatingRock => TownTerrainVisualFamily.Rugged,

            XianxiaTerrainType.WetlandMud or
            XianxiaTerrainType.SwampGround => qiDensity >= 0.66f
                ? TownTerrainVisualFamily.Spirit
                : TownTerrainVisualFamily.Plain,

            _ => ResolveBiomeFallback(biome, qiDensity, corruption)
        };
    }

    private static TownTerrainVisualFamily ResolveBiomeFallback(
        XianxiaBiomeType biome,
        float qiDensity,
        float corruption)
    {
        if (biome == XianxiaBiomeType.SnowPeaks)
        {
            return TownTerrainVisualFamily.Snow;
        }

        if (corruption >= 0.58f ||
            biome is XianxiaBiomeType.MistyMountains or
            XianxiaBiomeType.JadeHighlands or
            XianxiaBiomeType.DesertBadlands or
            XianxiaBiomeType.VolcanicWastes or
            XianxiaBiomeType.AncientRuinsLand)
        {
            return TownTerrainVisualFamily.Rugged;
        }

        if (qiDensity >= 0.66f ||
            biome is XianxiaBiomeType.BambooValley or
            XianxiaBiomeType.SacredForest or
            XianxiaBiomeType.CrystalFields or
            XianxiaBiomeType.FloatingIsles)
        {
            return TownTerrainVisualFamily.Spirit;
        }

        return TownTerrainVisualFamily.Plain;
    }

    private static bool IsDeepWater(XianxiaWaterType water)
    {
        return water is XianxiaWaterType.ClearLake or
            XianxiaWaterType.LotusLake or
            XianxiaWaterType.MistLake or
            XianxiaWaterType.SpiritLake or
            XianxiaWaterType.CrystalLake or
            XianxiaWaterType.FloatingLake or
            XianxiaWaterType.SkyReflectionLake;
    }
}
