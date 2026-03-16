using System;

namespace CountyIdle.Models;

/// <summary>
/// 存档槽摘要（用于存档列表展示）。
/// </summary>
public sealed class SaveSlotSummary
{
    // 数据库主键
    public long Id { get; set; }
    // 槽位键
    public string SlotKey { get; set; } = string.Empty;
    // 槽位名称
    public string SlotName { get; set; } = string.Empty;
    // 是否自动存档
    public bool IsAutosave { get; set; }
    // 创建时间（UTC）
    public DateTime CreatedAtUtc { get; set; }
    // 更新时间（UTC）
    public DateTime UpdatedAtUtc { get; set; }
    // 游戏分钟数
    public int GameMinutes { get; set; }
    // 总人口
    public int Population { get; set; }
    // 灵石/金钱
    public double Gold { get; set; }
    // 科技等级
    public int TechLevel { get; set; }
    // 民心
    public double Happiness { get; set; }
    // 威胁
    public double Threat { get; set; }
    // 探险深度
    public int ExplorationDepth { get; set; }
    // 仓储占用
    public double WarehouseUsed { get; set; }
    // 仓储容量
    public double WarehouseCapacity { get; set; }
    // 预览图路径（绝对路径）
    public string PreviewImagePath { get; set; } = string.Empty;
}
