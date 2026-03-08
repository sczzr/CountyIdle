using System;

namespace CountyIdle.Models;

public sealed class SaveSlotSummary
{
    public long Id { get; set; }
    public string SlotKey { get; set; } = string.Empty;
    public string SlotName { get; set; } = string.Empty;
    public bool IsAutosave { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public int GameMinutes { get; set; }
    public int Population { get; set; }
    public double Gold { get; set; }
    public int TechLevel { get; set; }
    public double Happiness { get; set; }
    public double Threat { get; set; }
    public int ExplorationDepth { get; set; }
    public double WarehouseUsed { get; set; }
    public double WarehouseCapacity { get; set; }
    public string PreviewImagePath { get; set; } = string.Empty;
}
