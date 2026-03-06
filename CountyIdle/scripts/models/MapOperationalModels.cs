using Godot;

namespace CountyIdle.Models;

public enum MapRegionScope
{
    None,
    World,
    Prefecture,
    CountyTown
}

public enum MapDirectiveAction
{
    None,
    RepairCourierRoad,
    ReliefVillages,
    RepairStreets,
    NightWatch
}

public enum MapConditionLevel
{
    Flourishing,
    Stable,
    Strained,
    Critical
}

public sealed class MapViewStyle
{
    public MapConditionLevel Condition { get; set; } = MapConditionLevel.Stable;
    public string TitleSuffix { get; set; } = "平稳";
    public string HintText { get; set; } = string.Empty;
    public Color AccentColor { get; set; } = new(0.93f, 0.90f, 0.80f, 1f);
    public Color BackdropColor { get; set; } = new(0.09f, 0.11f, 0.16f, 0.92f);
    public Color GridColor { get; set; } = new(0.16f, 0.20f, 0.28f, 0.55f);
    public Color OutlineColor { get; set; } = new(0.82f, 0.86f, 0.96f, 0.35f);
    public Color RouteColor { get; set; } = new(0.95f, 0.79f, 0.42f, 0.82f);
    public Color RiverColor { get; set; } = new(0.38f, 0.62f, 0.88f, 0.82f);
    public Color NodeColor { get; set; } = new(0.94f, 0.88f, 0.73f, 1f);
    public Color LabelColor { get; set; } = new(0.93f, 0.90f, 0.80f, 0.96f);
    public Color TerrainTint { get; set; } = Colors.White;
    public Color BuildingTint { get; set; } = Colors.White;
}

public sealed class MapDirectiveChoice
{
    public MapDirectiveAction Action { get; set; } = MapDirectiveAction.None;
    public string Label { get; set; } = string.Empty;
    public string HintText { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}

public sealed class MapOperationalSnapshot
{
    public MapViewStyle WorldStyle { get; set; } = new();
    public MapViewStyle PrefectureStyle { get; set; } = new();
    public MapViewStyle CountyTownStyle { get; set; } = new();
    public string ActiveStatusText { get; set; } = string.Empty;
    public Color ActiveStatusColor { get; set; } = new(0.93f, 0.90f, 0.80f, 1f);
    public MapDirectiveChoice PrimaryChoice { get; set; } = new();
    public MapDirectiveChoice SecondaryChoice { get; set; } = new();
    public bool ShowDirectiveRow { get; set; }
}
