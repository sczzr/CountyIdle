namespace CountyIdle.Models;

/// <summary>
/// 日历与时辰展示信息（用于 UI 顶栏与时间提示）。
/// </summary>
public sealed class GameCalendarInfo
{
	// 总游戏分钟数（用于时间推进与结算口径）
	public int TotalGameMinutes { get; init; }

	// 日期文本（年/月/日）
	public string DateText { get; init; } = string.Empty;

	// 顶栏主标题
	public string HeaderText { get; init; } = string.Empty;

	// 顶栏副标题/说明
	public string DetailText { get; init; } = string.Empty;

	// 节气名称
	public string SolarTermName { get; init; } = string.Empty;

	// 时辰名称（昼夜段）
	public string TimeOfDayName { get; init; } = string.Empty;

	// 当前季度名称
	public string QuarterName { get; init; } = string.Empty;

	// 季度进度文字
	public string QuarterProgressText { get; init; } = string.Empty;

	// 当日进度文字
	public string DayProgressText { get; init; } = string.Empty;

	// 季度进度百分比（0-1）
	public double QuarterProgressPercent { get; init; }

	// 当日进度百分比（0-1）
	public double DayProgressPercent { get; init; }

	// 节气进度百分比（0-1）
	public double SolarTermProgressPercent { get; init; }
}
