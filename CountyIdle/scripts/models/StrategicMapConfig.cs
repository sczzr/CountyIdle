using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

public sealed class StrategicMapConfig
{
    [JsonPropertyName("world")]
    public StrategicMapDefinition? World { get; set; }

    [JsonPropertyName("prefecture")]
    public StrategicMapDefinition? Prefecture { get; set; }
}

public sealed class StrategicMapDefinition
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("unit_scale")]
    public float UnitScale { get; set; } = 0.42f;

    [JsonPropertyName("grid_lines")]
    public int GridLines { get; set; } = 8;

    [JsonPropertyName("regions")]
    public List<StrategicPolygonDefinition> Regions { get; set; } = [];

    [JsonPropertyName("outlines")]
    public List<StrategicPolylineDefinition> Outlines { get; set; } = [];

    [JsonPropertyName("routes")]
    public List<StrategicPolylineDefinition> Routes { get; set; } = [];

    [JsonPropertyName("rivers")]
    public List<StrategicPolylineDefinition> Rivers { get; set; } = [];

    [JsonPropertyName("nodes")]
    public List<StrategicNodeDefinition> Nodes { get; set; } = [];
}

public sealed class StrategicPolygonDefinition
{
    [JsonPropertyName("fill_color")]
    public string FillColor { get; set; } = string.Empty;

    [JsonPropertyName("outline_color")]
    public string OutlineColor { get; set; } = string.Empty;

    [JsonPropertyName("outline_width")]
    public float OutlineWidth { get; set; } = 1.2f;

    [JsonPropertyName("points")]
    public List<StrategicPointDefinition> Points { get; set; } = [];
}

public sealed class StrategicPolylineDefinition
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public float Width { get; set; } = 1.2f;

    [JsonPropertyName("closed")]
    public bool Closed { get; set; }

    [JsonPropertyName("points")]
    public List<StrategicPointDefinition> Points { get; set; } = [];
}

public sealed class StrategicNodeDefinition
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("radius")]
    public float Radius { get; set; } = 4f;

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

public sealed class StrategicPointDefinition
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }
}
