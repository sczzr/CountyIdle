using Godot;

namespace CountyIdle.Models;

/// <summary>
/// 地图区域范围（用于调度与 UI 区分）。
/// </summary>
public enum MapRegionScope
{
    /// <summary>
    /// 无范围
    /// </summary>
    None,
    /// <summary>
    /// 世界图
    /// </summary>
    World,
    /// <summary>
    /// 外域（江陵府）
    /// </summary>
    Prefecture,
    /// <summary>
    /// 山门沙盘
    /// </summary>
    CountyTown
}

/// <summary>
/// 地图调度动作类型。
/// </summary>
public enum MapDirectiveAction
{
    /// <summary>
    /// 无动作
    /// </summary>
    None,
    /// <summary>
    /// 修缮驿路
    /// </summary>
    RepairCourierRoad,
    /// <summary>
    /// 抚恤附庸
    /// </summary>
    ReliefVillages,
    /// <summary>
    /// 修复街巷
    /// </summary>
    RepairStreets,
    /// <summary>
    /// 夜巡守护
    /// </summary>
    NightWatch
}

/// <summary>
/// 地图态势等级（影响配色与提示）。
/// </summary>
public enum MapConditionLevel
{
    /// <summary>
    /// 兴盛
    /// </summary>
    Flourishing,
    /// <summary>
    /// 平稳
    /// </summary>
    Stable,
    /// <summary>
    /// 紧张
    /// </summary>
    Strained,
    /// <summary>
    /// 危急
    /// </summary>
    Critical
}

/// <summary>
/// 地图视图样式（色调/提示）。
/// </summary>
public sealed class MapViewStyle
{
    // 当前态势等级
    public MapConditionLevel Condition { get; set; } = MapConditionLevel.Stable;
    // 标题后缀（例如“平稳”）
    public string TitleSuffix { get; set; } = "平稳";
    // 提示文本
    public string HintText { get; set; } = string.Empty;
    // 高亮色与背景色
    public Color AccentColor { get; set; } = new(0.93f, 0.90f, 0.80f, 1f);
    public Color BackdropColor { get; set; } = new(0.09f, 0.11f, 0.16f, 0.92f);
    public Color GridColor { get; set; } = new(0.16f, 0.20f, 0.28f, 0.55f);
    public Color OutlineColor { get; set; } = new(0.82f, 0.86f, 0.96f, 0.35f);
    // 路线/河流/节点等专用色
    public Color RouteColor { get; set; } = new(0.95f, 0.79f, 0.42f, 0.82f);
    public Color RiverColor { get; set; } = new(0.38f, 0.62f, 0.88f, 0.82f);
    public Color NodeColor { get; set; } = new(0.94f, 0.88f, 0.73f, 1f);
    public Color LabelColor { get; set; } = new(0.93f, 0.90f, 0.80f, 0.96f);
    // 地形与建筑色调
    public Color TerrainTint { get; set; } = Colors.White;
    public Color BuildingTint { get; set; } = Colors.White;
}

/// <summary>
/// 调度动作的 UI 选项。
/// </summary>
public sealed class MapDirectiveChoice
{
    // 动作类型
    public MapDirectiveAction Action { get; set; } = MapDirectiveAction.None;
    // 按钮文案
    public string Label { get; set; } = string.Empty;
    // 说明提示
    public string HintText { get; set; } = string.Empty;
    // 是否可用
    public bool Enabled { get; set; }
}

/// <summary>
/// 地图调度快照（用于主界面展示）。
/// </summary>
public sealed class MapOperationalSnapshot
{
    // 三张地图的样式
    public MapViewStyle WorldStyle { get; set; } = new();
    public MapViewStyle PrefectureStyle { get; set; } = new();
    public MapViewStyle CountyTownStyle { get; set; } = new();
    // 当前状态文字与颜色
    public string ActiveStatusText { get; set; } = string.Empty;
    public Color ActiveStatusColor { get; set; } = new(0.93f, 0.90f, 0.80f, 1f);
    // 主/次调度选择
    public MapDirectiveChoice PrimaryChoice { get; set; } = new();
    public MapDirectiveChoice SecondaryChoice { get; set; } = new();
    // 是否显示调度条
    public bool ShowDirectiveRow { get; set; }
}
