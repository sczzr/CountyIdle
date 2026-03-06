using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem : PanelContainer, IMapZoomView
{
    private const float TileHalfWidth = 22f;
    private const float TileHalfHeight = 11f;
    private const float TopPadding = 54f;
    private const float FootprintScale = 0.78f;
    private const float ZoomStep = 0.1f;

    private static readonly Color GroundColor = new(0.23f, 0.27f, 0.21f, 1.0f);
    private static readonly Color RoadColor = new(0.43f, 0.41f, 0.35f, 1.0f);
    private static readonly Color CourtyardColor = new(0.35f, 0.31f, 0.24f, 1.0f);
    private static readonly Color WaterColor = new(0.20f, 0.34f, 0.43f, 1.0f);
    private static readonly Color GridLineColor = new(0.10f, 0.10f, 0.12f, 0.45f);

    private static readonly Color WallBrightColor = new(0.86f, 0.78f, 0.64f, 1.0f);
    private static readonly Color WallDarkColor = new(0.72f, 0.64f, 0.52f, 1.0f);
    private static readonly Color RoofMainColor = new(0.27f, 0.34f, 0.45f, 1.0f);
    private static readonly Color RoofShadeColor = new(0.20f, 0.26f, 0.34f, 1.0f);
    private static readonly Color RoofRidgeColor = new(0.76f, 0.60f, 0.27f, 1.0f);
    private static readonly Color GateColor = new(0.58f, 0.20f, 0.19f, 1.0f);
    private static readonly Vector2 AtlasFallbackTileSize = new(96f, 96f);
    private static readonly Vector2 AtlasFallbackAnchor = new(48f, 62f);

    private const string TerrainAtlasTexturePath = "res://assets/tiles/county_reference_isometric/county_reference_isometric_atlas.png";
    private const string TerrainAtlasManifestPath = "res://assets/tiles/county_reference_isometric/county_reference_isometric_manifest.json";
    private const string TerrainGroundTexturePath = "res://assets/tiles/chinese_style_seamless/env_grass_meadow_a_seamless.png";
    private const string TerrainRoadTexturePath = "res://assets/tiles/chinese_style_seamless/env_stone_path_moss_a_seamless.png";
    private const string TerrainCourtyardTexturePath = "res://assets/tiles/chinese_style_seamless/env_soil_courtyard_a_seamless.png";
    private const string TerrainWaterTexturePath = "res://assets/tiles/chinese_style_seamless/env_water_pond_lotus_a_seamless.png";
    private const string WallBrightTexturePath = "res://assets/tiles/chinese_style_seamless_modular/house_wall_plaster_white_seamless.png";
    private const string WallDarkTexturePath = "res://assets/tiles/chinese_style_seamless_modular/house_wall_lattice_wood_seamless.png";
    private const string RoofTexturePath = "res://assets/tiles/chinese_style_seamless/house_roof_tile_bluegray_a_seamless.png";
    private const string GateTexturePath = "res://assets/tiles/building_elements_basic/moon_gate_a.png";

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
            $"县城地图（等距 Tilemap） · {_operationalStyle.TitleSuffix} · 房屋 {_mapData.Buildings.Count} · 场所 {_mapData.ActivityAnchors.Count} · 缩放 {(int)Mathf.Round(_zoom * 100f)}%";
        var interactionLine = _selectedActivityAnchor != null
            ? BuildSelectedAnchorHint(_selectedActivityAnchor)
            : "左键选中场所查看状态 · 右键取消选中";
        var operationalLine = string.IsNullOrWhiteSpace(_operationalStyle.HintText)
            ? interactionLine
            : $"{_operationalStyle.HintText}\n{interactionLine}";

        _mapHintLabel.Text = $"{summaryLine}\n{operationalLine}";
        _mapHintLabel.Modulate = _operationalStyle.AccentColor;
    }

    private Vector2 CalculateMapOrigin(TownMapData mapData)
    {
        var tileHalfWidth = ScaleValue(TileHalfWidth);
        var tileHalfHeight = ScaleValue(TileHalfHeight);
        var topPadding = ScaleValue(TopPadding);

        var minX = -(mapData.Height - 1) * tileHalfWidth;
        var maxX = (mapData.Width - 1) * tileHalfWidth;
        var mapCenterX = (minX + maxX) * 0.5f;

        var isoHeight = (mapData.Width + mapData.Height - 2) * tileHalfHeight;
        var remainingHeight = Math.Max(Size.Y - topPadding - isoHeight, 0f);
        var offsetY = topPadding + (remainingHeight * 0.15f);

        return new Vector2((Size.X * 0.5f) - mapCenterX, offsetY);
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

    private Vector2 GetIsoCellCenter(Vector2I cell, Vector2 origin)
    {
        var isoX = (cell.X - cell.Y) * ScaleValue(TileHalfWidth);
        var isoY = (cell.X + cell.Y) * ScaleValue(TileHalfHeight);
        return origin + new Vector2(isoX, isoY);
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
            var center = GetIsoCellCenter(cell, origin);
            var tile = CreateDiamond(center, ScaleValue(TileHalfWidth), ScaleValue(TileHalfHeight));
            var terrainType = mapData.GetTerrain(cell.X, cell.Y);

            DrawTexturedPolygon(
                tile,
                _terrainTextures.TryGetValue(terrainType, out var texture) ? texture : null,
                GetTerrainColor(terrainType));
            DrawGrid(tile);
        }
    }

    private void DrawGrid(Vector2[] tile)
    {
        var lineWidth = Math.Max(0.6f, ScaleValue(0.8f));
        DrawLine(tile[0], tile[1], GridLineColor, lineWidth);
        DrawLine(tile[1], tile[2], GridLineColor, lineWidth);
        DrawLine(tile[2], tile[3], GridLineColor, lineWidth);
        DrawLine(tile[3], tile[0], GridLineColor, lineWidth);
    }

    private void DrawStructures(TownMapData mapData, Vector2 origin)
    {
        var sortedStructures = new List<(int Depth, int Priority, TownBuildingData? Building, TownActivityAnchorData? Anchor)>();
        foreach (var building in mapData.Buildings)
        {
            sortedStructures.Add((building.Cell.X + building.Cell.Y, 0, building, null));
        }

        foreach (var anchor in mapData.ActivityAnchors)
        {
            sortedStructures.Add((anchor.LotCell.X + anchor.LotCell.Y, 1, null, anchor));
        }

        sortedStructures.Sort((left, right) =>
        {
            var depthCompare = left.Depth.CompareTo(right.Depth);
            if (depthCompare != 0)
            {
                return depthCompare;
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
        var center = GetIsoCellCenter(building.Cell, origin);

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

        var shadow = CreateDiamond(center + new Vector2(ScaleValue(3f), ScaleValue(4f)), ScaleValue(TileHalfWidth * 0.58f), ScaleValue(TileHalfHeight * 0.54f));
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
        _terrainAtlasTexture = LoadTextureOrNull(TerrainAtlasTexturePath);
        _terrainTextures[TownTerrainType.Ground] = LoadTextureOrNull(TerrainGroundTexturePath);
        _terrainTextures[TownTerrainType.Road] = LoadTextureOrNull(TerrainRoadTexturePath);
        _terrainTextures[TownTerrainType.Courtyard] = LoadTextureOrNull(TerrainCourtyardTexturePath);
        _terrainTextures[TownTerrainType.Water] = LoadTextureOrNull(TerrainWaterTexturePath);
        _wallBrightTexture = LoadTextureOrNull(WallBrightTexturePath);
        _wallDarkTexture = LoadTextureOrNull(WallDarkTexturePath);
        _roofTexture = LoadTextureOrNull(RoofTexturePath);
        _gateTexture = LoadTextureOrNull(GateTexturePath);
        LoadAtlasManifest();
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
            var center = GetIsoCellCenter(cell, origin);
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
}


