namespace CountyIdle.Models;

/// <summary>
/// 弟子指令类型（用于弟子谱调度）。
/// </summary>
public enum DiscipleDirectiveType
{
    /// <summary>
    /// 无指令
    /// </summary>
    None,
    /// <summary>
    /// 外务候补（历练/外务优先）
    /// </summary>
    OuterMissionCandidate,
    /// <summary>
    /// 执事候补（内务/治理优先）
    /// </summary>
    StewardCandidate
}
