using System;
using System.Collections.Generic;
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
    private const float DefaultMinZoom = 1f;
    private const float DefaultMaxZoom = 15.0f;
    private const float PrefectureMaxZoom = 5.0f;
    private const float PrefectureUrbanTextureZoom = 4.0f;
    private const float ZoomVelocityDamping = 8.0f;
    private const float ZoomLerpSpeed = 10.0f;
    private const float PanSpeed = 520f;
    private const int PopulationBucketSize = 24;
    private const int HousingBucketSize = 30;
    private const int ThreatBucketSize = 8;
    private const int SettlementBucketSize = 6;
    private const float HexGridFillAlphaEven = 0.08f;
    private const float HexGridFillAlphaOdd = 0.12f;
    private const string Layer1TileSetPath = "res://assets/ui/tilemap/L1_hex_tileset.tres";
    private const string WorldHexTileClipShaderPath = "res://assets/ui/tilemap/world_hex_tile_clip.gdshader";
    private static readonly Vector2I[] WorldPlainTileCoords =
    [
        new Vector2I(0, 1),
        new Vector2I(2, 1),
        new Vector2I(3, 1)
    ];
    private static readonly Vector2I[] WorldSpiritTileCoords =
    [
        new Vector2I(1, 1),
        new Vector2I(2, 0),
        new Vector2I(2, 1)
    ];
    private static readonly Vector2I[] WorldWaterTileCoords =
    [
        new Vector2I(1, 0),
        new Vector2I(3, 0)
    ];
    private static readonly Vector2I[] WorldRuggedTileCoords =
    [
        new Vector2I(0, 0),
        new Vector2I(1, 1),
        new Vector2I(3, 1)
    ];
    private static readonly Vector2I[] WorldSnowTileCoords =
    [
        new Vector2I(2, 0)
    ];
    private static readonly Color MapBackdropColor = new(0.09f, 0.11f, 0.16f, 0.92f);
    private static readonly Color GridColor = new(0.16f, 0.20f, 0.28f, 0.55f);
    private static readonly Color DefaultOutlineColor = new(0.82f, 0.86f, 0.96f, 0.35f);
    private static readonly Color DefaultRouteColor = new(0.95f, 0.79f, 0.42f, 0.82f);
    private static readonly Color DefaultRiverColor = new(0.38f, 0.62f, 0.88f, 0.82f);
    private static readonly Color DefaultNodeColor = new(0.94f, 0.88f, 0.73f, 1f);
    private static readonly Color DefaultLabelColor = new(0.93f, 0.90f, 0.80f, 0.96f);
    private static readonly HexEdgeDefinition[] WorldHexEdges =
    [
        new(HexDirectionMask.NorthEast, 0, 1, 1, -1),
        new(HexDirectionMask.East, 1, 2, 1, 0),
        new(HexDirectionMask.SouthEast, 2, 3, 0, 1),
        new(HexDirectionMask.SouthWest, 3, 4, -1, 1),
        new(HexDirectionMask.West, 4, 5, -1, 0),
        new(HexDirectionMask.NorthWest, 5, 0, 0, -1)
    ];

    [Export]
    private StrategicMapMode _mode = StrategicMapMode.World;

    private readonly StrategicMapConfigSystem _configSystem = new();
    private readonly PrefectureMapGeneratorSystem _prefectureGenerator = new();
    private readonly XianxiaWorldGeneratorSystem _xianxiaWorldGenerator = new();
    private readonly Color[] _hexTintColors = new Color[6];
    private StrategicMapDefinition _mapDefinition = new();
    private XianxiaWorldMapData? _xianxiaWorldMap;
    private Dictionary<(int Q, int R), XianxiaHexCellData> _xianxiaWorldCellLookup = [];
    private Dictionary<(int Q, int R), Vector2> _xianxiaWorldCenters = [];
    private Dictionary<(int Q, int R), Vector2I> _xianxiaWorldTileCells = [];
    private float _xianxiaWorldHexRadius = 0.01f;
    private Vector2 _xianxiaWorldTileCenterLocal = Vector2.Zero;
    private float _xianxiaWorldTileLayoutScale = 1f;
    private TileSet? _worldLayer1TileSet;
    private TileMapLayer? _worldTerrainTileLayer;
    private ShaderMaterial? _worldTerrainClipMaterial;
    private int _worldLayer1SourceId = -1;
    private Label? _titleLabel;
    private Node? _toneFx;
    private float _zoom = 1.0f;
    private float _targetZoom = 1.0f;
    private float _zoomVelocity;
    private Vector2 _panOffset = Vector2.Zero;
    private int? _populationBucket;
    private int? _housingBucket;
    private int? _threatBucket;
    private int? _settlementBucket;
    private MapViewStyle _operationalStyle = new();
    private XianxiaSiteData? _selectedWorldSite;

    public event Action<XianxiaSiteData?>? WorldSiteSelectionChanged;
    public float Zoom => _targetZoom;
    public float MinZoom => DefaultMinZoom;
    public float MaxZoom => _mode == StrategicMapMode.Prefecture ? PrefectureMaxZoom : DefaultMaxZoom;
    public float DefaultZoom => 1.0f;
    public XianxiaSiteData? SelectedWorldSite => _selectedWorldSite;

    public XianxiaHexCellData? GetWorldCellForSite(XianxiaSiteData? site)
    {
        if (site == null)
        {
            return null;
        }

        return _xianxiaWorldCellLookup.TryGetValue((site.Coord.Q, site.Coord.R), out var cell)
            ? cell
            : null;
    }

    public override void _Ready()
    {
        ClipContents = true;
        _titleLabel = GetNodeOrNull<Label>("Label");
        _toneFx = GetNodeOrNull<Node>("ToneFx");
        LoadAtlases();
        EnsureWorldTerrainLayerInfrastructure();
        if (_mode == StrategicMapMode.World)
        {
            try
            {
                _mapDefinition = _xianxiaWorldGenerator.GenerateStrategicDefinition(out var worldMap);
                CacheWorldLayout(worldMap);
            }
            catch (Exception exception)
            {
                GD.PushWarning($"Xianxia world generation failed, fallback to configured world map: {exception.Message}");
                _mapDefinition = _configSystem.GetWorldDefinition();
                _xianxiaWorldMap = null;
                _xianxiaWorldCellLookup = [];
                _xianxiaWorldCenters = [];
                _xianxiaWorldHexRadius = 0.01f;
                ClearWorldTerrainTileLayer();
            }
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
        UpdateWorldTerrainLayerLayout();
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            UpdateWorldTerrainLayerLayout();
            QueueRedraw();
        }
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        var needsUpdate = false;

        var panDirection = GetPanDirection();
        if (panDirection != Vector2.Zero)
        {
            _panOffset += panDirection.Normalized() * PanSpeed * dt;
            needsUpdate = true;
        }

        if (Mathf.Abs(_zoomVelocity) > 0.001f)
        {
            _targetZoom = Mathf.Clamp(_targetZoom + (_zoomVelocity * dt), MinZoom, MaxZoom);
            _zoomVelocity = Mathf.Lerp(_zoomVelocity, 0f, dt * ZoomVelocityDamping);
            needsUpdate = true;
        }

        var nextZoom = Mathf.Lerp(_zoom, _targetZoom, dt * ZoomLerpSpeed);
        if (!Mathf.IsEqualApprox(nextZoom, _zoom))
        {
            _zoom = nextZoom;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            UpdateTitle();
            UpdateWorldTerrainLayerLayout();
            QueueRedraw();
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
        {
            return;
        }

        if (mouseButton.ButtonIndex == MouseButton.Left && _mode == StrategicMapMode.World)
        {
            TrySelectWorldSite(mouseButton.Position);
            GetViewport().SetInputAsHandled();
        }
        else if (mouseButton.ButtonIndex == MouseButton.Right && _mode == StrategicMapMode.World)
        {
            ClearWorldSiteSelection();
            GetViewport().SetInputAsHandled();
        }
        else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            AdjustZoomAt(mouseButton.Position, ZoomStep);
            GetViewport().SetInputAsHandled();
        }
        else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            AdjustZoomAt(mouseButton.Position, -ZoomStep);
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree() || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        if (keyEvent.Keycode is Key.Up or Key.Down or Key.Left or Key.Right)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Draw()
    {
        if (Size.X < 40f || Size.Y < 40f)
        {
            return;
        }

        var mapRect = GetMapRect();
        DrawMapBackdrop(mapRect);
        var mapRectWithOffset = new Rect2(mapRect.Position + _panOffset, mapRect.Size);
        if (_mode != StrategicMapMode.World)
        {
            DrawGrid(mapRectWithOffset, _mapDefinition.GridLines);
        }

        var center = mapRectWithOffset.GetCenter();
        var unit = GetUnitForZoom(mapRect, _zoom);
        if (unit <= 8f)
        {
            return;
        }

        if (_mode == StrategicMapMode.World && _xianxiaWorldMap != null)
        {
            DrawWorldTextureCells(center, unit);
        }
        else
        {
            DrawRegions(center, unit);
        }
        if (_mode == StrategicMapMode.Prefecture && _zoom >= PrefectureUrbanTextureZoom)
        {
            DrawPrefectureUrbanFabric(center, unit);
        }

        if (_mode == StrategicMapMode.World && _xianxiaWorldMap != null)
        {
            DrawWorldEdgeOverlays(center, unit);
            DrawWorldCellSelectionOverlay(center, unit);
        }

        DrawPolylines(center, unit, _mapDefinition.Outlines, _operationalStyle.OutlineColor, 1.2f);
        DrawPolylines(center, unit, _mapDefinition.Routes, _operationalStyle.RouteColor, 1.4f);
        if (!(_mode == StrategicMapMode.World && _xianxiaWorldMap != null))
        {
            DrawPolylines(center, unit, _mapDefinition.Rivers, _operationalStyle.RiverColor, 1.8f);
        }
        DrawNodes(center, unit);
        DrawLabels(center, unit);
    }

    public void RefreshOperationalState(MapViewStyle style)
    {
        _operationalStyle = style ?? new MapViewStyle();
        CallToneFx("apply_title_tone", _operationalStyle.AccentColor);
        UpdateTitle();
        UpdateWorldTerrainLayerLayout();
        QueueRedraw();
    }

    private Rect2 GetMapRect()
    {
        return new Rect2(12f, 44f, Math.Max(Size.X - 24f, 8f), Math.Max(Size.Y - 56f, 8f));
    }

    private float GetUnitForZoom(Rect2 mapRect, float zoom)
    {
        var safeScale = Mathf.Clamp(_mapDefinition.UnitScale, 0.20f, 0.90f);
        return Math.Min(mapRect.Size.X, mapRect.Size.Y) * safeScale * zoom;
    }

    private void DrawMapBackdrop(Rect2 mapRect)
    {
        DrawRect(mapRect, _operationalStyle.BackdropColor, true);
    }

    private void LoadAtlases()
    {
    }

    public void SetZoom(float zoom)
    {
        SetZoomTarget(zoom);
    }

    public void AdjustZoom(float delta)
    {
        SetZoom(_targetZoom + delta);
    }

    public void ResetView()
    {
        _panOffset = Vector2.Zero;
        SetZoomTarget(DefaultZoom, null, true);
    }

    public void ClearWorldSiteSelection()
    {
        if (_selectedWorldSite == null)
        {
            return;
        }

        _selectedWorldSite = null;
        WorldSiteSelectionChanged?.Invoke(null);
        UpdateWorldTerrainLayerLayout();
        QueueRedraw();
    }

    private void AdjustZoomAt(Vector2 anchorPosition, float delta)
    {
        SetZoomTarget(_targetZoom + delta, anchorPosition);
    }

    private void SetZoomTarget(float zoom, Vector2? anchorPosition = null, bool force = false)
    {
        var clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        if (!force && Mathf.IsEqualApprox(clampedZoom, _targetZoom))
        {
            return;
        }

        var mapRect = GetMapRect();
        var baseCenter = mapRect.GetCenter();
        var unitCurrent = GetUnitForZoom(mapRect, _zoom);
        var unitNext = GetUnitForZoom(mapRect, clampedZoom);

        if (anchorPosition.HasValue && unitCurrent > 0.01f && unitNext > 0.01f)
        {
            var anchor = anchorPosition.Value;
            var mapSpace = (anchor - (baseCenter + _panOffset)) / unitCurrent;
            _panOffset = anchor - baseCenter - (mapSpace * unitNext);
        }

        _targetZoom = clampedZoom;
        _zoomVelocity = 0f;
        _zoom = clampedZoom;
        UpdateTitle();
        UpdateWorldTerrainLayerLayout();
        QueueRedraw();
    }

    private Vector2 GetPanDirection()
    {
        var direction = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W))
        {
            direction.Y += 1f;
        }

        if (Input.IsKeyPressed(Key.S))
        {
            direction.Y -= 1f;
        }

        if (Input.IsKeyPressed(Key.A))
        {
            direction.X += 1f;
        }

        if (Input.IsKeyPressed(Key.D))
        {
            direction.X -= 1f;
        }

        if (Input.IsKeyPressed(Key.Up))
        {
            direction.Y += 1f;
        }

        if (Input.IsKeyPressed(Key.Down))
        {
            direction.Y -= 1f;
        }

        if (Input.IsKeyPressed(Key.Left))
        {
            direction.X += 1f;
        }

        if (Input.IsKeyPressed(Key.Right))
        {
            direction.X -= 1f;
        }

        return direction;
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
        UpdateWorldTerrainLayerLayout();
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
        CallToneFx("apply_title_tone", _operationalStyle.AccentColor);
    }

    private void CallToneFx(string methodName, params Variant[] args)
    {
        _toneFx?.Call(methodName, args);
    }

    private void UpdateTitle()
    {
        if (_titleLabel == null)
        {
            return;
        }

        var mapName = string.IsNullOrWhiteSpace(_mapDefinition.Title)
            ? (_mode == StrategicMapMode.World ? SectMapSemanticRules.GetWorldMapTitle() : SectMapSemanticRules.GetLegacyPrefectureMapTitle())
            : _mapDefinition.Title;
        var statusSuffix = string.IsNullOrWhiteSpace(_operationalStyle.TitleSuffix)
            ? string.Empty
            : $" · {_operationalStyle.TitleSuffix}";
        _titleLabel.Text = $"{mapName}{statusSuffix} · 六角俯视 · 缩放 {(int)Mathf.Round(_zoom * 100f)}%";
    }

    private void DrawGrid(Rect2 mapRect, int lineCount)
    {
        var safeLineCount = Math.Clamp(lineCount, 3, 18);
        var columns = safeLineCount;
        var rows = Math.Max(safeLineCount - 1, 3);
        var sqrt3 = Mathf.Sqrt(3f);
        var radiusFromWidth = mapRect.Size.X / (sqrt3 * (columns + 0.5f));
        var radiusFromHeight = mapRect.Size.Y / ((rows * 1.5f) + 0.5f);
        var radius = Math.Max(Math.Min(radiusFromWidth, radiusFromHeight), 8f);
        var hexWidth = sqrt3 * radius;
        var totalWidth = (columns * hexWidth) + (hexWidth * 0.5f);
        var totalHeight = ((rows - 1) * radius * 1.5f) + (radius * 2f);
        var startX = mapRect.Position.X + ((mapRect.Size.X - totalWidth) * 0.5f) + (hexWidth * 0.5f);
        var startY = mapRect.Position.Y + ((mapRect.Size.Y - totalHeight) * 0.5f) + radius;
        var outlineBase = _operationalStyle.GridColor;
        var outlineColor = new Color(outlineBase.R, outlineBase.G, outlineBase.B, Math.Min(outlineBase.A + 0.08f, 0.70f));

        for (var row = 0; row < rows; row++)
        {
            var rowOffset = row % 2 == 0 ? 0f : hexWidth * 0.5f;
            var centerY = startY + (row * radius * 1.5f);

            for (var column = 0; column < columns; column++)
            {
                var centerX = startX + rowOffset + (column * hexWidth);
                var center = new Vector2(centerX, centerY);
                if (!mapRect.Grow(radius * 0.6f).HasPoint(center))
                {
                    continue;
                }

                var fillAlpha = (row + column) % 2 == 0 ? HexGridFillAlphaEven : HexGridFillAlphaOdd;
                var fillColor = new Color(outlineBase.R, outlineBase.G, outlineBase.B, fillAlpha);
                var hex = BuildHexPolygon(center, radius * 0.98f);
                DrawFilledPolygon(hex, fillColor);
                DrawPath(hex, outlineColor, 1f, true);
            }
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

            DrawHexMarker(canvasPoint, radius, color);
            if (_mode == StrategicMapMode.World && IsSelectedWorldSite(node))
            {
                var selectionHex = BuildHexPolygon(canvasPoint, radius * 1.34f);
                DrawPath(selectionHex, color.Lightened(0.25f), Math.Max(1.5f, radius * 0.18f), true);
                DrawCircle(canvasPoint, Math.Max(1.2f, radius * 0.24f), new Color(0.98f, 0.95f, 0.84f, 0.92f));
            }
        }
    }

    private void DrawWorldTextureCells(Vector2 center, float unit)
    {
        if (_xianxiaWorldMap == null || _xianxiaWorldCenters.Count == 0)
        {
            return;
        }

        var hexRadius = Math.Max(_xianxiaWorldHexRadius * unit, 2f);
        var tint = _operationalStyle.TerrainTint;
        foreach (var cell in _xianxiaWorldMap.Cells)
        {
            if (!_xianxiaWorldCenters.TryGetValue((cell.Coord.Q, cell.Coord.R), out var normalizedCenter))
            {
                continue;
            }

            var canvasCenter = ToCanvas(center, unit, normalizedCenter.X, normalizedCenter.Y);
            var hex = BuildHexPolygon(canvasCenter, hexRadius);
            if (!TryDrawWorldLayer1TileSetHex(hex, cell, tint))
            {
                DrawFilledPolygon(hex, tint);
            }
        }
    }

    private bool TryDrawWorldLayer1TileSetHex(Vector2[] hex, XianxiaHexCellData cell, Color tint)
    {
        if (_worldLayer1TileSet == null)
        {
            return false;
        }

        var variant = ResolveWorldLayer1TileVariant(cell);
        if (variant.SourceId < 0 ||
            _worldLayer1TileSet.GetSource(variant.SourceId) is not TileSetAtlasSource atlasSource ||
            atlasSource.Texture == null)
        {
            return false;
        }

        var textureRegion = atlasSource.GetTileTextureRegion(variant.AtlasCoords);
        if (textureRegion.Size == Vector2I.Zero)
        {
            return false;
        }

        for (var index = 0; index < _hexTintColors.Length; index++)
        {
            _hexTintColors[index] = tint;
        }

        DrawPolygon(
            hex,
            _hexTintColors,
            CreateAtlasRegionUv(new Rect2(textureRegion.Position, textureRegion.Size), atlasSource.Texture),
            atlasSource.Texture);
        return true;
    }

    private static bool IsMountainCell(XianxiaHexCellData cell)
    {
        return IsMountainTerrain(cell.Terrain) ||
               IsMountainBiome(cell.Biome);
    }

    private static bool IsMountainTerrain(XianxiaTerrainType terrain)
    {
        return terrain is XianxiaTerrainType.MountainRock or
            XianxiaTerrainType.MountainMoss or
            XianxiaTerrainType.MountainPlateau or
            XianxiaTerrainType.SnowPlain or
            XianxiaTerrainType.SnowRock;
    }

    private static bool IsSnowTerrain(XianxiaTerrainType terrain)
    {
        return terrain is XianxiaTerrainType.SnowPlain or XianxiaTerrainType.SnowRock;
    }

    private static bool IsMountainBiome(XianxiaBiomeType biome)
    {
        return biome is XianxiaBiomeType.MistyMountains or
            XianxiaBiomeType.JadeHighlands or
            XianxiaBiomeType.SnowPeaks;
    }

    private static int ResolveVariantColumn(XianxiaHexCellData cell, int salt)
    {
        var hash = HashCell(cell.Coord.Q, cell.Coord.R, salt);
        var variant = cell.Render?.VariantIndex ?? 0;
        return (variant + hash) % HexAtlas5x4.Columns;
    }

    private static int HashCell(int q, int r, int salt)
    {
        unchecked
        {
            var hash = (q * 73856093) ^ (r * 19349663) ^ (salt * 83492791);
            return hash & int.MaxValue;
        }
    }

    private static Vector2[] CreateAtlasRegionUv(Rect2 region, Texture2D texture)
    {
        var atlasWidth = texture.GetWidth();
        var atlasHeight = texture.GetHeight();

        return
        [
            new Vector2((region.Position.X + (region.Size.X * 0.5f)) / atlasWidth, region.Position.Y / atlasHeight),
            new Vector2((region.Position.X + region.Size.X) / atlasWidth, (region.Position.Y + (region.Size.Y * 0.25f)) / atlasHeight),
            new Vector2((region.Position.X + region.Size.X) / atlasWidth, (region.Position.Y + (region.Size.Y * 0.75f)) / atlasHeight),
            new Vector2((region.Position.X + (region.Size.X * 0.5f)) / atlasWidth, (region.Position.Y + region.Size.Y) / atlasHeight),
            new Vector2(region.Position.X / atlasWidth, (region.Position.Y + (region.Size.Y * 0.75f)) / atlasHeight),
            new Vector2(region.Position.X / atlasWidth, (region.Position.Y + (region.Size.Y * 0.25f)) / atlasHeight)
        ];
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
        if (string.Equals(kind, "raw_source", StringComparison.OrdinalIgnoreCase))
        {
            DrawRawSourceNode(canvasPoint, radius, color);
            return;
        }

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

    private void DrawRawSourceNode(Vector2 canvasPoint, float radius, Color color)
    {
        var outer = new[]
        {
            canvasPoint + new Vector2(0f, -radius * 1.12f),
            canvasPoint + new Vector2(radius * 0.96f, 0f),
            canvasPoint + new Vector2(0f, radius * 1.12f),
            canvasPoint + new Vector2(-radius * 0.96f, 0f)
        };
        var inner = new[]
        {
            canvasPoint + new Vector2(0f, -radius * 0.62f),
            canvasPoint + new Vector2(radius * 0.56f, 0f),
            canvasPoint + new Vector2(0f, radius * 0.62f),
            canvasPoint + new Vector2(-radius * 0.56f, 0f)
        };

        DrawFilledPolygon(outer, color.Darkened(0.10f));
        DrawFilledPolygon(inner, color.Lightened(0.14f));
        for (var index = 0; index < outer.Length; index++)
        {
            DrawLine(outer[index], outer[(index + 1) % outer.Length], color.Lightened(0.28f), 1.1f);
        }

        DrawCircle(canvasPoint, Math.Max(1.2f, radius * 0.22f), new Color(0.97f, 0.95f, 0.88f, 0.92f));
    }

    private void DrawHexMarker(Vector2 canvasPoint, float radius, Color color)
    {
        var hex = BuildHexPolygon(canvasPoint, Math.Max(radius * 1.12f, 3.2f));
        DrawFilledPolygon(hex, color);
        DrawPath(hex, color.Darkened(0.28f), Math.Max(radius * 0.16f, 1.0f), true);
        DrawCircle(canvasPoint, Math.Max(1.2f, radius * 0.24f), new Color(0.97f, 0.95f, 0.88f, 0.92f));
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

    private void CacheWorldLayout(XianxiaWorldMapData worldMap)
    {
        _xianxiaWorldMap = worldMap;
        _xianxiaWorldCellLookup = [];
        _xianxiaWorldCenters = [];
        _xianxiaWorldTileCells = [];
        _xianxiaWorldHexRadius = 0.01f;
        _xianxiaWorldTileCenterLocal = Vector2.Zero;
        _xianxiaWorldTileLayoutScale = 1f;
        ClearWorldTerrainTileLayer();

        var rawCenters = new Dictionary<(int Q, int R), Vector2>(worldMap.Cells.Count);
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var sqrt3 = Mathf.Sqrt(3f);

        foreach (var cell in worldMap.Cells)
        {
            _xianxiaWorldCellLookup[(cell.Coord.Q, cell.Coord.R)] = cell;
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
            return;
        }

        var worldCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        var spanX = Math.Max(maxX - minX, 0.01f);
        var spanY = Math.Max(maxY - minY, 0.01f);
        var scale = 1.82f / Math.Max(spanX, spanY);
        _xianxiaWorldHexRadius = 0.94f * scale;

        foreach (var pair in rawCenters)
        {
            _xianxiaWorldCenters[pair.Key] = (pair.Value - worldCenter) * scale;
        }

        UpdateWorldTerrainLayerLayout();
    }

    private bool TrySelectWorldSite(Vector2 mousePosition)
    {
        if (_xianxiaWorldMap == null || _xianxiaWorldCenters.Count == 0)
        {
            return false;
        }

        var mapRect = GetMapRect();
        var center = (new Rect2(mapRect.Position + _panOffset, mapRect.Size)).GetCenter();
        var unit = GetUnitForZoom(mapRect, _zoom);
        var bestSite = TryPickWorldSite(mousePosition, center, unit) ?? TryBuildWorldCellSite(mousePosition, center, unit);

        if (bestSite == null)
        {
            ClearWorldSiteSelection();
            return false;
        }

        if (IsSameSite(_selectedWorldSite, bestSite))
        {
            return true;
        }

        _selectedWorldSite = bestSite;
        WorldSiteSelectionChanged?.Invoke(_selectedWorldSite);
        QueueRedraw();
        return true;
    }

    private void DrawWorldEdgeOverlays(Vector2 center, float unit)
    {
        if (_xianxiaWorldMap == null || _xianxiaWorldCenters.Count == 0)
        {
            return;
        }

        var hexRadius = Math.Max(_xianxiaWorldHexRadius * unit, 2f);
        var riverColor = new Color(_operationalStyle.RiverColor.R, _operationalStyle.RiverColor.G, _operationalStyle.RiverColor.B, 0.94f);
        var riverOutline = riverColor.Darkened(0.32f);
        riverOutline.A = 0.96f;
        var roadColor = new Color(0.86f, 0.74f, 0.52f, 0.88f);
        var roadOutline = new Color(0.52f, 0.40f, 0.25f, 0.92f);
        var shoreColor = new Color(0.95f, 0.91f, 0.74f, 0.90f);
        var shoreOutline = new Color(0.60f, 0.54f, 0.41f, 0.86f);
        var cliffColor = new Color(0.19f, 0.16f, 0.17f, 0.86f);
        var cliffOutline = new Color(0.08f, 0.06f, 0.07f, 0.94f);

        foreach (var cell in _xianxiaWorldMap.Cells)
        {
            if (cell.RiverMask == HexDirectionMask.None &&
                cell.RoadMask == HexDirectionMask.None &&
                cell.CliffMask == HexDirectionMask.None &&
                cell.Water == XianxiaWaterType.None)
            {
                continue;
            }

            if (!_xianxiaWorldCenters.TryGetValue((cell.Coord.Q, cell.Coord.R), out var normalizedCenter))
            {
                continue;
            }

            var canvasCenter = ToCanvas(center, unit, normalizedCenter.X, normalizedCenter.Y);
            var hex = BuildHexPolygon(canvasCenter, hexRadius);

            if (cell.CliffMask != HexDirectionMask.None)
            {
                DrawMaskedHexEdges(cell.Coord, cell.CliffMask, hex, hexRadius * 0.12f, cliffColor, cliffOutline);
            }

            if (cell.Water != XianxiaWaterType.None)
            {
                DrawWaterShorelines(cell, hex, hexRadius * 0.10f, shoreColor, shoreOutline);
            }

            if (cell.RiverMask != HexDirectionMask.None)
            {
                DrawMaskedHexEdges(cell.Coord, cell.RiverMask, hex, hexRadius * 0.34f, riverColor, riverOutline);
            }

            if (cell.RoadMask != HexDirectionMask.None)
            {
                DrawMaskedHexEdges(cell.Coord, cell.RoadMask, hex, hexRadius * 0.18f, roadColor, roadOutline);
            }

            var overlapMask = cell.RiverMask & cell.RoadMask;
            if (overlapMask != HexDirectionMask.None)
            {
                DrawBridgeOverlays(cell.Coord, overlapMask, hex, hexRadius * 0.24f);
            }

            if (CountMaskBits(cell.RoadMask) >= 3)
            {
                DrawRoadJunction(canvasCenter, hexRadius, roadColor, roadOutline, CountMaskBits(cell.RoadMask));
            }
        }
    }

    private void DrawMaskedHexEdges(
        HexAxialCoordData coord,
        HexDirectionMask mask,
        Vector2[] hex,
        float thickness,
        Color fillColor,
        Color outlineColor)
    {
        foreach (var edge in WorldHexEdges)
        {
            if ((mask & edge.Mask) == HexDirectionMask.None || !OwnsWorldEdge(coord, edge))
            {
                continue;
            }

            DrawHexEdgeBand(hex[edge.StartIndex], hex[edge.EndIndex], Math.Max(thickness, 1.2f), fillColor, outlineColor);
        }
    }

    private bool OwnsWorldEdge(HexAxialCoordData coord, HexEdgeDefinition edge)
    {
        var neighborKey = (coord.Q + edge.NeighborQ, coord.R + edge.NeighborR);
        if (!_xianxiaWorldCenters.ContainsKey(neighborKey))
        {
            return true;
        }

        return coord.R < neighborKey.Item2 || (coord.R == neighborKey.Item2 && coord.Q < neighborKey.Item1);
    }

    private void DrawHexEdgeBand(Vector2 start, Vector2 end, float thickness, Color fillColor, Color outlineColor)
    {
        var tangent = end - start;
        if (tangent.LengthSquared() <= 0.0001f)
        {
            return;
        }

        tangent = tangent.Normalized();
        var normal = new Vector2(-tangent.Y, tangent.X);
        var halfThickness = thickness * 0.5f;
        var quad =
            new[]
            {
                start + (normal * halfThickness),
                end + (normal * halfThickness),
                end - (normal * halfThickness),
                start - (normal * halfThickness)
            };

        DrawFilledPolygon(quad, fillColor);
        DrawPath(quad, outlineColor, Math.Max(0.7f, thickness * 0.18f), true);
    }

    private void DrawWaterShorelines(XianxiaHexCellData cell, Vector2[] hex, float thickness, Color fillColor, Color outlineColor)
    {
        foreach (var edge in WorldHexEdges)
        {
            if (!OwnsWorldEdge(cell.Coord, edge) || !IsWaterBoundary(cell.Coord, edge))
            {
                continue;
            }

            DrawHexEdgeBand(hex[edge.StartIndex], hex[edge.EndIndex], Math.Max(thickness, 1.0f), fillColor, outlineColor);
        }
    }

    private bool IsWaterBoundary(HexAxialCoordData coord, HexEdgeDefinition edge)
    {
        if (!_xianxiaWorldCellLookup.TryGetValue((coord.Q, coord.R), out var cell) || cell.Water == XianxiaWaterType.None)
        {
            return false;
        }

        if (!_xianxiaWorldCellLookup.TryGetValue((coord.Q + edge.NeighborQ, coord.R + edge.NeighborR), out var neighbor))
        {
            return true;
        }

        return neighbor.Water == XianxiaWaterType.None;
    }

    private void DrawBridgeOverlays(HexAxialCoordData coord, HexDirectionMask overlapMask, Vector2[] hex, float scale)
    {
        var plankColor = new Color(0.73f, 0.61f, 0.44f, 0.96f);
        var plankOutline = new Color(0.38f, 0.27f, 0.16f, 0.94f);

        foreach (var edge in WorldHexEdges)
        {
            if ((overlapMask & edge.Mask) == HexDirectionMask.None || !OwnsWorldEdge(coord, edge))
            {
                continue;
            }

            DrawBridgeAtEdge(hex[edge.StartIndex], hex[edge.EndIndex], scale, plankColor, plankOutline);
        }
    }

    private void DrawBridgeAtEdge(Vector2 start, Vector2 end, float scale, Color fillColor, Color outlineColor)
    {
        var tangent = end - start;
        if (tangent.LengthSquared() <= 0.0001f)
        {
            return;
        }

        tangent = tangent.Normalized();
        var normal = new Vector2(-tangent.Y, tangent.X);
        var midpoint = (start + end) * 0.5f;
        var halfSpan = Math.Max(scale * 0.38f, 1.8f);
        var halfHeight = Math.Max(scale * 0.62f, 2.2f);

        for (var offsetIndex = -1; offsetIndex <= 1; offsetIndex++)
        {
            var offset = tangent * (offsetIndex * Math.Max(scale * 0.42f, 1.4f));
            var plankCenter = midpoint + offset;
            var quad =
                new[]
                {
                    plankCenter + (tangent * halfSpan) + (normal * halfHeight),
                    plankCenter - (tangent * halfSpan) + (normal * halfHeight),
                    plankCenter - (tangent * halfSpan) - (normal * halfHeight),
                    plankCenter + (tangent * halfSpan) - (normal * halfHeight)
                };
            DrawFilledPolygon(quad, fillColor);
            DrawPath(quad, outlineColor, Math.Max(0.7f, scale * 0.12f), true);
        }
    }

    private void DrawRoadJunction(Vector2 centerPoint, float radius, Color fillColor, Color outlineColor, int edgeCount)
    {
        var junctionRadius = Math.Max(radius * (edgeCount >= 4 ? 0.20f : 0.16f), 1.9f);
        var hex = BuildHexPolygon(centerPoint, junctionRadius);
        DrawFilledPolygon(hex, fillColor.Lightened(0.08f));
        DrawPath(hex, outlineColor, Math.Max(0.7f, junctionRadius * 0.18f), true);
    }

    private static int CountMaskBits(HexDirectionMask mask)
    {
        var bits = 0;
        foreach (var edge in WorldHexEdges)
        {
            if ((mask & edge.Mask) != HexDirectionMask.None)
            {
                bits++;
            }
        }

        return bits;
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

    private bool IsSelectedWorldSite(StrategicNodeDefinition node)
    {
        if (_selectedWorldSite == null)
        {
            return false;
        }

        if (!_xianxiaWorldCenters.TryGetValue((_selectedWorldSite.Coord.Q, _selectedWorldSite.Coord.R), out var normalizedCenter))
        {
            return false;
        }

        return Mathf.IsEqualApprox(node.X, normalizedCenter.X) &&
               Mathf.IsEqualApprox(node.Y, normalizedCenter.Y);
    }

    private static bool IsSameSite(XianxiaSiteData? left, XianxiaSiteData? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        return left.Coord.Q == right.Coord.Q &&
               left.Coord.R == right.Coord.R &&
               string.Equals(left.Label, right.Label, StringComparison.Ordinal);
    }

    private static float ResolveWorldSiteHitRadius(XianxiaSiteData site)
    {
        return site.PrimaryType switch
        {
            "ImmortalCity" => 5.1f,
            "Sect" => 4.8f,
            "CultivatorClan" => 4.5f,
            "MortalRealm" => 4.2f,
            "Market" => 4.0f,
            "Ruin" => 3.5f,
            _ => site.Role switch
            {
                XianxiaSiteRoleType.SectCandidate => 4.8f,
                XianxiaSiteRoleType.Settlement => 4.2f,
                XianxiaSiteRoleType.Ruin => 3.4f,
                _ => 3.4f
            }
        };
    }

    private static Vector2[] BuildHexPolygon(Vector2 center, float radius)
    {
        var halfWidth = Mathf.Sqrt(3f) * radius * 0.5f;
        var halfHeight = radius * 0.5f;

        return
        [
            center + new Vector2(0f, -radius),
            center + new Vector2(halfWidth, -halfHeight),
            center + new Vector2(halfWidth, halfHeight),
            center + new Vector2(0f, radius),
            center + new Vector2(-halfWidth, halfHeight),
            center + new Vector2(-halfWidth, -halfHeight)
        ];
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

    private readonly record struct HexEdgeDefinition(HexDirectionMask Mask, int StartIndex, int EndIndex, int NeighborQ, int NeighborR);
    private readonly record struct Layer1TileVariant(int SourceId, Vector2I AtlasCoords, int AlternativeTile);

    private void EnsureWorldTerrainLayerInfrastructure()
    {
        _worldTerrainTileLayer = GetNodeOrNull<TileMapLayer>("WorldTerrainTileLayer");
        if (_worldTerrainTileLayer == null)
        {
            return;
        }

        _worldTerrainTileLayer.ShowBehindParent = true;
        _worldTerrainTileLayer.ZIndex = -10;
        _worldTerrainTileLayer.Visible = false;
        _worldTerrainTileLayer.Material = LoadWorldTerrainClipMaterial();

        LoadWorldLayer1TileSet();
    }

    private void LoadWorldLayer1TileSet()
    {
        _worldLayer1TileSet = null;
        _worldLayer1SourceId = -1;

        if (!ResourceLoader.Exists(Layer1TileSetPath))
        {
            return;
        }

        var tileSet = ResourceLoader.Load<TileSet>(Layer1TileSetPath);
        if (tileSet == null)
        {
            return;
        }

        _worldLayer1TileSet = tileSet;
        _worldLayer1SourceId = tileSet.GetSourceCount() > 0 ? tileSet.GetSourceId(0) : -1;

        if (_worldTerrainTileLayer != null)
        {
            _worldTerrainTileLayer.TileSet = tileSet;
        }
    }

    private bool RebuildWorldTerrainTileLayer()
    {
        if (_xianxiaWorldMap == null || _worldTerrainTileLayer == null || _worldLayer1TileSet == null || _worldLayer1SourceId < 0)
        {
            ClearWorldTerrainTileLayer();
            return false;
        }

        _worldTerrainTileLayer.TileSet = _worldLayer1TileSet;
        _worldTerrainTileLayer.Clear();
        _xianxiaWorldTileCells = [];

        var tileSize = _worldLayer1TileSet.TileSize;
        var halfWidth = Math.Max(tileSize.X * 0.5f, 1f);
        var halfHeight = Math.Max(tileSize.Y * 0.5f, 1f);
        var rawCenters = new Dictionary<(int Q, int R), Vector2>(_xianxiaWorldMap.Cells.Count);
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var cell in _xianxiaWorldMap.Cells)
        {
            var key = (cell.Coord.Q, cell.Coord.R);
            var tileCell = ToWorldTileCell(cell.Coord);
            _xianxiaWorldTileCells[key] = tileCell;

            var variant = ResolveWorldLayer1TileVariant(cell);
            _worldTerrainTileLayer.SetCell(tileCell, variant.SourceId, variant.AtlasCoords, variant.AlternativeTile);

            var rawCenter = _worldTerrainTileLayer.MapToLocal(tileCell);
            rawCenters[key] = rawCenter;
            minX = MathF.Min(minX, rawCenter.X - halfWidth);
            maxX = MathF.Max(maxX, rawCenter.X + halfWidth);
            minY = MathF.Min(minY, rawCenter.Y - halfHeight);
            maxY = MathF.Max(maxY, rawCenter.Y + halfHeight);
        }

        if (rawCenters.Count == 0)
        {
            ClearWorldTerrainTileLayer();
            return false;
        }

        var worldCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        var spanX = Math.Max(maxX - minX, 0.01f);
        var spanY = Math.Max(maxY - minY, 0.01f);
        var scale = 1.82f / Math.Max(spanX, spanY);
        _xianxiaWorldTileCenterLocal = worldCenter;
        _xianxiaWorldTileLayoutScale = scale;
        _xianxiaWorldHexRadius = Math.Max(halfHeight * scale, 0.01f);

        foreach (var pair in rawCenters)
        {
            _xianxiaWorldCenters[pair.Key] = (pair.Value - worldCenter) * scale;
        }

        UpdateWorldTerrainLayerLayout();
        return true;
    }

    private void ClearWorldTerrainTileLayer()
    {
        _xianxiaWorldTileCells = [];
        _xianxiaWorldTileCenterLocal = Vector2.Zero;
        _xianxiaWorldTileLayoutScale = 1f;

        if (_worldTerrainTileLayer == null)
        {
            return;
        }

        _worldTerrainTileLayer.Clear();
        _worldTerrainTileLayer.Visible = false;
    }

    private void UpdateWorldTerrainLayerLayout()
    {
        if (_worldTerrainTileLayer == null)
        {
            return;
        }

        if (!IsWorldTerrainTileLayerReady())
        {
            _worldTerrainTileLayer.Visible = false;
            return;
        }

        var mapRect = GetMapRect();
        var unit = GetUnitForZoom(mapRect, _zoom);
        if (unit <= 0.01f)
        {
            _worldTerrainTileLayer.Visible = false;
            return;
        }

        var center = (new Rect2(mapRect.Position + _panOffset, mapRect.Size)).GetCenter();
        var scale = _xianxiaWorldTileLayoutScale * unit;
        _worldTerrainTileLayer.Visible = true;
        CallToneFx("apply_terrain_tint", _operationalStyle.TerrainTint);
        _worldTerrainTileLayer.Scale = Vector2.One * scale;
        _worldTerrainTileLayer.Position = center - (_xianxiaWorldTileCenterLocal * scale);
    }

    private bool IsWorldTerrainTileLayerReady()
    {
        return _mode == StrategicMapMode.World &&
               _worldTerrainTileLayer != null &&
               _worldLayer1TileSet != null &&
               _xianxiaWorldMap != null &&
               _xianxiaWorldTileCells.Count > 0;
    }

    private ShaderMaterial? LoadWorldTerrainClipMaterial()
    {
        if (_worldTerrainClipMaterial != null)
        {
            return _worldTerrainClipMaterial;
        }

        if (!ResourceLoader.Exists(WorldHexTileClipShaderPath))
        {
            return null;
        }

        var shader = ResourceLoader.Load<Shader>(WorldHexTileClipShaderPath);
        if (shader == null)
        {
            return null;
        }

        _worldTerrainClipMaterial = new ShaderMaterial
        {
            Shader = shader
        };
        return _worldTerrainClipMaterial;
    }

    private void DrawWorldCellSelectionOverlay(Vector2 center, float unit)
    {
        if (_selectedWorldSite == null ||
            !_xianxiaWorldCenters.TryGetValue((_selectedWorldSite.Coord.Q, _selectedWorldSite.Coord.R), out var normalizedCenter))
        {
            return;
        }

        var hexRadius = Math.Max(_xianxiaWorldHexRadius * unit, 2f);
        var canvasCenter = ToCanvas(center, unit, normalizedCenter.X, normalizedCenter.Y);
        var hex = BuildHexPolygon(canvasCenter, hexRadius);
        DrawFilledPolygon(hex, new Color(0.96f, 0.93f, 0.78f, 0.10f));
        DrawPath(hex, new Color(0.97f, 0.92f, 0.68f, 0.92f), Math.Max(1.3f, hexRadius * 0.12f), true);
    }

    private Layer1TileVariant ResolveWorldLayer1TileVariant(XianxiaHexCellData cell)
    {
        var coords = ResolveWorldTileCoords(cell);
        var variantIndex = ResolveVariantColumn(cell, 521) % coords.Length;
        return new Layer1TileVariant(_worldLayer1SourceId, coords[variantIndex], 0);
    }

    private static Vector2I[] ResolveWorldTileCoords(XianxiaHexCellData cell)
    {
        if (cell.Water != XianxiaWaterType.None)
        {
            return WorldWaterTileCoords;
        }

        if (IsSnowTerrain(cell.Terrain) || cell.Biome == XianxiaBiomeType.SnowPeaks)
        {
            return WorldSnowTileCoords;
        }

        if (IsMountainCell(cell) ||
            cell.CliffMask != HexDirectionMask.None ||
            cell.Biome is XianxiaBiomeType.DesertBadlands or XianxiaBiomeType.VolcanicWastes or XianxiaBiomeType.AncientRuinsLand ||
            cell.Corruption > 0.58f)
        {
            return WorldRuggedTileCoords;
        }

        if (cell.QiDensity > 0.66f ||
            cell.Biome is XianxiaBiomeType.BambooValley or XianxiaBiomeType.SacredForest or XianxiaBiomeType.JadeHighlands or XianxiaBiomeType.CrystalFields or XianxiaBiomeType.FloatingIsles ||
            cell.Terrain is XianxiaTerrainType.SpiritSoil or XianxiaTerrainType.CrystalGround or XianxiaTerrainType.CloudGround)
        {
            return WorldSpiritTileCoords;
        }

        return WorldPlainTileCoords;
    }

    private static Vector2I ToWorldTileCell(HexAxialCoordData coord)
    {
        var row = coord.R;
        var column = coord.Q + (row >> 1);
        return new Vector2I(column, row);
    }
}
