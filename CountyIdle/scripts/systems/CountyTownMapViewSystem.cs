using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem : PanelContainer, IMapZoomView
{
    private const float TileHalfWidth = 16f;
    private const float TileHalfHeight = 9f;
    private const float HexRadius = 18f;
    private const float HexHalfWidthFactor = 0.8660254f;
    private const float HexVerticalStepFactor = 1.5f;
    private const float TopPadding = 54f;
    private const float FootprintScale = 0.78f;
    private const float ZoomStep = 0.1f;
    private const float MaxZoomLevel = 10.0f;
    private const float ZoomVelocityDamping = 8.0f;
    private const float ZoomLerpSpeed = 10.0f;
    private const float TerrainDetailZoomThreshold = 1.18f;
    private const bool ShowLayer2TerrainOverlay = false;
    private static readonly bool DrawTerrainGridOutline = false;
    private const float PanSpeed = 520f;
    private const float TerrainHexFillScale = 1.01f;

    private static readonly Color GroundColor = new(0.23f, 0.27f, 0.21f, 1.0f);
    private static readonly Color RoadColor = new(0.43f, 0.41f, 0.35f, 1.0f);
    private static readonly Color CourtyardColor = new(0.35f, 0.31f, 0.24f, 1.0f);
    private static readonly Color WaterColor = new(0.20f, 0.34f, 0.43f, 1.0f);
    private static readonly Color GridLineColor = new(0.10f, 0.10f, 0.12f, 0.22f);
    private static readonly Color RoadCoreColor = new(0.61f, 0.57f, 0.48f, 0.96f);
    private static readonly Color RoadEdgeColor = new(0.78f, 0.72f, 0.60f, 0.82f);
    private static readonly Color WaterCoreColor = new(0.27f, 0.45f, 0.56f, 0.94f);
    private static readonly Color WaterShoreColor = new(0.67f, 0.83f, 0.92f, 0.82f);
    private static readonly Color WaterRippleColor = new(0.84f, 0.93f, 0.98f, 0.46f);
    private static readonly Color BuildingFootprintColor = new(0.26f, 0.24f, 0.21f, 0.92f);
    private static readonly Color BuildingFootprintEdgeColor = new(0.58f, 0.51f, 0.42f, 0.86f);

    private static readonly Color WallBrightColor = new(0.86f, 0.78f, 0.64f, 1.0f);
    private static readonly Color WallDarkColor = new(0.72f, 0.64f, 0.52f, 1.0f);
    private static readonly Color RoofMainColor = new(0.27f, 0.34f, 0.45f, 1.0f);
    private static readonly Color RoofShadeColor = new(0.20f, 0.26f, 0.34f, 1.0f);
    private static readonly Color RoofRidgeColor = new(0.76f, 0.60f, 0.27f, 1.0f);
    private static readonly Color GateColor = new(0.58f, 0.20f, 0.19f, 1.0f);
    private static readonly Vector2 AtlasFallbackTileSize = new(96f, 96f);
    private static readonly Vector2 AtlasFallbackAnchor = new(48f, 62f);
    private static readonly Vector2I[] Layer1TilemapAPlainCoords =
    [
        new Vector2I(0, 0),
        new Vector2I(1, 0),
        new Vector2I(2, 0),
        new Vector2I(3, 0)
    ];
    private static readonly Vector2I[] Layer1TilemapASpiritCoords =
    [
        new Vector2I(0, 1),
        new Vector2I(1, 1),
        new Vector2I(2, 1),
        new Vector2I(3, 1)
    ];
    private static readonly Vector2I[] Layer1TilemapBDeepWaterCoords =
    [
        new Vector2I(0, 0),
        new Vector2I(1, 0),
        new Vector2I(2, 0)
    ];
    private static readonly Vector2I[] Layer1TilemapBShallowWaterCoords =
    [
        new Vector2I(3, 0),
        new Vector2I(0, 1)
    ];
    private static readonly Vector2I[] Layer1TilemapBFoothillCoords =
    [
        new Vector2I(1, 1),
        new Vector2I(2, 1),
        new Vector2I(3, 1)
    ];
    private static readonly Vector2I[] Layer1TilemapCGroundCoords =
    [
        new Vector2I(0, 1),
        new Vector2I(2, 1),
        new Vector2I(3, 1)
    ];
    private static readonly Vector2I[] Layer1TilemapCRoadCoords =
    [
        new Vector2I(0, 0),
        new Vector2I(1, 1),
        new Vector2I(3, 1)
    ];
    private static readonly Vector2I[] Layer1TilemapCCourtyardCoords =
    [
        new Vector2I(1, 1),
        new Vector2I(2, 0),
        new Vector2I(2, 1)
    ];
    private static readonly Vector2I[] Layer1TilemapCWaterCoords =
    [
        new Vector2I(1, 0),
        new Vector2I(3, 0)
    ];

    private const string GeographyAtlasPath = "res://assets/ui/tilemap/tileset_geography.png";
    private const string Layer1TileSetPath = "res://assets/ui/tilemap/L1_hex_tileset.tres";

    private const string TerrainAtlasManifestPath = "res://assets/map/manifests/l1_terrain_manifest.json";
    private const string TerrainGroundTexturePath = "";
    private const string TerrainRoadTexturePath = "res://assets/map/connectors/roads/map_l2_road_core_v01.svg";
    private const string TerrainRoadLinkTexturePath = "res://assets/map/connectors/roads/map_l2_road_link_v01.svg";
    private const string TerrainCourtyardTexturePath = "res://assets/map/connectors/courtyards/map_l2_courtyard_core_v01.svg";
    private const string TerrainWaterTexturePath = "res://assets/map/connectors/rivers/map_l2_water_core_v01.svg";
    private const string TerrainWaterLinkTexturePath = "res://assets/map/connectors/rivers/map_l2_water_link_v01.svg";
    private const string WallBrightTexturePath = "";
    private const string WallDarkTexturePath = "";
    private const string RoofTexturePath = "";
    private const string GateTexturePath = "";

    private readonly TownMapGeneratorSystem _generator = new();
    private readonly Dictionary<TownTerrainType, Texture2D?> _terrainTextures = new();
    private readonly Dictionary<string, Texture2D> _terrainAtlasTextures = new(StringComparer.Ordinal);
    private readonly Dictionary<string, MapLayerAtlasDefinition> _terrainAtlasDefinitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, MapLayerAtlasTileDefinition> _terrainAtlasTiles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _terrainAtlasFamilyVariants = new(StringComparer.Ordinal);
    private readonly Dictionary<TownTerrainType, List<Layer1TileVariant>> _layer1TileVariants = new();
    private readonly Color[] _hexTintColors = new Color[6];

    private TownMapData? _mapData;
    private Button _regenerateButton = null!;
    private Label _mapHintLabel = null!;
    private Node2D? _hoverFx;
    private Node? _toneFx;
    private HexAtlas5x4? _geographyAtlas;
    private TileSet? _layer1TileSet;
    private Texture2D? _wallBrightTexture;
    private Texture2D? _wallDarkTexture;
    private Texture2D? _roofTexture;
    private Texture2D? _gateTexture;
    private Texture2D? _roadLinkTexture;
    private Texture2D? _waterLinkTexture;
    private int _layoutSeed;
    private int _populationHint = 120;
    private int _housingHint = 180;
    private int _eliteHint = 8;
    private float _zoom = 1.22f;
    private float _zoomTarget = 1.22f;
    private float _zoomVelocity;
    private Vector2 _panOffset = Vector2.Zero;
    private bool _isInitialized;
    private MapViewStyle _operationalStyle = new();
    private TownActivityAnchorData? _selectedActivityAnchor;
    private Vector2I? _selectedCell;
    private Vector2I? _hoveredCell;
    private int? _selectedResidentDiscipleId;
    private bool _usesExternalMap;
    private string _externalMapTitle = string.Empty;
    private string _externalMapInteractionHint = string.Empty;

    public float Zoom => _zoom;
    public float MinZoom => 0.6f;
    public float MaxZoom => MaxZoomLevel;
    public float DefaultZoom => 1.22f;

    public event Action<int, JobType?>? DiscipleInspectionRequested;
    public event Action<TownMapSelectionSummary>? SelectionSummaryChanged;

    public void SetExternalMap(TownMapData mapData, string titleText, string interactionHint)
    {
        _usesExternalMap = true;
        _externalMapTitle = string.IsNullOrWhiteSpace(titleText) ? "局部沙盘" : titleText;
        _externalMapInteractionHint = string.IsNullOrWhiteSpace(interactionHint) ? "左键点选局部地块，右键清除当前选中。" : interactionHint;
        _mapData = mapData;
        _selectedActivityAnchor = null;
        _selectedCell = null;
        _selectedResidentDiscipleId = null;
        _residentWalkers.Clear();
        ClearHoverState();

        if (_regenerateButton != null)
        {
            _regenerateButton.Visible = false;
        }

        if (_mapHintLabel != null)
        {
            _mapHintLabel.Visible = true;
        }

        UpdateMapHint();
        QueueRedraw();
    }

    public void ClearExternalMap()
    {
        _usesExternalMap = false;
        _externalMapTitle = string.Empty;
        _externalMapInteractionHint = string.Empty;

        if (_regenerateButton != null)
        {
            _regenerateButton.Visible = false;
        }

        if (_mapHintLabel != null)
        {
            _mapHintLabel.Visible = false;
        }

        _mapData = null;
        _selectedActivityAnchor = null;
        _selectedCell = null;
        _selectedResidentDiscipleId = null;
        _residentWalkers.Clear();
        ClearHoverState();
        NotifySelectionSummaryChanged();
        QueueRedraw();
    }

    public override void _Ready()
    {
        ClipContents = true;

        _regenerateButton = GetNode<Button>("RegenerateButton");
        _mapHintLabel = GetNode<Label>("MapHintLabel");
        _hoverFx = GetNodeOrNull<Node2D>("HoverFx");
        _toneFx = GetNodeOrNull<Node>("ToneFx");

        _regenerateButton.Pressed += OnRegeneratePressed;
        LoadTextures();
        _layoutSeed = (int)(Time.GetUnixTimeFromSystem() % int.MaxValue);
        _isInitialized = true;
        _zoomTarget = _zoom;

        RebuildMap();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            UpdateHoverState(mouseMotion.Position);
            return;
        }

        if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
        {
            return;
        }

        if (mouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            AdjustZoomAt(mouseButton.Position, ZoomStep);
            GetViewport().SetInputAsHandled();
        }
        else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            AdjustZoomAt(mouseButton.Position, -ZoomStep);
            GetViewport().SetInputAsHandled();
        }
        else if (mouseButton.ButtonIndex == MouseButton.Right)
        {
            if (_selectedActivityAnchor != null || _selectedCell != null || _selectedResidentDiscipleId.HasValue)
            {
                _selectedActivityAnchor = null;
                _selectedCell = null;
                _selectedResidentDiscipleId = null;
                UpdateMapHint();
                QueueRedraw();
                GetViewport().SetInputAsHandled();
            }
        }
        else if (mouseButton.ButtonIndex == MouseButton.Left && HandleAnchorSelection(mouseButton.Position))
        {
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            QueueRedraw();
            RefreshHoverVisual();
        }
        else if (what == NotificationMouseExit)
        {
            ClearHoverState();
        }
    }

    public override void _Draw()
    {
        if (_mapData == null)
        {
            return;
        }

        var origin = CalculateMapOrigin(_mapData);
        DrawTerrain(_mapData, origin);
        DrawStructures(_mapData, origin);
        DrawSelectedCellOverlay(_mapData, origin);
    }

    public void SetZoom(float zoom)
    {
        var clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        if (Mathf.IsEqualApprox(clampedZoom, _zoomTarget))
        {
            return;
        }

        _zoomTarget = clampedZoom;
        _zoomVelocity = 0f;
        _zoom = clampedZoom;
        UpdateMapHint();
        QueueRedraw();
    }

    public void AdjustZoom(float delta)
    {
        SetZoom(_zoomTarget + delta);
    }

    public void ResetView()
    {
        _panOffset = Vector2.Zero;
        SetZoomTarget(DefaultZoom, null, true);
    }

    private void AdjustZoomAt(Vector2 anchorPosition, float delta)
    {
        SetZoomTarget(_zoomTarget + delta, anchorPosition);
    }

    private void SetZoomTarget(float zoom, Vector2? anchorPosition = null, bool force = false)
    {
        var clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        if (!force && Mathf.IsEqualApprox(clampedZoom, _zoomTarget))
        {
            return;
        }

        if (_mapData != null && anchorPosition.HasValue)
        {
            var baseOrigin = CalculateBaseMapOrigin(_mapData, _zoom);
            var anchor = anchorPosition.Value;
            var mapSpace = (anchor - (baseOrigin + _panOffset)) / _zoom;
            var nextOrigin = CalculateBaseMapOrigin(_mapData, clampedZoom);
            _panOffset = anchor - nextOrigin - (mapSpace * clampedZoom);
        }

        _zoomTarget = clampedZoom;
        _zoomVelocity = 0f;
        _zoom = clampedZoom;
        UpdateMapHint();
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

    public void RefreshMap(int populationHint, int housingHint, int eliteHint)
    {
        if (_usesExternalMap)
        {
            return;
        }

        var safePopulation = Math.Max(populationHint, 0);
        var safeHousing = Math.Max(housingHint, 0);
        var safeElite = Math.Max(eliteHint, 0);

        if (_mapData != null &&
            safePopulation == _populationHint &&
            safeHousing == _housingHint &&
            safeElite == _eliteHint)
        {
            return;
        }

        _populationHint = safePopulation;
        _housingHint = safeHousing;
        _eliteHint = safeElite;

        if (!_isInitialized)
        {
            return;
        }

        RebuildMap();
    }

    public void RefreshOperationalState(MapViewStyle style)
    {
        _operationalStyle = style ?? new MapViewStyle();
        UpdateMapHint();
        QueueRedraw();
    }

    private void OnRegeneratePressed()
    {
        if (!_isInitialized || _usesExternalMap)
        {
            return;
        }

        _layoutSeed = (_layoutSeed * 1103515245 + 12345) & int.MaxValue;
        RebuildMap();
    }

    private void RebuildMap()
    {
        if (_usesExternalMap)
        {
            UpdateMapHint();
            QueueRedraw();
            return;
        }

        _mapData = _generator.Generate(_populationHint, _housingHint, _eliteHint, _layoutSeed);
        _selectedActivityAnchor = null;
        _selectedCell = null;
        _hoveredCell = null;
        _selectedResidentDiscipleId = null;
        _residentWalkers.Clear();
        ClearHoverState();
        UpdateMapHint();
        QueueRedraw();
    }

    private void UpdateMapHint()
    {
        if (_mapData == null)
        {
            if (_mapHintLabel != null)
            {
                _mapHintLabel.Text = string.Empty;
            }

            CallToneFx("reset_hint_tone");
            return;
        }

        var summaryLine = _usesExternalMap
            ? $"{_externalMapTitle} · 缩放 {(int)Mathf.Round(_zoom * 100f)}%"
            : $"浮云宗·天衍峰（hex 俯瞰） · {_operationalStyle.TitleSuffix} · 院域检视 · 缩放 {(int)Mathf.Round(_zoom * 100f)}%";
        var interactionLine = _usesExternalMap
            ? _externalMapInteractionHint
            : SectMapSemanticRules.GetMapInteractionHint();
        var operationalLine = string.IsNullOrWhiteSpace(_operationalStyle.HintText)
            ? interactionLine
            : $"{_operationalStyle.HintText}\n{interactionLine}";

        _mapHintLabel.Text = $"{summaryLine}\n{operationalLine}";
        CallToneFx("apply_hint_tone", _operationalStyle.AccentColor);
        NotifySelectionSummaryChanged();
    }

    private void CallToneFx(string methodName, params Variant[] args)
    {
        _toneFx?.Call(methodName, args);
    }

    private Vector2 CalculateMapOrigin(TownMapData mapData)
    {
        return CalculateBaseMapOrigin(mapData, _zoom) + _panOffset;
    }

    private Vector2 CalculateBaseMapOrigin(TownMapData mapData, float zoom)
    {
        var radius = GetScaledHexRadius(zoom);
        var hexWidth = GetScaledHexWidth(zoom);
        var topPadding = ScaleValue(TopPadding, zoom);
        var hasOddRow = mapData.Height > 1;
        var mapWidth = (mapData.Width * hexWidth) + (hasOddRow ? hexWidth * 0.5f : 0f);
        var mapHeight = (Math.Max(mapData.Height - 1, 0) * radius * HexVerticalStepFactor) + (radius * 2f);
        var remainingHeight = Math.Max(Size.Y - topPadding - mapHeight, 0f);
        var remainingWidth = Math.Max(Size.X - mapWidth, 0f);
        var offsetX = (remainingWidth * 0.5f) + (hexWidth * 0.5f);
        var offsetY = topPadding + (remainingHeight * 0.12f) + radius;

        return new Vector2(offsetX, offsetY);
    }

    private bool HandleAnchorSelection(Vector2 localPosition)
    {
        if (_mapData == null)
        {
            return false;
        }

        var origin = CalculateMapOrigin(_mapData);
        var selectedAnchor = PickActivityAnchorAt(localPosition, origin);
        if (selectedAnchor != null)
        {
            _selectedActivityAnchor = selectedAnchor;
            _selectedCell = selectedAnchor.LotCell;
            TryInspectAnchorResidents(selectedAnchor);
            UpdateMapHint();
            QueueRedraw();
            return true;
        }

        var selectedCell = PickCellAt(localPosition, origin);
        if (selectedCell.HasValue)
        {
            _selectedActivityAnchor = null;
            _selectedCell = selectedCell.Value;
            _selectedResidentDiscipleId = null;
            UpdateMapHint();
            QueueRedraw();
            return true;
        }

        if (_selectedActivityAnchor == null && _selectedCell == null)
        {
            return false;
        }

        _selectedActivityAnchor = null;
        _selectedCell = null;
        UpdateMapHint();
        QueueRedraw();
        return true;
    }

    private TownActivityAnchorData? TryRestoreSelectedAnchor(TownActivityAnchorType? anchorType, string? label)
    {
        if (_mapData == null || anchorType == null || string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        foreach (var anchor in _mapData.ActivityAnchors)
        {
            if (anchor.AnchorType == anchorType.Value &&
                string.Equals(anchor.Label, label, StringComparison.Ordinal))
            {
                return anchor;
            }
        }

        return null;
    }

    private void RequestDiscipleInspection(int discipleId, JobType? preferredJobType)
    {
        DiscipleInspectionRequested?.Invoke(discipleId, preferredJobType);
    }

    private Vector2 GetTownCellCenter(Vector2I cell, Vector2 origin)
    {
        return GetProjectedTownCellCenter(cell, origin);
    }

    private Vector2 GetProjectedTownCellCenter(Vector2I cell, Vector2 origin)
    {
        var hexWidth = GetScaledHexWidth();
        var hexVerticalStep = GetScaledHexVerticalStep();
        var rowOffset = (cell.Y & 1) == 0 ? 0f : hexWidth * 0.5f;
        var centerX = origin.X + rowOffset + (cell.X * hexWidth);
        var centerY = origin.Y + (cell.Y * hexVerticalStep);
        return new Vector2(centerX, centerY);
    }

    private Vector2I? PickCellAt(Vector2 localPosition, Vector2 origin)
    {
        if (_mapData == null)
        {
            return null;
        }

        foreach (var cell in _mapData.EnumerateAllCells())
        {
            var center = GetTownCellCenter(cell, origin);
            var tile = CreateHex(center, GetScaledHexRadius() * 0.98f);
            if (Geometry2D.IsPointInPolygon(localPosition, tile))
            {
                return cell;
            }
        }

        return null;
    }

    private void UpdateHoverState(Vector2 localPosition)
    {
        if (_mapData == null)
        {
            ClearHoverState();
            return;
        }

        var origin = CalculateMapOrigin(_mapData);
        if (PickActivityAnchorAt(localPosition, origin) != null)
        {
            ClearHoverState();
            return;
        }

        var hoveredCell = PickCellAt(localPosition, origin);
        if (hoveredCell == _hoveredCell)
        {
            return;
        }

        _hoveredCell = hoveredCell;
        RefreshHoverVisual();
    }

    private void RefreshHoverVisual()
    {
        if (_hoverFx == null || _mapData == null || _hoveredCell == null)
        {
            _hoverFx?.Call("hide_hover");
            return;
        }

        var origin = CalculateMapOrigin(_mapData);
        var center = GetTownCellCenter(_hoveredCell.Value, origin);
        var polygon = CreateHex(center, GetScaledHexRadius() * 0.99f);

        _hoverFx.Call("show_hover", polygon, "default");
    }

    private void ClearHoverState()
    {
        _hoveredCell = null;
        _hoverFx?.Call("hide_hover");
    }

    private bool ShouldDrawTerrainDetails()
    {
        return ShowLayer2TerrainOverlay && _zoom >= TerrainDetailZoomThreshold;
    }

    private void DrawTerrain(TownMapData mapData, Vector2 origin)
    {
        var showTerrainDetails = ShouldDrawTerrainDetails();
        foreach (var cell in mapData.EnumerateAllCells())
        {
            var center = GetTownCellCenter(cell, origin);
            var tile = CreateHex(center, GetScaledHexRadius() * TerrainHexFillScale);
            var terrainType = mapData.GetTerrain(cell.X, cell.Y);

            DrawTerrainBaseLayer(mapData, cell, center, tile, terrainType);

            if (DrawTerrainGridOutline)
            {
                DrawGrid(tile);
            }

            if (showTerrainDetails)
            {
                DrawTerrainSemanticOverlay(mapData, cell, center, origin);
            }
        }
    }

    private void DrawTerrainBaseLayer(
        TownMapData mapData,
        Vector2I cell,
        Vector2 center,
        Vector2[] tile,
        TownTerrainType terrainType)
    {
        if (TryDrawLayer1TileSetHex(cell, tile, terrainType))
        {
            return;
        }

        if (TryDrawLayer1TerrainAtlas(cell, tile, terrainType))
        {
            return;
        }

        if (_geographyAtlas != null)
        {
            var row = GetTownAtlasRow(terrainType);
            var col = GetCellHash(cell, 91) % HexAtlas5x4.Columns;
            DrawAtlasHex(tile, _geographyAtlas, row, col, GetTerrainColor(terrainType));
            return;
        }

        DrawTexturedPolygon(
            tile,
            _terrainTextures.TryGetValue(terrainType, out var texture) ? texture : null,
            GetTerrainColor(terrainType));
    }

    private bool TryDrawLayer1TerrainAtlas(Vector2I cell, Vector2[] tile, TownTerrainType terrainType)
    {
        if (_terrainAtlasTiles.Count == 0)
        {
            return false;
        }

        if (!TryGetTerrainAtlasKey(cell, terrainType, out var atlasKey))
        {
            return false;
        }

        return DrawAtlasTile(atlasKey, tile, GetTerrainAtlasTint(terrainType));
    }

    private bool TryDrawLayer1TileSetHex(Vector2I cell, Vector2[] tile, TownTerrainType terrainType)
    {
        if (_layer1TileSet == null || !TryGetLayer1TileVariant(cell, terrainType, out var variant))
        {
            return false;
        }

        if (_layer1TileSet.GetSource(variant.SourceId) is not TileSetAtlasSource atlasSource || atlasSource.Texture == null)
        {
            return false;
        }

        var textureRegion = atlasSource.GetTileTextureRegion(variant.AtlasCoords);
        for (var index = 0; index < _hexTintColors.Length; index++)
        {
            _hexTintColors[index] = TintColor(Colors.White, _operationalStyle.TerrainTint);
        }

        DrawPolygon(
            tile,
            _hexTintColors,
            CreateAtlasRegionUv(new Rect2(textureRegion.Position, textureRegion.Size), atlasSource.Texture),
            atlasSource.Texture);
        return true;
    }

    private bool TryGetTerrainAtlasKey(Vector2I cell, TownTerrainType terrainType, out string atlasKey)
    {
        atlasKey = string.Empty;
        if (_terrainAtlasTiles.Count == 0)
        {
            return false;
        }

        var candidates = terrainType switch
        {
            TownTerrainType.Water => BuildTerrainAtlasCandidates("shallow_water", "deep_water"),
            TownTerrainType.Road => BuildTerrainAtlasCandidates("plain"),
            TownTerrainType.Courtyard => BuildTerrainAtlasCandidates("plain", "spirit_vein"),
            _ => BuildTerrainAtlasCandidates("plain", "spirit_vein", "foothill")
        };

        if (candidates.Count == 0)
        {
            return false;
        }

        var variantIndex = GetCellHash(cell, 211 + ((int)terrainType * 17)) % candidates.Count;
        atlasKey = candidates[variantIndex];
        return true;
    }

    private Color GetTerrainAtlasTint(TownTerrainType terrainType)
    {
        var terrainTint = terrainType switch
        {
            TownTerrainType.Road => new Color(0.93f, 0.89f, 0.78f, 1.0f),
            TownTerrainType.Courtyard => new Color(0.96f, 0.93f, 0.84f, 1.0f),
            _ => Colors.White
        };

        return TintColor(terrainTint, _operationalStyle.TerrainTint);
    }

    private void DrawSelectedCellOverlay(TownMapData mapData, Vector2 origin)
    {
        if (_selectedCell == null || !mapData.IsInside(_selectedCell.Value))
        {
            return;
        }

        var center = GetTownCellCenter(_selectedCell.Value, origin);
        var outerHex = CreateHex(center, GetScaledHexRadius() * 1.08f);
        var innerHex = CreateHex(center, GetScaledHexRadius() * 0.98f);
        var contentKind = mapData.GetCellCompound(_selectedCell.Value)?.ContentKind;
        var haloColor = TownActivityAnchorVisualRules.GetSelectionHaloColor(contentKind);
        var glowColor = TownActivityAnchorVisualRules.GetSelectionGlowColor(contentKind);
        var outlineColor = TownActivityAnchorVisualRules.GetSelectionOutlineColor(contentKind);

        DrawColoredPolygon(outerHex, haloColor);
        DrawColoredPolygon(innerHex, glowColor);
        DrawClosedPolyline(innerHex, outlineColor, Math.Max(1.1f, ScaleValue(1.4f)));
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

    private void DrawAtlasTerrain(TownMapData mapData, Vector2 origin)
    {
        if (_geographyAtlas == null)
        {
            return;
        }

        var tint = _operationalStyle.TerrainTint;
        var showTerrainDetails = ShouldDrawTerrainDetails();
        foreach (var cell in mapData.EnumerateAllCells())
        {
            var center = GetTownCellCenter(cell, origin);
            var hex = CreateHex(center, GetScaledHexRadius() * 0.98f);
            var terrainType = mapData.GetTerrain(cell.X, cell.Y);
            var row = GetTownAtlasRow(terrainType);
            var col = GetCellHash(cell, 91) % HexAtlas5x4.Columns;
            DrawAtlasHex(hex, _geographyAtlas, row, col, tint);
            DrawGrid(hex);
            if (showTerrainDetails)
            {
                DrawTerrainSemanticOverlay(mapData, cell, center, origin);
            }
        }
    }

    private void DrawGrid(Vector2[] tile)
    {
        var lineWidth = Math.Max(0.5f, ScaleValue(0.7f));
        for (var index = 0; index < tile.Length; index++)
        {
            DrawLine(tile[index], tile[(index + 1) % tile.Length], GridLineColor, lineWidth);
        }
    }

    private void DrawTerrainSemanticOverlay(TownMapData mapData, Vector2I cell, Vector2 center, Vector2 origin)
    {
        var terrainType = mapData.GetTerrain(cell.X, cell.Y);
        switch (terrainType)
        {
            case TownTerrainType.Road:
                DrawRoadSemanticOverlay(mapData, cell, center, origin);
                break;
            case TownTerrainType.Courtyard:
                DrawCourtyardSemanticOverlay(center);
                break;
            case TownTerrainType.Water:
                DrawWaterSemanticOverlay(mapData, cell, center, origin);
                break;
        }
    }

    private void DrawRoadSemanticOverlay(TownMapData mapData, Vector2I cell, Vector2 center, Vector2 origin)
    {
        DrawTerrainDecal(TownTerrainType.Road, center, new Vector2(GetScaledHexWidth() * 0.84f, GetScaledHexRadius() * 0.86f), Colors.White);

        var roadHex = CreateHex(center, GetScaledHexRadius() * 0.42f);
        DrawColoredPolygon(roadHex, TintColor(RoadCoreColor, _operationalStyle.TerrainTint));
        DrawClosedPolyline(roadHex, TintColor(RoadEdgeColor, _operationalStyle.TerrainTint), Math.Max(0.8f, ScaleValue(0.9f)));

        foreach (var neighborOffset in GetHexNeighborOffsets(cell.Y))
        {
            var neighbor = cell + neighborOffset;
            if (!IsTerrain(mapData, neighbor, TownTerrainType.Road))
            {
                continue;
            }

            if (neighbor.Y < cell.Y || (neighbor.Y == cell.Y && neighbor.X <= cell.X))
            {
                continue;
            }

            var neighborCenter = GetTownCellCenter(neighbor, origin);
            DrawRoadConnectorTexture(center, neighborCenter);
        }
    }

    private void DrawRoadConnectorTexture(Vector2 fromCenter, Vector2 toCenter)
    {
        if (TryDrawConnectorTexture(
                _roadLinkTexture,
                fromCenter,
                toCenter,
                TintColor(new Color(0.96f, 0.92f, 0.84f, 0.94f), _operationalStyle.TerrainTint),
                GetScaledHexRadius() * 0.58f,
                GetScaledHexRadius() * 0.42f,
                GetScaledHexRadius() * 0.26f))
        {
            return;
        }

        var direction = toCenter - fromCenter;
        var length = direction.Length();
        if (length <= 0.01f)
        {
            return;
        }

        var normal = direction / length;
        var perpendicular = new Vector2(-normal.Y, normal.X);
        var inset = GetScaledHexRadius() * 0.24f;
        var halfWidth = GetScaledHexRadius() * 0.16f;
        var start = fromCenter + (normal * inset);
        var end = toCenter - (normal * inset);
        var connector = new[]
        {
            start + (perpendicular * halfWidth),
            end + (perpendicular * halfWidth),
            end - (perpendicular * halfWidth),
            start - (perpendicular * halfWidth)
        };

        DrawColoredPolygon(connector, TintColor(RoadCoreColor, _operationalStyle.TerrainTint));
        DrawOpenPolyline(connector, TintColor(RoadEdgeColor, _operationalStyle.TerrainTint), Math.Max(0.7f, ScaleValue(0.8f)));
    }

    private void DrawCourtyardSemanticOverlay(Vector2 center)
    {
        DrawTerrainDecal(TownTerrainType.Courtyard, center, new Vector2(GetScaledHexWidth() * 0.88f, GetScaledHexRadius() * 0.90f), Colors.White);

        var courtyardHex = CreateHex(center, GetScaledHexRadius() * 0.52f);
        DrawClosedPolyline(courtyardHex, TintColor(new Color(0.61f, 0.53f, 0.42f, 0.46f), _operationalStyle.TerrainTint), Math.Max(0.7f, ScaleValue(0.8f)));
    }

    private void DrawWaterSemanticOverlay(TownMapData mapData, Vector2I cell, Vector2 center, Vector2 origin)
    {
        DrawTerrainDecal(TownTerrainType.Water, center, new Vector2(GetScaledHexWidth() * 0.92f, GetScaledHexRadius() * 0.92f), Colors.White);

        var waterHex = CreateHex(center, GetScaledHexRadius() * 0.72f);
        DrawColoredPolygon(waterHex, TintColor(WaterCoreColor, _operationalStyle.TerrainTint));

        if (HasTerrainBoundary(mapData, cell, TownTerrainType.Water))
        {
            DrawClosedPolyline(waterHex, TintColor(WaterShoreColor, _operationalStyle.TerrainTint), Math.Max(0.9f, ScaleValue(1.0f)));
        }

        foreach (var neighborOffset in GetHexNeighborOffsets(cell.Y))
        {
            var neighbor = cell + neighborOffset;
            if (!IsTerrain(mapData, neighbor, TownTerrainType.Water))
            {
                continue;
            }

            if (neighbor.Y < cell.Y || (neighbor.Y == cell.Y && neighbor.X <= cell.X))
            {
                continue;
            }

            var neighborCenter = GetTownCellCenter(neighbor, origin);
            DrawWaterConnectorTexture(center, neighborCenter);
        }

        var rippleHex = CreateHex(center + new Vector2(0f, -ScaleValue(0.9f)), GetScaledHexRadius() * 0.34f);
        DrawOpenPolyline(rippleHex, TintColor(WaterRippleColor, _operationalStyle.TerrainTint), Math.Max(0.7f, ScaleValue(0.8f)));
    }

    private void DrawWaterConnectorTexture(Vector2 fromCenter, Vector2 toCenter)
    {
        if (TryDrawConnectorTexture(
                _waterLinkTexture,
                fromCenter,
                toCenter,
                TintColor(new Color(0.86f, 0.96f, 1.0f, 0.96f), _operationalStyle.TerrainTint),
                GetScaledHexRadius() * 0.62f,
                GetScaledHexRadius() * 0.48f,
                GetScaledHexRadius() * 0.30f))
        {
            return;
        }

        var direction = toCenter - fromCenter;
        var length = direction.Length();
        if (length <= 0.01f)
        {
            return;
        }

        var normal = direction / length;
        var perpendicular = new Vector2(-normal.Y, normal.X);
        var inset = GetScaledHexRadius() * 0.28f;
        var halfWidth = GetScaledHexRadius() * 0.18f;
        var start = fromCenter + (normal * inset);
        var end = toCenter - (normal * inset);
        var connector = new[]
        {
            start + (perpendicular * halfWidth),
            end + (perpendicular * halfWidth),
            end - (perpendicular * halfWidth),
            start - (perpendicular * halfWidth)
        };

        DrawColoredPolygon(connector, TintColor(WaterCoreColor, _operationalStyle.TerrainTint));
        DrawOpenPolyline(connector, TintColor(WaterShoreColor, _operationalStyle.TerrainTint), Math.Max(0.7f, ScaleValue(0.8f)));
    }

    private void DrawStructures(TownMapData mapData, Vector2 origin)
    {
        var sortedStructures = new List<(float DepthY, float DepthX, int Priority, TownBuildingData? Building, TownActivityAnchorData? Anchor)>();
        foreach (var building in mapData.Buildings)
        {
            var center = GetTownCellCenter(building.Cell, origin);
            sortedStructures.Add((center.Y, center.X, 0, building, null));
        }

        foreach (var anchor in mapData.ActivityAnchors)
        {
            var center = GetTownCellCenter(anchor.LotCell, origin);
            sortedStructures.Add((center.Y, center.X, 1, null, anchor));
        }

        sortedStructures.Sort((left, right) =>
        {
            var depthCompare = left.DepthY.CompareTo(right.DepthY);
            if (depthCompare != 0)
            {
                return depthCompare;
            }

            var xCompare = left.DepthX.CompareTo(right.DepthX);
            if (xCompare != 0)
            {
                return xCompare;
            }

            return left.Priority.CompareTo(right.Priority);
        });

        foreach (var structure in sortedStructures)
        {
            if (structure.Building != null)
            {
                DrawBuilding(structure.Building, origin);
                continue;
            }

            if (structure.Anchor != null)
            {
                DrawActivityAnchorBuilding(structure.Anchor, origin);
            }
        }
    }

    private void DrawBuilding(TownBuildingData building, Vector2 origin)
    {
        var center = GetTownCellCenter(building.Cell, origin);
        var footprintPlate = CreateHex(center + new Vector2(0f, ScaleValue(1.6f)), GetScaledHexRadius() * 0.72f);
        DrawColoredPolygon(footprintPlate, TintColor(BuildingFootprintColor, _operationalStyle.BuildingTint));
        DrawPolyline(footprintPlate, TintColor(BuildingFootprintEdgeColor, _operationalStyle.BuildingTint), Math.Max(0.9f, ScaleValue(1.0f)), true);

        var footprint = CreateDiamond(center, ScaleValue(TileHalfWidth * FootprintScale), ScaleValue(TileHalfHeight * FootprintScale));
        var baseTop = footprint[0];
        var baseRight = footprint[1];
        var baseBottom = footprint[2];
        var baseLeft = footprint[3];

        var wallHeight = ScaleValue(building.Floors == 1 ? 18f : 28f);
        var roofLift = ScaleValue(building.Floors == 1 ? 6f : 8f);
        var wallOffset = new Vector2(0f, -wallHeight);

        var roofTop = baseTop + wallOffset;
        var roofRight = baseRight + wallOffset;
        var roofBottom = baseBottom + wallOffset;
        var roofLeft = baseLeft + wallOffset;

        var shadow = CreateHex(center + new Vector2(ScaleValue(3f), ScaleValue(4f)), GetScaledHexRadius() * 0.52f);
        DrawColoredPolygon(shadow, new Color(0f, 0f, 0f, 0.18f));

        var leftWall = new[] { baseLeft, baseBottom, roofBottom, roofLeft };
        var rightWall = new[] { baseBottom, baseRight, roofRight, roofBottom };
        DrawTexturedPolygon(leftWall, _wallDarkTexture, TintColor(WallDarkColor, _operationalStyle.BuildingTint));
        DrawTexturedPolygon(rightWall, _wallBrightTexture, TintColor(WallBrightColor, _operationalStyle.BuildingTint));

        var eaveTop = roofTop + new Vector2(0f, -roofLift);
        var eaveRight = roofRight + new Vector2(ScaleValue(4f), ScaleValue(2f));
        var eaveBottom = roofBottom + new Vector2(0f, ScaleValue(3f));
        var eaveLeft = roofLeft + new Vector2(-ScaleValue(4f), ScaleValue(2f));

        var roofFace = new[] { eaveTop, eaveRight, eaveBottom, eaveLeft };
        DrawTexturedPolygon(roofFace, _roofTexture, TintColor(RoofMainColor, _operationalStyle.BuildingTint));

        var roofShade = new[] { roofTop, roofRight, eaveRight, eaveTop };
        DrawTexturedPolygon(roofShade, _roofTexture, TintColor(RoofShadeColor, _operationalStyle.BuildingTint));

        var ridgeStart = (eaveTop + eaveLeft) * 0.5f;
        var ridgeEnd = (eaveTop + eaveRight) * 0.5f;
        DrawLine(ridgeStart, ridgeEnd, TintColor(RoofRidgeColor, _operationalStyle.BuildingTint), Math.Max(1f, ScaleValue(1.8f)));

        var edgeWidth = Math.Max(0.8f, ScaleValue(1.0f));
        DrawLine(eaveTop, eaveRight, GridLineColor, edgeWidth);
        DrawLine(eaveRight, eaveBottom, GridLineColor, edgeWidth);
        DrawLine(eaveBottom, eaveLeft, GridLineColor, edgeWidth);
        DrawLine(eaveLeft, eaveTop, GridLineColor, edgeWidth);

        if (building.HasMoonGate)
        {
            DrawMoonGate(baseBottom, building.Facing);
        }
    }

    private void DrawMoonGate(Vector2 baseBottom, TownFacing facing)
    {
        var offset = facing switch
        {
            TownFacing.North => new Vector2(-ScaleValue(4f), -ScaleValue(2f)),
            TownFacing.South => new Vector2(ScaleValue(4f), -ScaleValue(2f)),
            TownFacing.East => new Vector2(ScaleValue(6f), -ScaleValue(1f)),
            _ => new Vector2(-ScaleValue(6f), -ScaleValue(1f))
        };

        var gateCenter = baseBottom + offset;
        if (_gateTexture != null)
        {
            var gateSize = new Vector2(ScaleValue(10f), ScaleValue(10f));
            var gateRect = new Rect2(gateCenter - (gateSize * 0.5f), gateSize);
            DrawTextureRect(_gateTexture, gateRect, false, TintColor(GateColor, _operationalStyle.BuildingTint));
        }

        DrawCircle(gateCenter, Math.Max(1.2f, ScaleValue(2.6f)), TintColor(GateColor, _operationalStyle.BuildingTint));
        DrawArc(gateCenter, Math.Max(1.4f, ScaleValue(2.8f)), 0f, Mathf.Tau, 14, TintColor(RoofRidgeColor, _operationalStyle.BuildingTint), Math.Max(0.6f, ScaleValue(0.9f)));
    }

    private void LoadTextures()
    {
        _terrainTextures.Clear();
        _terrainAtlasTextures.Clear();
        _terrainAtlasTiles.Clear();
        _terrainAtlasFamilyVariants.Clear();
        _layer1TileVariants.Clear();
        _geographyAtlas = HexAtlas5x4.TryLoad(GeographyAtlasPath);
        LoadAtlasManifest();
        LoadLayer1TileSet();
        _terrainTextures[TownTerrainType.Ground] = null;
        _terrainTextures[TownTerrainType.Road] = LoadTextureOrNull(TerrainRoadTexturePath);
        _terrainTextures[TownTerrainType.Courtyard] = LoadTextureOrNull(TerrainCourtyardTexturePath);
        _terrainTextures[TownTerrainType.Water] = LoadTextureOrNull(TerrainWaterTexturePath);
        _roadLinkTexture = LoadTextureOrNull(TerrainRoadLinkTexturePath);
        _waterLinkTexture = LoadTextureOrNull(TerrainWaterLinkTexturePath);
        _wallBrightTexture = null;
        _wallDarkTexture = null;
        _roofTexture = null;
        _gateTexture = null;
    }

    private void DrawTexturedPolygon(Vector2[] polygon, Texture2D? texture, Color fallbackColor)
    {
        if (texture == null)
        {
            DrawColoredPolygon(polygon, fallbackColor);
            return;
        }

        var uvs = CreateTextureUv(texture);
        var vertexColors = new[] { fallbackColor, fallbackColor, fallbackColor, fallbackColor };
        DrawPolygon(polygon, vertexColors, uvs, texture);
    }

    private static Vector2[] CreateTextureUv(Texture2D texture)
    {
        var width = texture.GetWidth();
        var height = texture.GetHeight();
        return
        [
            new Vector2(0f, 0f),
            new Vector2(width, 0f),
            new Vector2(width, height),
            new Vector2(0f, height)
        ];
    }

    private void DrawAtlasHex(Vector2[] hex, HexAtlas5x4 atlas, int row, int col, Color tint)
    {
        for (var index = 0; index < _hexTintColors.Length; index++)
        {
            _hexTintColors[index] = tint;
        }

        DrawPolygon(hex, _hexTintColors, atlas.GetUv(col, row), atlas.Texture);
    }

    private static Texture2D? LoadTextureOrNull(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        return ResourceLoader.Load<Texture2D>(path);
    }

    private static Vector2[] CreateDiamond(Vector2 center, float halfWidth, float halfHeight)
    {
        return
        [
            new Vector2(center.X, center.Y - halfHeight),
            new Vector2(center.X + halfWidth, center.Y),
            new Vector2(center.X, center.Y + halfHeight),
            new Vector2(center.X - halfWidth, center.Y)
        ];
    }

    private static Vector2[] CreateHex(Vector2 center, float radius)
    {
        var halfWidth = radius * HexHalfWidthFactor;
        var halfHeight = radius * 0.5f;
        return
        [
            new Vector2(center.X, center.Y - radius),
            new Vector2(center.X + halfWidth, center.Y - halfHeight),
            new Vector2(center.X + halfWidth, center.Y + halfHeight),
            new Vector2(center.X, center.Y + radius),
            new Vector2(center.X - halfWidth, center.Y + halfHeight),
            new Vector2(center.X - halfWidth, center.Y - halfHeight)
        ];
    }

    private static int GetTownAtlasRow(TownTerrainType terrainType)
    {
        return terrainType switch
        {
            TownTerrainType.Road => 1,
            TownTerrainType.Courtyard => 2,
            TownTerrainType.Water => 3,
            _ => 0
        };
    }


    private void DrawClosedPolyline(Vector2[] polygon, Color color, float width)
    {
        for (var index = 0; index < polygon.Length; index++)
        {
            DrawLine(polygon[index], polygon[(index + 1) % polygon.Length], color, width);
        }
    }

    private void DrawOpenPolyline(Vector2[] polygon, Color color, float width)
    {
        for (var index = 0; index < polygon.Length - 1; index++)
        {
            DrawLine(polygon[index], polygon[index + 1], color, width);
        }
    }

    private List<string> BuildTerrainAtlasCandidates(params string[] families)
    {
        var candidates = new List<string>();
        foreach (var family in families)
        {
            if (_terrainAtlasFamilyVariants.TryGetValue(family, out var variants))
            {
                candidates.AddRange(variants);
            }
        }

        return candidates;
    }

    private void LoadAtlasManifest()
    {
        var manifest = MapLayerAtlasManifest.TryLoad(TerrainAtlasManifestPath);
        if (manifest == null)
        {
            return;
        }

        foreach (var atlasEntry in manifest.Atlases.Values)
        {
            var atlasTexture = LoadTextureOrNull(atlasEntry.SourceImage);
            if (atlasTexture == null)
            {
                GD.PushWarning($"Missing terrain atlas texture: {atlasEntry.SourceImage}");
                continue;
            }

            _terrainAtlasDefinitions[atlasEntry.AtlasName] = atlasEntry;
            _terrainAtlasTextures[atlasEntry.AtlasName] = atlasTexture;
        }

        foreach (var tileEntry in manifest.Tiles.Values)
        {
            if (!_terrainAtlasTextures.ContainsKey(tileEntry.AtlasName))
            {
                continue;
            }

            _terrainAtlasTiles[tileEntry.AssetId] = tileEntry;
            if (!_terrainAtlasFamilyVariants.TryGetValue(tileEntry.Family, out var variants))
            {
                variants = new List<string>();
                _terrainAtlasFamilyVariants[tileEntry.Family] = variants;
            }

            variants.Add(tileEntry.AssetId);
        }
    }

    private void LoadLayer1TileSet()
    {
        _layer1TileSet = null;

        if (!ResourceLoader.Exists(Layer1TileSetPath))
        {
            return;
        }

        var tileSet = ResourceLoader.Load<TileSet>(Layer1TileSetPath);
        if (tileSet == null)
        {
            return;
        }

        _layer1TileSet = tileSet;
        BuildLayer1TileVariantLookup(tileSet);
    }

    private void BuildLayer1TileVariantLookup(TileSet tileSet)
    {
        for (var index = 0; index < tileSet.GetSourceCount(); index++)
        {
            var sourceId = tileSet.GetSourceId(index);
            if (tileSet.GetSource(sourceId) is not TileSetAtlasSource atlasSource)
            {
                continue;
            }

            var resourcePath = atlasSource.Texture?.ResourcePath ?? string.Empty;
            var normalizedPath = resourcePath.Replace('\\', '/');
            if (normalizedPath.EndsWith("/L1_tilemap_a.png", StringComparison.OrdinalIgnoreCase))
            {
                AddLayer1TileVariants(TownTerrainType.Ground, sourceId, Layer1TilemapAPlainCoords);
                AddLayer1TileVariants(TownTerrainType.Ground, sourceId, Layer1TilemapASpiritCoords);
                AddLayer1TileVariants(TownTerrainType.Road, sourceId, Layer1TilemapAPlainCoords);
                AddLayer1TileVariants(TownTerrainType.Courtyard, sourceId, Layer1TilemapASpiritCoords);
                AddLayer1TileVariants(TownTerrainType.Courtyard, sourceId, Layer1TilemapAPlainCoords);
                continue;
            }

            if (normalizedPath.EndsWith("/L1_tilemap_b.png", StringComparison.OrdinalIgnoreCase))
            {
                AddLayer1TileVariants(TownTerrainType.Ground, sourceId, Layer1TilemapBFoothillCoords);
                AddLayer1TileVariants(TownTerrainType.Water, sourceId, Layer1TilemapBDeepWaterCoords);
                AddLayer1TileVariants(TownTerrainType.Water, sourceId, Layer1TilemapBShallowWaterCoords);
                continue;
            }

            if (normalizedPath.EndsWith("/L1_tilemap_c.png", StringComparison.OrdinalIgnoreCase))
            {
                AddLayer1TileVariants(TownTerrainType.Ground, sourceId, Layer1TilemapCGroundCoords);
                AddLayer1TileVariants(TownTerrainType.Road, sourceId, Layer1TilemapCRoadCoords);
                AddLayer1TileVariants(TownTerrainType.Courtyard, sourceId, Layer1TilemapCCourtyardCoords);
                AddLayer1TileVariants(TownTerrainType.Water, sourceId, Layer1TilemapCWaterCoords);
            }
        }
    }

    private void AddLayer1TileVariants(TownTerrainType terrainType, int sourceId, Vector2I[] atlasCoords)
    {
        if (!_layer1TileVariants.TryGetValue(terrainType, out var variants))
        {
            variants = new List<Layer1TileVariant>();
            _layer1TileVariants[terrainType] = variants;
        }

        foreach (var atlasCoord in atlasCoords)
        {
            variants.Add(new Layer1TileVariant(sourceId, atlasCoord, 0));
        }
    }

    private bool TryGetLayer1TileVariant(Vector2I cell, TownTerrainType terrainType, out Layer1TileVariant variant)
    {
        variant = default;
        if (!_layer1TileVariants.TryGetValue(terrainType, out var variants) || variants.Count == 0)
        {
            return false;
        }

        variant = variants[GetCellHash(cell, 509 + ((int)terrainType * 97)) % variants.Count];
        return true;
    }
    private void DrawTerrainDecal(TownTerrainType terrainType, Vector2 center, Vector2 size, Color tint)
    {
        if (!_terrainTextures.TryGetValue(terrainType, out var texture) || texture == null)
        {
            return;
        }

        var rect = new Rect2(center - (size * 0.5f), size);
        DrawTextureRect(texture, rect, false, TintColor(tint, _operationalStyle.TerrainTint));
    }

    private static Vector2I[] GetHexNeighborOffsets(int row)
    {
        var isOddRow = (row & 1) == 1;
        return isOddRow
            ?
            [
                new Vector2I(0, -1),
                new Vector2I(1, -1),
                Vector2I.Right,
                new Vector2I(1, 1),
                new Vector2I(0, 1),
                Vector2I.Left
            ]
            :
            [
                new Vector2I(-1, -1),
                new Vector2I(0, -1),
                Vector2I.Right,
                new Vector2I(0, 1),
                new Vector2I(-1, 1),
                Vector2I.Left
            ];
    }

    private bool TryDrawConnectorTexture(
        Texture2D? texture,
        Vector2 fromCenter,
        Vector2 toCenter,
        Color tint,
        float thickness,
        float minLength,
        float inset)
    {
        if (texture == null)
        {
            return false;
        }

        var direction = toCenter - fromCenter;
        var length = direction.Length();
        if (length <= 0.01f)
        {
            return false;
        }

        var usableLength = Math.Max(minLength, length - (inset * 2f));
        var midpoint = (fromCenter + toCenter) * 0.5f;
        var angle = direction.Angle();
        var rect = new Rect2(-usableLength * 0.5f, -thickness * 0.5f, usableLength, thickness);

        DrawSetTransform(midpoint, angle, Vector2.One);
        DrawTextureRect(texture, rect, false, tint);
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
        return true;
    }

    private static bool IsTerrain(TownMapData mapData, Vector2I cell, TownTerrainType terrainType)
    {
        return mapData.IsInside(cell) && mapData.GetTerrain(cell.X, cell.Y) == terrainType;
    }

    private static bool IsNearTerrain(TownMapData mapData, Vector2I cell, TownTerrainType terrainType, int radius)
    {
        for (var offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (var offsetY = -radius; offsetY <= radius; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                var target = cell + new Vector2I(offsetX, offsetY);
                if (!mapData.IsInside(target))
                {
                    continue;
                }

                if (mapData.GetTerrain(target.X, target.Y) == terrainType)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasTerrainBoundary(TownMapData mapData, Vector2I cell, TownTerrainType terrainType)
    {
        foreach (var neighborOffset in GetHexNeighborOffsets(cell.Y))
        {
            var neighbor = cell + neighborOffset;
            if (!mapData.IsInside(neighbor) || mapData.GetTerrain(neighbor.X, neighbor.Y) != terrainType)
            {
                return true;
            }
        }

        return false;
    }

    private int GetCellHash(Vector2I cell, int salt)
    {
        unchecked
        {
            var hash = (cell.X * 73856093) ^
                       (cell.Y * 19349663) ^
                       ((_layoutSeed + salt) * 83492791);
            return hash & int.MaxValue;
        }
    }

    private bool DrawAtlasTile(string atlasKey, Vector2[] tile, Color tint)
    {
        if (!_terrainAtlasTiles.TryGetValue(atlasKey, out var tileDefinition) ||
            !_terrainAtlasTextures.TryGetValue(tileDefinition.AtlasName, out var atlasTexture))
        {
            return false;
        }

        for (var index = 0; index < _hexTintColors.Length; index++)
        {
            _hexTintColors[index] = tint;
        }

        DrawPolygon(tile, _hexTintColors, CreateAtlasRegionUv(tileDefinition.PixelRegion, atlasTexture), atlasTexture);
        return true;
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

    private Color GetTerrainColor(TownTerrainType terrainType)
    {
        var baseColor = terrainType switch
        {
            TownTerrainType.Road => RoadColor,
            TownTerrainType.Courtyard => CourtyardColor,
            TownTerrainType.Water => WaterColor,
            _ => GroundColor
        };
        return TintColor(baseColor, _operationalStyle.TerrainTint);
    }

    private static Color TintColor(Color baseColor, Color tint)
    {
        return new Color(
            baseColor.R * tint.R,
            baseColor.G * tint.G,
            baseColor.B * tint.B,
            baseColor.A);
    }

    private float ScaleValue(float baseValue)
    {
        return baseValue * _zoom;
    }

    private static float ScaleValue(float baseValue, float zoom)
    {
        return baseValue * zoom;
    }

    private float GetScaledHexRadius()
    {
        return GetScaledHexRadius(_zoom);
    }

    private static float GetScaledHexRadius(float zoom)
    {
        return HexRadius * zoom;
    }

    private float GetScaledHexWidth()
    {
        return GetScaledHexWidth(_zoom);
    }

    private static float GetScaledHexWidth(float zoom)
    {
        return GetScaledHexRadius(zoom) * (HexHalfWidthFactor * 2f);
    }

    private float GetScaledHexVerticalStep()
    {
        return GetScaledHexVerticalStep(_zoom);
    }

    private static float GetScaledHexVerticalStep(float zoom)
    {
        return GetScaledHexRadius(zoom) * HexVerticalStepFactor;
    }
}

internal readonly record struct Layer1TileVariant(int SourceId, Vector2I AtlasCoords, int AlternativeTile);

