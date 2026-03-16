namespace CountyIdle.Models;

/// <summary>
/// 宗门发展方向类型。
/// </summary>
public enum SectDevelopmentDirectionType
{
    /// <summary>
    /// 平衡发展
    /// </summary>
    Balanced,
    /// <summary>
    /// 供养优先
    /// </summary>
    SupplyFirst,
    /// <summary>
    /// 传功优先
    /// </summary>
    DoctrineFirst,
    /// <summary>
    /// 防御优先
    /// </summary>
    DefenseFirst,
    /// <summary>
    /// 外务优先
    /// </summary>
    OutreachFirst
}

/// <summary>
/// 宗门法令类型。
/// </summary>
public enum SectLawType
{
    /// <summary>
    /// 仁政
    /// </summary>
    Benevolent,
    /// <summary>
    /// 纪律
    /// </summary>
    Discipline,
    /// <summary>
    /// 功绩
    /// </summary>
    Merit,
    /// <summary>
    /// 开讲
    /// </summary>
    OpenLectures
}

/// <summary>
/// 育才方案类型。
/// </summary>
public enum SectTalentPlanType
{
    /// <summary>
    /// 招收弟子
    /// </summary>
    RecruitDisciples,
    /// <summary>
    /// 阵法研修
    /// </summary>
    ArrayScholarship,
    /// <summary>
    /// 执事培养
    /// </summary>
    StewardTraining,
    /// <summary>
    /// 外务历练
    /// </summary>
    OuterMissions
}
