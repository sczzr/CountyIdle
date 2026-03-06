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
    private static readonly Color MapBackdropColor = new(0.09f, 0.11f, 0.16f, 0.92f);
    private static readonly Color GridColor = new(0.16f, 0.20f, 0.28f, 0.55f);
    private static readonly Color DefaultOutlineColor = new(0.82f, 0.86f, 0.96f, 0.35f);
    private static readonly Color DefaultRouteColor = new(0.95f, 0.79f, 0.42f, 0.82f);
    private static readonly Color DefaultRiverColor = new(0.38f, 0.62f, 0.88f, 0.82f);
    private static readonly Color DefaultNodeColor = new(0.94f, 0.88f, 0.73f, 1f);

    [Export]
    private StrategicMapMode _mode = StrategicMapMode.World;

    private readonly StrategicMapConfigSystem _configSystem = new();
    private StrategicMapDefinition _mapDefinition = new();
    private Label? _titleLabel;
    private float _zoom = 1.0f;

    public float Zoom => _zoom;
    public float MinZoom => 0.6f;
    public float MaxZoom => 2.2f;
    public float DefaultZoom => 1.0f;

    public override void _Ready()
    {
        ClipContents = true;
        _titleLabel = GetNodeOrNull<Label>("Label");
        _mapDefinition = _mode == StrategicMapMode.World
            ? _configSystem.GetWorldDefinition()
            : _configSystem.GetPrefectureDefinition();

        ConfigureTitleLabel();
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
        DrawPolylines(center, unit, _mapDefinition.Outlines, DefaultOutlineColor, 1.2f);
        DrawPolylines(center, unit, _mapDefinition.Routes, DefaultRouteColor, 1.4f);
        DrawPolylines(center, unit, _mapDefinition.Rivers, DefaultRiverColor, 1.8f);
        DrawNodes(center, unit);
    }

    private void DrawMapBackdrop(Rect2 mapRect)
    {
        DrawRect(mapRect, MapBackdropColor, true);
    }

    public void SetZoom(float zoom)
    {
        var clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        if (Mathf.IsEqualApprox(clampedZoom, _zoom))
        {
            return;
        }

        _zoom = clampedZoom;
        UpdateTitle();
        QueueRedraw();
    }

    public void AdjustZoom(float delta)
    {
        SetZoom(_zoom + delta);
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
        _titleLabel.Text = $"{mapName} · 缩放 {(int)Mathf.Round(_zoom * 100f)}%";
    }

    private void DrawGrid(Rect2 mapRect, int lineCount)
    {
        var safeLineCount = Math.Clamp(lineCount, 2, 16);
        for (var index = 1; index < safeLineCount; index++)
        {
            var progress = index / (float)safeLineCount;
            var x = mapRect.Position.X + (mapRect.Size.X * progress);
            var y = mapRect.Position.Y + (mapRect.Size.Y * progress);
            DrawLine(new Vector2(x, mapRect.Position.Y), new Vector2(x, mapRect.End.Y), GridColor, 1f);
            DrawLine(new Vector2(mapRect.Position.X, y), new Vector2(mapRect.End.X, y), GridColor, 1f);
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

            var outlineColor = ParseColor(region.OutlineColor, DefaultOutlineColor);
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
            var radius = Math.Max(node.Radius, 1.8f);
            var color = ParseColor(node.Color, DefaultNodeColor);
            DrawCircle(canvasPoint, radius, color);
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
