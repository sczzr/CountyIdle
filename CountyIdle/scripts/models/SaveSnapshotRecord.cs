using System;

namespace CountyIdle.Models;

public sealed class SaveSnapshotRecord
{
    public long Id { get; set; }
    public long SlotId { get; set; }
    public int SchemaVersion { get; set; }
    public string GameStateJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
