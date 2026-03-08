using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed partial class XianxiaWorldGeneratorSystem
{
    private readonly XianxiaWorldGenerationConfigSystem _configSystem = new();

    private static readonly AxialCoord[] NeighborDirections =
    [
        new(1, 0),
        new(1, -1),
        new(0, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1)
    ];

    private const int MaxMountainRangeLength = 18;
    private const int MaxRiverLength = 30;
    private const int MaxDragonVeinLength = 42;

    public XianxiaWorldMapData Generate()
    {
        return Generate(_configSystem.GetConfig());
    }

    public XianxiaWorldMapData Generate(XianxiaWorldGenerationConfig config)
    {
        var random = new Random(config.Seed);
        var worldMap = new XianxiaWorldMapData
        {
            Seed = config.Seed,
            Width = config.Width,
            Height = config.Height
        };

        var cells = CreateCells(worldMap, config);
        GenerateBaseFields(cells, config);
        GenerateMountainRanges(cells, random, config);
        GenerateSurfaceWater(cells, config);
        GenerateRivers(worldMap, cells, random, config);
        MarkCliffs(cells, config);
        GenerateDragonVeins(worldMap, cells, random, config);
        AssignSpiritualZones(cells, config);
        GenerateResources(cells, config);
        GenerateWonders(worldMap, cells, random, config);
        GenerateSectCandidates(worldMap, cells, config);
        GenerateSites(worldMap, cells, random, config);
        GenerateRoads(worldMap, cells);
        AssignOverlays(cells, config);
        ResolveRenderData(cells, config);

        return worldMap;
    }

    public StrategicMapDefinition GenerateStrategicDefinition()
    {
        return GenerateStrategicDefinition(_configSystem.GetConfig(), out _);
    }

    public StrategicMapDefinition GenerateStrategicDefinition(out XianxiaWorldMapData worldMap)
    {
        return GenerateStrategicDefinition(_configSystem.GetConfig(), out worldMap);
    }

    public StrategicMapDefinition GenerateStrategicDefinition(XianxiaWorldGenerationConfig config, out XianxiaWorldMapData worldMap)
    {
        worldMap = Generate(config);
        return BuildStrategicDefinition(worldMap, config);
    }

    private static Dictionary<(int Q, int R), CellContext> CreateCells(XianxiaWorldMapData worldMap, XianxiaWorldGenerationConfig config)
    {
        var cells = new Dictionary<(int Q, int R), CellContext>(config.Width * config.Height);

        for (var row = 0; row < config.Height; row++)
        {
            for (var column = 0; column < config.Width; column++)
            {
                var q = column - (row >> 1);
                var cell = new XianxiaHexCellData
                {
                    Coord = new HexAxialCoordData
                    {
                        Q = q,
                        R = row
                    }
                };

                worldMap.Cells.Add(cell);
                cells[(q, row)] = new CellContext(cell);
            }
        }

        return cells;
    }

    private static void GenerateBaseFields(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            var coord = new AxialCoord(context.Cell.Coord.Q, context.Cell.Coord.R);
            var plane = ToPlane(coord);

            var continent = FractalNoise(plane.X * 0.065f, plane.Y * 0.065f, config.Seed + 11, 4, 0.52f);
            var ridgeSource = FractalNoise(plane.X * 0.110f, plane.Y * 0.110f, config.Seed + 29, 3, 0.55f);
            var ridges = 1f - Mathf.Abs((ridgeSource * 2f) - 1f);
            var local = FractalNoise(plane.X * 0.200f, plane.Y * 0.200f, config.Seed + 43, 4, 0.56f);
            var height01 = Mathf.Clamp((continent * 0.55f) + (ridges * 0.25f) + (local * 0.20f), 0f, 1f);

            var latitude = config.Height <= 1
                ? 0.5f
                : context.Cell.Coord.R / (float)Math.Max(config.Height - 1, 1);
            var equatorWarmth = 1f - Mathf.Abs((latitude * 2f) - 1f);

            var temperatureNoise = FractalNoise(plane.X * 0.085f, plane.Y * 0.085f, config.Seed + 61, 3, 0.48f);
            var moistureNoise = FractalNoise(plane.X * 0.090f, plane.Y * 0.090f, config.Seed + 79, 4, 0.50f);
            var qiNoise = FractalNoise(plane.X * 0.140f, plane.Y * 0.140f, config.Seed + 97, 4, 0.54f);
            var fireNoise = FractalNoise(plane.X * 0.115f, plane.Y * 0.115f, config.Seed + 113, 3, 0.52f);
            var ancientNoise = FractalNoise(plane.X * 0.130f, plane.Y * 0.130f, config.Seed + 139, 3, 0.50f);
            var bambooNoise = FractalNoise(plane.X * 0.160f, plane.Y * 0.160f, config.Seed + 151, 3, 0.54f);
            var crystalNoise = FractalNoise(plane.X * 0.155f, plane.Y * 0.155f, config.Seed + 173, 4, 0.46f);
            var skyNoise = FractalNoise(plane.X * 0.090f, plane.Y * 0.090f, config.Seed + 191, 2, 0.60f);

            var temperature = Mathf.Clamp(
                config.BaseTemperature +
                (equatorWarmth * 0.34f) -
                (height01 * 0.32f) +
                ((temperatureNoise - 0.5f) * 0.18f),
                0f,
                1f);

            var moisture = Mathf.Clamp(
                config.BaseMoisture +
                ((moistureNoise - 0.5f) * 0.46f) +
                ((1f - height01) * 0.18f),
                0f,
                1f);

            var fertility = Mathf.Clamp(
                (moisture * 0.62f) +
                ((1f - Mathf.Abs(temperature - 0.55f)) * 0.24f) +
                ((1f - height01) * 0.14f),
                0f,
                1f);

            var corruption = config.CorruptionEnabled
                ? Mathf.Clamp((fireNoise * 0.46f) + (ancientNoise * 0.34f) - (moisture * 0.10f), 0f, 1f)
                : 0f;

            var qiDensity = Mathf.Clamp(
                (qiNoise * 0.44f) +
                (height01 * 0.18f) +
                (moisture * 0.14f) +
                ((1f - corruption) * 0.16f) +
                (crystalNoise * 0.08f),
                0f,
                1f);

            context.Height01 = height01;
            context.FirePotential = fireNoise;
            context.AncientPotential = ancientNoise;
            context.BambooPotential = bambooNoise;
            context.CrystalPotential = crystalNoise;
            context.SkyPotential = skyNoise;

            context.Cell.Height = (int)Mathf.Round(height01 * 100f);
            context.Cell.Temperature = temperature;
            context.Cell.Moisture = moisture;
            context.Cell.Fertility = fertility;
            context.Cell.Corruption = corruption;
            context.Cell.QiDensity = qiDensity;
            context.Cell.MonsterThreat = Mathf.Clamp((corruption * 0.58f) + (height01 * 0.12f) + ((1f - fertility) * 0.12f), 0f, 1f);
            context.Cell.Biome = ResolveBiome(context, config);
            context.Cell.Terrain = ResolveTerrain(context);
            context.Cell.ElementAffinity = ResolveBiomeElement(context.Cell.Biome);
            context.Cell.IsPassable = context.Cell.Biome != XianxiaBiomeType.FloatingIsles || config.FloatingIslesEnabled;
        }
    }

    private static XianxiaBiomeType ResolveBiome(CellContext context, XianxiaWorldGenerationConfig config)
    {
        if (config.FloatingIslesEnabled && context.SkyPotential > 0.86f && context.Height01 > 0.88f && context.Cell.QiDensity > 0.72f)
        {
            return XianxiaBiomeType.FloatingIsles;
        }

        if (context.Height01 > 0.84f && context.Cell.Temperature < 0.34f)
        {
            return XianxiaBiomeType.SnowPeaks;
        }

        if (context.FirePotential > 0.80f && context.Height01 > 0.60f)
        {
            return XianxiaBiomeType.VolcanicWastes;
        }

        if (context.AncientPotential > 0.76f && context.Height01 is > 0.32f and < 0.72f)
        {
            return XianxiaBiomeType.AncientRuinsLand;
        }

        if (context.CrystalPotential > 0.72f && context.Cell.QiDensity > 0.68f)
        {
            return XianxiaBiomeType.CrystalFields;
        }

        if (context.Height01 > 0.68f && context.Cell.Moisture > 0.58f)
        {
            return XianxiaBiomeType.MistyMountains;
        }

        if (context.Height01 > 0.62f && context.Cell.QiDensity > 0.60f)
        {
            return XianxiaBiomeType.JadeHighlands;
        }

        if (context.BambooPotential > 0.72f && context.Cell.Moisture > 0.62f && context.Cell.Temperature is > 0.42f and < 0.72f)
        {
            return XianxiaBiomeType.BambooValley;
        }

        if (context.Cell.QiDensity > 0.72f && context.Cell.Moisture > 0.58f)
        {
            return XianxiaBiomeType.SacredForest;
        }

        if (context.Cell.Temperature > 0.72f && context.Cell.Moisture < 0.30f)
        {
            return XianxiaBiomeType.DesertBadlands;
        }

        if (context.Cell.Moisture > 0.72f && context.Cell.Corruption > 0.42f)
        {
            return XianxiaBiomeType.SpiritSwamps;
        }

        return XianxiaBiomeType.TemperatePlains;
    }

    private static XianxiaTerrainType ResolveTerrain(CellContext context)
    {
        return context.Cell.Biome switch
        {
            XianxiaBiomeType.TemperatePlains => context.Cell.Moisture > 0.62f
                ? XianxiaTerrainType.GrassLush
                : context.Cell.Fertility > 0.58f
                    ? XianxiaTerrainType.WildflowerMeadow
                    : XianxiaTerrainType.GrassSparse,
            XianxiaBiomeType.BambooValley => context.Cell.Moisture > 0.76f ? XianxiaTerrainType.WetlandMud : XianxiaTerrainType.BambooGround,
            XianxiaBiomeType.MistyMountains => context.Cell.Moisture > 0.60f ? XianxiaTerrainType.MountainMoss : XianxiaTerrainType.MountainRock,
            XianxiaBiomeType.SacredForest => context.Cell.QiDensity > 0.78f ? XianxiaTerrainType.AncientForestFloor : XianxiaTerrainType.ForestGround,
            XianxiaBiomeType.JadeHighlands => context.CrystalPotential > 0.64f ? XianxiaTerrainType.CrystalGround : XianxiaTerrainType.MountainPlateau,
            XianxiaBiomeType.SnowPeaks => context.Cell.Height > 92 ? XianxiaTerrainType.SnowRock : XianxiaTerrainType.SnowPlain,
            XianxiaBiomeType.CrystalFields => context.Cell.QiDensity > 0.76f ? XianxiaTerrainType.SpiritSoil : XianxiaTerrainType.CrystalGround,
            XianxiaBiomeType.VolcanicWastes => context.FirePotential > 0.88f ? XianxiaTerrainType.VolcanicRock : XianxiaTerrainType.AshGround,
            XianxiaBiomeType.SpiritSwamps => context.Cell.Moisture > 0.78f ? XianxiaTerrainType.SwampGround : XianxiaTerrainType.WetlandMud,
            XianxiaBiomeType.AncientRuinsLand => context.AncientPotential > 0.82f ? XianxiaTerrainType.AncientStone : XianxiaTerrainType.RuinedGround,
            XianxiaBiomeType.DesertBadlands => context.Cell.Fertility < 0.22f ? XianxiaTerrainType.DesertRock : XianxiaTerrainType.DesertSand,
            XianxiaBiomeType.FloatingIsles => context.SkyPotential > 0.88f ? XianxiaTerrainType.CloudGround : XianxiaTerrainType.FloatingRock,
            _ => XianxiaTerrainType.GrassSparse
        };
    }

    private static XianxiaElementType ResolveBiomeElement(XianxiaBiomeType biome)
    {
        return biome switch
        {
            XianxiaBiomeType.BambooValley => XianxiaElementType.Wood,
            XianxiaBiomeType.SacredForest => XianxiaElementType.Wood,
            XianxiaBiomeType.MistyMountains => XianxiaElementType.Earth,
            XianxiaBiomeType.JadeHighlands => XianxiaElementType.Earth,
            XianxiaBiomeType.SnowPeaks => XianxiaElementType.Water,
            XianxiaBiomeType.CrystalFields => XianxiaElementType.Metal,
            XianxiaBiomeType.VolcanicWastes => XianxiaElementType.Fire,
            XianxiaBiomeType.SpiritSwamps => XianxiaElementType.Yin,
            XianxiaBiomeType.AncientRuinsLand => XianxiaElementType.Yin,
            XianxiaBiomeType.DesertBadlands => XianxiaElementType.Earth,
            XianxiaBiomeType.FloatingIsles => XianxiaElementType.Yang,
            _ => XianxiaElementType.Earth
        };
    }

    private static void GenerateMountainRanges(Dictionary<(int Q, int R), CellContext> cells, Random random, XianxiaWorldGenerationConfig config)
    {
        var candidates = new List<CellContext>(cells.Values);
        candidates.Sort((left, right) => right.Height01.CompareTo(left.Height01));

        var usedOrigins = new List<AxialCoord>();
        var rangeCount = random.Next(config.MountainRangeCountMin, config.MountainRangeCountMax + 1);

        foreach (var candidate in candidates)
        {
            if (usedOrigins.Count >= rangeCount)
            {
                break;
            }

            if (candidate.Height01 < 0.56f || candidate.Cell.Biome == XianxiaBiomeType.FloatingIsles)
            {
                continue;
            }

            var coord = ToCoord(candidate.Cell);
            if (!IsFarEnough(coord, usedOrigins, 7))
            {
                continue;
            }

            usedOrigins.Add(coord);
            GrowMountainRange(cells, random, coord);
        }
    }

    private static void GrowMountainRange(
        Dictionary<(int Q, int R), CellContext> cells,
        Random random,
        AxialCoord start)
    {
        var current = start;
        var heading = random.Next(NeighborDirections.Length);
        var used = new HashSet<(int Q, int R)>();
        var length = random.Next(8, MaxMountainRangeLength + 1);

        for (var step = 0; step < length; step++)
        {
            if (!cells.TryGetValue((current.Q, current.R), out var context) || !used.Add((current.Q, current.R)))
            {
                break;
            }

            RaiseMountainCell(context, step == 0 ? 0.16f : 0.11f);
            foreach (var neighbor in NeighborDirections)
            {
                if (cells.TryGetValue((current.Q + neighbor.Q, current.R + neighbor.R), out var neighborContext))
                {
                    RaiseMountainCell(neighborContext, 0.035f);
                }
            }

            var next = PickNextMountainCoord(cells, random, current, heading, used);
            if (next == null)
            {
                break;
            }

            heading = next.Value.NextHeading;
            current = next.Value.Coord;
        }
    }

    private static void RaiseMountainCell(CellContext context, float amount)
    {
        context.Height01 = Mathf.Clamp(context.Height01 + amount, 0f, 1f);
        context.Cell.Height = (int)Mathf.Round(context.Height01 * 100f);
        context.Cell.QiDensity = Mathf.Clamp(context.Cell.QiDensity + (amount * 0.45f), 0f, 1f);
        context.Cell.Fertility = Mathf.Clamp(context.Cell.Fertility - (amount * 0.18f), 0f, 1f);

        if (context.Cell.Temperature < 0.34f && context.Height01 > 0.82f)
        {
            context.Cell.Biome = XianxiaBiomeType.SnowPeaks;
            context.Cell.Terrain = XianxiaTerrainType.SnowRock;
            context.Cell.ElementAffinity = XianxiaElementType.Water;
            return;
        }

        if (context.CrystalPotential > 0.74f && context.Cell.QiDensity > 0.66f)
        {
            context.Cell.Biome = XianxiaBiomeType.CrystalFields;
            context.Cell.Terrain = XianxiaTerrainType.CrystalGround;
            context.Cell.ElementAffinity = XianxiaElementType.Metal;
            return;
        }

        context.Cell.Biome = context.Cell.QiDensity > 0.62f
            ? XianxiaBiomeType.JadeHighlands
            : XianxiaBiomeType.MistyMountains;
        context.Cell.Terrain = context.Cell.Moisture > 0.48f
            ? XianxiaTerrainType.MountainMoss
            : XianxiaTerrainType.MountainRock;
        context.Cell.ElementAffinity = context.Cell.QiDensity > 0.62f
            ? XianxiaElementType.Earth
            : XianxiaElementType.Metal;
    }

    private static void GenerateSurfaceWater(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            if (context.Height01 > config.LakeThreshold)
            {
                continue;
            }

            if (context.Cell.Moisture > 0.72f)
            {
                context.Cell.Water = context.Cell.QiDensity > 0.64f
                    ? XianxiaWaterType.SpiritLake
                    : XianxiaWaterType.ClearLake;
                context.Cell.IsLake = true;
                context.Cell.IsPassable = false;
                continue;
            }

            if (context.Cell.Moisture > 0.60f)
            {
                context.Cell.Water = XianxiaWaterType.MarshWater;
                context.Cell.Terrain = XianxiaTerrainType.WetlandMud;
                context.Cell.Biome = XianxiaBiomeType.SpiritSwamps;
            }
        }
    }

    private static void GenerateRivers(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), CellContext> cells,
        Random random,
        XianxiaWorldGenerationConfig config)
    {
        var sources = SelectSpacedCells(
            cells.Values,
            context => context.Height01 > 0.68f && context.Cell.Moisture > 0.40f && context.Cell.Water == XianxiaWaterType.None,
            context => (context.Height01 * 0.68f) + (context.Cell.Moisture * 0.32f),
            random.Next(config.RiverSourceCountMin, config.RiverSourceCountMax + 1),
            6);

        var riverIndex = 1;
        foreach (var source in sources)
        {
            var path = BuildRiverPath(cells, source, random);
            if (path.Count < 3)
            {
                continue;
            }

            source.Cell.IsRiverSource = true;
            if (source.Cell.Water == XianxiaWaterType.None)
            {
                source.Cell.Water = source.Cell.QiDensity > 0.70f
                    ? XianxiaWaterType.SacredPool
                    : XianxiaWaterType.MountainSpring;
            }

            var riverData = new RiverPathData
            {
                Id = $"river_{riverIndex:D2}",
                SourceCoord = CloneCoord(source.Cell.Coord),
                MouthCoord = CloneCoord(path[^1].Cell.Coord),
                FeedsSpiritZone = source.Cell.QiDensity > 0.62f
            };

            foreach (var step in path)
            {
                riverData.Nodes.Add(new XianxiaPathNodeData
                {
                    Coord = CloneCoord(step.Cell.Coord),
                    Weight = step.Cell.Height
                });
            }

            worldMap.Rivers.Add(riverData);
            riverIndex++;
        }
    }

    private static List<CellContext> BuildRiverPath(
        Dictionary<(int Q, int R), CellContext> cells,
        CellContext source,
        Random random)
    {
        var path = new List<CellContext> { source };
        var visited = new HashSet<(int Q, int R)> { (source.Cell.Coord.Q, source.Cell.Coord.R) };
        var current = source;

        for (var step = 0; step < MaxRiverLength; step++)
        {
            var next = PickNextRiverCell(cells, current, random, visited);
            if (next == null)
            {
                break;
            }

            ConnectRiverCells(current, next);
            path.Add(next);
            visited.Add((next.Cell.Coord.Q, next.Cell.Coord.R));
            current = next;

            if (current.Cell.Water != XianxiaWaterType.None || current.Height01 < 0.18f)
            {
                if (current.Cell.Water == XianxiaWaterType.None)
                {
                    current.Cell.Water = current.Cell.QiDensity > 0.66f
                        ? XianxiaWaterType.SpiritLake
                        : XianxiaWaterType.ClearLake;
                    current.Cell.IsLake = true;
                    current.Cell.IsPassable = false;
                }

                break;
            }
        }

        return path;
    }

    private static void ConnectRiverCells(CellContext from, CellContext to)
    {
        var direction = ResolveDirection(ToCoord(from.Cell), ToCoord(to.Cell));
        if (direction == HexDirectionMask.None)
        {
            return;
        }

        from.Cell.RiverMask |= direction;
        to.Cell.RiverMask |= Opposite(direction);
    }

    private static void MarkCliffs(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            var coord = ToCoord(context.Cell);
            foreach (var neighbor in NeighborDirections)
            {
                if (!cells.TryGetValue((coord.Q + neighbor.Q, coord.R + neighbor.R), out var neighborContext))
                {
                    continue;
                }

                var heightDiff = context.Cell.Height - neighborContext.Cell.Height;
                if (heightDiff < config.CliffThreshold)
                {
                    continue;
                }

                var mask = ResolveDirection(coord, ToCoord(neighborContext.Cell));
                context.Cell.CliffMask |= mask;
                context.Cell.Cliff = ResolveCliffType(context);
            }
        }
    }

    private static NextStep? PickNextMountainCoord(
        Dictionary<(int Q, int R), CellContext> cells,
        Random random,
        AxialCoord current,
        int heading,
        HashSet<(int Q, int R)> used)
    {
        NextStep? best = null;
        var bestScore = float.MinValue;

        for (var delta = -1; delta <= 1; delta++)
        {
            var directionIndex = (heading + delta + NeighborDirections.Length) % NeighborDirections.Length;
            var direction = NeighborDirections[directionIndex];
            var candidateCoord = new AxialCoord(current.Q + direction.Q, current.R + direction.R);
            if (used.Contains((candidateCoord.Q, candidateCoord.R)) ||
                !cells.TryGetValue((candidateCoord.Q, candidateCoord.R), out var candidate))
            {
                continue;
            }

            var score = candidate.Height01 + (candidate.Cell.QiDensity * 0.24f) + ((1f - candidate.Cell.Corruption) * 0.12f) + ((float)random.NextDouble() * 0.08f);
            if (score > bestScore)
            {
                bestScore = score;
                best = new NextStep(candidateCoord, directionIndex);
            }
        }

        return best;
    }

    private static CellContext? PickNextRiverCell(
        Dictionary<(int Q, int R), CellContext> cells,
        CellContext current,
        Random random,
        HashSet<(int Q, int R)> visited)
    {
        CellContext? best = null;
        var bestScore = float.MaxValue;
        var currentCoord = ToCoord(current.Cell);

        foreach (var direction in NeighborDirections)
        {
            var candidateCoord = new AxialCoord(currentCoord.Q + direction.Q, currentCoord.R + direction.R);
            if (visited.Contains((candidateCoord.Q, candidateCoord.R)) ||
                !cells.TryGetValue((candidateCoord.Q, candidateCoord.R), out var candidate))
            {
                continue;
            }

            var downhillBias = candidate.Height01 - current.Height01;
            var score =
                (candidate.Height01 * 0.62f) +
                ((1f - candidate.Cell.Moisture) * 0.10f) +
                (Math.Max(downhillBias, 0f) * 0.34f) +
                ((float)random.NextDouble() * 0.04f);

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private static void GenerateDragonVeins(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), CellContext> cells,
        Random random,
        XianxiaWorldGenerationConfig config)
    {
        var sourcePool = SelectSpacedCells(
            cells.Values,
            context => context.Height01 > 0.70f && context.Cell.QiDensity > 0.54f && context.Cell.Water == XianxiaWaterType.None,
            context => (context.Height01 * 0.56f) + (context.Cell.QiDensity * 0.44f),
            random.Next(config.MajorDragonVeinCountMin, config.MajorDragonVeinCountMax + 1),
            8);

        var sinkPool = SelectSpacedCells(
            cells.Values,
            context => context.Height01 > 0.28f && context.Height01 < 0.72f && context.Cell.QiDensity > 0.52f && context.Cell.Corruption < 0.72f,
            context => (context.Cell.QiDensity * 0.58f) + (context.Cell.Fertility * 0.24f) + ((1f - context.Cell.Corruption) * 0.18f),
            Math.Max(sourcePool.Count, config.MajorDragonVeinCountMin),
            6);

        var traversalCounts = new Dictionary<(int Q, int R), int>();
        var veinIndex = 1;

        for (var index = 0; index < sourcePool.Count; index++)
        {
            var source = sourcePool[index];
            var sink = sinkPool[Math.Min(index, sinkPool.Count - 1)];
            var path = BuildDragonVeinPath(cells, source, sink, random);
            if (path.Count < 4)
            {
                continue;
            }

            RegisterDragonVein(worldMap, traversalCounts, veinIndex, true, path);
            veinIndex++;
        }

        var minorSources = SelectSpacedCells(
            cells.Values,
            context => context.Cell.QiDensity > 0.62f && context.Cell.SpiritualZone == XianxiaSpiritualZoneType.None,
            context => context.Cell.QiDensity,
            random.Next(config.MinorDragonVeinCountMin, config.MinorDragonVeinCountMax + 1),
            5);

        foreach (var source in minorSources)
        {
            var sink = FindNearestDragonVeinTarget(worldMap, source);
            if (sink == null || !cells.TryGetValue((sink.Value.Q, sink.Value.R), out var sinkContext))
            {
                continue;
            }

            var path = BuildDragonVeinPath(cells, source, sinkContext, random);
            if (path.Count < 3)
            {
                continue;
            }

            RegisterDragonVein(worldMap, traversalCounts, veinIndex, false, path);
            veinIndex++;
        }

        foreach (var cell in cells.Values)
        {
            if (!traversalCounts.TryGetValue((cell.Cell.Coord.Q, cell.Cell.Coord.R), out var count))
            {
                continue;
            }

            if (count >= 3)
            {
                cell.Cell.IsDragonVeinCore = true;
                cell.Cell.SpiritualZone = XianxiaSpiritualZoneType.DragonNode;
                cell.Cell.QiDensity = Mathf.Clamp(cell.Cell.QiDensity + 0.18f, 0f, 1f);
            }
            else if (count >= 2 && cell.Cell.SpiritualZone == XianxiaSpiritualZoneType.None)
            {
                cell.Cell.SpiritualZone = XianxiaSpiritualZoneType.DragonNode;
                cell.Cell.QiDensity = Mathf.Clamp(cell.Cell.QiDensity + 0.10f, 0f, 1f);
            }
        }
    }

    private static List<CellContext> BuildDragonVeinPath(
        Dictionary<(int Q, int R), CellContext> cells,
        CellContext source,
        CellContext sink,
        Random random)
    {
        var path = new List<CellContext> { source };
        var current = source;
        var targetCoord = ToCoord(sink.Cell);
        var visited = new HashSet<(int Q, int R)> { (source.Cell.Coord.Q, source.Cell.Coord.R) };

        for (var step = 0; step < MaxDragonVeinLength; step++)
        {
            if (current.Cell.Coord.Q == sink.Cell.Coord.Q && current.Cell.Coord.R == sink.Cell.Coord.R)
            {
                break;
            }

            CellContext? best = null;
            var bestScore = float.MaxValue;

            foreach (var direction in NeighborDirections)
            {
                var candidateCoord = new AxialCoord(current.Cell.Coord.Q + direction.Q, current.Cell.Coord.R + direction.R);
                if (visited.Contains((candidateCoord.Q, candidateCoord.R)) ||
                    !cells.TryGetValue((candidateCoord.Q, candidateCoord.R), out var candidate))
                {
                    continue;
                }

                var distance = HexDistance(candidateCoord, targetCoord);
                var score =
                    (distance * 0.56f) -
                    (candidate.Cell.QiDensity * 0.26f) -
                    (candidate.Height01 * 0.12f) +
                    (candidate.Cell.Corruption * 0.22f) +
                    ((float)random.NextDouble() * 0.05f);

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            if (best == null)
            {
                break;
            }

            visited.Add((best.Cell.Coord.Q, best.Cell.Coord.R));
            path.Add(best);
            current = best;
        }

        return path;
    }

    private static void RegisterDragonVein(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), int> traversalCounts,
        int veinIndex,
        bool isMajor,
        IReadOnlyList<CellContext> path)
    {
        var elementAffinity = ResolveVeinElement(path[0].Cell, path[^1].Cell);
        var vein = new DragonVeinPathData
        {
            Id = $"vein_{veinIndex:D2}",
            IsMajor = isMajor,
            ElementAffinity = elementAffinity,
            SourceCoord = CloneCoord(path[0].Cell.Coord),
            SinkCoord = CloneCoord(path[^1].Cell.Coord)
        };

        foreach (var step in path)
        {
            vein.Nodes.Add(new XianxiaPathNodeData
            {
                Coord = CloneCoord(step.Cell.Coord),
                Weight = step.Cell.QiDensity
            });

            traversalCounts[(step.Cell.Coord.Q, step.Cell.Coord.R)] =
                traversalCounts.TryGetValue((step.Cell.Coord.Q, step.Cell.Coord.R), out var count)
                    ? count + 1
                    : 1;

            step.Cell.SpiritualZone = isMajor
                ? XianxiaSpiritualZoneType.MajorSpiritVein
                : step.Cell.SpiritualZone == XianxiaSpiritualZoneType.MajorSpiritVein
                    ? XianxiaSpiritualZoneType.MajorSpiritVein
                    : XianxiaSpiritualZoneType.MinorSpiritVein;
            step.Cell.QiDensity = Mathf.Clamp(step.Cell.QiDensity + (isMajor ? 0.18f : 0.10f), 0f, 1f);
            step.Cell.ElementAffinity = elementAffinity == XianxiaElementType.None
                ? step.Cell.ElementAffinity
                : elementAffinity;
        }

        worldMap.DragonVeins.Add(vein);
    }

    private static AxialCoord? FindNearestDragonVeinTarget(XianxiaWorldMapData worldMap, CellContext source)
    {
        AxialCoord? best = null;
        var bestDistance = int.MaxValue;
        var sourceCoord = ToCoord(source.Cell);

        foreach (var vein in worldMap.DragonVeins)
        {
            foreach (var node in vein.Nodes)
            {
                var coord = new AxialCoord(node.Coord.Q, node.Coord.R);
                var distance = HexDistance(sourceCoord, coord);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = coord;
                }
            }
        }

        return best;
    }

    private static void AssignSpiritualZones(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            if (context.Cell.SpiritualZone is XianxiaSpiritualZoneType.MajorSpiritVein or XianxiaSpiritualZoneType.MinorSpiritVein or XianxiaSpiritualZoneType.DragonNode)
            {
                continue;
            }

            if (context.Cell.Water != XianxiaWaterType.None && context.Cell.QiDensity > 0.72f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.SpiritPool;
                context.Cell.ElementAffinity = XianxiaElementType.Water;
                continue;
            }

            if (context.Cell.Biome == XianxiaBiomeType.AncientRuinsLand && context.Cell.QiDensity > 0.66f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.AncientCultivationGround;
                context.Cell.ElementAffinity = XianxiaElementType.Yin;
                continue;
            }

            if (config.QiStormsEnabled && context.Cell.QiDensity > 0.84f && context.Cell.Corruption < 0.34f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.QiStormField;
                context.Cell.ElementAffinity = XianxiaElementType.Yang;
                continue;
            }

            if (context.Cell.Corruption > 0.78f && context.Cell.QiDensity > 0.62f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.ChaosEnergyZone;
                context.Cell.ElementAffinity = XianxiaElementType.Chaos;
                continue;
            }

            if (context.Cell.QiDensity > 0.74f &&
                context.Cell.Temperature is > 0.42f and < 0.62f &&
                context.Cell.Moisture is > 0.42f and < 0.62f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.FiveElementsZone;
                context.Cell.ElementAffinity = XianxiaElementType.None;
                continue;
            }

            if (context.Cell.Temperature < 0.32f && context.Cell.QiDensity > 0.60f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.YinEnergyZone;
                context.Cell.ElementAffinity = XianxiaElementType.Yin;
                continue;
            }

            if (context.Cell.Temperature > 0.72f && context.Cell.QiDensity > 0.60f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.YangEnergyZone;
                context.Cell.ElementAffinity = XianxiaElementType.Yang;
                continue;
            }

            if (context.Cell.QiDensity > 0.58f)
            {
                context.Cell.SpiritualZone = ResolveElementVein(context.Cell.ElementAffinity);
                continue;
            }

            if (context.Cell.QiDensity > 0.50f)
            {
                context.Cell.SpiritualZone = XianxiaSpiritualZoneType.QiRichGround;
            }
        }
    }

    private static void GenerateResources(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            var roll = Hash01(context.Cell.Coord.Q, context.Cell.Coord.R, config.Seed + 701);
            if (roll < 0.54f)
            {
                continue;
            }

            var weighted = new List<WeightedResource>();
            AddResourceCandidates(weighted, context);
            context.Cell.Resource = PickWeightedResource(weighted, Hash01(context.Cell.Coord.Q, context.Cell.Coord.R, config.Seed + 733));
        }
    }

    private static void GenerateWonders(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), CellContext> cells,
        Random random,
        XianxiaWorldGenerationConfig config)
    {
        var candidates = new List<WonderCandidate>();
        foreach (var context in cells.Values)
        {
            var wonderType = ResolveWonderType(context, config, out var score);
            if (wonderType == XianxiaWonderType.None || score < 0.72f)
            {
                continue;
            }

            candidates.Add(new WonderCandidate(context, wonderType, score));
        }

        candidates.Sort((left, right) => right.Score.CompareTo(left.Score));
        var selectedCoords = new List<AxialCoord>();
        var wonderCount = random.Next(config.WonderCountMin, config.WonderCountMax + 1);

        foreach (var candidate in candidates)
        {
            if (worldMap.Wonders.Count >= wonderCount)
            {
                break;
            }

            var coord = ToCoord(candidate.Context.Cell);
            if (!IsFarEnough(coord, selectedCoords, 5))
            {
                continue;
            }

            selectedCoords.Add(coord);
            candidate.Context.Cell.Wonder = candidate.Wonder;
            candidate.Context.Cell.QiDensity = Mathf.Clamp(candidate.Context.Cell.QiDensity + 0.18f, 0f, 1f);
            candidate.Context.Cell.ElementAffinity = ResolveWonderElement(candidate.Wonder, candidate.Context.Cell.ElementAffinity);

            worldMap.Wonders.Add(new WonderSiteData
            {
                Wonder = candidate.Wonder,
                Coord = CloneCoord(candidate.Context.Cell.Coord),
                InfluenceRadius = ResolveWonderInfluenceRadius(candidate.Wonder),
                QiBonus = 0.18f,
                ElementAffinity = candidate.Context.Cell.ElementAffinity
            });
        }
    }

    private static void GenerateSectCandidates(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), CellContext> cells,
        XianxiaWorldGenerationConfig config)
    {
        var scored = new List<SectCandidateSelection>();
        foreach (var context in cells.Values)
        {
            if (!context.Cell.IsPassable || context.Cell.Water != XianxiaWaterType.None || context.Cell.Wonder != XianxiaWonderType.None)
            {
                continue;
            }

            var score = ComputeSectCandidateScore(cells, worldMap, context, config);
            if (score < 0.45f)
            {
                continue;
            }

            scored.Add(new SectCandidateSelection(context, score));
        }

        scored.Sort((left, right) => right.Score.CompareTo(left.Score));
        var selectedCoords = new List<AxialCoord>();

        foreach (var candidate in scored)
        {
            if (worldMap.SectCandidates.Count >= config.SectCandidateCount)
            {
                break;
            }

            var coord = ToCoord(candidate.Context.Cell);
            if (!IsFarEnough(coord, selectedCoords, 4))
            {
                continue;
            }

            selectedCoords.Add(coord);
            candidate.Context.Cell.IsSectCandidate = true;

            worldMap.SectCandidates.Add(new SectCandidateSiteData
            {
                Coord = CloneCoord(candidate.Context.Cell.Coord),
                Score = candidate.Score,
                ElementAffinity = candidate.Context.Cell.ElementAffinity,
                NearbyResources = GatherNearbyResources(cells, candidate.Context, 3),
                NearbyWonders = GatherNearbyWonders(worldMap, candidate.Context, 6),
                PrimarySpiritualZone = candidate.Context.Cell.SpiritualZone,
                Defensibility = ComputeDefensibility(cells, candidate.Context),
                WaterAccess = ComputeWaterAccess(cells, candidate.Context),
                TravelConnectivity = ComputeTravelConnectivity(candidate.Context)
            });
        }
    }

    private static void GenerateSites(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), CellContext> cells,
        Random random,
        XianxiaWorldGenerationConfig config)
    {
        for (var index = 0; index < Math.Min(worldMap.SectCandidates.Count, 4); index++)
        {
            var candidate = worldMap.SectCandidates[index];
            if (!cells.TryGetValue((candidate.Coord.Q, candidate.Coord.R), out var context))
            {
                continue;
            }

            context.Cell.Structure = XianxiaStructureType.SectFoundation;
            worldMap.Sites.Add(new XianxiaSiteData
            {
                Role = XianxiaSiteRoleType.SectCandidate,
                Coord = CloneCoord(candidate.Coord),
                Structure = XianxiaStructureType.SectFoundation,
                Label = $"宗门候选 {index + 1}",
                Importance = 3
            });
        }

        var settlementPool = SelectSpacedCells(
            cells.Values,
            context => context.Cell.Fertility > 0.48f &&
                       context.Cell.Water == XianxiaWaterType.None &&
                       context.Cell.Wonder == XianxiaWonderType.None &&
                       context.Cell.Structure == XianxiaStructureType.None &&
                       !context.Cell.IsSectCandidate,
            context => (context.Cell.Fertility * 0.44f) + (ComputeWaterAccess(cells, context) * 0.28f) + ((1f - context.Cell.Corruption) * 0.28f),
            config.SettlementCount,
            4);

        for (var index = 0; index < settlementPool.Count; index++)
        {
            var structure = index % 2 == 0 ? XianxiaStructureType.VillageBase : XianxiaStructureType.MarketSquare;
            settlementPool[index].Cell.Structure = structure;
            worldMap.Sites.Add(new XianxiaSiteData
            {
                Role = XianxiaSiteRoleType.Settlement,
                Coord = CloneCoord(settlementPool[index].Cell.Coord),
                Structure = structure,
                Label = index % 2 == 0 ? $"附庸据点 {index + 1}" : $"坊市 {index + 1}",
                Importance = 2
            });
        }

        var ruinPool = SelectSpacedCells(
            cells.Values,
            context => context.Cell.Biome == XianxiaBiomeType.AncientRuinsLand &&
                       context.Cell.Structure == XianxiaStructureType.None &&
                       context.Cell.Wonder == XianxiaWonderType.None,
            context => (context.AncientPotential * 0.56f) + (context.Cell.Corruption * 0.18f) + (context.Cell.QiDensity * 0.26f),
            config.RuinCount,
            3);

        for (var index = 0; index < ruinPool.Count; index++)
        {
            var structure = index % 2 == 0 ? XianxiaStructureType.AncientCityRuins : XianxiaStructureType.RuinsPlatform;
            ruinPool[index].Cell.Structure = structure;
            worldMap.Sites.Add(new XianxiaSiteData
            {
                Role = XianxiaSiteRoleType.Ruin,
                Coord = CloneCoord(ruinPool[index].Cell.Coord),
                Structure = structure,
                Label = $"古遗迹 {index + 1}",
                Importance = 1
            });
        }
    }

    private static void GenerateRoads(
        XianxiaWorldMapData worldMap,
        Dictionary<(int Q, int R), CellContext> cells)
    {
        var hubs = new List<RoadHub>();
        foreach (var site in worldMap.Sites)
        {
            if (site.Role != XianxiaSiteRoleType.Settlement && site.Role != XianxiaSiteRoleType.SectCandidate)
            {
                continue;
            }

            if (!cells.TryGetValue((site.Coord.Q, site.Coord.R), out var context))
            {
                continue;
            }

            hubs.Add(new RoadHub(site, context));
        }

        if (hubs.Count < 2)
        {
            return;
        }

        hubs.Sort((left, right) =>
        {
            var importanceOrder = right.Site.Importance.CompareTo(left.Site.Importance);
            if (importanceOrder != 0)
            {
                return importanceOrder;
            }

            return left.Site.Label.CompareTo(right.Site.Label);
        });

        var connections = new HashSet<string>(StringComparer.Ordinal);
        var connected = new List<RoadHub> { hubs[0] };

        for (var index = 1; index < hubs.Count; index++)
        {
            var current = hubs[index];
            RoadHub? nearest = null;
            var nearestDistance = int.MaxValue;

            foreach (var existing in connected)
            {
                var distance = HexDistance(ToCoord(current.Context.Cell), ToCoord(existing.Context.Cell));
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = existing;
                }
            }

            if (nearest.HasValue)
            {
                TryRegisterRoadConnection(cells, current.Context, nearest.Value.Context, connections);
            }

            connected.Add(current);
        }

        for (var index = 0; index < hubs.Count; index++)
        {
            RoadHub? bestExtra = null;
            var bestScore = float.MaxValue;

            for (var otherIndex = index + 1; otherIndex < hubs.Count; otherIndex++)
            {
                var current = hubs[index];
                var other = hubs[otherIndex];
                var key = BuildRoadConnectionKey(ToCoord(current.Context.Cell), ToCoord(other.Context.Cell));
                if (connections.Contains(key))
                {
                    continue;
                }

                var distance = HexDistance(ToCoord(current.Context.Cell), ToCoord(other.Context.Cell));
                if (distance < 4 || distance > 13)
                {
                    continue;
                }

                var score = distance - (Math.Min(current.Site.Importance, other.Site.Importance) * 0.5f);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestExtra = other;
                }
            }

            if (bestExtra.HasValue)
            {
                TryRegisterRoadConnection(cells, hubs[index].Context, bestExtra.Value.Context, connections);
            }
        }
    }

    private static void TryRegisterRoadConnection(
        Dictionary<(int Q, int R), CellContext> cells,
        CellContext from,
        CellContext to,
        HashSet<string> connections)
    {
        var fromCoord = ToCoord(from.Cell);
        var toCoord = ToCoord(to.Cell);
        var key = BuildRoadConnectionKey(fromCoord, toCoord);
        if (connections.Contains(key))
        {
            return;
        }

        var path = BuildRoadPath(cells, from, to);
        if (path.Count < 2)
        {
            return;
        }

        for (var index = 0; index < path.Count - 1; index++)
        {
            ConnectRoadCells(path[index], path[index + 1]);
        }

        connections.Add(key);
    }

    private static string BuildRoadConnectionKey(AxialCoord left, AxialCoord right)
    {
        if (left.R > right.R || (left.R == right.R && left.Q > right.Q))
        {
            (left, right) = (right, left);
        }

        return $"{left.Q},{left.R}->{right.Q},{right.R}";
    }

    private static List<CellContext> BuildRoadPath(
        Dictionary<(int Q, int R), CellContext> cells,
        CellContext source,
        CellContext target)
    {
        var start = ToCoord(source.Cell);
        var goal = ToCoord(target.Cell);
        var frontier = new PriorityQueue<AxialCoord, float>();
        var cameFrom = new Dictionary<(int Q, int R), AxialCoord>();
        var costSoFar = new Dictionary<(int Q, int R), float>
        {
            [(start.Q, start.R)] = 0f
        };

        frontier.Enqueue(start, 0f);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.Q == goal.Q && current.R == goal.R)
            {
                break;
            }

            foreach (var direction in NeighborDirections)
            {
                var next = new AxialCoord(current.Q + direction.Q, current.R + direction.R);
                if (!cells.TryGetValue((next.Q, next.R), out var candidate) || !IsRoadTraversable(candidate, next, goal))
                {
                    continue;
                }

                var newCost = costSoFar[(current.Q, current.R)] + ResolveRoadTraversalCost(candidate);
                if (costSoFar.TryGetValue((next.Q, next.R), out var existingCost) && newCost >= existingCost)
                {
                    continue;
                }

                costSoFar[(next.Q, next.R)] = newCost;
                cameFrom[(next.Q, next.R)] = current;
                frontier.Enqueue(next, newCost + HexDistance(next, goal));
            }
        }

        if (!costSoFar.ContainsKey((goal.Q, goal.R)))
        {
            return [];
        }

        var reversed = new List<CellContext>();
        var cursor = goal;
        while (true)
        {
            reversed.Add(cells[(cursor.Q, cursor.R)]);
            if (cursor.Q == start.Q && cursor.R == start.R)
            {
                break;
            }

            cursor = cameFrom[(cursor.Q, cursor.R)];
        }

        reversed.Reverse();
        return reversed;
    }

    private static bool IsRoadTraversable(CellContext context, AxialCoord coord, AxialCoord goal)
    {
        if (coord.Q == goal.Q && coord.R == goal.R)
        {
            return true;
        }

        if (!context.Cell.IsPassable || context.Cell.Water != XianxiaWaterType.None)
        {
            return false;
        }

        return true;
    }

    private static float ResolveRoadTraversalCost(CellContext context)
    {
        var cost = 1f;
        cost += context.Cell.CliffMask != HexDirectionMask.None ? 1.2f : 0f;
        cost += context.Cell.RiverMask != HexDirectionMask.None ? 0.9f : 0f;
        cost += context.Cell.Corruption * 0.8f;
        cost += context.Cell.Biome switch
        {
            XianxiaBiomeType.MistyMountains => 1.0f,
            XianxiaBiomeType.SnowPeaks => 1.1f,
            XianxiaBiomeType.SpiritSwamps => 1.2f,
            XianxiaBiomeType.DesertBadlands => 0.6f,
            XianxiaBiomeType.FloatingIsles => 2.4f,
            _ => 0.2f
        };
        cost += context.Cell.Terrain switch
        {
            XianxiaTerrainType.MountainRock => 0.9f,
            XianxiaTerrainType.MountainMoss => 0.8f,
            XianxiaTerrainType.MountainPlateau => 0.7f,
            XianxiaTerrainType.SwampGround => 1.3f,
            XianxiaTerrainType.WetlandMud => 0.9f,
            XianxiaTerrainType.SnowRock => 0.7f,
            XianxiaTerrainType.DesertSand => 0.4f,
            _ => 0f
        };
        cost -= context.Cell.RoadMask != HexDirectionMask.None ? 0.45f : 0f;
        return Math.Max(cost, 0.35f);
    }

    private static void ConnectRoadCells(CellContext from, CellContext to)
    {
        var direction = ResolveDirection(ToCoord(from.Cell), ToCoord(to.Cell));
        if (direction == HexDirectionMask.None)
        {
            return;
        }

        from.Cell.RoadMask |= direction;
        to.Cell.RoadMask |= Opposite(direction);
    }

    private static void AssignOverlays(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            if (context.Cell.Structure != XianxiaStructureType.None || context.Cell.Wonder != XianxiaWonderType.None)
            {
                continue;
            }

            var roll = Hash01(context.Cell.Coord.Q, context.Cell.Coord.R, config.Seed + 823);
            context.Cell.Overlay = ResolveOverlay(context, roll);
        }
    }

    private static void ResolveRenderData(Dictionary<(int Q, int R), CellContext> cells, XianxiaWorldGenerationConfig config)
    {
        foreach (var context in cells.Values)
        {
            var transitionMask = HexDirectionMask.None;
            foreach (var direction in NeighborDirections)
            {
                if (!cells.TryGetValue((context.Cell.Coord.Q + direction.Q, context.Cell.Coord.R + direction.R), out var neighborContext))
                {
                    continue;
                }

                if (neighborContext.Cell.Terrain != context.Cell.Terrain)
                {
                    transitionMask |= ResolveDirection(ToCoord(context.Cell), ToCoord(neighborContext.Cell));
                }
            }

            context.Cell.TransitionMask = transitionMask;
            context.Cell.Render.BaseTileKey = ToTileKey(context.Cell.Terrain);
            context.Cell.Render.TransitionTileKey = transitionMask == HexDirectionMask.None ? string.Empty : $"{ToTileKey(context.Cell.Terrain)}_transition";
            context.Cell.Render.WaterTileKey = context.Cell.Water != XianxiaWaterType.None
                ? ToTileKey(context.Cell.Water)
                : context.Cell.RiverMask != HexDirectionMask.None
                    ? "river_flow"
                    : string.Empty;
            context.Cell.Render.CliffTileKey = context.Cell.Cliff != XianxiaCliffType.None ? ToTileKey(context.Cell.Cliff) : string.Empty;
            context.Cell.Render.OverlayTileKey = context.Cell.Overlay != XianxiaOverlayType.None ? ToTileKey(context.Cell.Overlay) : string.Empty;
            context.Cell.Render.ResourceTileKey = context.Cell.Resource != XianxiaResourceType.None ? ToTileKey(context.Cell.Resource) : string.Empty;
            context.Cell.Render.SpiritualTileKey = context.Cell.SpiritualZone != XianxiaSpiritualZoneType.None ? ToTileKey(context.Cell.SpiritualZone) : string.Empty;
            context.Cell.Render.StructureTileKey = context.Cell.Structure != XianxiaStructureType.None ? ToTileKey(context.Cell.Structure) : string.Empty;
            context.Cell.Render.WonderTileKey = context.Cell.Wonder != XianxiaWonderType.None ? ToTileKey(context.Cell.Wonder) : string.Empty;
            context.Cell.Render.BiomeSkinKey = ToTileKey(context.Cell.Biome);
            context.Cell.Render.VariantIndex = (int)(Hash01(context.Cell.Coord.Q, context.Cell.Coord.R, config.Seed + 859) * 3.0f);
        }
    }

    private static List<CellContext> SelectSpacedCells(
        IEnumerable<CellContext> source,
        Func<CellContext, bool> predicate,
        Func<CellContext, float> scoreSelector,
        int targetCount,
        int spacing)
    {
        var scored = new List<CellContext>();
        foreach (var context in source)
        {
            if (predicate(context))
            {
                scored.Add(context);
            }
        }

        scored.Sort((left, right) => scoreSelector(right).CompareTo(scoreSelector(left)));
        var output = new List<CellContext>();

        foreach (var context in scored)
        {
            if (output.Count >= targetCount)
            {
                break;
            }

            if (!IsFarEnough(ToCoord(context.Cell), output, spacing))
            {
                continue;
            }

            output.Add(context);
        }

        return output;
    }

    private static bool IsFarEnough(AxialCoord coord, IReadOnlyList<AxialCoord> existing, int minimumDistance)
    {
        foreach (var other in existing)
        {
            if (HexDistance(coord, other) < minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFarEnough(AxialCoord coord, IReadOnlyList<CellContext> existing, int minimumDistance)
    {
        foreach (var other in existing)
        {
            if (HexDistance(coord, ToCoord(other.Cell)) < minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    private static float ComputeSectCandidateScore(
        Dictionary<(int Q, int R), CellContext> cells,
        XianxiaWorldMapData worldMap,
        CellContext context,
        XianxiaWorldGenerationConfig config)
    {
        var resourceDiversity = Math.Min(GatherNearbyResources(cells, context, 3).Count / 6f, 1f);
        var wonderProximity = Math.Min(GatherNearbyWonders(worldMap, context, 6).Count / 2f, 1f);
        var defensibility = ComputeDefensibility(cells, context);
        var waterAccess = ComputeWaterAccess(cells, context);
        var travelConnectivity = ComputeTravelConnectivity(context);

        return
            (config.SectQiWeight * context.Cell.QiDensity) +
            (config.SectResourceWeight * resourceDiversity) +
            (config.SectDefensibilityWeight * defensibility) +
            (config.SectWaterAccessWeight * waterAccess) +
            (config.SectWonderWeight * wonderProximity) +
            (config.SectConnectivityWeight * travelConnectivity) +
            (config.SectFertilityWeight * context.Cell.Fertility) -
            (config.SectCorruptionPenalty * context.Cell.Corruption) -
            (config.SectMonsterThreatPenalty * context.Cell.MonsterThreat);
    }

    private static float ComputeDefensibility(Dictionary<(int Q, int R), CellContext> cells, CellContext context)
    {
        var cliffCount = 0;
        foreach (var direction in NeighborDirections)
        {
            if (cells.TryGetValue((context.Cell.Coord.Q + direction.Q, context.Cell.Coord.R + direction.R), out var neighbor) &&
                context.Cell.Height - neighbor.Cell.Height >= 18)
            {
                cliffCount++;
            }
        }

        return Mathf.Clamp((context.Height01 * 0.58f) + (cliffCount / 6f * 0.42f), 0f, 1f);
    }

    private static float ComputeWaterAccess(Dictionary<(int Q, int R), CellContext> cells, CellContext context)
    {
        var bestScore = 0f;
        foreach (var other in cells.Values)
        {
            if (other.Cell.Water == XianxiaWaterType.None && other.Cell.RiverMask == HexDirectionMask.None)
            {
                continue;
            }

            var distance = HexDistance(ToCoord(context.Cell), ToCoord(other.Cell));
            if (distance > 4)
            {
                continue;
            }

            var score = 1f - (distance / 4f);
            bestScore = Math.Max(bestScore, score);
        }

        return bestScore;
    }

    private static float ComputeTravelConnectivity(CellContext context)
    {
        var connectivity = 0.34f;
        if (context.Cell.SpiritualZone is XianxiaSpiritualZoneType.MajorSpiritVein or XianxiaSpiritualZoneType.MinorSpiritVein)
        {
            connectivity += 0.28f;
        }

        if (context.Cell.RiverMask != HexDirectionMask.None)
        {
            connectivity += 0.22f;
        }

        if (context.Cell.Biome is XianxiaBiomeType.TemperatePlains or XianxiaBiomeType.BambooValley)
        {
            connectivity += 0.16f;
        }

        return Mathf.Clamp(connectivity, 0f, 1f);
    }

    private static List<XianxiaResourceType> GatherNearbyResources(Dictionary<(int Q, int R), CellContext> cells, CellContext context, int radius)
    {
        var found = new HashSet<XianxiaResourceType>();
        foreach (var other in cells.Values)
        {
            if (other.Cell.Resource == XianxiaResourceType.None)
            {
                continue;
            }

            if (HexDistance(ToCoord(context.Cell), ToCoord(other.Cell)) <= radius)
            {
                found.Add(other.Cell.Resource);
            }
        }

        return [.. found];
    }

    private static List<XianxiaWonderType> GatherNearbyWonders(XianxiaWorldMapData worldMap, CellContext context, int radius)
    {
        var found = new List<XianxiaWonderType>();
        foreach (var wonder in worldMap.Wonders)
        {
            var distance = HexDistance(ToCoord(context.Cell), new AxialCoord(wonder.Coord.Q, wonder.Coord.R));
            if (distance <= radius)
            {
                found.Add(wonder.Wonder);
            }
        }

        return found;
    }

    private static void AddResourceCandidates(List<WeightedResource> weighted, CellContext context)
    {
        switch (context.Cell.Terrain)
        {
            case XianxiaTerrainType.MountainRock:
            case XianxiaTerrainType.MountainPlateau:
                weighted.Add(new WeightedResource(XianxiaResourceType.StoneResource, 14));
                weighted.Add(new WeightedResource(XianxiaResourceType.IronOre, 11));
                weighted.Add(new WeightedResource(XianxiaResourceType.GoldOre, 6));
                break;
            case XianxiaTerrainType.MountainMoss:
                weighted.Add(new WeightedResource(XianxiaResourceType.StoneResource, 10));
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritStone, 8));
                break;
            case XianxiaTerrainType.BambooGround:
                weighted.Add(new WeightedResource(XianxiaResourceType.BambooResource, 14));
                weighted.Add(new WeightedResource(XianxiaResourceType.JadeBamboo, 6));
                break;
            case XianxiaTerrainType.AncientForestFloor:
                weighted.Add(new WeightedResource(XianxiaResourceType.AncientWood, 12));
                weighted.Add(new WeightedResource(XianxiaResourceType.ImmortalPeach, 5));
                break;
            case XianxiaTerrainType.ForestGround:
                weighted.Add(new WeightedResource(XianxiaResourceType.AncientWood, 8));
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritHerbs, 8));
                break;
            case XianxiaTerrainType.DesertSand:
            case XianxiaTerrainType.DesertRock:
                weighted.Add(new WeightedResource(XianxiaResourceType.SaltDeposit, 11));
                weighted.Add(new WeightedResource(XianxiaResourceType.GoldOre, 6));
                weighted.Add(new WeightedResource(XianxiaResourceType.ObsidianRock, 5));
                break;
            case XianxiaTerrainType.CrystalGround:
                weighted.Add(new WeightedResource(XianxiaResourceType.CrystalOre, 12));
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritCrystal, 8));
                weighted.Add(new WeightedResource(XianxiaResourceType.SoulCrystal, 5));
                break;
            case XianxiaTerrainType.SpiritSoil:
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritStone, 9));
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritHerbs, 9));
                break;
            case XianxiaTerrainType.VolcanicRock:
            case XianxiaTerrainType.AshGround:
                weighted.Add(new WeightedResource(XianxiaResourceType.FireCrystal, 11));
                weighted.Add(new WeightedResource(XianxiaResourceType.ObsidianRock, 9));
                weighted.Add(new WeightedResource(XianxiaResourceType.HeavenIron, 4));
                break;
            case XianxiaTerrainType.WetlandMud:
            case XianxiaTerrainType.SwampGround:
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritHerbs, 8));
                weighted.Add(new WeightedResource(XianxiaResourceType.LotusSpirit, 7));
                break;
            case XianxiaTerrainType.SnowRock:
            case XianxiaTerrainType.SnowPlain:
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritStone, 6));
                weighted.Add(new WeightedResource(XianxiaResourceType.WaterCrystal, 6));
                break;
            case XianxiaTerrainType.AncientStone:
            case XianxiaTerrainType.RuinedGround:
                weighted.Add(new WeightedResource(XianxiaResourceType.SoulCrystal, 8));
                weighted.Add(new WeightedResource(XianxiaResourceType.HeavenIron, 5));
                weighted.Add(new WeightedResource(XianxiaResourceType.DragonBone, 3));
                break;
        }

        switch (context.Cell.SpiritualZone)
        {
            case XianxiaSpiritualZoneType.MajorSpiritVein:
            case XianxiaSpiritualZoneType.DragonNode:
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritStone, 10));
                weighted.Add(new WeightedResource(XianxiaResourceType.SpiritCrystal, 8));
                break;
            case XianxiaSpiritualZoneType.FireVein:
                weighted.Add(new WeightedResource(XianxiaResourceType.FireCrystal, 12));
                break;
            case XianxiaSpiritualZoneType.WaterVein:
            case XianxiaSpiritualZoneType.SpiritPool:
                weighted.Add(new WeightedResource(XianxiaResourceType.WaterCrystal, 9));
                weighted.Add(new WeightedResource(XianxiaResourceType.LotusSpirit, 8));
                break;
            case XianxiaSpiritualZoneType.WoodVein:
                weighted.Add(new WeightedResource(XianxiaResourceType.JadeBamboo, 7));
                weighted.Add(new WeightedResource(XianxiaResourceType.ImmortalPeach, 7));
                break;
            case XianxiaSpiritualZoneType.MetalVein:
                weighted.Add(new WeightedResource(XianxiaResourceType.JadeOre, 9));
                weighted.Add(new WeightedResource(XianxiaResourceType.CrystalOre, 8));
                break;
            case XianxiaSpiritualZoneType.EarthVein:
                weighted.Add(new WeightedResource(XianxiaResourceType.JadeOre, 7));
                weighted.Add(new WeightedResource(XianxiaResourceType.StoneResource, 7));
                break;
            case XianxiaSpiritualZoneType.ChaosEnergyZone:
                weighted.Add(new WeightedResource(XianxiaResourceType.VoidCrystal, 8));
                weighted.Add(new WeightedResource(XianxiaResourceType.SoulCrystal, 6));
                break;
        }
    }

    private static XianxiaResourceType PickWeightedResource(List<WeightedResource> weighted, float roll)
    {
        if (weighted.Count == 0)
        {
            return XianxiaResourceType.None;
        }

        var totalWeight = 0;
        foreach (var item in weighted)
        {
            totalWeight += item.Weight;
        }

        var threshold = roll * totalWeight;
        var running = 0f;
        foreach (var item in weighted)
        {
            running += item.Weight;
            if (threshold <= running)
            {
                return item.Resource;
            }
        }

        return weighted[^1].Resource;
    }

    private static XianxiaWonderType ResolveWonderType(CellContext context, XianxiaWorldGenerationConfig config, out float score)
    {
        score = 0f;
        switch (context.Cell.Biome)
        {
            case XianxiaBiomeType.FloatingIsles when config.FloatingIslesEnabled && context.SkyPotential > 0.74f:
                score = (context.SkyPotential * 0.60f) + (context.Cell.QiDensity * 0.40f);
                return context.Cell.Height > 92 ? XianxiaWonderType.FloatingMountainCluster : XianxiaWonderType.FloatingIslandChain;
            case XianxiaBiomeType.SacredForest when context.Cell.QiDensity > 0.76f:
                score = (context.Cell.QiDensity * 0.56f) + (context.Cell.Moisture * 0.44f);
                return XianxiaWonderType.SpiritForestHeart;
            case XianxiaBiomeType.JadeHighlands when context.Cell.QiDensity > 0.70f:
                score = (context.Height01 * 0.48f) + (context.Cell.QiDensity * 0.52f);
                return XianxiaWonderType.JadeMountain;
            case XianxiaBiomeType.SnowPeaks when context.Height01 > 0.84f:
                score = (context.Height01 * 0.64f) + ((1f - context.Cell.Temperature) * 0.36f);
                return XianxiaWonderType.ImmortalPeak;
            case XianxiaBiomeType.CrystalFields when context.CrystalPotential > 0.70f:
                score = (context.CrystalPotential * 0.54f) + (context.Cell.QiDensity * 0.46f);
                return XianxiaWonderType.CrystalMountainRange;
            case XianxiaBiomeType.VolcanicWastes when context.FirePotential > 0.82f:
                score = (context.FirePotential * 0.60f) + (context.Height01 * 0.40f);
                return XianxiaWonderType.PhoenixNestPeak;
            case XianxiaBiomeType.AncientRuinsLand when context.AncientPotential > 0.74f:
                score = (context.AncientPotential * 0.58f) + (context.Cell.QiDensity * 0.42f);
                return context.Cell.QiDensity > 0.74f ? XianxiaWonderType.CelestialPalaceRuins : XianxiaWonderType.AncientImmortalRuins;
            case XianxiaBiomeType.BambooValley when context.BambooPotential > 0.76f:
                score = (context.BambooPotential * 0.58f) + (context.Cell.Moisture * 0.42f);
                return XianxiaWonderType.SacredBambooSea;
            case XianxiaBiomeType.SpiritSwamps when context.Cell.Water != XianxiaWaterType.None && context.Cell.QiDensity > 0.72f:
                score = (context.Cell.QiDensity * 0.54f) + (context.Cell.Moisture * 0.46f);
                return XianxiaWonderType.ThousandLotusLake;
            default:
                if (context.Cell.IsDragonVeinCore)
                {
                    score = 0.90f;
                    return XianxiaWonderType.DragonVeinCore;
                }

                if (context.Cell.SpiritualZone == XianxiaSpiritualZoneType.FiveElementsZone && context.Cell.QiDensity > 0.70f)
                {
                    score = 0.82f;
                    return XianxiaWonderType.FiveElementsPillar;
                }

                if (context.Cell.Terrain == XianxiaTerrainType.RuinedGround && context.Cell.Corruption > 0.72f)
                {
                    score = 0.78f;
                    return XianxiaWonderType.DragonBoneValley;
                }

                return XianxiaWonderType.None;
        }
    }

    private static XianxiaOverlayType ResolveOverlay(CellContext context, float roll)
    {
        if (roll < 0.28f)
        {
            return XianxiaOverlayType.None;
        }

        return context.Cell.Biome switch
        {
            XianxiaBiomeType.SacredForest => roll > 0.84f ? XianxiaOverlayType.GiantTree : XianxiaOverlayType.DenseForest,
            XianxiaBiomeType.BambooValley => roll > 0.86f ? XianxiaOverlayType.BambooGrove : XianxiaOverlayType.BambooForest,
            XianxiaBiomeType.MistyMountains => roll > 0.78f ? XianxiaOverlayType.MossPatch : XianxiaOverlayType.RockCluster,
            XianxiaBiomeType.JadeHighlands => roll > 0.82f ? XianxiaOverlayType.CrystalPlants : XianxiaOverlayType.RockCluster,
            XianxiaBiomeType.SnowPeaks => roll > 0.80f ? XianxiaOverlayType.PineForest : XianxiaOverlayType.StoneDebris,
            XianxiaBiomeType.CrystalFields => roll > 0.72f ? XianxiaOverlayType.CrystalPlants : XianxiaOverlayType.SpiritGrass,
            XianxiaBiomeType.VolcanicWastes => XianxiaOverlayType.StoneDebris,
            XianxiaBiomeType.SpiritSwamps => roll > 0.70f ? XianxiaOverlayType.LotusCluster : XianxiaOverlayType.FernCluster,
            XianxiaBiomeType.AncientRuinsLand => roll > 0.76f ? XianxiaOverlayType.AncientVines : XianxiaOverlayType.StoneDebris,
            XianxiaBiomeType.DesertBadlands => XianxiaOverlayType.RockCluster,
            XianxiaBiomeType.FloatingIsles => roll > 0.70f ? XianxiaOverlayType.SpiritGrass : XianxiaOverlayType.GlowingTree,
            _ => roll > 0.72f ? XianxiaOverlayType.WildflowerField : XianxiaOverlayType.TallGrass
        };
    }

    private static StrategicMapDefinition BuildStrategicDefinition(XianxiaWorldMapData worldMap, XianxiaWorldGenerationConfig config)
    {
        var layout = BuildWorldLayout(worldMap);
        return new StrategicMapDefinition
        {
            Title = config.WorldTitle,
            UnitScale = config.UnitScale,
            GridLines = config.GridLines,
            Regions = BuildCellRegions(worldMap, layout),
            Outlines = BuildWorldOutlines(layout),
            Routes = BuildDragonVeinPolylines(worldMap, layout),
            Rivers = BuildRiverPolylines(worldMap, layout),
            Nodes = BuildWorldNodes(worldMap, layout),
            Labels = BuildWorldLabels(worldMap, layout)
        };
    }

    private static WorldLayoutData BuildWorldLayout(XianxiaWorldMapData worldMap)
    {
        var rawCenters = new Dictionary<(int Q, int R), Vector2>(worldMap.Cells.Count);
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var sqrt3 = Mathf.Sqrt(3f);

        foreach (var cell in worldMap.Cells)
        {
            var row = cell.Coord.R;
            var column = cell.Coord.Q + (row >> 1);
            var rawCenter = new Vector2(
                sqrt3 * (column + ((row & 1) == 0 ? 0f : 0.5f)),
                row * 1.5f);
            rawCenters[(cell.Coord.Q, cell.Coord.R)] = rawCenter;

            minX = MathF.Min(minX, rawCenter.X - (sqrt3 * 0.5f));
            maxX = MathF.Max(maxX, rawCenter.X + (sqrt3 * 0.5f));
            minY = MathF.Min(minY, rawCenter.Y - 1f);
            maxY = MathF.Max(maxY, rawCenter.Y + 1f);
        }

        if (rawCenters.Count == 0)
        {
            return new WorldLayoutData
            {
                HexRadius = 0.01f,
                Bounds = new Rect2(new Vector2(-0.9f, -0.6f), new Vector2(1.8f, 1.2f))
            };
        }

        var worldCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        var spanX = Math.Max(maxX - minX, 0.01f);
        var spanY = Math.Max(maxY - minY, 0.01f);
        var scale = 1.82f / Math.Max(spanX, spanY);
        var layout = new WorldLayoutData
        {
            HexRadius = 0.94f * scale
        };

        var normalizedMinX = float.MaxValue;
        var normalizedMinY = float.MaxValue;
        var normalizedMaxX = float.MinValue;
        var normalizedMaxY = float.MinValue;

        foreach (var pair in rawCenters)
        {
            var normalized = (pair.Value - worldCenter) * scale;
            layout.Centers[pair.Key] = normalized;

            normalizedMinX = MathF.Min(normalizedMinX, normalized.X - layout.HexRadius);
            normalizedMaxX = MathF.Max(normalizedMaxX, normalized.X + layout.HexRadius);
            normalizedMinY = MathF.Min(normalizedMinY, normalized.Y - layout.HexRadius);
            normalizedMaxY = MathF.Max(normalizedMaxY, normalized.Y + layout.HexRadius);
        }

        layout.Bounds = new Rect2(
            new Vector2(normalizedMinX, normalizedMinY),
            new Vector2(normalizedMaxX - normalizedMinX, normalizedMaxY - normalizedMinY));
        return layout;
    }

    private static List<StrategicPolygonDefinition> BuildCellRegions(XianxiaWorldMapData worldMap, WorldLayoutData layout)
    {
        var polygons = new List<StrategicPolygonDefinition>(worldMap.Cells.Count);

        foreach (var cell in worldMap.Cells)
        {
            if (!layout.Centers.TryGetValue((cell.Coord.Q, cell.Coord.R), out var center))
            {
                continue;
            }

            var fill = ResolveCellFillColor(cell);
            polygons.Add(new StrategicPolygonDefinition
            {
                FillColor = ToHtmlColor(fill),
                OutlineColor = ToHtmlColor(ResolveCellOutlineColor(fill, cell)),
                OutlineWidth = cell.Water != XianxiaWaterType.None ? 0.9f : 0.65f,
                Points = BuildHexPoints(center, layout.HexRadius)
            });
        }

        return polygons;
    }

    private static List<StrategicPolylineDefinition> BuildWorldOutlines(WorldLayoutData layout)
    {
        var rect = layout.Bounds.Grow(layout.HexRadius * 1.45f);
        var center = rect.GetCenter();
        var halfWidth = rect.Size.X * 0.5f;
        var halfHeight = rect.Size.Y * 0.5f;
        var points =
            new List<StrategicPointDefinition>
            {
                new() { X = center.X, Y = center.Y - halfHeight },
                new() { X = center.X + halfWidth, Y = center.Y - (halfHeight * 0.42f) },
                new() { X = center.X + halfWidth, Y = center.Y + (halfHeight * 0.42f) },
                new() { X = center.X, Y = center.Y + halfHeight },
                new() { X = center.X - halfWidth, Y = center.Y + (halfHeight * 0.42f) },
                new() { X = center.X - halfWidth, Y = center.Y - (halfHeight * 0.42f) }
            };

        return
        [
            new StrategicPolylineDefinition
            {
                Color = "#E7D8B2A8",
                Width = 1.6f,
                Closed = true,
                Points = points
            }
        ];
    }

    private static List<StrategicPolylineDefinition> BuildRiverPolylines(XianxiaWorldMapData worldMap, WorldLayoutData layout)
    {
        var polylines = new List<StrategicPolylineDefinition>(worldMap.Rivers.Count);

        foreach (var river in worldMap.Rivers)
        {
            var points = BuildPathPoints(river.Nodes, layout);
            if (points.Count < 2)
            {
                continue;
            }

            var color = river.FeedsSpiritZone
                ? new Color(0.47f, 0.82f, 0.94f, 0.96f)
                : new Color(0.36f, 0.61f, 0.90f, 0.88f);
            polylines.Add(new StrategicPolylineDefinition
            {
                Color = ToHtmlColor(color),
                Width = river.FeedsSpiritZone ? 2.2f : 1.8f,
                Closed = false,
                Points = points
            });
        }

        return polylines;
    }

    private static List<StrategicPolylineDefinition> BuildDragonVeinPolylines(XianxiaWorldMapData worldMap, WorldLayoutData layout)
    {
        var polylines = new List<StrategicPolylineDefinition>(worldMap.DragonVeins.Count);

        foreach (var vein in worldMap.DragonVeins)
        {
            var points = BuildPathPoints(vein.Nodes, layout);
            if (points.Count < 2)
            {
                continue;
            }

            var baseColor = ResolveElementColor(vein.ElementAffinity);
            polylines.Add(new StrategicPolylineDefinition
            {
                Color = ToHtmlColor(vein.IsMajor ? baseColor.Lightened(0.10f) : baseColor),
                Width = vein.IsMajor ? 2.6f : 1.6f,
                Closed = false,
                Points = points
            });
        }

        return polylines;
    }

    private static List<StrategicNodeDefinition> BuildWorldNodes(XianxiaWorldMapData worldMap, WorldLayoutData layout)
    {
        var nodes = new List<StrategicNodeDefinition>();

        foreach (var wonder in worldMap.Wonders)
        {
            if (!layout.Centers.TryGetValue((wonder.Coord.Q, wonder.Coord.R), out var center))
            {
                continue;
            }

            nodes.Add(new StrategicNodeDefinition
            {
                X = center.X,
                Y = center.Y,
                Radius = 5.6f,
                Color = ToHtmlColor(ResolveWonderColor(wonder.Wonder)),
                Kind = "wonder"
            });
        }

        foreach (var site in worldMap.Sites)
        {
            if (!layout.Centers.TryGetValue((site.Coord.Q, site.Coord.R), out var center))
            {
                continue;
            }

            nodes.Add(new StrategicNodeDefinition
            {
                X = center.X,
                Y = center.Y,
                Radius = ResolveSiteRadius(site.Role),
                Color = ToHtmlColor(ResolveSiteColor(site.Role, site.Structure)),
                Kind = ResolveSiteKind(site.Role)
            });
        }

        foreach (var candidate in worldMap.SectCandidates)
        {
            if (!layout.Centers.TryGetValue((candidate.Coord.Q, candidate.Coord.R), out var center))
            {
                continue;
            }

            nodes.Add(new StrategicNodeDefinition
            {
                X = center.X,
                Y = center.Y,
                Radius = 2.6f,
                Color = ToHtmlColor(ResolveElementColor(candidate.ElementAffinity).Lightened(0.08f)),
                Kind = "sect_candidate"
            });
        }

        foreach (var resourceNode in SelectRareResourceNodes(worldMap))
        {
            if (!layout.Centers.TryGetValue((resourceNode.Coord.Q, resourceNode.Coord.R), out var center))
            {
                continue;
            }

            nodes.Add(new StrategicNodeDefinition
            {
                X = center.X,
                Y = center.Y,
                Radius = 2.2f,
                Color = ToHtmlColor(ResolveResourceColor(resourceNode.Resource)),
                Kind = "resource"
            });
        }

        return nodes;
    }

    private static List<StrategicLabelDefinition> BuildWorldLabels(XianxiaWorldMapData worldMap, WorldLayoutData layout)
    {
        var labels = new List<StrategicLabelDefinition>();

        foreach (var wonder in worldMap.Wonders)
        {
            if (!layout.Centers.TryGetValue((wonder.Coord.Q, wonder.Coord.R), out var center))
            {
                continue;
            }

            labels.Add(new StrategicLabelDefinition
            {
                X = center.X,
                Y = center.Y - (layout.HexRadius * 1.3f),
                Text = ResolveWonderName(wonder.Wonder),
                Color = "#F4E6C4F2",
                FontSize = 12,
                MinZoom = 0.72f
            });
        }

        foreach (var site in worldMap.Sites)
        {
            if (!layout.Centers.TryGetValue((site.Coord.Q, site.Coord.R), out var center))
            {
                continue;
            }

            labels.Add(new StrategicLabelDefinition
            {
                X = center.X,
                Y = center.Y + (layout.HexRadius * 1.2f),
                Text = site.Label,
                Color = "#EEDCB8E6",
                FontSize = site.Role == XianxiaSiteRoleType.SectCandidate ? 11 : 10,
                MinZoom = site.Role == XianxiaSiteRoleType.SectCandidate ? 0.88f : 1.05f
            });
        }

        return labels;
    }

    private static List<StrategicPointDefinition> BuildPathPoints(IReadOnlyList<XianxiaPathNodeData> nodes, WorldLayoutData layout)
    {
        var points = new List<StrategicPointDefinition>(nodes.Count);
        foreach (var node in nodes)
        {
            if (!layout.Centers.TryGetValue((node.Coord.Q, node.Coord.R), out var center))
            {
                continue;
            }

            points.Add(new StrategicPointDefinition
            {
                X = center.X,
                Y = center.Y
            });
        }

        return points;
    }

    private static List<StrategicPointDefinition> BuildHexPoints(Vector2 center, float radius)
    {
        var halfWidth = Mathf.Sqrt(3f) * radius * 0.5f;
        var halfHeight = radius * 0.5f;

        return
        [
            new StrategicPointDefinition { X = center.X, Y = center.Y - radius },
            new StrategicPointDefinition { X = center.X + halfWidth, Y = center.Y - halfHeight },
            new StrategicPointDefinition { X = center.X + halfWidth, Y = center.Y + halfHeight },
            new StrategicPointDefinition { X = center.X, Y = center.Y + radius },
            new StrategicPointDefinition { X = center.X - halfWidth, Y = center.Y + halfHeight },
            new StrategicPointDefinition { X = center.X - halfWidth, Y = center.Y - halfHeight }
        ];
    }

    private static List<RareResourceNode> SelectRareResourceNodes(XianxiaWorldMapData worldMap)
    {
        var scored = new List<RareResourceNode>();
        foreach (var cell in worldMap.Cells)
        {
            if (!TryGetRareResourceWeight(cell.Resource, out var weight))
            {
                continue;
            }

            scored.Add(new RareResourceNode(cell.Coord, cell.Resource, weight + (cell.QiDensity * 18f) + (cell.Height * 0.06f)));
        }

        scored.Sort((left, right) => right.Score.CompareTo(left.Score));
        var selected = new List<RareResourceNode>();

        foreach (var resourceNode in scored)
        {
            if (selected.Count >= 18)
            {
                break;
            }

            var coord = new AxialCoord(resourceNode.Coord.Q, resourceNode.Coord.R);
            var tooClose = false;
            foreach (var existing in selected)
            {
                if (HexDistance(coord, new AxialCoord(existing.Coord.Q, existing.Coord.R)) < 3)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                selected.Add(resourceNode);
            }
        }

        return selected;
    }

    private static bool TryGetRareResourceWeight(XianxiaResourceType resource, out float weight)
    {
        weight = resource switch
        {
            XianxiaResourceType.PhoenixFeather => 100f,
            XianxiaResourceType.DragonBone => 96f,
            XianxiaResourceType.HeavenIron => 92f,
            XianxiaResourceType.VoidCrystal => 90f,
            XianxiaResourceType.SpiritCrystal => 74f,
            XianxiaResourceType.SoulCrystal => 72f,
            XianxiaResourceType.JadeOre => 64f,
            XianxiaResourceType.SpiritStone => 60f,
            XianxiaResourceType.CrystalOre => 56f,
            _ => 0f
        };

        return weight > 0f;
    }

    private static Color ResolveCellFillColor(XianxiaHexCellData cell)
    {
        var baseColor = cell.Terrain switch
        {
            XianxiaTerrainType.GrassLush => new Color(0.28f, 0.55f, 0.33f, 0.94f),
            XianxiaTerrainType.GrassSparse => new Color(0.38f, 0.55f, 0.33f, 0.94f),
            XianxiaTerrainType.WildflowerMeadow => new Color(0.52f, 0.66f, 0.42f, 0.94f),
            XianxiaTerrainType.ForestGround => new Color(0.18f, 0.38f, 0.22f, 0.95f),
            XianxiaTerrainType.BambooGround => new Color(0.20f, 0.48f, 0.34f, 0.95f),
            XianxiaTerrainType.AncientForestFloor => new Color(0.14f, 0.30f, 0.19f, 0.95f),
            XianxiaTerrainType.MountainRock => new Color(0.42f, 0.43f, 0.46f, 0.95f),
            XianxiaTerrainType.MountainMoss => new Color(0.32f, 0.43f, 0.36f, 0.95f),
            XianxiaTerrainType.MountainPlateau => new Color(0.47f, 0.45f, 0.41f, 0.95f),
            XianxiaTerrainType.DesertSand => new Color(0.73f, 0.61f, 0.38f, 0.95f),
            XianxiaTerrainType.DesertRock => new Color(0.58f, 0.46f, 0.30f, 0.95f),
            XianxiaTerrainType.WetlandMud => new Color(0.35f, 0.41f, 0.26f, 0.95f),
            XianxiaTerrainType.SwampGround => new Color(0.27f, 0.34f, 0.24f, 0.95f),
            XianxiaTerrainType.SnowPlain => new Color(0.82f, 0.88f, 0.92f, 0.96f),
            XianxiaTerrainType.SnowRock => new Color(0.69f, 0.75f, 0.82f, 0.96f),
            XianxiaTerrainType.VolcanicRock => new Color(0.29f, 0.22f, 0.24f, 0.96f),
            XianxiaTerrainType.AshGround => new Color(0.41f, 0.31f, 0.29f, 0.96f),
            XianxiaTerrainType.CrystalGround => new Color(0.45f, 0.62f, 0.78f, 0.96f),
            XianxiaTerrainType.SpiritSoil => new Color(0.36f, 0.62f, 0.50f, 0.96f),
            XianxiaTerrainType.AncientStone => new Color(0.48f, 0.48f, 0.43f, 0.96f),
            XianxiaTerrainType.RuinedGround => new Color(0.37f, 0.33f, 0.34f, 0.96f),
            XianxiaTerrainType.FloatingRock => new Color(0.66f, 0.71f, 0.78f, 0.96f),
            XianxiaTerrainType.CloudGround => new Color(0.84f, 0.89f, 0.94f, 0.96f),
            _ => new Color(0.39f, 0.48f, 0.42f, 0.94f)
        };

        if (cell.Water != XianxiaWaterType.None || cell.RiverMask != HexDirectionMask.None)
        {
            baseColor = baseColor.Lerp(new Color(0.31f, 0.60f, 0.84f, 0.96f), cell.Water != XianxiaWaterType.None ? 0.64f : 0.28f);
        }

        if (cell.SpiritualZone != XianxiaSpiritualZoneType.None)
        {
            baseColor = baseColor.Lerp(ResolveElementColor(cell.ElementAffinity), 0.18f);
        }

        if (cell.Corruption > 0.60f)
        {
            baseColor = baseColor.Lerp(new Color(0.48f, 0.18f, 0.24f, 0.98f), Mathf.Clamp((cell.Corruption - 0.60f) * 0.55f, 0f, 0.32f));
        }

        if (cell.Wonder != XianxiaWonderType.None)
        {
            baseColor = baseColor.Lightened(0.15f);
        }
        else if (cell.Structure != XianxiaStructureType.None)
        {
            baseColor = baseColor.Lightened(0.08f);
        }

        return baseColor;
    }

    private static Color ResolveCellOutlineColor(Color fill, XianxiaHexCellData cell)
    {
        var outline = fill.Darkened(0.28f);
        outline.A = cell.Wonder != XianxiaWonderType.None ? 0.92f : 0.62f;
        return outline;
    }

    private static Color ResolveWonderColor(XianxiaWonderType wonder)
    {
        return wonder switch
        {
            XianxiaWonderType.FloatingMountainCluster => new Color(0.84f, 0.90f, 0.98f, 1f),
            XianxiaWonderType.GiantWorldTree => new Color(0.38f, 0.76f, 0.42f, 1f),
            XianxiaWonderType.CelestialPalaceRuins => new Color(0.82f, 0.79f, 0.92f, 1f),
            XianxiaWonderType.DragonBoneValley => new Color(0.82f, 0.67f, 0.54f, 1f),
            XianxiaWonderType.ImmortalPeak => new Color(0.90f, 0.94f, 0.98f, 1f),
            XianxiaWonderType.JadeMountain => new Color(0.52f, 0.84f, 0.66f, 1f),
            XianxiaWonderType.SpiritForestHeart => new Color(0.36f, 0.80f, 0.50f, 1f),
            XianxiaWonderType.ThousandLotusLake => new Color(0.52f, 0.74f, 0.92f, 1f),
            XianxiaWonderType.SacredBambooSea => new Color(0.42f, 0.74f, 0.58f, 1f),
            XianxiaWonderType.HeavenGateRuins => new Color(0.88f, 0.82f, 0.72f, 1f),
            XianxiaWonderType.PhoenixNestPeak => new Color(0.96f, 0.54f, 0.28f, 1f),
            XianxiaWonderType.AncientImmortalRuins => new Color(0.74f, 0.68f, 0.78f, 1f),
            XianxiaWonderType.FiveElementsPillar => new Color(0.88f, 0.74f, 0.40f, 1f),
            XianxiaWonderType.DragonVeinCore => new Color(0.98f, 0.84f, 0.34f, 1f),
            XianxiaWonderType.CrystalMountainRange => new Color(0.60f, 0.72f, 0.96f, 1f),
            XianxiaWonderType.FloatingIslandChain => new Color(0.76f, 0.90f, 0.96f, 1f),
            _ => new Color(0.92f, 0.86f, 0.70f, 1f)
        };
    }

    private static Color ResolveSiteColor(XianxiaSiteRoleType role, XianxiaStructureType structure)
    {
        return role switch
        {
            XianxiaSiteRoleType.SectCandidate => ResolveStructureColor(structure).Lightened(0.12f),
            XianxiaSiteRoleType.Settlement => ResolveStructureColor(structure),
            XianxiaSiteRoleType.Ruin => new Color(0.62f, 0.58f, 0.56f, 1f),
            XianxiaSiteRoleType.WonderAnchor => new Color(0.88f, 0.80f, 0.46f, 1f),
            XianxiaSiteRoleType.ResourceHub => new Color(0.52f, 0.76f, 0.86f, 1f),
            _ => new Color(0.88f, 0.81f, 0.60f, 1f)
        };
    }

    private static Color ResolveStructureColor(XianxiaStructureType structure)
    {
        return structure switch
        {
            XianxiaStructureType.SectFoundation => new Color(0.72f, 0.60f, 0.90f, 1f),
            XianxiaStructureType.SectMainHall => new Color(0.80f, 0.66f, 0.94f, 1f),
            XianxiaStructureType.VillageBase => new Color(0.88f, 0.76f, 0.54f, 1f),
            XianxiaStructureType.MarketSquare => new Color(0.94f, 0.64f, 0.40f, 1f),
            XianxiaStructureType.AncientCityRuins => new Color(0.66f, 0.63f, 0.67f, 1f),
            XianxiaStructureType.RuinsPlatform => new Color(0.58f, 0.54f, 0.55f, 1f),
            XianxiaStructureType.SpiritObelisk => new Color(0.48f, 0.78f, 0.84f, 1f),
            XianxiaStructureType.HeavenlyGate => new Color(0.90f, 0.82f, 0.62f, 1f),
            _ => new Color(0.84f, 0.72f, 0.58f, 1f)
        };
    }

    private static Color ResolveResourceColor(XianxiaResourceType resource)
    {
        return resource switch
        {
            XianxiaResourceType.JadeOre => new Color(0.48f, 0.84f, 0.66f, 1f),
            XianxiaResourceType.SpiritStone => new Color(0.56f, 0.84f, 0.92f, 1f),
            XianxiaResourceType.CrystalOre => new Color(0.68f, 0.76f, 0.98f, 1f),
            XianxiaResourceType.DragonBone => new Color(0.88f, 0.72f, 0.56f, 1f),
            XianxiaResourceType.PhoenixFeather => new Color(0.98f, 0.54f, 0.28f, 1f),
            XianxiaResourceType.HeavenIron => new Color(0.72f, 0.78f, 0.90f, 1f),
            XianxiaResourceType.VoidCrystal => new Color(0.72f, 0.54f, 0.96f, 1f),
            XianxiaResourceType.SpiritCrystal => new Color(0.54f, 0.94f, 0.84f, 1f),
            XianxiaResourceType.SoulCrystal => new Color(0.68f, 0.60f, 0.90f, 1f),
            _ => new Color(0.80f, 0.82f, 0.70f, 1f)
        };
    }

    private static Color ResolveElementColor(XianxiaElementType element)
    {
        return element switch
        {
            XianxiaElementType.Wood => new Color(0.34f, 0.78f, 0.46f, 1f),
            XianxiaElementType.Fire => new Color(0.92f, 0.43f, 0.25f, 1f),
            XianxiaElementType.Earth => new Color(0.78f, 0.64f, 0.34f, 1f),
            XianxiaElementType.Metal => new Color(0.76f, 0.82f, 0.92f, 1f),
            XianxiaElementType.Water => new Color(0.40f, 0.68f, 0.96f, 1f),
            XianxiaElementType.Yin => new Color(0.56f, 0.44f, 0.78f, 1f),
            XianxiaElementType.Yang => new Color(0.96f, 0.78f, 0.36f, 1f),
            XianxiaElementType.Chaos => new Color(0.76f, 0.38f, 0.86f, 1f),
            _ => new Color(0.84f, 0.80f, 0.66f, 1f)
        };
    }

    private static float ResolveSiteRadius(XianxiaSiteRoleType role)
    {
        return role switch
        {
            XianxiaSiteRoleType.SectCandidate => 4.8f,
            XianxiaSiteRoleType.Settlement => 4.2f,
            XianxiaSiteRoleType.Ruin => 3.4f,
            XianxiaSiteRoleType.WonderAnchor => 5.0f,
            XianxiaSiteRoleType.ResourceHub => 3.0f,
            _ => 3.4f
        };
    }

    private static string ResolveSiteKind(XianxiaSiteRoleType role)
    {
        return role switch
        {
            XianxiaSiteRoleType.SectCandidate => "sect",
            XianxiaSiteRoleType.Settlement => "settlement",
            XianxiaSiteRoleType.Ruin => "ruin",
            XianxiaSiteRoleType.WonderAnchor => "wonder",
            XianxiaSiteRoleType.ResourceHub => "resource",
            _ => "site"
        };
    }

    private static string ResolveWonderName(XianxiaWonderType wonder)
    {
        return wonder switch
        {
            XianxiaWonderType.FloatingMountainCluster => "浮空群山",
            XianxiaWonderType.GiantWorldTree => "世界树",
            XianxiaWonderType.CelestialPalaceRuins => "天宫遗墟",
            XianxiaWonderType.DragonBoneValley => "龙骨谷",
            XianxiaWonderType.ImmortalPeak => "不朽峰",
            XianxiaWonderType.JadeMountain => "玉岳",
            XianxiaWonderType.SpiritForestHeart => "灵林之心",
            XianxiaWonderType.ThousandLotusLake => "千莲湖",
            XianxiaWonderType.SacredBambooSea => "圣竹海",
            XianxiaWonderType.HeavenGateRuins => "天门古墟",
            XianxiaWonderType.PhoenixNestPeak => "凤巢峰",
            XianxiaWonderType.AncientImmortalRuins => "古仙遗迹",
            XianxiaWonderType.FiveElementsPillar => "五行天柱",
            XianxiaWonderType.DragonVeinCore => "龙脉核心",
            XianxiaWonderType.CrystalMountainRange => "晶山脉",
            XianxiaWonderType.FloatingIslandChain => "浮岛链",
            _ => "奇观"
        };
    }

    private static XianxiaCliffType ResolveCliffType(CellContext context)
    {
        return context.Cell.Biome switch
        {
            XianxiaBiomeType.BambooValley => XianxiaCliffType.BambooCliff,
            XianxiaBiomeType.SacredForest => XianxiaCliffType.ForestCliff,
            XianxiaBiomeType.JadeHighlands => XianxiaCliffType.JadeCliff,
            XianxiaBiomeType.SnowPeaks => XianxiaCliffType.SnowCliff,
            XianxiaBiomeType.CrystalFields => XianxiaCliffType.CrystalCliff,
            XianxiaBiomeType.VolcanicWastes => XianxiaCliffType.WaterfallCliff,
            XianxiaBiomeType.AncientRuinsLand => context.Cell.Corruption > 0.72f ? XianxiaCliffType.DragonBoneCliff : XianxiaCliffType.AncientRuinsCliff,
            XianxiaBiomeType.DesertBadlands => XianxiaCliffType.SandstoneCliff,
            XianxiaBiomeType.FloatingIsles => XianxiaCliffType.FloatingCliff,
            XianxiaBiomeType.MistyMountains => XianxiaCliffType.MountainCliff,
            _ => context.Cell.Moisture > 0.58f ? XianxiaCliffType.MossCliff : XianxiaCliffType.RockCliff
        };
    }

    private static XianxiaElementType ResolveVeinElement(XianxiaHexCellData start, XianxiaHexCellData end)
    {
        if (start.ElementAffinity == end.ElementAffinity && start.ElementAffinity != XianxiaElementType.None)
        {
            return start.ElementAffinity;
        }

        if (start.ElementAffinity == XianxiaElementType.Chaos || end.ElementAffinity == XianxiaElementType.Chaos)
        {
            return XianxiaElementType.Chaos;
        }

        if (start.ElementAffinity == XianxiaElementType.Yang || end.ElementAffinity == XianxiaElementType.Yang)
        {
            return XianxiaElementType.Yang;
        }

        if (start.ElementAffinity == XianxiaElementType.Yin || end.ElementAffinity == XianxiaElementType.Yin)
        {
            return XianxiaElementType.Yin;
        }

        if (end.ElementAffinity != XianxiaElementType.None)
        {
            return end.ElementAffinity;
        }

        return start.ElementAffinity;
    }

    private static XianxiaSpiritualZoneType ResolveElementVein(XianxiaElementType element)
    {
        return element switch
        {
            XianxiaElementType.Wood => XianxiaSpiritualZoneType.WoodVein,
            XianxiaElementType.Fire => XianxiaSpiritualZoneType.FireVein,
            XianxiaElementType.Earth => XianxiaSpiritualZoneType.EarthVein,
            XianxiaElementType.Metal => XianxiaSpiritualZoneType.MetalVein,
            XianxiaElementType.Water => XianxiaSpiritualZoneType.WaterVein,
            XianxiaElementType.Yin => XianxiaSpiritualZoneType.YinEnergyZone,
            XianxiaElementType.Yang => XianxiaSpiritualZoneType.YangEnergyZone,
            XianxiaElementType.Chaos => XianxiaSpiritualZoneType.ChaosEnergyZone,
            _ => XianxiaSpiritualZoneType.FiveElementsZone
        };
    }

    private static XianxiaElementType ResolveWonderElement(XianxiaWonderType wonder, XianxiaElementType fallback)
    {
        return wonder switch
        {
            XianxiaWonderType.FloatingMountainCluster => XianxiaElementType.Yang,
            XianxiaWonderType.GiantWorldTree => XianxiaElementType.Wood,
            XianxiaWonderType.CelestialPalaceRuins => XianxiaElementType.Yang,
            XianxiaWonderType.DragonBoneValley => XianxiaElementType.Yin,
            XianxiaWonderType.ImmortalPeak => XianxiaElementType.Water,
            XianxiaWonderType.JadeMountain => XianxiaElementType.Earth,
            XianxiaWonderType.SpiritForestHeart => XianxiaElementType.Wood,
            XianxiaWonderType.ThousandLotusLake => XianxiaElementType.Water,
            XianxiaWonderType.SacredBambooSea => XianxiaElementType.Wood,
            XianxiaWonderType.HeavenGateRuins => XianxiaElementType.Yang,
            XianxiaWonderType.PhoenixNestPeak => XianxiaElementType.Fire,
            XianxiaWonderType.AncientImmortalRuins => XianxiaElementType.Yin,
            XianxiaWonderType.FiveElementsPillar => XianxiaElementType.Earth,
            XianxiaWonderType.DragonVeinCore => XianxiaElementType.Chaos,
            XianxiaWonderType.CrystalMountainRange => XianxiaElementType.Metal,
            XianxiaWonderType.FloatingIslandChain => XianxiaElementType.Water,
            _ => fallback
        };
    }

    private static int ResolveWonderInfluenceRadius(XianxiaWonderType wonder)
    {
        return wonder switch
        {
            XianxiaWonderType.GiantWorldTree => 4,
            XianxiaWonderType.CrystalMountainRange => 4,
            XianxiaWonderType.FloatingMountainCluster => 4,
            XianxiaWonderType.DragonVeinCore => 4,
            XianxiaWonderType.ThousandLotusLake => 3,
            XianxiaWonderType.SacredBambooSea => 3,
            XianxiaWonderType.ImmortalPeak => 3,
            _ => 2
        };
    }

    private static string ToTileKey<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var source = value.ToString();
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        var output = string.Empty;
        for (var index = 0; index < source.Length; index++)
        {
            var character = source[index];
            if (char.IsUpper(character) && index > 0)
            {
                output += "_";
            }

            output += char.ToLowerInvariant(character);
        }

        return output;
    }

    private static HexDirectionMask ResolveDirection(AxialCoord from, AxialCoord to)
    {
        var dq = to.Q - from.Q;
        var dr = to.R - from.R;

        return (dq, dr) switch
        {
            (1, 0) => HexDirectionMask.East,
            (1, -1) => HexDirectionMask.NorthEast,
            (0, -1) => HexDirectionMask.NorthWest,
            (-1, 0) => HexDirectionMask.West,
            (-1, 1) => HexDirectionMask.SouthWest,
            (0, 1) => HexDirectionMask.SouthEast,
            _ => HexDirectionMask.None
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

    private static AxialCoord ToCoord(XianxiaHexCellData cell)
    {
        return new AxialCoord(cell.Coord.Q, cell.Coord.R);
    }

    private static HexAxialCoordData CloneCoord(HexAxialCoordData source)
    {
        return new HexAxialCoordData
        {
            Q = source.Q,
            R = source.R
        };
    }

    private static Vector2 ToPlane(AxialCoord coord)
    {
        return new Vector2(
            Mathf.Sqrt(3f) * (coord.Q + (coord.R * 0.5f)),
            coord.R * 1.5f);
    }

    private static int HexDistance(AxialCoord left, AxialCoord right)
    {
        var dq = left.Q - right.Q;
        var dr = left.R - right.R;
        var ds = (left.Q + left.R) - (right.Q + right.R);
        return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
    }

    private static float RandomRange(Random random, float min, float max)
    {
        return min + ((float)random.NextDouble() * (max - min));
    }

    private static float Hash01(int x, int y, int seed)
    {
        unchecked
        {
            var hash = seed;
            hash = (hash * 397) ^ x;
            hash = (hash * 397) ^ y;
            hash ^= hash >> 13;
            hash *= 1274126177;
            hash ^= hash >> 16;
            return (hash & 0x7FFFFFFF) / (float)int.MaxValue;
        }
    }

    private static float FractalNoise(float x, float y, int seed, int octaves, float persistence)
    {
        var amplitude = 1f;
        var frequency = 1f;
        var total = 0f;
        var maxValue = 0f;

        for (var octave = 0; octave < Math.Max(octaves, 1); octave++)
        {
            total += SampleValueNoise(x * frequency, y * frequency, seed + (octave * 1987)) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2f;
        }

        if (maxValue <= 0.0001f)
        {
            return 0.5f;
        }

        return total / maxValue;
    }

    private static float SampleValueNoise(float x, float y, int seed)
    {
        var floorX = Mathf.FloorToInt(x);
        var floorY = Mathf.FloorToInt(y);
        var tx = x - floorX;
        var ty = y - floorY;
        var smoothX = tx * tx * (3f - (2f * tx));
        var smoothY = ty * ty * (3f - (2f * ty));

        var n00 = Hash01(floorX, floorY, seed);
        var n10 = Hash01(floorX + 1, floorY, seed);
        var n01 = Hash01(floorX, floorY + 1, seed);
        var n11 = Hash01(floorX + 1, floorY + 1, seed);
        var ix0 = Mathf.Lerp(n00, n10, smoothX);
        var ix1 = Mathf.Lerp(n01, n11, smoothX);
        return Mathf.Lerp(ix0, ix1, smoothY);
    }

    private static string ToHtmlColor(Color color)
    {
        var red = (int)Mathf.Round(Mathf.Clamp(color.R, 0f, 1f) * 255f);
        var green = (int)Mathf.Round(Mathf.Clamp(color.G, 0f, 1f) * 255f);
        var blue = (int)Mathf.Round(Mathf.Clamp(color.B, 0f, 1f) * 255f);
        var alpha = (int)Mathf.Round(Mathf.Clamp(color.A, 0f, 1f) * 255f);
        return $"#{red:X2}{green:X2}{blue:X2}{alpha:X2}";
    }

    private readonly record struct AxialCoord(int Q, int R);

    private readonly record struct NextStep(AxialCoord Coord, int NextHeading);

    private readonly record struct WeightedResource(XianxiaResourceType Resource, int Weight);

    private readonly record struct WonderCandidate(CellContext Context, XianxiaWonderType Wonder, float Score);

    private readonly record struct SectCandidateSelection(CellContext Context, float Score);

    private readonly record struct RoadHub(XianxiaSiteData Site, CellContext Context);

    private readonly record struct RareResourceNode(HexAxialCoordData Coord, XianxiaResourceType Resource, float Score);

    private sealed class CellContext
    {
        public CellContext(XianxiaHexCellData cell)
        {
            Cell = cell;
        }

        public XianxiaHexCellData Cell { get; }

        public float Height01 { get; set; }

        public float FirePotential { get; set; }

        public float AncientPotential { get; set; }

        public float BambooPotential { get; set; }

        public float CrystalPotential { get; set; }

        public float SkyPotential { get; set; }
    }

    private sealed class WorldLayoutData
    {
        public Dictionary<(int Q, int R), Vector2> Centers { get; } = [];

        public float HexRadius { get; set; }

        public Rect2 Bounds { get; set; }
    }
}
