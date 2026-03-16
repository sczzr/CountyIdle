using System;

namespace CountyIdle.Models;

/// <summary>
/// 存档快照记录（SQLite 快照表映射）。
/// </summary>
public sealed class SaveSnapshotRecord
{
    // 快照主键
    public long Id { get; set; }
    // 所属槽位 ID
    public long SlotId { get; set; }
    // 存档结构版本
    public int SchemaVersion { get; set; }
    // 游戏状态 JSON
    public string GameStateJson { get; set; } = string.Empty;
    // 创建时间（UTC）
    public DateTime CreatedAtUtc { get; set; }
}
