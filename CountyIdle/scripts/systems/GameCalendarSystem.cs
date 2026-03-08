using System;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed class GameCalendarSystem
{
    private const string EraName = "景禾";
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;
    private const int DaysPerMonth = 30;
    private const int MonthsPerYear = 12;
    private const int MonthsPerQuarter = 3;
    private const int DaysPerSolarTerm = 15;
    private const int SolarTermsPerYear = 24;
    private const int MinutesPerDay = MinutesPerHour * HoursPerDay;
    private const int MinutesPerMonth = MinutesPerDay * DaysPerMonth;
    private const int MinutesPerQuarter = MinutesPerMonth * MonthsPerQuarter;
    private const int MinutesPerSolarTerm = MinutesPerDay * DaysPerSolarTerm;
    private const int MinutesPerYear = MinutesPerDay * DaysPerMonth * MonthsPerYear;

    private static readonly string[] MonthNames =
    {
        "正月",
        "二月",
        "三月",
        "四月",
        "五月",
        "六月",
        "七月",
        "八月",
        "九月",
        "十月",
        "冬月",
        "腊月"
    };

    private static readonly string[] DayNames =
    {
        "初一",
        "初二",
        "初三",
        "初四",
        "初五",
        "初六",
        "初七",
        "初八",
        "初九",
        "初十",
        "十一",
        "十二",
        "十三",
        "十四",
        "十五",
        "十六",
        "十七",
        "十八",
        "十九",
        "二十",
        "廿一",
        "廿二",
        "廿三",
        "廿四",
        "廿五",
        "廿六",
        "廿七",
        "廿八",
        "廿九",
        "三十"
    };

    private static readonly string[] SolarTermNames =
    {
        "立春",
        "雨水",
        "惊蛰",
        "春分",
        "清明",
        "谷雨",
        "立夏",
        "小满",
        "芒种",
        "夏至",
        "小暑",
        "大暑",
        "立秋",
        "处暑",
        "白露",
        "秋分",
        "寒露",
        "霜降",
        "立冬",
        "小雪",
        "大雪",
        "冬至",
        "小寒",
        "大寒"
    };

    private static readonly string[] DoubleHourNames =
    {
        "子时",
        "丑时",
        "寅时",
        "卯时",
        "辰时",
        "巳时",
        "午时",
        "未时",
        "申时",
        "酉时",
        "戌时",
        "亥时"
    };

    private static readonly string[] QuarterNames =
    {
        "春季",
        "夏季",
        "秋季",
        "冬季"
    };

    public GameCalendarInfo Describe(int gameMinutes)
    {
        var safeGameMinutes = Math.Max(gameMinutes, 0);
        var minuteOfYear = safeGameMinutes % MinutesPerYear;
        var yearNumber = (safeGameMinutes / MinutesPerYear) + 1;
        var monthIndex = minuteOfYear / MinutesPerMonth;
        var minuteOfMonth = minuteOfYear % MinutesPerMonth;
        var dayIndex = minuteOfMonth / MinutesPerDay;
        var minuteOfDay = minuteOfMonth % MinutesPerDay;
        var hour = minuteOfDay / MinutesPerHour;
        var minute = minuteOfDay % MinutesPerHour;
        var quarterIndex = minuteOfYear / MinutesPerQuarter;
        var minuteOfQuarter = minuteOfYear % MinutesPerQuarter;
        var quarterDay = (minuteOfQuarter / MinutesPerDay) + 1;
        var solarTermIndex = minuteOfYear / MinutesPerSolarTerm;
        var minuteOfSolarTerm = minuteOfYear % MinutesPerSolarTerm;

        var yearLabel = $"{EraName}{FormatRegnalYear(yearNumber)}年";
        var monthLabel = MonthNames[monthIndex];
        var dayLabel = DayNames[dayIndex];
        var solarTermName = SolarTermNames[Math.Clamp(solarTermIndex, 0, SolarTermsPerYear - 1)];
        var timeOfDayName = DoubleHourNames[(hour / 2) % DoubleHourNames.Length];
        var quarterName = QuarterNames[Math.Clamp(quarterIndex, 0, QuarterNames.Length - 1)];
        var quarterLabel = $"第{ToChineseNumber(quarterIndex + 1)}季度·{quarterName}";
        var clockText = $"{hour:00}:{minute:00}";
        var dateText = $"{yearLabel} {monthLabel} {dayLabel}";

        return new GameCalendarInfo
        {
            TotalGameMinutes = safeGameMinutes,
            DateText = dateText,
            HeaderText = $"{dateText} · {solarTermName}",
            DetailText = $"{quarterName} · {solarTermName} · {timeOfDayName} · {clockText}",
            SolarTermName = solarTermName,
            TimeOfDayName = timeOfDayName,
            QuarterName = quarterName,
            QuarterProgressText = $"{quarterLabel} · 季内第{ToChineseNumber(quarterDay)}日",
            DayProgressText = $"今日 {clockText} · {timeOfDayName}",
            QuarterProgressPercent = minuteOfQuarter / (double)MinutesPerQuarter * 100.0,
            DayProgressPercent = minuteOfDay / (double)MinutesPerDay * 100.0,
            SolarTermProgressPercent = minuteOfSolarTerm / (double)MinutesPerSolarTerm * 100.0
        };
    }

    public int GetQuarterIndex(int gameMinutes)
    {
        var safeGameMinutes = Math.Max(gameMinutes, 0);
        var minuteOfYear = safeGameMinutes % MinutesPerYear;
        return Math.Clamp(minuteOfYear / MinutesPerQuarter, 0, QuarterNames.Length - 1);
    }

    public string GetQuarterLabel(int gameMinutes)
    {
        return QuarterNames[GetQuarterIndex(gameMinutes)];
    }

    private static string FormatRegnalYear(int yearNumber)
    {
        return yearNumber <= 1 ? "元" : ToChineseNumber(yearNumber);
    }

    private static string ToChineseNumber(int value)
    {
        if (value <= 0)
        {
            return "零";
        }

        if (value >= 10000)
        {
            return value.ToString();
        }

        var thousands = value / 1000;
        var hundreds = (value / 100) % 10;
        var tens = (value / 10) % 10;
        var ones = value % 10;

        var result = string.Empty;

        if (thousands > 0)
        {
            result += DigitToChinese(thousands) + "千";
        }

        if (hundreds > 0)
        {
            result += DigitToChinese(hundreds) + "百";
        }
        else if (thousands > 0 && (tens > 0 || ones > 0))
        {
            result += "零";
        }

        if (tens > 0)
        {
            if (tens == 1 && thousands == 0 && hundreds == 0)
            {
                result += "十";
            }
            else
            {
                result += DigitToChinese(tens) + "十";
            }
        }
        else if ((hundreds > 0 || thousands > 0) && ones > 0)
        {
            if (!result.EndsWith("零", StringComparison.Ordinal))
            {
                result += "零";
            }
        }

        if (ones > 0)
        {
            result += DigitToChinese(ones);
        }

        return result;
    }

    private static string DigitToChinese(int value)
    {
        return value switch
        {
            0 => "零",
            1 => "一",
            2 => "二",
            3 => "三",
            4 => "四",
            5 => "五",
            6 => "六",
            7 => "七",
            8 => "八",
            9 => "九",
            _ => value.ToString()
        };
    }
}
