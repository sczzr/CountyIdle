namespace CountyIdle.Models;

/// <summary>
/// 弟子装备档案（用于弟子谱展示与存档持久化）。
/// </summary>
public sealed record DiscipleEquipmentProfile(
    string WeaponName,
    string ArmorName,
    string RelicName,
    string TalismanName,
    string QualityTag,
    int GearScore);
