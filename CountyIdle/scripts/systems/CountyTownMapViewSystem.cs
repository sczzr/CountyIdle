using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem : PanelContainer
{
    private const float TileHalfWidth = 14f;
    private const float TileHalfHeight = 7f;
    private const float TopPadding = 42f;
    private const float FootprintScale = 0.72f;

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

    private TownMapData? _mapData;
    private Button _regenerateButton = null!;
    private Label _mapHintLabel = null!;
    private Texture2D? _wallBrightTexture;
    private Texture2D? _wallDarkTexture;
    private Texture2D? _roofTexture;
    private Texture2D? _gateTexture;
    private int _layoutSeed;
    private int _populationHint = 120;
    private int _housingHint = 180;
    private int _eliteHint = 8;
    private bool _isInitialized;

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
        DrawBuildings(_mapData, origin);
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
        _mapData = _generator.Generate(_populationHint, _housingHint, _eliteHint, _layoutSeed);
        _mapHintLabel.Text = $"县城地图（2.5D） 房屋 {_mapData.Buildings.Count} · 种子 {_layoutSeed % 10000:D4}";
        QueueRedraw();
    }

    private Vector2 CalculateMapOrigin(TownMapData mapData)
    {
        var minX = -(mapData.Height - 1) * TileHalfWidth;
        var maxX = (mapData.Width - 1) * TileHalfWidth;
        var mapCenterX = (minX + maxX) * 0.5f;

        var isoHeight = (mapData.Width + mapData.Height - 2) * TileHalfHeight;
        var remainingHeight = Math.Max(Size.Y - TopPadding - isoHeight, 0f);
        var offsetY = TopPadding + (remainingHeight * 0.15f);

        return new Vector2((Size.X * 0.5f) - mapCenterX, offsetY);
    }

    private static Vector2 GetIsoCellCenter(Vector2I cell, Vector2 origin)
    {
        var isoX = (cell.X - cell.Y) * TileHalfWidth;
        var isoY = (cell.X + cell.Y) * TileHalfHeight;
        return origin + new Vector2(isoX, isoY);
    }

    private void DrawTerrain(TownMapData mapData, Vector2 origin)
    {
        foreach (var cell in mapData.EnumerateAllCells())
        {
            var center = GetIsoCellCenter(cell, origin);
            var tile = CreateDiamond(center, TileHalfWidth, TileHalfHeight);
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
        DrawLine(tile[0], tile[1], GridLineColor, 0.8f);
        DrawLine(tile[1], tile[2], GridLineColor, 0.8f);
        DrawLine(tile[2], tile[3], GridLineColor, 0.8f);
        DrawLine(tile[3], tile[0], GridLineColor, 0.8f);
    }

    private void DrawBuildings(TownMapData mapData, Vector2 origin)
    {
        var sortedBuildings = new List<TownBuildingData>(mapData.Buildings);
        sortedBuildings.Sort((left, right) =>
        {
            var leftDepth = left.Cell.X + left.Cell.Y;
            var rightDepth = right.Cell.X + right.Cell.Y;
            return leftDepth.CompareTo(rightDepth);
        });

        foreach (var building in sortedBuildings)
        {
            DrawBuilding(building, origin);
        }
    }

    private void DrawBuilding(TownBuildingData building, Vector2 origin)
    {
        var center = GetIsoCellCenter(building.Cell, origin);

        var footprint = CreateDiamond(center, TileHalfWidth * FootprintScale, TileHalfHeight * FootprintScale);
        var baseTop = footprint[0];
        var baseRight = footprint[1];
        var baseBottom = footprint[2];
        var baseLeft = footprint[3];

        var wallHeight = building.Floors == 1 ? 18f : 28f;
        var roofLift = building.Floors == 1 ? 6f : 8f;
        var wallOffset = new Vector2(0f, -wallHeight);

        var roofTop = baseTop + wallOffset;
        var roofRight = baseRight + wallOffset;
        var roofBottom = baseBottom + wallOffset;
        var roofLeft = baseLeft + wallOffset;

        var shadow = CreateDiamond(center + new Vector2(3f, 4f), TileHalfWidth * 0.58f, TileHalfHeight * 0.54f);
        DrawColoredPolygon(shadow, new Color(0f, 0f, 0f, 0.18f));

        var leftWall = new[] { baseLeft, baseBottom, roofBottom, roofLeft };
        var rightWall = new[] { baseBottom, baseRight, roofRight, roofBottom };
        DrawTexturedPolygon(leftWall, _wallDarkTexture, WallDarkColor);
        DrawTexturedPolygon(rightWall, _wallBrightTexture, WallBrightColor);

        var eaveTop = roofTop + new Vector2(0f, -roofLift);
        var eaveRight = roofRight + new Vector2(4f, 2f);
        var eaveBottom = roofBottom + new Vector2(0f, 3f);
        var eaveLeft = roofLeft + new Vector2(-4f, 2f);

        var roofFace = new[] { eaveTop, eaveRight, eaveBottom, eaveLeft };
        DrawTexturedPolygon(roofFace, _roofTexture, RoofMainColor);

        var roofShade = new[] { roofTop, roofRight, eaveRight, eaveTop };
        DrawTexturedPolygon(roofShade, _roofTexture, RoofShadeColor);

        var ridgeStart = (eaveTop + eaveLeft) * 0.5f;
        var ridgeEnd = (eaveTop + eaveRight) * 0.5f;
        DrawLine(ridgeStart, ridgeEnd, RoofRidgeColor, 1.8f);

        DrawLine(eaveTop, eaveRight, GridLineColor, 1.0f);
        DrawLine(eaveRight, eaveBottom, GridLineColor, 1.0f);
        DrawLine(eaveBottom, eaveLeft, GridLineColor, 1.0f);
        DrawLine(eaveLeft, eaveTop, GridLineColor, 1.0f);

        if (building.HasMoonGate)
        {
            DrawMoonGate(baseBottom, building.Facing);
        }
    }

    private void DrawMoonGate(Vector2 baseBottom, TownFacing facing)
    {
        var offset = facing switch
        {
            TownFacing.North => new Vector2(-4f, -2f),
            TownFacing.South => new Vector2(4f, -2f),
            TownFacing.East => new Vector2(6f, -1f),
            _ => new Vector2(-6f, -1f)
        };

        var gateCenter = baseBottom + offset;
        if (_gateTexture != null)
        {
            var gateSize = new Vector2(10f, 10f);
            var gateRect = new Rect2(gateCenter - (gateSize * 0.5f), gateSize);
            DrawTextureRect(_gateTexture, gateRect, false, GateColor);
        }

        DrawCircle(gateCenter, 2.6f, GateColor);
        DrawArc(gateCenter, 2.8f, 0f, Mathf.Tau, 14, RoofRidgeColor, 0.9f);
    }

    private void LoadTextures()
    {
        _terrainTextures[TownTerrainType.Ground] = LoadTextureOrNull(TerrainGroundTexturePath);
        _terrainTextures[TownTerrainType.Road] = LoadTextureOrNull(TerrainRoadTexturePath);
        _terrainTextures[TownTerrainType.Courtyard] = LoadTextureOrNull(TerrainCourtyardTexturePath);
        _terrainTextures[TownTerrainType.Water] = LoadTextureOrNull(TerrainWaterTexturePath);

        _wallBrightTexture = LoadTextureOrNull(WallBrightTexturePath);
        _wallDarkTexture = LoadTextureOrNull(WallDarkTexturePath);
        _roofTexture = LoadTextureOrNull(RoofTexturePath);
        _gateTexture = LoadTextureOrNull(GateTexturePath);
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

    private static Color GetTerrainColor(TownTerrainType terrainType)
    {
        return terrainType switch
        {
            TownTerrainType.Road => RoadColor,
            TownTerrainType.Courtyard => CourtyardColor,
            TownTerrainType.Water => WaterColor,
            _ => GroundColor
        };
    }
}
