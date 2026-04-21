using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

/// <summary>
/// 战略地图配置（世界图/外域图）。
/// </summary>
public sealed class StrategicMapConfig
{
    // 世界图配置
    [JsonPropertyName("world")]
    public StrategicMapDefinition? World { get; set; }

    // 外域图配置
    [JsonPropertyName("prefecture")]
    public StrategicMapDefinition? Prefecture { get; set; }
}

/// <summary>
/// 单张战略地图定义（区域/路线/节点/标签）。
/// </summary>
public sealed class StrategicMapDefinition
{
    // 地图标题
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    // 单位缩放
    [JsonPropertyName("unit_scale")]
    public float UnitScale { get; set; } = 0.42f;

    // 网格线数量
    [JsonPropertyName("grid_lines")]
    public int GridLines { get; set; } = 8;

    // 区域多边形
    [JsonPropertyName("regions")]
    public List<StrategicPolygonDefinition> Regions { get; set; } = [];

    // 外轮廓折线
    [JsonPropertyName("outlines")]
    public List<StrategicPolylineDefinition> Outlines { get; set; } = [];

    // 路线折线
    [JsonPropertyName("routes")]
    public List<StrategicPolylineDefinition> Routes { get; set; } = [];

    // 河流折线
    [JsonPropertyName("rivers")]
    public List<StrategicPolylineDefinition> Rivers { get; set; } = [];

    // 节点定义
    [JsonPropertyName("nodes")]
    public List<StrategicNodeDefinition> Nodes { get; set; } = [];

    // 标签定义
    [JsonPropertyName("labels")]
    public List<StrategicLabelDefinition> Labels { get; set; } = [];

    // 可选的世界图交互数据，仅世界图配置使用；用于保留 hex 点选、站点详情与二级地图入口。
    [JsonPropertyName("interactive_world")]
    public XianxiaWorldMapData? InteractiveWorld { get; set; }
}

/// <summary>
/// 战略地图多边形区域定义。
/// </summary>
public sealed class StrategicPolygonDefinition
{
    // 填充色（字符串色值）
    [JsonPropertyName("fill_color")]
    public string FillColor { get; set; } = string.Empty;

    // 边线颜色
    [JsonPropertyName("outline_color")]
    public string OutlineColor { get; set; } = string.Empty;

    // 边线宽度
    [JsonPropertyName("outline_width")]
    public float OutlineWidth { get; set; } = 1.2f;

    // 顶点坐标
    [JsonPropertyName("points")]
    public List<StrategicPointDefinition> Points { get; set; } = [];
}

/// <summary>
/// 战略地图折线定义（路线/河流/轮廓）。
/// </summary>
public sealed class StrategicPolylineDefinition
{
    // 线条颜色
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    // 线条宽度
    [JsonPropertyName("width")]
    public float Width { get; set; } = 1.2f;

    // 是否闭合
    [JsonPropertyName("closed")]
    public bool Closed { get; set; }

    // 折线点集
    [JsonPropertyName("points")]
    public List<StrategicPointDefinition> Points { get; set; } = [];
}

/// <summary>
/// 战略地图节点定义。
/// </summary>
public sealed class StrategicNodeDefinition
{
    // X 坐标
    [JsonPropertyName("x")]
    public float X { get; set; }

    // Y 坐标
    [JsonPropertyName("y")]
    public float Y { get; set; }

    // 半径
    [JsonPropertyName("radius")]
    public float Radius { get; set; } = 4f;

    // 颜色
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    // 节点类型
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
}

/// <summary>
/// 战略地图坐标点。
/// </summary>
public sealed class StrategicPointDefinition
{
    // X 坐标
    [JsonPropertyName("x")]
    public float X { get; set; }

    // Y 坐标
    [JsonPropertyName("y")]
    public float Y { get; set; }
}

/// <summary>
/// 战略地图文本标签定义。
/// </summary>
public sealed class StrategicLabelDefinition
{
    // X 坐标
    [JsonPropertyName("x")]
    public float X { get; set; }

    // Y 坐标
    [JsonPropertyName("y")]
    public float Y { get; set; }

    // 文本内容
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    // 文本颜色
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    // 字号
    [JsonPropertyName("font_size")]
    public int FontSize { get; set; } = 12;

    // 最小可见缩放
    [JsonPropertyName("min_zoom")]
    public float MinZoom { get; set; } = 0.6f;
}
