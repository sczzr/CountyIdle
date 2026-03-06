using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class StrategicMapViewSystem : PanelContainer, IMapZoomView
{
    public enum StrategicMapMode
    {
        World = 0,
        Prefecture = 1
    }

    private const float ZoomStep = 0.1f;
    private const float DefaultMinZoom = 0.6f;
    private const float DefaultMaxZoom = 2.2f;
    private const float PrefectureMaxZoom = 5.6f;
    private const float PrefectureUrbanTextureZoom = 4.0f;
    private const double ZoomTweenDurationSeconds = 0.18;
    private const int PopulationBucketSize = 24;
    private const int HousingBucketSize = 30;
    private const int ThreatBucketSize = 8;
    private const int SettlementBucketSize = 6;
    private static readonly Color MapBackdropColor = new(0.09f, 0.11f, 0.16f, 0.92f);
    private static readonly Color GridColor = new(0.16f, 0.20f, 0.28f, 0.55f);
    private static readonly Color DefaultOutlineColor = new(0.82f, 0.86f, 0.96f, 0.35f);
    private static readonly Color DefaultRouteColor = new(0.95f, 0.79f, 0.42f, 0.82f);
    private static readonly Color DefaultRiverColor = new(0.38f, 0.62f, 0.88f, 0.82f);
    private static readonly Color DefaultNodeColor = new(0.94f, 0.88f, 0.73f, 1f);
    private static readonly Color DefaultLabelColor = new(0.93f, 0.90f, 0.80f, 0.96f);

    [Export]
    private StrategicMapMode _mode = StrategicMapMode.World;

    private readonly StrategicMapConfigSystem _configSystem = new();
    private readonly PrefectureMapGeneratorSystem _prefectureGenerator = new();
    private StrategicMapDefinition _mapDefinition = new();
    private Label? _titleLabel;
    private float _zoom = 1.0f;
    private float _targetZoom = 1.0f;
    private Tween? _zoomTween;
    private int? _populationBucket;
    private int? _housingBucket;
    private int? _threatBucket;
    private int? _settlementBucket;
    private MapViewStyle _operationalStyle = new();

    public float Zoom => _targetZoom;
    public float MinZoom => DefaultMinZoom;
    public float MaxZoom => _mode == StrategicMapMode.Prefecture ? PrefectureMaxZoom : DefaultMaxZoom;
    public float DefaultZoom => 1.0f;

    public override void _Ready()
    {
        ClipContents = true;
        _titleLabel = GetNodeOrNull<Label>("Label");
        if (_mode == StrategicMapMode.World)
        {
            _mapDefinition = _configSystem.GetWorldDefinition();
        }
        else
        {
            _mapDefinition = _prefectureGenerator.Generate(120, 180, 10, 0);
            _populationBucket = 120 / PopulationBucketSize;
            _housingBucket = 180 / HousingBucketSize;
            _threatBucket = 10 / ThreatBucketSize;
            _settlementBucket = 0;
        }

        ConfigureTitleLabel();
        _targetZoom = _zoom;
        UpdateTitle();
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            QueueRedraw();
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
        {
            return;
        }

        if (mouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            AdjustZoom(ZoomStep);
            GetViewport().SetInputAsHandled();
        }
        else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            AdjustZoom(-ZoomStep);
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Draw()
    {
        if (Size.X < 40f || Size.Y < 40f)
        {
            return;
        }

        var mapRect = new Rect2(12f, 44f, Math.Max(Size.X - 24f, 8f), Math.Max(Size.Y - 56f, 8f));
        DrawMapBackdrop(mapRect);
        DrawGrid(mapRect, _mapDefinition.GridLines);

        var center = mapRect.GetCenter();
        var safeScale = Mathf.Clamp(_mapDefinition.UnitScale, 0.20f, 0.90f);
        var unit = Math.Min(mapRect.Size.X, mapRect.Size.Y) * safeScale * _zoom;
        if (unit <= 8f)
        {
            return;
        }

        DrawRegions(center, unit);
        if (_mode == StrategicMapMode.Prefecture && _zoom >= PrefectureUrbanTextureZoom)
        {
            DrawPrefectureUrbanFabric(center, unit);
        }

        DrawPolylines(center, unit, _mapDefinition.Outlines, _operationalStyle.OutlineColor, 1.2f);
        DrawPolylines(center, unit, _mapDefinition.Routes, _operationalStyle.RouteColor, 1.4f);
        DrawPolylines(center, unit, _mapDefinition.Rivers, _operationalStyle.RiverColor, 1.8f);
        DrawNodes(center, unit);
        DrawLabels(center, unit);
    }

    public void RefreshOperationalState(MapViewStyle style)
    {
        _operationalStyle = style ?? new MapViewStyle();
        if (_titleLabel != null)
        {
            _titleLabel.Modulate = _operationalStyle.AccentColor;
        }

        UpdateTitle();
        QueueRedraw();
    }

    private void DrawMapBackdrop(Rect2 mapRect)
    {
        DrawRect(mapRect, _operationalStyle.BackdropColor, true);
    }

    public void SetZoom(float zoom)
    {
        var clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        if (Mathf.IsEqualApprox(clampedZoom, _targetZoom))
        {
            return;
        }

        _targetZoom = clampedZoom;
        _zoomTween?.Kill();
        _zoomTween = CreateTween();
        _zoomTween.SetTrans(Tween.TransitionType.Cubic);
        _zoomTween.SetEase(Tween.EaseType.Out);
        _zoomTween.TweenMethod(Callable.From<float>(SetAnimatedZoom), _zoom, _targetZoom, ZoomTweenDurationSeconds);
        UpdateTitle();
    }

    public void AdjustZoom(float delta)
    {
        SetZoom(_zoom + delta);
    }

    public void RefreshPrefectureMap(int populationHint, int housingHint, double threatHint, int hourSettlements)
    {
        if (_mode != StrategicMapMode.Prefecture)
        {
            return;
        }

        var safePopulation = Math.Max(populationHint, 0);
        var safeHousing = Math.Max(housingHint, 0);
        var safeThreat = Math.Max(threatHint, 0d);
        var safeSettlements = Math.Max(hourSettlements, 0);

        var nextPopulationBucket = safePopulation / PopulationBucketSize;
        var nextHousingBucket = safeHousing / HousingBucketSize;
        var nextThreatBucket = (int)Math.Floor(safeThreat / ThreatBucketSize);
        var nextSettlementBucket = safeSettlements / SettlementBucketSize;

        if (_populationBucket == nextPopulationBucket &&
            _housingBucket == nextHousingBucket &&
            _threatBucket == nextThreatBucket &&
            _settlementBucket == nextSettlementBucket)
        {
            return;
        }

        _populationBucket = nextPopulationBucket;
        _housingBucket = nextHousingBucket;
        _threatBucket = nextThreatBucket;
        _settlementBucket = nextSettlementBucket;

        _mapDefinition = _prefectureGenerator.Generate(safePopulation, safeHousing, safeThreat, safeSettlements);
        UpdateTitle();
        QueueRedraw();
    }

    private void ConfigureTitleLabel()
    {
        if (_titleLabel == null)
        {
            return;
        }

        _titleLabel.SizeFlagsHorizontal = (int)SizeFlags.ShrinkBegin;
        _titleLabel.SizeFlagsVertical = (int)SizeFlags.ShrinkBegin;
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
        _titleLabel.VerticalAlignment = VerticalAlignment.Center;
        _titleLabel.Position = new Vector2(12f, 10f);
        _titleLabel.Modulate = new Color(0.93f, 0.90f, 0.80f, 1f);
    }

    private void UpdateTitle()
    {
        if (_titleLabel == null)
        {
            return;
        }

        var mapName = string.IsNullOrWhiteSpace(_mapDefinition.Title)
            ? (_mode == StrategicMapMode.World ? "天下州域" : "周边郡图")
            : _mapDefinition.Title;
        var statusSuffix = string.IsNullOrWhiteSpace(_operationalStyle.TitleSuffix)
            ? string.Empty
            : $" · {_operationalStyle.TitleSuffix}";
        _titleLabel.Text = $"{mapName}{statusSuffix} · 缩放 {(int)Mathf.Round(_zoom * 100f)}%";
    }

    private void DrawGrid(Rect2 mapRect, int lineCount)
    {
        var safeLineCount = Math.Clamp(lineCount, 2, 16);
        for (var index = 1; index < safeLineCount; index++)
        {
            var progress = index / (float)safeLineCount;
            var x = mapRect.Position.X + (mapRect.Size.X * progress);
            var y = mapRect.Position.Y + (mapRect.Size.Y * progress);
            DrawLine(new Vector2(x, mapRect.Position.Y), new Vector2(x, mapRect.End.Y), _operationalStyle.GridColor, 1f);
            DrawLine(new Vector2(mapRect.Position.X, y), new Vector2(mapRect.End.X, y), _operationalStyle.GridColor, 1f);
        }
    }

    private void DrawRegions(Vector2 center, float unit)
    {
        foreach (var region in _mapDefinition.Regions)
        {
            var polygon = BuildPoints(center, unit, region.Points);
            if (polygon.Length < 3)
            {
                continue;
            }

            var fillColor = ParseColor(region.FillColor, new Color(0.32f, 0.45f, 0.58f, 0.90f));
            DrawFilledPolygon(polygon, fillColor);

            var outlineColor = ParseColor(region.OutlineColor, _operationalStyle.OutlineColor);
            var outlineWidth = Math.Max(region.OutlineWidth, 0.6f);
            DrawPath(polygon, outlineColor, outlineWidth, true);
        }
    }

    private void DrawPolylines(
        Vector2 center,
        float unit,
        System.Collections.Generic.List<StrategicPolylineDefinition>? polylines,
        Color fallbackColor,
        float fallbackWidth)
    {
        if (polylines == null || polylines.Count == 0)
        {
            return;
        }

        foreach (var polyline in polylines)
        {
            var points = BuildPoints(center, unit, polyline.Points);
            if (points.Length < 2)
            {
                continue;
            }

            var color = ParseColor(polyline.Color, fallbackColor);
            var width = Math.Max(polyline.Width, fallbackWidth);
            DrawPath(points, color, width, polyline.Closed);
        }
    }

    private void DrawNodes(Vector2 center, float unit)
    {
        foreach (var node in _mapDefinition.Nodes)
        {
            var canvasPoint = ToCanvas(center, unit, node.X, node.Y);
            var radius = ResolveNodeRadius(node);
            var color = ParseColor(node.Color, _operationalStyle.NodeColor);

            if (_mode == StrategicMapMode.Prefecture && _zoom >= 2.4f)
            {
                DrawPrefectureNode(canvasPoint, radius, color, node.Kind, node.X, node.Y);
                continue;
            }

            DrawCircle(canvasPoint, radius, color);
        }
    }

    private float ResolveNodeRadius(StrategicNodeDefinition node)
    {
        var baseRadius = Math.Max(node.Radius, 1.8f);
        if (_mode != StrategicMapMode.Prefecture)
        {
            return baseRadius;
        }

        var zoomFactor = Mathf.Clamp(1f + ((_zoom - 1f) * 0.28f), 1f, 1.9f);
        return baseRadius * zoomFactor;
    }

    private void DrawPrefectureNode(Vector2 canvasPoint, float radius, Color color, string nodeKind, float nodeX, float nodeY)
    {
        var kind = string.IsNullOrWhiteSpace(nodeKind) ? "settlement" : nodeKind;
        if (string.Equals(kind, "ward", StringComparison.OrdinalIgnoreCase) && _zoom >= 3.4f)
        {
            var cityCenter = GetPrefectureCityCenter();
            var offset = new Vector2(nodeX, nodeY) - cityCenter;
            var patternSeed = CreateBlockPatternSeed(nodeX, nodeY);
            var isMarketWard = Mathf.Abs(offset.X) <= 0.055f || Mathf.Abs(offset.Y) <= 0.035f;
            var verticalMainStreet = Mathf.Abs(offset.X) <= 0.05f || ((patternSeed & 1) == 0);
            DrawWardStreetBlock(canvasPoint, radius, color, verticalMainStreet, isMarketWard, patternSeed);
            return;
        }

        if (string.Equals(kind, "landmark", StringComparison.OrdinalIgnoreCase) && _zoom >= 3.2f)
        {
            DrawCourtyardCompound(canvasPoint, radius, color, false);
            return;
        }

        if (string.Equals(kind, "city", StringComparison.OrdinalIgnoreCase) && _zoom >= 3.2f)
        {
            DrawCourtyardCompound(canvasPoint, radius * 1.15f, color, true);
            return;
        }

        if (string.Equals(kind, "settlement", StringComparison.OrdinalIgnoreCase) && _zoom >= 3.6f)
        {
            DrawVillageCluster(canvasPoint, radius, color);
            return;
        }

        if (string.Equals(kind, "ward", StringComparison.OrdinalIgnoreCase))
        {
            var size = new Vector2(radius * 2.2f, radius * 1.8f);
            DrawRect(new Rect2(canvasPoint - (size * 0.5f), size), color, true);
            DrawLine(canvasPoint + new Vector2(-size.X * 0.5f, 0f), canvasPoint + new Vector2(0f, -size.Y * 0.55f), color.Darkened(0.32f), 1.2f);
            DrawLine(canvasPoint + new Vector2(0f, -size.Y * 0.55f), canvasPoint + new Vector2(size.X * 0.5f, 0f), color.Darkened(0.32f), 1.2f);
            return;
        }

        if (string.Equals(kind, "landmark", StringComparison.OrdinalIgnoreCase))
        {
            var width = radius * 2.6f;
            var height = radius * 2.2f;
            var rect = new Rect2(canvasPoint - new Vector2(width * 0.5f, height * 0.35f), new Vector2(width, height * 0.8f));
            DrawRect(rect, color, true);
            DrawLine(rect.Position, new Vector2(canvasPoint.X, rect.Position.Y - height * 0.45f), color.Darkened(0.35f), 1.4f);
            DrawLine(new Vector2(rect.End.X, rect.Position.Y), new Vector2(canvasPoint.X, rect.Position.Y - height * 0.45f), color.Darkened(0.35f), 1.4f);
            return;
        }

        DrawCircle(canvasPoint, radius, color);
    }

    private void SetAnimatedZoom(float value)
    {
        _zoom = value;
        UpdateTitle();
        QueueRedraw();
    }

    private void DrawWardStreetBlock(Vector2 canvasPoint, float radius, Color color, bool verticalMainStreet, bool isMarketWard, int patternSeed)
    {
        var blockSize = verticalMainStreet
            ? new Vector2(radius * 3.8f, radius * 5.0f)
            : new Vector2(radius * 5.0f, radius * 3.8f);
        var blockRect = new Rect2(canvasPoint - (blockSize * 0.5f), blockSize);
        DrawRect(blockRect, color.Darkened(0.14f), true);
        DrawRect(new Rect2(blockRect.Position + new Vector2(1.0f, 1.0f), blockRect.Size - new Vector2(2.0f, 2.0f)), color.Darkened(0.04f), false);

        var streetColor = color.Lightened(0.24f);
        var laneColor = color.Lightened(0.16f);

        if (verticalMainStreet)
        {
            var streetWidth = Math.Max(2.6f, radius * 0.52f);
            var streetRect = new Rect2(
                new Vector2(canvasPoint.X - (streetWidth * 0.5f), blockRect.Position.Y),
                new Vector2(streetWidth, blockRect.Size.Y));
            DrawRect(streetRect, streetColor, true);

            var crossLaneHeight = Math.Max(1.5f, radius * 0.18f);
            DrawRect(new Rect2(new Vector2(blockRect.Position.X, blockRect.Position.Y + (blockRect.Size.Y * 0.30f)), new Vector2(blockRect.Size.X, crossLaneHeight)), laneColor, true);
            DrawRect(new Rect2(new Vector2(blockRect.Position.X, blockRect.Position.Y + (blockRect.Size.Y * 0.68f)), new Vector2(blockRect.Size.X, crossLaneHeight)), laneColor, true);

            var leftBand = new Rect2(blockRect.Position + new Vector2(radius * 0.10f, radius * 0.14f), new Vector2((blockRect.Size.X * 0.5f) - streetWidth * 0.5f - radius * 0.16f, blockRect.Size.Y - radius * 0.28f));
            var rightBand = new Rect2(new Vector2(streetRect.End.X + radius * 0.06f, blockRect.Position.Y + radius * 0.14f), new Vector2((blockRect.End.X - streetRect.End.X) - radius * 0.16f, blockRect.Size.Y - radius * 0.28f));
            DrawBuildingBand(leftBand, 6 + (patternSeed % 3), false, color);
            DrawBuildingBand(rightBand, 6 + ((patternSeed / 2) % 3), false, color);

            if (_zoom >= 4.9f)
            {
                var topCourt = new Rect2(blockRect.Position + new Vector2(blockRect.Size.X * 0.22f, radius * 0.16f), new Vector2(blockRect.Size.X * 0.18f, blockRect.Size.Y * 0.12f));
                var bottomCourt = new Rect2(new Vector2(blockRect.Position.X + blockRect.Size.X * 0.60f, blockRect.End.Y - (blockRect.Size.Y * 0.16f)), new Vector2(blockRect.Size.X * 0.18f, blockRect.Size.Y * 0.12f));
                DrawCourtyardPatch(topCourt, color);
                DrawCourtyardPatch(bottomCourt, color);
            }

            if (isMarketWard)
            {
                DrawMarketStalls(streetRect, true, color, patternSeed);
            }
        }
        else
        {
            var streetHeight = Math.Max(2.6f, radius * 0.52f);
            var streetRect = new Rect2(
                new Vector2(blockRect.Position.X, canvasPoint.Y - (streetHeight * 0.5f)),
                new Vector2(blockRect.Size.X, streetHeight));
            DrawRect(streetRect, streetColor, true);

            var crossLaneWidth = Math.Max(1.5f, radius * 0.18f);
            DrawRect(new Rect2(new Vector2(blockRect.Position.X + (blockRect.Size.X * 0.28f), blockRect.Position.Y), new Vector2(crossLaneWidth, blockRect.Size.Y)), laneColor, true);
            DrawRect(new Rect2(new Vector2(blockRect.Position.X + (blockRect.Size.X * 0.70f), blockRect.Position.Y), new Vector2(crossLaneWidth, blockRect.Size.Y)), laneColor, true);

            var topBand = new Rect2(blockRect.Position + new Vector2(radius * 0.14f, radius * 0.12f), new Vector2(blockRect.Size.X - radius * 0.28f, (blockRect.Size.Y * 0.5f) - streetHeight * 0.5f - radius * 0.12f));
            var bottomBand = new Rect2(new Vector2(blockRect.Position.X + radius * 0.14f, streetRect.End.Y + radius * 0.06f), new Vector2(blockRect.Size.X - radius * 0.28f, (blockRect.End.Y - streetRect.End.Y) - radius * 0.18f));
            DrawBuildingBand(topBand, 7 + (patternSeed % 3), true, color);
            DrawBuildingBand(bottomBand, 7 + ((patternSeed / 2) % 3), true, color);

            if (_zoom >= 4.9f)
            {
                var leftCourt = new Rect2(blockRect.Position + new Vector2(radius * 0.18f, blockRect.Size.Y * 0.22f), new Vector2(blockRect.Size.X * 0.12f, blockRect.Size.Y * 0.18f));
                var rightCourt = new Rect2(new Vector2(blockRect.End.X - (blockRect.Size.X * 0.18f), blockRect.Position.Y + blockRect.Size.Y * 0.60f), new Vector2(blockRect.Size.X * 0.12f, blockRect.Size.Y * 0.18f));
                DrawCourtyardPatch(leftCourt, color);
                DrawCourtyardPatch(rightCourt, color);
            }

            if (isMarketWard)
            {
                DrawMarketStalls(streetRect, false, color, patternSeed);
            }
        }
    }

    private void DrawHouseRow(Vector2 start, int houseCount, float houseWidth, float houseHeight, Color color)
    {
        var gap = houseWidth * 0.18f;
        for (var index = 0; index < houseCount; index++)
        {
            var left = start.X + (index * (houseWidth + gap));
            var rect = new Rect2(new Vector2(left, start.Y), new Vector2(houseWidth, houseHeight));
            var fill = index % 2 == 0 ? color.Lightened(0.08f) : color;
            DrawRect(rect, fill, true);
            DrawLine(rect.Position, new Vector2(rect.Position.X + (houseWidth * 0.5f), rect.Position.Y - (houseHeight * 0.36f)), fill.Darkened(0.32f), 1.0f);
            DrawLine(new Vector2(rect.End.X, rect.Position.Y), new Vector2(rect.Position.X + (houseWidth * 0.5f), rect.Position.Y - (houseHeight * 0.36f)), fill.Darkened(0.32f), 1.0f);
        }
    }

    private void DrawCourtyardCompound(Vector2 canvasPoint, float radius, Color color, bool isCentral)
    {
        var width = radius * (isCentral ? 4.8f : 4.0f);
        var height = radius * (isCentral ? 3.8f : 3.2f);
        var outerRect = new Rect2(canvasPoint - new Vector2(width * 0.5f, height * 0.5f), new Vector2(width, height));
        DrawRect(outerRect, color.Darkened(0.12f), true);

        var yardInset = new Vector2(width * 0.18f, height * 0.22f);
        var innerRect = new Rect2(outerRect.Position + yardInset, outerRect.Size - (yardInset * 2f));
        DrawRect(innerRect, color.Lightened(0.22f), true);

        var hallRect = new Rect2(new Vector2(outerRect.Position.X + width * 0.18f, outerRect.Position.Y + height * 0.08f), new Vector2(width * 0.64f, height * 0.20f));
        DrawRect(hallRect, color, true);
        DrawLine(hallRect.Position, new Vector2(canvasPoint.X, hallRect.Position.Y - (height * 0.18f)), color.Darkened(0.35f), 1.2f);
        DrawLine(new Vector2(hallRect.End.X, hallRect.Position.Y), new Vector2(canvasPoint.X, hallRect.Position.Y - (height * 0.18f)), color.Darkened(0.35f), 1.2f);

        var gateRect = new Rect2(new Vector2(canvasPoint.X - (width * 0.10f), outerRect.End.Y - (height * 0.16f)), new Vector2(width * 0.20f, height * 0.10f));
        DrawRect(gateRect, color.Lightened(0.10f), true);
    }

    private void DrawVillageCluster(Vector2 canvasPoint, float radius, Color color)
    {
        var offsets = new[]
        {
            new Vector2(-radius * 0.9f, 0f),
            new Vector2(0f, -radius * 0.55f),
            new Vector2(radius * 0.9f, 0f),
            new Vector2(0f, radius * 0.7f)
        };

        foreach (var offset in offsets)
        {
            var center = canvasPoint + offset;
            var size = new Vector2(radius * 1.18f, radius * 0.92f);
            var rect = new Rect2(center - (size * 0.5f), size);
            DrawRect(rect, color, true);
            DrawLine(rect.Position, new Vector2(center.X, rect.Position.Y - (size.Y * 0.34f)), color.Darkened(0.30f), 0.9f);
            DrawLine(new Vector2(rect.End.X, rect.Position.Y), new Vector2(center.X, rect.Position.Y - (size.Y * 0.34f)), color.Darkened(0.30f), 0.9f);
        }
    }

    private void DrawPrefectureUrbanFabric(Vector2 center, float unit)
    {
        if (!TryGetPrefectureUrbanBounds(out var bounds, out var cityCenter, out var wardCount))
        {
            return;
        }

        var topLeft = ToCanvas(center, unit, bounds.Position.X, bounds.Position.Y);
        var bottomRight = ToCanvas(center, unit, bounds.End.X, bounds.End.Y);
        var cityRect = new Rect2(topLeft, bottomRight - topLeft);
        var cityCenterCanvas = ToCanvas(center, unit, cityCenter.X, cityCenter.Y);

        var wallColor = new Color(0.84f, 0.74f, 0.57f, 0.22f);
        DrawRect(cityRect, wallColor, false);

        var mainRoadColor = new Color(0.95f, 0.82f, 0.54f, 0.22f);
        var laneColor = new Color(0.86f, 0.72f, 0.50f, 0.15f);
        var mainRoadWidth = Math.Max(7.0f, cityRect.Size.X * 0.055f);
        var sideRoadWidth = Math.Max(3.0f, cityRect.Size.X * 0.016f);

        var mainAvenue = new Rect2(
            new Vector2(cityCenterCanvas.X - (mainRoadWidth * 0.5f), cityRect.Position.Y),
            new Vector2(mainRoadWidth, cityRect.Size.Y));
        DrawRect(mainAvenue, mainRoadColor, true);

        var marketRect = new Rect2(
            new Vector2(cityCenterCanvas.X - (mainRoadWidth * 1.35f), cityCenterCanvas.Y - (mainRoadWidth * 0.78f)),
            new Vector2(mainRoadWidth * 2.7f, mainRoadWidth * 1.56f));
        DrawRect(marketRect, new Color(0.96f, 0.84f, 0.60f, 0.18f), true);

        var verticalLaneCount = Math.Clamp((int)Mathf.Round(Mathf.Sqrt(wardCount)) - 1, 4, 8);
        for (var index = 0; index < verticalLaneCount; index++)
        {
            var t = (index + 1f) / (verticalLaneCount + 1f);
            var x = Mathf.Lerp(cityRect.Position.X + (cityRect.Size.X * 0.12f), cityRect.End.X - (cityRect.Size.X * 0.12f), t);
            if (Mathf.Abs(x - cityCenterCanvas.X) < mainRoadWidth * 0.85f)
            {
                continue;
            }

            DrawRect(new Rect2(new Vector2(x - (sideRoadWidth * 0.5f), cityRect.Position.Y), new Vector2(sideRoadWidth, cityRect.Size.Y)), laneColor, true);
        }

        var horizontalLaneCount = Math.Clamp((int)Mathf.Round(Mathf.Sqrt(wardCount * 0.72f)), 4, 7);
        for (var index = 0; index < horizontalLaneCount; index++)
        {
            var t = (index + 1f) / (horizontalLaneCount + 1f);
            var y = Mathf.Lerp(cityRect.Position.Y + (cityRect.Size.Y * 0.10f), cityRect.End.Y - (cityRect.Size.Y * 0.10f), t);
            DrawRect(new Rect2(new Vector2(cityRect.Position.X, y - (sideRoadWidth * 0.5f)), new Vector2(cityRect.Size.X, sideRoadWidth)), laneColor, true);
        }

        if (_zoom >= 4.8f)
        {
            DrawMainAvenueFrontage(mainAvenue, cityRect, new Color(0.78f, 0.60f, 0.40f, 0.28f));
        }
    }

    private bool TryGetPrefectureUrbanBounds(out Rect2 bounds, out Vector2 cityCenter, out int wardCount)
    {
        wardCount = 0;
        cityCenter = Vector2.Zero;
        var hasUrbanNode = false;
        var hasCityNode = false;
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var node in _mapDefinition.Nodes)
        {
            if (!IsPrefectureUrbanNode(node.Kind))
            {
                continue;
            }

            hasUrbanNode = true;
            minX = MathF.Min(minX, node.X);
            minY = MathF.Min(minY, node.Y);
            maxX = MathF.Max(maxX, node.X);
            maxY = MathF.Max(maxY, node.Y);

            if (string.Equals(node.Kind, "ward", StringComparison.OrdinalIgnoreCase))
            {
                wardCount++;
            }

            if (string.Equals(node.Kind, "city", StringComparison.OrdinalIgnoreCase))
            {
                cityCenter = new Vector2(node.X, node.Y);
                hasCityNode = true;
            }
        }

        if (!hasUrbanNode)
        {
            bounds = new Rect2();
            return false;
        }

        if (!hasCityNode)
        {
            cityCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        }

        var paddingX = 0.05f;
        var paddingY = 0.04f;
        bounds = new Rect2(
            new Vector2(minX - paddingX, minY - paddingY),
            new Vector2((maxX - minX) + (paddingX * 2f), (maxY - minY) + (paddingY * 2f)));
        return true;
    }

    private Vector2 GetPrefectureCityCenter()
    {
        return TryGetPrefectureUrbanBounds(out _, out var cityCenter, out _) ? cityCenter : Vector2.Zero;
    }

    private static bool IsPrefectureUrbanNode(string nodeKind)
    {
        return string.Equals(nodeKind, "ward", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeKind, "landmark", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nodeKind, "city", StringComparison.OrdinalIgnoreCase);
    }

    private static int CreateBlockPatternSeed(float x, float y)
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 31) + (int)MathF.Round((x + 1.4f) * 1000f);
            hash = (hash * 31) + (int)MathF.Round((y + 1.4f) * 1000f);
            return Math.Abs(hash);
        }
    }

    private void DrawBuildingBand(Rect2 bandRect, int buildingCount, bool splitAlongX, Color color)
    {
        if (bandRect.Size.X <= 3f || bandRect.Size.Y <= 3f || buildingCount <= 0)
        {
            return;
        }

        var gap = splitAlongX
            ? Math.Max(0.8f, bandRect.Size.X * 0.018f)
            : Math.Max(0.8f, bandRect.Size.Y * 0.018f);
        var segmentLength = splitAlongX
            ? (bandRect.Size.X - (gap * (buildingCount - 1))) / buildingCount
            : (bandRect.Size.Y - (gap * (buildingCount - 1))) / buildingCount;

        if (segmentLength <= 1.2f)
        {
            return;
        }

        for (var index = 0; index < buildingCount; index++)
        {
            Rect2 rect;
            if (splitAlongX)
            {
                var left = bandRect.Position.X + index * (segmentLength + gap);
                rect = new Rect2(new Vector2(left, bandRect.Position.Y), new Vector2(segmentLength, bandRect.Size.Y));
            }
            else
            {
                var top = bandRect.Position.Y + index * (segmentLength + gap);
                rect = new Rect2(new Vector2(bandRect.Position.X, top), new Vector2(bandRect.Size.X, segmentLength));
            }

            var insetX = Math.Max(0.4f, rect.Size.X * 0.08f);
            var insetY = Math.Max(0.4f, rect.Size.Y * 0.10f);
            var houseRect = new Rect2(rect.Position + new Vector2(insetX * 0.5f, insetY * 0.5f), rect.Size - new Vector2(insetX, insetY));
            var fill = index % 2 == 0 ? color.Lightened(0.10f) : color.Lightened(0.02f);
            DrawRect(houseRect, fill, true);

            var roofHeight = Math.Max(0.9f, houseRect.Size.Y * 0.18f);
            DrawRect(new Rect2(houseRect.Position, new Vector2(houseRect.Size.X, roofHeight)), fill.Darkened(0.16f), true);
        }
    }

    private void DrawCourtyardPatch(Rect2 patchRect, Color color)
    {
        if (patchRect.Size.X <= 2f || patchRect.Size.Y <= 2f)
        {
            return;
        }

        DrawRect(patchRect, color.Darkened(0.08f), true);
        var yardRect = new Rect2(
            patchRect.Position + new Vector2(patchRect.Size.X * 0.18f, patchRect.Size.Y * 0.22f),
            patchRect.Size - new Vector2(patchRect.Size.X * 0.36f, patchRect.Size.Y * 0.44f));
        DrawRect(yardRect, color.Lightened(0.24f), true);
    }

    private void DrawMarketStalls(Rect2 streetRect, bool verticalStreet, Color color, int patternSeed)
    {
        var stallCount = 6 + (patternSeed % 4);
        var awningColor = color.Lightened(0.26f);

        for (var index = 0; index < stallCount; index++)
        {
            if (verticalStreet)
            {
                var t = (index + 0.5f) / stallCount;
                var y = Mathf.Lerp(streetRect.Position.Y + 1.6f, streetRect.End.Y - 1.6f, t);
                var leftStall = new Rect2(new Vector2(streetRect.Position.X - streetRect.Size.X * 0.55f, y - 0.9f), new Vector2(streetRect.Size.X * 0.36f, 1.8f));
                var rightStall = new Rect2(new Vector2(streetRect.End.X + streetRect.Size.X * 0.19f, y - 0.9f), new Vector2(streetRect.Size.X * 0.36f, 1.8f));
                DrawRect(leftStall, awningColor, true);
                DrawRect(rightStall, index % 2 == 0 ? awningColor.Darkened(0.08f) : awningColor, true);
            }
            else
            {
                var t = (index + 0.5f) / stallCount;
                var x = Mathf.Lerp(streetRect.Position.X + 1.6f, streetRect.End.X - 1.6f, t);
                var topStall = new Rect2(new Vector2(x - 0.9f, streetRect.Position.Y - streetRect.Size.Y * 0.55f), new Vector2(1.8f, streetRect.Size.Y * 0.36f));
                var bottomStall = new Rect2(new Vector2(x - 0.9f, streetRect.End.Y + streetRect.Size.Y * 0.19f), new Vector2(1.8f, streetRect.Size.Y * 0.36f));
                DrawRect(topStall, awningColor, true);
                DrawRect(bottomStall, index % 2 == 0 ? awningColor.Darkened(0.08f) : awningColor, true);
            }
        }
    }

    private void DrawMainAvenueFrontage(Rect2 mainAvenue, Rect2 cityRect, Color frontageColor)
    {
        var segmentCount = 12;
        var segmentHeight = (cityRect.Size.Y - (segmentCount - 1) * 2.4f) / segmentCount;
        if (segmentHeight <= 2f)
        {
            return;
        }

        var leftBandX = mainAvenue.Position.X - (mainAvenue.Size.X * 0.58f);
        var rightBandX = mainAvenue.End.X + (mainAvenue.Size.X * 0.16f);
        var frontageWidth = mainAvenue.Size.X * 0.36f;

        for (var index = 0; index < segmentCount; index++)
        {
            var top = cityRect.Position.Y + index * (segmentHeight + 2.4f);
            var leftRect = new Rect2(new Vector2(leftBandX, top), new Vector2(frontageWidth, segmentHeight));
            var rightRect = new Rect2(new Vector2(rightBandX, top), new Vector2(frontageWidth, segmentHeight));
            DrawRect(leftRect, frontageColor, true);
            DrawRect(rightRect, index % 2 == 0 ? frontageColor.Lightened(0.08f) : frontageColor, true);
        }
    }

    private void DrawLabels(Vector2 center, float unit)
    {
        if (_mapDefinition.Labels == null || _mapDefinition.Labels.Count == 0)
        {
            return;
        }

        var font = GetThemeDefaultFont();
        if (font == null)
        {
            return;
        }

        foreach (var label in _mapDefinition.Labels)
        {
            if (string.IsNullOrWhiteSpace(label.Text))
            {
                continue;
            }

            if (_zoom < label.MinZoom)
            {
                continue;
            }

            var canvasPoint = ToCanvas(center, unit, label.X, label.Y);
            var color = ParseColor(label.Color, _operationalStyle.LabelColor);
            var zoomedFontSize = (int)Mathf.Round(label.FontSize * Mathf.Clamp(0.90f + (_zoom * 0.16f), 0.90f, 1.26f));
            var fontSize = Math.Clamp(zoomedFontSize, 10, 18);

            DrawString(font, canvasPoint, label.Text, HorizontalAlignment.Left, -1f, fontSize, color);
        }
    }

    private static Vector2[] BuildPoints(Vector2 center, float unit, System.Collections.Generic.List<StrategicPointDefinition>? points)
    {
        if (points == null || points.Count == 0)
        {
            return [];
        }

        var output = new Vector2[points.Count];
        for (var index = 0; index < points.Count; index++)
        {
            output[index] = ToCanvas(center, unit, points[index].X, points[index].Y);
        }

        return output;
    }

    private static Vector2 ToCanvas(Vector2 center, float unit, float x, float y)
    {
        return center + new Vector2(x * unit, y * unit);
    }

    private void DrawPath(Vector2[] points, Color color, float width, bool closed)
    {
        for (var index = 0; index < points.Length - 1; index++)
        {
            DrawLine(points[index], points[index + 1], color, width);
        }

        if (closed && points.Length > 2)
        {
            DrawLine(points[^1], points[0], color, width);
        }
    }

    private void DrawFilledPolygon(Vector2[] polygon, Color color)
    {
        var vertexColors = new Color[polygon.Length];
        for (var index = 0; index < vertexColors.Length; index++)
        {
            vertexColors[index] = color;
        }

        DrawPolygon(polygon, vertexColors);
    }

    private static Color ParseColor(string colorHex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return fallback;
        }

        return Color.FromString(colorHex, fallback);
    }
}
