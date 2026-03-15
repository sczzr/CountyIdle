namespace CountyIdle.Models;

public sealed class GameCalendarInfo
{
	public int TotalGameMinutes { get; init; }

	public string DateText { get; init; } = string.Empty;

	public string HeaderText { get; init; } = string.Empty;

	public string DetailText { get; init; } = string.Empty;

	public string SolarTermName { get; init; } = string.Empty;

	public string TimeOfDayName { get; init; } = string.Empty;

	public string QuarterName { get; init; } = string.Empty;

	public string QuarterProgressText { get; init; } = string.Empty;

	public string DayProgressText { get; init; } = string.Empty;

	public double QuarterProgressPercent { get; init; }

	public double DayProgressPercent { get; init; }

	public double SolarTermProgressPercent { get; init; }
}
