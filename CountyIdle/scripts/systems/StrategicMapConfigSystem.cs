using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public class StrategicMapConfigSystem
{
    private const string ConfigPath = "res://data/strategic_maps.json";
    private const float CoordinateWarnLimit = 1.20f;
    private const float MinLineWidth = 0.4f;
    private const float MinNodeRadius = 1.0f;
    private const int MinLabelFontSize = 10;
    private const int MaxLabelFontSize = 18;
    private const float MinLabelZoom = 0.6f;
    private const float MaxLabelZoom = 2.2f;
    private static readonly Regex HexColorRegex = new("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static StrategicMapConfig? _cachedConfig;

    public StrategicMapDefinition GetWorldDefinition()
    {
        return Load().World!;
    }

    public StrategicMapDefinition GetPrefectureDefinition()
    {
        return Load().Prefecture!;
    }

    private static StrategicMapConfig Load()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        _cachedConfig = TryLoadConfigFromFile() ?? BuildFallbackConfig();
        Normalize(_cachedConfig);
        return _cachedConfig;
    }

    private static StrategicMapConfig? TryLoadConfigFromFile()
    {
        if (!FileAccess.FileExists(ConfigPath))
        {
            GD.PushWarning($"Strategic map config not found at {ConfigPath}, fallback will be used.");
            return null;
        }

        try
        {
            using var file = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            var config = JsonSerializer.Deserialize<StrategicMapConfig>(json, JsonOptions);
            if (config == null)
            {
                GD.PushWarning("Strategic map config deserialize returned null, fallback will be used.");
                return null;
            }

            return config;
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Strategic map config load failed: {ex.Message}, fallback will be used.");
            return null;
        }
    }

    private static void Normalize(StrategicMapConfig config)
    {
        var warnings = new List<string>();

        config.World ??= BuildFallbackWorldDefinition();
        config.Prefecture ??= BuildFallbackPrefectureDefinition();

        NormalizeDefinition(config.World, "world", SectMapSemanticRules.GetWorldMapTitle(), warnings);
        NormalizeDefinition(config.Prefecture, "prefecture", SectMapSemanticRules.GetLegacyPrefectureMapTitle(), warnings);

        foreach (var warning in warnings)
        {
            GD.PushWarning($"[StrategicMapConfig] {warning}");
        }
    }

    private static void NormalizeDefinition(
        StrategicMapDefinition definition,
        string mapKey,
        string fallbackTitle,
        List<string> warnings)
    {
        var originalScale = definition.UnitScale;
        var originalGridLines = definition.GridLines;

        definition.Title = string.IsNullOrWhiteSpace(definition.Title) ? fallbackTitle : definition.Title;
        definition.UnitScale = Mathf.Clamp(definition.UnitScale, 0.20f, 0.90f);
        definition.GridLines = Math.Clamp(definition.GridLines, 2, 16);

        definition.Regions ??= [];
        definition.Outlines ??= [];
        definition.Routes ??= [];
        definition.Rivers ??= [];
        definition.Nodes ??= [];
        definition.Labels ??= [];

        foreach (var region in definition.Regions)
        {
            region.Points ??= [];
        }

        foreach (var outline in definition.Outlines)
        {
            outline.Points ??= [];
        }

        foreach (var route in definition.Routes)
        {
            route.Points ??= [];
        }

        foreach (var river in definition.Rivers)
        {
            river.Points ??= [];
        }

        if (!Mathf.IsEqualApprox(originalScale, definition.UnitScale))
        {
            warnings.Add($"{mapKey}.unit_scale={originalScale:0.###} 超出范围，已夹紧到 {definition.UnitScale:0.###}。");
        }

        if (originalGridLines != definition.GridLines)
        {
            warnings.Add($"{mapKey}.grid_lines={originalGridLines} 超出范围，已夹紧到 {definition.GridLines}。");
        }

        ValidateRegions(definition.Regions, mapKey, warnings);
        ValidatePolylines(definition.Outlines, $"{mapKey}.outlines", warnings);
        ValidatePolylines(definition.Routes, $"{mapKey}.routes", warnings);
        ValidatePolylines(definition.Rivers, $"{mapKey}.rivers", warnings);
        ValidateNodes(definition.Nodes, mapKey, warnings);
        ValidateLabels(definition.Labels, mapKey, warnings);
    }

    private static void ValidateRegions(
        List<StrategicPolygonDefinition> regions,
        string mapKey,
        List<string> warnings)
    {
        for (var regionIndex = 0; regionIndex < regions.Count; regionIndex++)
        {
            var region = regions[regionIndex];
            var regionPath = $"{mapKey}.regions[{regionIndex}]";

            if (region.Points.Count < 3)
            {
                warnings.Add($"{regionPath}.points 数量为 {region.Points.Count}，区域至少需要 3 个点。");
            }

            ValidatePoints(region.Points, $"{regionPath}.points", warnings);

            if (!IsColorHexValid(region.FillColor))
            {
                warnings.Add($"{regionPath}.fill_color='{region.FillColor}' 非法，渲染时将使用回退颜色。");
                region.FillColor = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(region.OutlineColor) && !IsColorHexValid(region.OutlineColor))
            {
                warnings.Add($"{regionPath}.outline_color='{region.OutlineColor}' 非法，渲染时将使用回退颜色。");
                region.OutlineColor = string.Empty;
            }

            if (region.OutlineWidth < MinLineWidth)
            {
                warnings.Add($"{regionPath}.outline_width={region.OutlineWidth:0.###} 过小，已重置为 1.2。");
                region.OutlineWidth = 1.2f;
            }
        }
    }

    private static void ValidatePolylines(
        List<StrategicPolylineDefinition> polylines,
        string listPath,
        List<string> warnings)
    {
        for (var index = 0; index < polylines.Count; index++)
        {
            var polyline = polylines[index];
            var polylinePath = $"{listPath}[{index}]";
            var minimumPoints = polyline.Closed ? 3 : 2;
            if (polyline.Points.Count < minimumPoints)
            {
                warnings.Add($"{polylinePath}.points 数量为 {polyline.Points.Count}，当前线段至少需要 {minimumPoints} 个点。");
            }

            ValidatePoints(polyline.Points, $"{polylinePath}.points", warnings);

            if (!IsColorHexValid(polyline.Color))
            {
                warnings.Add($"{polylinePath}.color='{polyline.Color}' 非法，渲染时将使用回退颜色。");
                polyline.Color = string.Empty;
            }

            if (polyline.Width < MinLineWidth)
            {
                warnings.Add($"{polylinePath}.width={polyline.Width:0.###} 过小，已重置为 1.2。");
                polyline.Width = 1.2f;
            }
        }
    }

    private static void ValidateNodes(
        List<StrategicNodeDefinition> nodes,
        string mapKey,
        List<string> warnings)
    {
        for (var nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
        {
            var node = nodes[nodeIndex];
            var nodePath = $"{mapKey}.nodes[{nodeIndex}]";
            ValidateCoordinate(node.X, node.Y, nodePath, warnings);

            if (!string.IsNullOrWhiteSpace(node.Color) && !IsColorHexValid(node.Color))
            {
                warnings.Add($"{nodePath}.color='{node.Color}' 非法，渲染时将使用回退颜色。");
                node.Color = string.Empty;
            }

            if (node.Radius < MinNodeRadius)
            {
                warnings.Add($"{nodePath}.radius={node.Radius:0.###} 过小，已重置为 4.0。");
                node.Radius = 4f;
            }
        }
    }

    private static void ValidateLabels(
        List<StrategicLabelDefinition> labels,
        string mapKey,
        List<string> warnings)
    {
        for (var labelIndex = 0; labelIndex < labels.Count; labelIndex++)
        {
            var label = labels[labelIndex];
            var labelPath = $"{mapKey}.labels[{labelIndex}]";
            ValidateCoordinate(label.X, label.Y, labelPath, warnings);

            if (string.IsNullOrWhiteSpace(label.Text))
            {
                warnings.Add($"{labelPath}.text 为空，渲染时将忽略该标签。");
            }

            if (!string.IsNullOrWhiteSpace(label.Color) && !IsColorHexValid(label.Color))
            {
                warnings.Add($"{labelPath}.color='{label.Color}' 非法，渲染时将使用回退颜色。");
                label.Color = string.Empty;
            }

            var clampedFontSize = Math.Clamp(label.FontSize, MinLabelFontSize, MaxLabelFontSize);
            if (clampedFontSize != label.FontSize)
            {
                warnings.Add($"{labelPath}.font_size={label.FontSize} 超出范围，已夹紧到 {clampedFontSize}。");
                label.FontSize = clampedFontSize;
            }

            var clampedMinZoom = Mathf.Clamp(label.MinZoom, MinLabelZoom, MaxLabelZoom);
            if (!Mathf.IsEqualApprox(clampedMinZoom, label.MinZoom))
            {
                warnings.Add($"{labelPath}.min_zoom={label.MinZoom:0.##} 超出范围，已夹紧到 {clampedMinZoom:0.##}。");
                label.MinZoom = clampedMinZoom;
            }
        }
    }

    private static void ValidatePoints(List<StrategicPointDefinition> points, string listPath, List<string> warnings)
    {
        for (var pointIndex = 0; pointIndex < points.Count; pointIndex++)
        {
            var point = points[pointIndex];
            ValidateCoordinate(point.X, point.Y, $"{listPath}[{pointIndex}]", warnings);
        }
    }

    private static void ValidateCoordinate(float x, float y, string pointPath, List<string> warnings)
    {
        if (Math.Abs(x) > CoordinateWarnLimit || Math.Abs(y) > CoordinateWarnLimit)
        {
            warnings.Add($"{pointPath} 坐标 ({x:0.###}, {y:0.###}) 超出建议区间 [-{CoordinateWarnLimit:0.##}, {CoordinateWarnLimit:0.##}]，可能导致裁切。");
        }
    }

    private static bool IsColorHexValid(string colorText)
    {
        return !string.IsNullOrWhiteSpace(colorText) && HexColorRegex.IsMatch(colorText);
    }

    private static StrategicMapConfig BuildFallbackConfig()
    {
        return new StrategicMapConfig
        {
            World = BuildFallbackWorldDefinition(),
            Prefecture = BuildFallbackPrefectureDefinition()
        };
    }

    private static StrategicMapDefinition BuildFallbackWorldDefinition()
    {
        return new StrategicMapDefinition
        {
            Title = "世界地图",
            UnitScale = 0.42f,
            GridLines = 8,
            Regions =
            [
                new StrategicPolygonDefinition
                {
                    FillColor = "#4772A8EA",
                    OutlineColor = "#D0DBF359",
                    OutlineWidth = 1.2f,
                    Points =
                    [
                        Pt(-0.70f, -0.76f),
                        Pt(-0.20f, -0.92f),
                        Pt(0.16f, -0.72f),
                        Pt(-0.10f, -0.42f),
                        Pt(-0.56f, -0.46f)
                    ]
                },
                new StrategicPolygonDefinition
                {
                    FillColor = "#5A8657EA",
                    OutlineColor = "#D0DBF359",
                    OutlineWidth = 1.2f,
                    Points =
                    [
                        Pt(-0.86f, -0.24f),
                        Pt(-0.62f, -0.52f),
                        Pt(-0.24f, -0.36f),
                        Pt(-0.34f, 0.00f),
                        Pt(-0.70f, 0.24f),
                        Pt(-0.92f, 0.02f)
                    ]
                }
            ],
            Routes =
            [
                new StrategicPolylineDefinition
                {
                    Color = "#F2CA6DD1",
                    Width = 2.0f,
                    Points = [Pt(-0.10f, -0.18f), Pt(0.34f, -0.04f)]
                },
                new StrategicPolylineDefinition
                {
                    Color = "#F2CA6DD1",
                    Width = 2.0f,
                    Points = [Pt(-0.10f, -0.18f), Pt(0.12f, 0.56f)]
                },
                new StrategicPolylineDefinition
                {
                    Color = "#F2CA6DD1",
                    Width = 2.0f,
                    Points = [Pt(-0.10f, -0.18f), Pt(-0.52f, -0.04f)]
                }
            ],
            Nodes =
            [
                new StrategicNodeDefinition { X = -0.10f, Y = -0.18f, Radius = 5f, Color = "#FFEBC0FF" },
                new StrategicNodeDefinition { X = 0.34f, Y = -0.04f, Radius = 4f, Color = "#F0DBB2FF" },
                new StrategicNodeDefinition { X = 0.12f, Y = 0.56f, Radius = 4f, Color = "#F0DBB2FF" },
                new StrategicNodeDefinition { X = -0.52f, Y = -0.04f, Radius = 4f, Color = "#F0DBB2FF" }
            ]
        };
    }

    private static StrategicMapDefinition BuildFallbackPrefectureDefinition()
    {
        return new StrategicMapDefinition
        {
            Title = "江陵府外域",
            UnitScale = 0.42f,
            GridLines = 8,
            Outlines =
            [
                new StrategicPolylineDefinition
                {
                    Color = "#8DB0CAE5",
                    Width = 1.6f,
                    Closed = true,
                    Points =
                    [
                        Pt(-0.62f, -0.42f),
                        Pt(-0.20f, -0.58f),
                        Pt(0.24f, -0.48f),
                        Pt(0.62f, -0.20f),
                        Pt(0.54f, 0.28f),
                        Pt(0.10f, 0.52f),
                        Pt(-0.34f, 0.46f),
                        Pt(-0.64f, 0.10f)
                    ]
                }
            ],
            Rivers =
            [
                new StrategicPolylineDefinition
                {
                    Color = "#619EDEE0",
                    Width = 2.2f,
                    Points =
                    [
                        Pt(-0.78f, -0.66f),
                        Pt(-0.52f, -0.30f),
                        Pt(-0.30f, -0.06f),
                        Pt(0.02f, 0.08f),
                        Pt(0.30f, 0.26f),
                        Pt(0.56f, 0.56f)
                    ]
                }
            ],
            Nodes =
            [
                new StrategicNodeDefinition { X = -0.62f, Y = -0.42f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = -0.20f, Y = -0.58f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = 0.24f, Y = -0.48f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = 0.62f, Y = -0.20f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = 0.54f, Y = 0.28f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = 0.10f, Y = 0.52f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = -0.34f, Y = 0.46f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = -0.64f, Y = 0.10f, Radius = 4f, Color = "#DEE6F7FF" },
                new StrategicNodeDefinition { X = -0.02f, Y = -0.02f, Radius = 6f, Color = "#FFE6B2FF" }
            ]
        };
    }

    private static StrategicPointDefinition Pt(float x, float y)
    {
        return new StrategicPointDefinition { X = x, Y = y };
    }
}
