using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed class PrefectureMapGeneratorSystem
{
    private const float BorderMinRadiusX = 0.74f;
    private const float BorderMaxRadiusX = 1.04f;
    private const float BorderMinRadiusY = 0.58f;
    private const float BorderMaxRadiusY = 0.90f;
    private const float MainNodeRadius = 6.6f;
    private const float RawSourceNodeRadius = 4.6f;
    private readonly PrefectureCityThemeConfigSystem _themeConfigSystem = new();

    private sealed class EnvironmentAnchors
    {
        public Vector2 Forest { get; set; }
        public Vector2 Lake { get; set; }
        public Vector2 Farmland { get; set; }
        public Vector2 Mountain { get; set; }
        public Vector2 MainAvenue { get; set; }
        public Vector2 RiverGate { get; set; }
    }

    private sealed class CityBuildingNode
    {
        public string Name { get; set; } = string.Empty;
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public string Color { get; set; } = "#F5DDAEFF";
        public bool IsLandmark { get; set; }
    }

    private sealed class CityBounds
    {
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
    }

    public StrategicMapDefinition Generate(int populationHint, int housingHint, double threatHint, int hourSettlements)
    {
        var theme = _themeConfigSystem.GetTheme();
        var safePopulation = Math.Max(populationHint, 40);
        var safeHousing = Math.Max(housingHint, safePopulation);
        var safeThreat = Math.Clamp((float)threatHint, 0f, 100f);
        var safeHourSettlements = Math.Max(hourSettlements, 0);
        var densityFactor = Mathf.Clamp((safePopulation + safeHousing) / 620f, 0.90f, 1.45f);

        var seed = ComposeSeed(safePopulation, safeHousing, safeThreat, safeHourSettlements);
        var random = new Random(seed);
        var threatFactor = safeThreat / 100f;

        var borderPoints = BuildBorder(random, threatFactor, densityFactor);
        var countySeat = new Vector2(RandomRange(random, -0.035f, 0.035f), RandomRange(random, -0.025f, 0.025f));
        var settlements = BuildSettlementNodes(random, borderPoints, countySeat, safePopulation, safeHousing, threatFactor, densityFactor);
        var cityBuildings = BuildCityBuildingNodes(random, countySeat, borderPoints, safePopulation, safeHousing, densityFactor, theme);

        var regions = BuildRegions(random, borderPoints, settlements, countySeat, cityBuildings, threatFactor, densityFactor, out var anchors);
        var cityBounds = ComputeCityBounds(cityBuildings, countySeat);
        var routes = BuildRoutes(random, settlements, countySeat, borderPoints, cityBuildings, cityBounds, threatFactor);
        var rivers = BuildRivers(random, countySeat, threatFactor, cityBounds, out var riverGate);
        anchors.RiverGate = riverGate;
        anchors.MainAvenue = new Vector2(countySeat.X, cityBounds.MinY - 0.02f);
        var nodes = BuildNodeDefinitions(settlements, countySeat, cityBuildings, anchors);
        var labels = BuildLabels(settlements, countySeat, cityBuildings, cityBounds, anchors, theme);

        var outline = new StrategicPolylineDefinition
        {
            Color = "#9FB9CCE0",
            Width = 1.8f,
            Closed = true,
            Points = ToPoints(borderPoints)
        };

        var outerOutline = new StrategicPolylineDefinition
        {
            Color = "#D0DBF359",
            Width = 1.2f,
            Closed = true,
            Points = ToPoints(ScaleBorder(borderPoints, 1.08f))
        };

        return new StrategicMapDefinition
        {
            Title = theme.MapTitle,
            UnitScale = 0.54f,
            GridLines = 8,
            Regions = regions,
            Outlines = [outline, outerOutline],
            Routes = routes,
            Rivers = rivers,
            Nodes = nodes,
            Labels = labels
        };
    }

    private static int ComposeSeed(int population, int housing, float threat, int hourSettlements)
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 31) + (population / 8);
            hash = (hash * 31) + (housing / 8);
            hash = (hash * 31) + (int)MathF.Floor(threat / 3f);
            hash = (hash * 31) + (hourSettlements / 3);
            return hash == 0 ? 20260306 : hash;
        }
    }

    private static List<Vector2> BuildBorder(Random random, float threatFactor, float densityFactor)
    {
        var pointCount = 18 + random.Next(0, 7);
        var points = new List<Vector2>(pointCount);
        var baseRadiusX = RandomRange(random, 0.88f, 0.98f) * densityFactor;
        var baseRadiusY = RandomRange(random, 0.68f, 0.82f) * densityFactor;

        for (var index = 0; index < pointCount; index++)
        {
            var t = index / (float)pointCount;
            var angle = (Mathf.Tau * t) + RandomRange(random, -0.10f, 0.10f);
            var localNoise = RandomRange(random, -0.08f, 0.11f);
            var threatNoise = RandomRange(random, -0.10f, 0.10f) * threatFactor;

            var radiusX = Mathf.Clamp(baseRadiusX + localNoise + threatNoise, BorderMinRadiusX, BorderMaxRadiusX);
            var radiusY = Mathf.Clamp(baseRadiusY + (localNoise * 0.80f) + (threatNoise * 0.90f), BorderMinRadiusY, BorderMaxRadiusY);
            points.Add(new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY));
        }

        return points;
    }

    private static List<Vector2> BuildSettlementNodes(
        Random random,
        List<Vector2> border,
        Vector2 countySeat,
        int populationHint,
        int housingHint,
        float threatFactor,
        float densityFactor)
    {
        var targetCount = Math.Clamp(16 + (populationHint / 60) + (housingHint / 120), 16, 34);
        var points = new List<Vector2>(targetCount);
        var attempts = targetCount * 12;
        var minDistance = 0.082f / densityFactor;

        while (points.Count < targetCount && attempts > 0)
        {
            attempts--;
            var angle = RandomRange(random, 0f, Mathf.Tau);
            var ringDistance = RandomRange(random, 0.28f, 0.90f - (threatFactor * 0.07f));
            var candidate = countySeat + new Vector2(Mathf.Cos(angle) * ringDistance * 0.94f, Mathf.Sin(angle) * ringDistance * 0.72f);
            candidate = PullInsideBorder(candidate, countySeat, border);

            if (!IsPointInPolygon(candidate, border))
            {
                continue;
            }

            if (DistanceToClosest(points, candidate) < minDistance)
            {
                continue;
            }

            points.Add(candidate);
        }

        points.Sort((left, right) =>
        {
            var leftAngle = Mathf.Atan2(left.Y - countySeat.Y, left.X - countySeat.X);
            var rightAngle = Mathf.Atan2(right.Y - countySeat.Y, right.X - countySeat.X);
            return leftAngle.CompareTo(rightAngle);
        });

        return points;
    }

    private static List<CityBuildingNode> BuildCityBuildingNodes(
        Random random,
        Vector2 countySeat,
        List<Vector2> border,
        int populationHint,
        int housingHint,
        float densityFactor,
        PrefectureCityThemeConfig theme)
    {
        var nodes = new List<CityBuildingNode>();

        var defaultLandmarks = new (string fallbackName, Vector2 offset, float radius, string color)[]
        {
            ("外务殿", new Vector2(-0.01f, -0.04f), 4.4f, "#FFE6B8FF"),
            ("云津坊市", new Vector2(0.08f, -0.01f), 4.1f, "#F1D4A7FF"),
            ("观星台", new Vector2(-0.08f, -0.01f), 4.0f, "#E7D9BFFF"),
            ("灵舟渡口", new Vector2(0.12f, 0.03f), 3.8f, "#DCC8AEFF"),
            ("储灵库", new Vector2(0.00f, 0.09f), 3.9f, "#D8B88FFF"),
            ("藏经别院", new Vector2(-0.10f, 0.06f), 3.7f, "#D8D8CCFF"),
            ("演武校场", new Vector2(-0.12f, -0.10f), 3.6f, "#D8C1A4FF"),
            ("山门牌坊", new Vector2(0.00f, -0.12f), 3.7f, "#F2DBB5FF")
        };

        for (var landmarkIndex = 0; landmarkIndex < defaultLandmarks.Length; landmarkIndex++)
        {
            var landmark = defaultLandmarks[landmarkIndex];
            var landmarkName = ResolveConfiguredName(theme.LandmarkNames, landmarkIndex, landmark.fallbackName);
            var position = countySeat + landmark.offset + new Vector2(RandomRange(random, -0.006f, 0.006f), RandomRange(random, -0.006f, 0.006f));
            nodes.Add(new CityBuildingNode
            {
                Name = landmarkName,
                Position = PullInsideBorder(position, countySeat, border),
                Radius = landmark.radius,
                Color = landmark.color,
                IsLandmark = true
            });
        }

        var wardCols = Math.Clamp(7 + (populationHint / 140), 7, 11);
        var wardRows = Math.Clamp(6 + (housingHint / 170), 6, 10);
        var spacingX = 0.034f / Mathf.Clamp(densityFactor, 0.95f, 1.28f);
        var spacingY = 0.028f / Mathf.Clamp(densityFactor, 0.95f, 1.28f);
        var origin = countySeat - new Vector2(((wardCols - 1) * spacingX) * 0.5f, ((wardRows - 1) * spacingY) * 0.5f);
        var wardNamePool = theme.WardNamePool.Count > 0
            ? theme.WardNamePool
            : new List<string> { "外门居舍", "炼器作坊", "沿街商铺", "论道茶寮", "行商栈舍", "储运库房" };
        var wardColorPool = new[] { "#CDAA7AD0", "#C8A67AD0", "#D7B78ED0", "#C6A47BD0", "#D2B58ED0", "#BE9970D0" };

        for (var row = 0; row < wardRows; row++)
        {
            for (var col = 0; col < wardCols; col++)
            {
                if (row == wardRows / 2 && col == wardCols / 2)
                {
                    continue;
                }

                var cell = origin + new Vector2(col * spacingX, row * spacingY);
                var jitter = new Vector2(RandomRange(random, -0.010f, 0.010f), RandomRange(random, -0.010f, 0.010f));
                var position = PullInsideBorder(cell + jitter, countySeat, border);
                var poolIndex = Math.Abs((row * 13 + col * 7) % wardNamePool.Count);
                var colorIndex = poolIndex % wardColorPool.Length;
                nodes.Add(new CityBuildingNode
                {
                    Name = wardNamePool[poolIndex],
                    Position = position,
                    Radius = RandomRange(random, 1.9f, 2.6f),
                    Color = wardColorPool[colorIndex],
                    IsLandmark = false
                });
            }
        }

        return nodes;
    }

    private static List<StrategicPolygonDefinition> BuildRegions(
        Random random,
        List<Vector2> border,
        List<Vector2> settlements,
        Vector2 countySeat,
        List<CityBuildingNode> cityBuildings,
        float threatFactor,
        float densityFactor,
        out EnvironmentAnchors anchors)
    {
        anchors = new EnvironmentAnchors
        {
            Forest = new Vector2(-0.64f, -0.40f),
            Lake = new Vector2(0.58f, 0.30f),
            Farmland = new Vector2(0.20f, 0.20f),
            Mountain = new Vector2(0.58f, -0.50f),
            MainAvenue = countySeat,
            RiverGate = new Vector2(0.20f, 0.04f)
        };

        var regions = new List<StrategicPolygonDefinition>
        {
            new()
            {
                FillColor = "#5E764CE6",
                OutlineColor = "#C4D3B36A",
                OutlineWidth = 1.0f,
                Points = ToPoints(border)
            }
        };

        var farmlandCount = 3 + (int)MathF.Round(densityFactor * 2f);
        for (var index = 0; index < farmlandCount; index++)
        {
            var source = settlements.Count > 0 ? settlements[index % settlements.Count] : countySeat;
            var farmlandCenter = source.Lerp(countySeat, 0.34f);
            farmlandCenter += new Vector2(RandomRange(random, -0.06f, 0.06f), RandomRange(random, -0.05f, 0.05f));
            farmlandCenter = PullInsideBorder(farmlandCenter, countySeat, border);
            if (index == 0)
            {
                anchors.Farmland = farmlandCenter;
            }

            regions.Add(CreateRegion(random, border, countySeat, farmlandCenter, 0.24f, 0.14f, "#8EAA63D0", "#C7D8A26E", 0.7f, 9));
        }

        var forestCount = 6 + random.Next(0, 3);
        for (var index = 0; index < forestCount; index++)
        {
            var edge = border[(index * border.Count) / forestCount];
            var forestCenter = edge.Lerp(countySeat, 0.34f + RandomRange(random, 0.00f, 0.10f));
            forestCenter += new Vector2(RandomRange(random, -0.05f, 0.05f), RandomRange(random, -0.05f, 0.05f));
            forestCenter = PullInsideBorder(forestCenter, countySeat, border);
            if (index == 0)
            {
                anchors.Forest = forestCenter;
            }

            regions.Add(CreateRegion(random, border, countySeat, forestCenter, 0.17f, 0.12f, "#4C7342D8", "#86AE826E", 0.72f, 8));
        }

        var mountainCount = 3 + random.Next(0, 3);
        for (var index = 0; index < mountainCount; index++)
        {
            var edge = border[(border.Count / 3 + (index * 2)) % border.Count];
            var mountainCenter = edge.Lerp(countySeat, 0.30f + RandomRange(random, 0.00f, 0.08f));
            mountainCenter += new Vector2(RandomRange(random, -0.04f, 0.04f), RandomRange(random, -0.04f, 0.04f));
            mountainCenter = PullInsideBorder(mountainCenter, countySeat, border);
            if (index == 0)
            {
                anchors.Mountain = mountainCenter;
            }

            regions.Add(CreateRegion(random, border, countySeat, mountainCenter, 0.15f, 0.10f, "#696C72D8", "#B9BEC670", 0.75f, 7));
        }

        var lakeCount = 2 + random.Next(0, 2);
        for (var index = 0; index < lakeCount; index++)
        {
            var angle = RandomRange(random, 0.12f, Mathf.Pi - 0.12f);
            var radius = RandomRange(random, 0.46f, 0.70f);
            var lakeCenter = countySeat + new Vector2(Mathf.Cos(angle) * radius * 0.88f, Mathf.Sin(angle) * radius * 0.64f);
            lakeCenter = PullInsideBorder(lakeCenter, countySeat, border);
            if (index == 0)
            {
                anchors.Lake = lakeCenter;
            }

            regions.Add(CreateRegion(random, border, countySeat, lakeCenter, 0.13f, 0.09f, "#4D86BDE6", "#90C2E680", 0.72f, 7));
        }

        var cityBounds = ComputeCityBounds(cityBuildings, countySeat);
        var outerCityCenter = new Vector2((cityBounds.MinX + cityBounds.MaxX) * 0.5f, (cityBounds.MinY + cityBounds.MaxY) * 0.5f);
        regions.Add(CreateRegion(random, border, countySeat, outerCityCenter, 0.26f, 0.19f, "#826D53D8", "#D8C4A694", 1.0f, 12));
        regions.Add(CreateRegion(random, border, countySeat, countySeat, 0.17f, 0.12f, "#6E5B45D8", "#E2D1B690", 0.9f, 10));

        return regions;
    }

    private static StrategicPolygonDefinition CreateRegion(
        Random random,
        List<Vector2> border,
        Vector2 countySeat,
        Vector2 center,
        float radiusX,
        float radiusY,
        string fillColor,
        string outlineColor,
        float outlineWidth,
        int pointCount)
    {
        var points = new List<Vector2>(pointCount);
        for (var pointIndex = 0; pointIndex < pointCount; pointIndex++)
        {
            var progress = pointIndex / (float)pointCount;
            var angle = (Mathf.Tau * progress) + RandomRange(random, -0.18f, 0.18f);
            var scaleX = radiusX + RandomRange(random, -0.02f, 0.03f);
            var scaleY = radiusY + RandomRange(random, -0.02f, 0.03f);
            var vertex = center + new Vector2(Mathf.Cos(angle) * scaleX, Mathf.Sin(angle) * scaleY);
            points.Add(PullInsideBorder(vertex, countySeat, border));
        }

        return new StrategicPolygonDefinition
        {
            FillColor = fillColor,
            OutlineColor = outlineColor,
            OutlineWidth = outlineWidth,
            Points = ToPoints(points)
        };
    }

    private static List<StrategicPolylineDefinition> BuildRoutes(
        Random random,
        List<Vector2> settlements,
        Vector2 countySeat,
        List<Vector2> border,
        List<CityBuildingNode> cityBuildings,
        CityBounds cityBounds,
        float threatFactor)
    {
        var routes = new List<StrategicPolylineDefinition>();

        foreach (var settlement in settlements)
        {
            var midpoint = (countySeat + settlement) * 0.5f;
            var direction = (settlement - countySeat).Normalized();
            var curve = new Vector2(-direction.Y, direction.X) * RandomRange(random, -0.06f, 0.06f);
            var bendPoint = PullInsideBorder(midpoint + curve, countySeat, border);

            routes.Add(new StrategicPolylineDefinition
            {
                Color = "#F2CA6DD1",
                Width = 1.6f + (threatFactor * 0.35f),
                Points = ToPoints([countySeat, bendPoint, settlement])
            });
        }

        if (settlements.Count >= 4)
        {
            for (var index = 0; index < settlements.Count; index += 2)
            {
                var current = settlements[index];
                var next = settlements[(index + 2) % settlements.Count];
                var midpoint = (current + next) * 0.5f;
                var innerMidpoint = midpoint.Lerp(countySeat, 0.24f + RandomRange(random, 0f, 0.08f));

                routes.Add(new StrategicPolylineDefinition
                {
                    Color = "#DEB76CB8",
                    Width = 1.2f,
                    Points = ToPoints([current, innerMidpoint, next])
                });
            }
        }

        var gateCount = Math.Clamp(6 + (int)MathF.Round(threatFactor * 4f), 6, 10);
        for (var gateIndex = 0; gateIndex < gateCount; gateIndex++)
        {
            var borderPoint = border[(gateIndex * border.Count) / gateCount];
            var targetSettlement = settlements.Count > 0 ? settlements[gateIndex % settlements.Count] : countySeat;
            var midpoint = borderPoint.Lerp(targetSettlement, 0.40f) + new Vector2(RandomRange(random, -0.03f, 0.03f), RandomRange(random, -0.03f, 0.03f));

            routes.Add(new StrategicPolylineDefinition
            {
                Color = "#E5BF72BA",
                Width = 1.12f,
                Points = ToPoints([borderPoint, midpoint, targetSettlement])
            });
        }

        var verticalRoadCount = 6;
        for (var index = 0; index <= verticalRoadCount; index++)
        {
            var t = index / (float)verticalRoadCount;
            var x = Mathf.Lerp(cityBounds.MinX, cityBounds.MaxX, t);
            routes.Add(new StrategicPolylineDefinition
            {
                Color = index == verticalRoadCount / 2 ? "#F2C777D2" : "#CDA065B8",
                Width = index == verticalRoadCount / 2 ? 1.9f : (index % 2 == 0 ? 1.08f : 0.92f),
                Points = ToPoints([new Vector2(x, cityBounds.MinY), new Vector2(x, cityBounds.MaxY)])
            });
        }

        var horizontalRoadCount = 5;
        for (var index = 0; index <= horizontalRoadCount; index++)
        {
            var t = index / (float)horizontalRoadCount;
            var y = Mathf.Lerp(cityBounds.MinY, cityBounds.MaxY, t);
            routes.Add(new StrategicPolylineDefinition
            {
                Color = "#C69A63B5",
                Width = index == horizontalRoadCount / 2 ? 1.45f : (index % 2 == 0 ? 0.98f : 0.88f),
                Points = ToPoints([new Vector2(cityBounds.MinX, y), new Vector2(cityBounds.MaxX, y)])
            });
        }

        foreach (var building in cityBuildings)
        {
            if (!building.IsLandmark)
            {
                continue;
            }

            routes.Add(new StrategicPolylineDefinition
            {
                Color = "#D5AB6BC4",
                Width = 1.1f,
                Points = ToPoints([countySeat, building.Position])
            });
        }

        return routes;
    }

    private static List<StrategicPolylineDefinition> BuildRivers(
        Random random,
        Vector2 countySeat,
        float threatFactor,
        CityBounds cityBounds,
        out Vector2 riverGate)
    {
        var mainRiverPoints = new List<Vector2>(8);
        var start = new Vector2(-1.04f, RandomRange(random, -0.60f, -0.36f));
        var end = new Vector2(1.02f, RandomRange(random, 0.28f, 0.62f));
        var wobble = 0.11f + (threatFactor * 0.08f);

        for (var index = 0; index < 8; index++)
        {
            var t = index / 7f;
            var basePoint = start.Lerp(end, t);
            var swing = MathF.Sin((t * Mathf.Tau * 1.2f) + RandomRange(random, -0.38f, 0.38f)) * wobble;
            var drift = RandomRange(random, -0.03f, 0.03f);
            mainRiverPoints.Add(new Vector2(basePoint.X + drift, basePoint.Y + swing));
        }

        riverGate = mainRiverPoints[4];
        var canalEnd = new Vector2(countySeat.X + 0.02f, cityBounds.MaxY - 0.02f);
        var canalPoints = new List<Vector2>(5);
        for (var index = 0; index < 5; index++)
        {
            var t = index / 4f;
            var point = riverGate.Lerp(canalEnd, t);
            point += new Vector2(RandomRange(random, -0.015f, 0.015f), RandomRange(random, -0.015f, 0.015f));
            canalPoints.Add(point);
        }

        return
        [
            new StrategicPolylineDefinition
            {
                Color = "#619EDEE0",
                Width = 2.3f,
                Points = ToPoints(mainRiverPoints)
            },
            new StrategicPolylineDefinition
            {
                Color = "#79B6E8D4",
                Width = 1.5f,
                Points = ToPoints(canalPoints)
            }
        ];
    }

    private static List<StrategicNodeDefinition> BuildNodeDefinitions(
        List<Vector2> settlements,
        Vector2 countySeat,
        List<CityBuildingNode> cityBuildings,
        EnvironmentAnchors anchors)
    {
        var nodes = new List<StrategicNodeDefinition>
        {
            new()
            {
                X = countySeat.X,
                Y = countySeat.Y,
                Radius = MainNodeRadius,
                Color = "#FFE6B2FF",
                Kind = "city"
            }
        };

        nodes.AddRange(
        [
            CreateRawSourceNode(anchors.Forest, "#6F985BFF"),
            CreateRawSourceNode(anchors.Lake, "#70A8DDFF"),
            CreateRawSourceNode(anchors.Mountain, "#A7ABB5FF"),
            CreateRawSourceNode(anchors.Farmland, "#C3B368FF")
        ]);

        for (var index = 0; index < settlements.Count; index++)
        {
            var point = settlements[index];
            nodes.Add(new StrategicNodeDefinition
            {
                X = point.X,
                Y = point.Y,
                Radius = index % 4 == 0 ? 4.9f : 4.1f,
                Color = "#DEE6F7FF",
                Kind = "settlement"
            });
        }

        foreach (var building in cityBuildings)
        {
            nodes.Add(new StrategicNodeDefinition
            {
                X = building.Position.X,
                Y = building.Position.Y,
                Radius = building.Radius,
                Color = building.Color,
                Kind = building.IsLandmark ? "landmark" : "ward"
            });
        }

        return nodes;
    }

    private static StrategicNodeDefinition CreateRawSourceNode(Vector2 position, string color)
    {
        return new StrategicNodeDefinition
        {
            X = position.X,
            Y = position.Y,
            Radius = RawSourceNodeRadius,
            Color = color,
            Kind = "raw_source"
        };
    }

    private static List<StrategicLabelDefinition> BuildLabels(
        List<Vector2> settlements,
        Vector2 countySeat,
        List<CityBuildingNode> cityBuildings,
        CityBounds cityBounds,
        EnvironmentAnchors anchors,
        PrefectureCityThemeConfig theme)
    {
        var northGateAnchor = new Vector2((cityBounds.MinX + cityBounds.MaxX) * 0.5f, cityBounds.MinY - 0.02f);
        var southGateAnchor = new Vector2((cityBounds.MinX + cityBounds.MaxX) * 0.5f, cityBounds.MaxY + 0.03f);
        var eastGateAnchor = new Vector2(cityBounds.MaxX + 0.02f, (cityBounds.MinY + cityBounds.MaxY) * 0.5f);
        var westGateAnchor = new Vector2(cityBounds.MinX - 0.04f, (cityBounds.MinY + cityBounds.MaxY) * 0.5f);

        var labels = new List<StrategicLabelDefinition>
        {
            new() { X = countySeat.X - 0.04f, Y = countySeat.Y - 0.13f, Text = theme.CityTitle, Color = "#FFE8BDFF", FontSize = 14, MinZoom = 0.6f },
            new() { X = anchors.Forest.X + 0.03f, Y = anchors.Forest.Y - 0.02f, Text = ComposeRawSourceLabel(theme.ForestName, "林木 / 药材 / 皮毛"), Color = "#DAECD0FF", FontSize = 12, MinZoom = 0.72f },
            new() { X = anchors.Lake.X + 0.03f, Y = anchors.Lake.Y + 0.02f, Text = ComposeRawSourceLabel(theme.LakeName, "卤水 / 芦苇 / 黏土"), Color = "#D8ECFFFF", FontSize = 12, MinZoom = 0.72f },
            new() { X = anchors.Mountain.X + 0.03f, Y = anchors.Mountain.Y - 0.02f, Text = ComposeRawSourceLabel(theme.MountainName, "原石 / 铜矿 / 铁矿"), Color = "#E1E3EAFF", FontSize = 12, MinZoom = 0.72f },
            new() { X = anchors.Farmland.X + 0.03f, Y = anchors.Farmland.Y + 0.02f, Text = ComposeRawSourceLabel(theme.FarmlandName, "麻料"), Color = "#E9F1C8FF", FontSize = 12, MinZoom = 0.72f },
            new() { X = anchors.MainAvenue.X - 0.02f, Y = anchors.MainAvenue.Y, Text = theme.MainAvenueName, Color = "#F6DCAEFF", FontSize = 12, MinZoom = 0.85f },
            new() { X = anchors.RiverGate.X + 0.02f, Y = anchors.RiverGate.Y + 0.01f, Text = theme.RiverGateName, Color = "#CBE5FFFF", FontSize = 11, MinZoom = 0.9f },
            new() { X = cityBounds.MinX - 0.02f, Y = cityBounds.MinY - 0.02f, Text = theme.InnerCityName, Color = "#EBD2ADFF", FontSize = 11, MinZoom = 0.9f },
            new() { X = cityBounds.MaxX - 0.02f, Y = cityBounds.MaxY + 0.03f, Text = theme.OuterWardsName, Color = "#EBD2ADFF", FontSize = 11, MinZoom = 0.9f },
            new() { X = northGateAnchor.X - 0.01f, Y = northGateAnchor.Y, Text = theme.GateNames.North, Color = "#F5E5C3FF", FontSize = 10, MinZoom = 1.0f },
            new() { X = southGateAnchor.X - 0.01f, Y = southGateAnchor.Y, Text = theme.GateNames.South, Color = "#F5E5C3FF", FontSize = 10, MinZoom = 1.0f },
            new() { X = eastGateAnchor.X, Y = eastGateAnchor.Y, Text = theme.GateNames.East, Color = "#F5E5C3FF", FontSize = 10, MinZoom = 1.0f },
            new() { X = westGateAnchor.X, Y = westGateAnchor.Y, Text = theme.GateNames.West, Color = "#F5E5C3FF", FontSize = 10, MinZoom = 1.0f }
        };

        if (settlements.Count > 0)
        {
            var roadAnchor = countySeat.Lerp(settlements[0], 0.58f);
            labels.Add(new StrategicLabelDefinition
            {
                X = roadAnchor.X,
                Y = roadAnchor.Y,
                Text = "官道",
                Color = "#F8DFB1FF",
                FontSize = 11,
                MinZoom = 0.85f
            });
        }

        foreach (var building in cityBuildings)
        {
            if (!building.IsLandmark)
            {
                continue;
            }

            labels.Add(new StrategicLabelDefinition
            {
                X = building.Position.X + 0.012f,
                Y = building.Position.Y - 0.012f,
                Text = building.Name,
                Color = "#F7EAD1FF",
                FontSize = 10,
                MinZoom = 1.1f
            });
        }

        return labels;
    }

    private static string ComposeRawSourceLabel(string regionName, string sourceText)
    {
        if (string.IsNullOrWhiteSpace(regionName))
        {
            return sourceText;
        }

        return $"{regionName} · {sourceText}";
    }

    private static CityBounds ComputeCityBounds(List<CityBuildingNode> cityBuildings, Vector2 countySeat)
    {
        if (cityBuildings.Count == 0)
        {
            return new CityBounds
            {
                MinX = countySeat.X - 0.18f,
                MaxX = countySeat.X + 0.18f,
                MinY = countySeat.Y - 0.14f,
                MaxY = countySeat.Y + 0.14f
            };
        }

        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        foreach (var building in cityBuildings)
        {
            minX = Math.Min(minX, building.Position.X);
            maxX = Math.Max(maxX, building.Position.X);
            minY = Math.Min(minY, building.Position.Y);
            maxY = Math.Max(maxY, building.Position.Y);
        }

        return new CityBounds
        {
            MinX = minX - 0.04f,
            MaxX = maxX + 0.04f,
            MinY = minY - 0.04f,
            MaxY = maxY + 0.04f
        };
    }

    private static List<Vector2> ScaleBorder(List<Vector2> border, float scale)
    {
        var result = new List<Vector2>(border.Count);
        foreach (var point in border)
        {
            result.Add(point * scale);
        }

        return result;
    }

    private static Vector2 PullInsideBorder(Vector2 candidate, Vector2 center, List<Vector2> border)
    {
        if (IsPointInPolygon(candidate, border))
        {
            return candidate;
        }

        var pulled = candidate;
        for (var step = 0; step < 7; step++)
        {
            pulled = pulled.Lerp(center, 0.20f);
            if (IsPointInPolygon(pulled, border))
            {
                return pulled;
            }
        }

        return center;
    }

    private static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        var inside = false;
        for (var index = 0; index < polygon.Count; index++)
        {
            var next = (index + 1) % polygon.Count;
            var left = polygon[index];
            var right = polygon[next];

            var intersects = ((left.Y > point.Y) != (right.Y > point.Y)) &&
                             (point.X < ((right.X - left.X) * (point.Y - left.Y) / ((right.Y - left.Y) + 0.0001f)) + left.X);
            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static float DistanceToClosest(List<Vector2> points, Vector2 candidate)
    {
        if (points.Count == 0)
        {
            return float.MaxValue;
        }

        var minDistance = float.MaxValue;
        foreach (var point in points)
        {
            var distance = point.DistanceTo(candidate);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    private static List<StrategicPointDefinition> ToPoints(IReadOnlyList<Vector2> points)
    {
        var result = new List<StrategicPointDefinition>(points.Count);
        for (var index = 0; index < points.Count; index++)
        {
            result.Add(new StrategicPointDefinition
            {
                X = points[index].X,
                Y = points[index].Y
            });
        }

        return result;
    }

    private static string ResolveConfiguredName(IReadOnlyList<string> configuredNames, int index, string fallbackName)
    {
        if (index >= 0 && index < configuredNames.Count && !string.IsNullOrWhiteSpace(configuredNames[index]))
        {
            return configuredNames[index].Trim();
        }

        return fallbackName;
    }

    private static float RandomRange(Random random, float minInclusive, float maxInclusive)
    {
        return minInclusive + ((float)random.NextDouble() * (maxInclusive - minInclusive));
    }
}
