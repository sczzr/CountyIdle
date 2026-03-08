using System;
using System.Collections.Generic;
using System.Text.Json;
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

    private static readonly Color GroundColor = new(0.23f, 0.27f, 0.21f, 1.0f);
    private static readonly Color RoadColor = new(0.43f, 0.41f, 0.35f, 1.0f);
    private static readonly Color CourtyardColor = new(0.35f, 0.31f, 0.24f, 1.0f);
    private static readonly Color WaterColor = new(0.20f, 0.34f, 0.43f, 1.0f);
    private static readonly Color GridLineColor = new(0.10f, 0.10f, 0.12f, 0.45f);
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

    private const string TerrainAtlasTexturePath = "";
    private const string TerrainAtlasManifestPath = "";
    private const string TerrainGroundTexturePath = "";
    private const string TerrainRoadTexturePath = "";
    private const string TerrainCourtyardTexturePath = "";
    private const string TerrainWaterTexturePath = "";
    private const string WallBrightTexturePath = "";
    private const string WallDarkTexturePath = "";
    private const string RoofTexturePath = "";
    private const string GateTexturePath = "";

    private readonly TownMapGeneratorSystem _generator = new();
    private readonly Dictionary<TownTerrainType, Texture2D?> _terrainTextures = new();
    private readonly Dictionary<string, Rect2> _atlasRegions = new();

    private TownMapData? _mapData;
    private Button _regenerateButton = null!;
    private Label _mapHintLabel = null!;
    private Texture2D? _terrainAtlasTexture;
    private Texture2D? _wallBrightTexture;
    private Texture2D? _wallDarkTexture;
    private Texture2D? _roofTexture;
    private Texture2D? _gateTexture;
    private Vector2 _atlasTileSize = AtlasFallbackTileSize;
    private Vector2 _atlasAnchor = AtlasFallbackAnchor;
    private int _layoutSeed;
    private int _populationHint = 120;
    private int _housingHint = 180;
    private int _eliteHint = 8;
    private float _zoom = 1.0f;
    private bool _isInitialized;
    private MapViewStyle _operationalStyle = new();
    private TownActivityAnchorData? _selectedActivityAnchor;

    public float Zoom => _zoom;
    public float MinZoom => 0.6f;
    public float MaxZoom => 2.2f;
    public float DefaultZoom => 1.0f;

    public override void _Ready()
    {
        ClipContents = true;

        _regenerateButton = GetNode<Button>("RegenerateButton");
        _mapHintLabel = GetNode<Label>("MapHintLabel");

        _regenerateButton.Pressed += OnRegeneratePressed;
        LoadTextures();
        _layoutSeed = (int)(Time.GetUnixTimeFromSystem() % int.MaxValue);
        _isInitialized = true;

        RebuildMap();
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
        else if (mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (HandleAnchorSelection(mouseButton.Position))
            {
                GetViewport().SetInputAsHandled();
            }
        }
        else if (mouseButton.ButtonIndex == MouseButton.Right)
        {
            if (_selectedActivityAnchor != null)
            {
                _selectedActivityAnchor = null;
                UpdateMapHint();
                QueueRedraw();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            QueueRedraw();
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
        DrawResidents(_mapData, origin);
    }

    public void SetZoom(float zoom)
    {
        var clampedZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
        if (Mathf.IsEqualApprox(clampedZoom, _zoom))
        {
            return;
        }

        _zoom = clampedZoom;
        UpdateMapHint();
        QueueRedraw();
    }

    public void AdjustZoom(float delta)
    {
        SetZoom(_zoom + delta);
    }

    public void RefreshMap(int populationHint, int housingHint, int eliteHint)
    {
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
        if (!_isInitialized)
        {
            return;
        }

        _layoutSeed = (_layoutSeed * 1103515245 + 12345) & int.MaxValue;
        RebuildMap();
    }

    private void RebuildMap()
    {
        var selectedAnchorType = _selectedActivityAnchor?.AnchorType;
        var selectedAnchorLabel = _selectedActivityAnchor?.Label;
        _mapData = _generator.Generate(_populationHint, _housingHint, _eliteHint, _layoutSeed);
        _selectedActivityAnchor = TryRestoreSelectedAnchor(selectedAnchorType, selectedAnchorLabel);
        RebuildResidents();
        UpdateMapHint();
        QueueRedraw();
    }

    private void UpdateMapHint()
    {
        if (_mapData == null)
        {
            return;
        }

        var summaryLine =
            $"宗门地图（hex 俯瞰） · {_operationalStyle.TitleSuffix} · 建筑 {_mapData.Buildings.Count} · 场所 {_mapData.ActivityAnchors.Count} · 缩放 {(int)Mathf.Round(_zoom * 100f)}%";
        var interactionLine = _selectedActivityAnchor != null
            ? BuildSelectedAnchorHint(_selectedActivityAnchor)
            : SectMapSemanticRules.GetMapInteractionHint();
        var operationalLine = string.IsNullOrWhiteSpace(_operationalStyle.HintText)
            ? interactionLine
            : $"{_operationalStyle.HintText}\n{interactionLine}";

        _mapHintLabel.Text = $"{summaryLine}\n{operationalLine}";
        _mapHintLabel.Modulate = _operationalStyle.AccentColor;
    }

    private Vector2 CalculateMapOrigin(TownMapData mapData)
    {
        var radius = GetScaledHexRadius();
        var hexWidth = GetScaledHexWidth();
        var topPadding = ScaleValue(TopPadding);
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
            UpdateMapHint();
            QueueRedraw();
            return true;
        }

        if (_selectedActivityAnchor == null)
        {
            return false;
        }

        _selectedActivityAnchor = null;
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

    private Vector2 GetTownCellCenter(Vector2I cell, Vector2 origin)
    {
        var hexWidth = GetScaledHexWidth();
        var hexVerticalStep = GetScaledHexVerticalStep();
        var rowOffset = (cell.Y & 1) == 0 ? 0f : hexWidth * 0.5f;
        var centerX = origin.X + rowOffset + (cell.X * hexWidth);
        var centerY = origin.Y + (cell.Y * hexVerticalStep);
        return new Vector2(centerX, centerY);
    }

    private void DrawTerrain(TownMapData mapData, Vector2 origin)
    {
        if (_terrainAtlasTexture != null && _atlasRegions.Count > 0)
        {
            DrawAtlasTerrain(mapData, origin);
            return;
        }

        foreach (var cell in mapData.EnumerateAllCells())
        {
            var center = GetTownCellCenter(cell, origin);
            var tile = CreateHex(center, GetScaledHexRadius() * 0.98f);
            var terrainType = mapData.GetTerrain(cell.X, cell.Y);

            DrawTexturedPolygon(
                tile,
                _terrainTextures.TryGetValue(terrainType, out var texture) ? texture : null,
                GetTerrainColor(terrainType));
            DrawGrid(tile);
            DrawTerrainSemanticOverlay(mapData, cell, center, origin);
        }
    }

    private void DrawGrid(Vector2[] tile)
    {
        var lineWidth = Math.Max(0.6f, ScaleValue(0.8f));
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
            case TownTerrainType.Water:
                DrawWaterSemanticOverlay(mapData, cell, center);
                break;
        }
    }

    private void DrawRoadSemanticOverlay(TownMapData mapData, Vector2I cell, Vector2 center, Vector2 origin)
    {
        var roadHex = CreateHex(center, GetScaledHexRadius() * 0.44f);
        DrawColoredPolygon(roadHex, TintColor(RoadCoreColor, _operationalStyle.TerrainTint));
        DrawClosedPolyline(roadHex, TintColor(RoadEdgeColor, _operationalStyle.TerrainTint), Math.Max(0.8f, ScaleValue(0.9f)));

        var cardinalNeighbors = new[] { Vector2I.Up, Vector2I.Right, Vector2I.Down, Vector2I.Left };
        foreach (var neighborOffset in cardinalNeighbors)
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
            DrawRoadConnector(center, neighborCenter);
        }
    }

    private void DrawRoadConnector(Vector2 fromCenter, Vector2 toCenter)
    {
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

    private void DrawWaterSemanticOverlay(TownMapData mapData, Vector2I cell, Vector2 center)
    {
        var waterHex = CreateHex(center, GetScaledHexRadius() * 0.72f);
        DrawColoredPolygon(waterHex, TintColor(WaterCoreColor, _operationalStyle.TerrainTint));

        if (HasTerrainBoundary(mapData, cell, TownTerrainType.Water))
        {
            DrawClosedPolyline(waterHex, TintColor(WaterShoreColor, _operationalStyle.TerrainTint), Math.Max(0.9f, ScaleValue(1.0f)));
        }

        var rippleHex = CreateHex(center + new Vector2(0f, -ScaleValue(0.9f)), GetScaledHexRadius() * 0.34f);
        DrawOpenPolyline(rippleHex, TintColor(WaterRippleColor, _operationalStyle.TerrainTint), Math.Max(0.7f, ScaleValue(0.8f)));
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
        _atlasRegions.Clear();
        _terrainAtlasTexture = null;
        _terrainTextures[TownTerrainType.Ground] = null;
        _terrainTextures[TownTerrainType.Road] = null;
        _terrainTextures[TownTerrainType.Courtyard] = null;
        _terrainTextures[TownTerrainType.Water] = null;
        _wallBrightTexture = null;
        _wallDarkTexture = null;
        _roofTexture = null;
        _gateTexture = null;
        _atlasTileSize = AtlasFallbackTileSize;
        _atlasAnchor = AtlasFallbackAnchor;
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

    private void LoadAtlasManifest()
    {
        _atlasTileSize = AtlasFallbackTileSize;
        _atlasAnchor = AtlasFallbackAnchor;

        if (_terrainAtlasTexture == null || !FileAccess.FileExists(TerrainAtlasManifestPath))
        {
            return;
        }

        try
        {
            using var manifest = JsonDocument.Parse(FileAccess.GetFileAsString(TerrainAtlasManifestPath));
            var root = manifest.RootElement;

            if (root.TryGetProperty("tile_pixel_size", out var tilePixelSizeElement) &&
                tilePixelSizeElement.ValueKind == JsonValueKind.Array &&
                tilePixelSizeElement.GetArrayLength() >= 2)
            {
                _atlasTileSize = new Vector2(
                    tilePixelSizeElement[0].GetSingle(),
                    tilePixelSizeElement[1].GetSingle());
            }

            if (root.TryGetProperty("render_anchor", out var renderAnchorElement) &&
                renderAnchorElement.ValueKind == JsonValueKind.Array &&
                renderAnchorElement.GetArrayLength() >= 2)
            {
                _atlasAnchor = new Vector2(
                    renderAnchorElement[0].GetSingle(),
                    renderAnchorElement[1].GetSingle());
            }

            if (!root.TryGetProperty("tiles", out var tilesElement) || tilesElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var tileProperty in tilesElement.EnumerateObject())
            {
                if (!tileProperty.Value.TryGetProperty("pixel_region", out var regionElement) ||
                    regionElement.ValueKind != JsonValueKind.Array ||
                    regionElement.GetArrayLength() < 4)
                {
                    continue;
                }

                _atlasRegions[tileProperty.Name] = new Rect2(
                    regionElement[0].GetSingle(),
                    regionElement[1].GetSingle(),
                    regionElement[2].GetSingle(),
                    regionElement[3].GetSingle());
            }
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Failed to load county reference atlas manifest: {exception.Message}");
            _atlasRegions.Clear();
        }
    }

    private void DrawAtlasTerrain(TownMapData mapData, Vector2 origin)
    {
        if (_terrainAtlasTexture == null)
        {
            return;
        }

        var sortedCells = new List<Vector2I>(mapData.EnumerateAllCells());
        sortedCells.Sort(static (left, right) =>
        {
            var leftDepth = left.X + left.Y;
            var rightDepth = right.X + right.Y;
            var depthComparison = leftDepth.CompareTo(rightDepth);
            return depthComparison != 0 ? depthComparison : left.X.CompareTo(right.X);
        });

        var buildingCells = new HashSet<Vector2I>();
        foreach (var building in mapData.Buildings)
        {
            buildingCells.Add(building.Cell);
        }

        foreach (var cell in sortedCells)
        {
            var center = GetTownCellCenter(cell, origin);
            DrawAtlasTile(GetTerrainAtlasKey(mapData, cell), center);

            var overlayKey = GetOverlayAtlasKey(mapData, cell, buildingCells);
            if (!string.IsNullOrEmpty(overlayKey))
            {
                DrawAtlasTile(overlayKey, center);
            }
        }
    }

    private string GetTerrainAtlasKey(TownMapData mapData, Vector2I cell)
    {
        var terrainType = mapData.GetTerrain(cell.X, cell.Y);
        return terrainType switch
        {
            TownTerrainType.Road => $"road_{GetRoadMask(mapData, cell)}",
            TownTerrainType.Courtyard => $"courtyard_{GetCellHash(cell, 17) % 3}",
            TownTerrainType.Water => "water_0",
            _ => $"grass_{GetCellHash(cell, 31) % 4}"
        };
    }

    private string? GetOverlayAtlasKey(TownMapData mapData, Vector2I cell, HashSet<Vector2I> buildingCells)
    {
        if (buildingCells.Contains(cell))
        {
            return null;
        }

        var terrainType = mapData.GetTerrain(cell.X, cell.Y);
        var hash = GetCellHash(cell, 73);

        switch (terrainType)
        {
            case TownTerrainType.Ground:
            {
                var overlayRoll = hash % 20;
                if (IsNearTerrain(mapData, cell, TownTerrainType.Water, 1) && overlayRoll <= 2)
                {
                    return overlayRoll == 0 ? "prop_lilies" : "prop_reed";
                }

                if (IsNearTerrain(mapData, cell, TownTerrainType.Road, 1) && overlayRoll == 3)
                {
                    return "prop_stepping";
                }

                return overlayRoll switch
                {
                    0 => "prop_bush",
                    1 => "prop_flowers",
                    2 => "prop_rock_small",
                    4 => "prop_rock_large",
                    _ => null
                };
            }
            case TownTerrainType.Courtyard:
            {
                var overlayRoll = hash % 16;
                return overlayRoll switch
                {
                    0 => "accent_plaza",
                    1 => "accent_border",
                    2 => "prop_rock_small",
                    3 => "prop_stepping",
                    _ => null
                };
            }
            case TownTerrainType.Water:
                return hash % 5 == 0 ? "prop_lilies" : null;
            default:
                return null;
        }
    }

    private int GetRoadMask(TownMapData mapData, Vector2I cell)
    {
        var mask = 0;

        if (IsTerrain(mapData, cell + Vector2I.Up, TownTerrainType.Road))
        {
            mask |= 1;
        }

        if (IsTerrain(mapData, cell + Vector2I.Right, TownTerrainType.Road))
        {
            mask |= 2;
        }

        if (IsTerrain(mapData, cell + Vector2I.Down, TownTerrainType.Road))
        {
            mask |= 4;
        }

        if (IsTerrain(mapData, cell + Vector2I.Left, TownTerrainType.Road))
        {
            mask |= 8;
        }

        return mask;
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
        var cardinalNeighbors = new[] { Vector2I.Up, Vector2I.Right, Vector2I.Down, Vector2I.Left };
        foreach (var neighborOffset in cardinalNeighbors)
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

    private void DrawAtlasTile(string atlasKey, Vector2 center)
    {
        if (_terrainAtlasTexture == null || !_atlasRegions.TryGetValue(atlasKey, out var region))
        {
            return;
        }

        var destinationSize = new Vector2(ScaleValue(_atlasTileSize.X), ScaleValue(_atlasTileSize.Y));
        var scaledAnchor = new Vector2(ScaleValue(_atlasAnchor.X), ScaleValue(_atlasAnchor.Y));
        var destinationRect = new Rect2(center - scaledAnchor, destinationSize);
        DrawTextureRectRegion(_terrainAtlasTexture, destinationRect, region, Colors.White);
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

    private float GetScaledHexRadius()
    {
        return ScaleValue(HexRadius);
    }

    private float GetScaledHexWidth()
    {
        return GetScaledHexRadius() * (HexHalfWidthFactor * 2f);
    }

    private float GetScaledHexVerticalStep()
    {
        return GetScaledHexRadius() * HexVerticalStepFactor;
    }
}


