using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed class PrefectureCityThemeConfigSystem
{
    private const string ConfigPath = "res://data/prefecture_city_theme.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static PrefectureCityThemeConfig? _cachedConfig;

    public PrefectureCityThemeConfig GetTheme()
    {
        if (_cachedConfig != null)
        {
            return _cachedConfig;
        }

        _cachedConfig = TryLoadFromFile() ?? BuildFallbackTheme();
        Normalize(_cachedConfig);
        return _cachedConfig;
    }

    private static PrefectureCityThemeConfig? TryLoadFromFile()
    {
        if (!FileAccess.FileExists(ConfigPath))
        {
            GD.PushWarning($"Prefecture city theme config not found at {ConfigPath}, fallback will be used.");
            return null;
        }

        try
        {
            using var file = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            var config = JsonSerializer.Deserialize<PrefectureCityThemeConfig>(json, JsonOptions);
            if (config == null)
            {
                GD.PushWarning("Prefecture city theme config deserialize returned null, fallback will be used.");
                return null;
            }

            return config;
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Prefecture city theme config load failed: {ex.Message}, fallback will be used.");
            return null;
        }
    }

    private static void Normalize(PrefectureCityThemeConfig theme)
    {
        theme.MapTitle = EnsureText(theme.MapTitle, "周边郡图（开封城式）");
        theme.CityTitle = EnsureText(theme.CityTitle, "开封郡城");
        theme.ForestName = EnsureText(theme.ForestName, "青岚森林");
        theme.LakeName = EnsureText(theme.LakeName, "月湖");
        theme.MountainName = EnsureText(theme.MountainName, "东岭山脉");
        theme.FarmlandName = EnsureText(theme.FarmlandName, "汴梁农田");
        theme.MainAvenueName = EnsureText(theme.MainAvenueName, "御街");
        theme.RiverGateName = EnsureText(theme.RiverGateName, "汴河渡口");
        theme.InnerCityName = EnsureText(theme.InnerCityName, "内城");
        theme.OuterWardsName = EnsureText(theme.OuterWardsName, "外郭坊市");

        theme.GateNames ??= new PrefectureGateNames();
        theme.GateNames.North = EnsureText(theme.GateNames.North, "北门");
        theme.GateNames.South = EnsureText(theme.GateNames.South, "南门");
        theme.GateNames.East = EnsureText(theme.GateNames.East, "东门");
        theme.GateNames.West = EnsureText(theme.GateNames.West, "西门");

        theme.LandmarkNames ??= [];
        if (theme.LandmarkNames.Count == 0)
        {
            theme.LandmarkNames.AddRange(BuildFallbackTheme().LandmarkNames);
        }

        theme.WardNamePool ??= [];
        if (theme.WardNamePool.Count == 0)
        {
            theme.WardNamePool.AddRange(BuildFallbackTheme().WardNamePool);
        }

        for (var i = 0; i < theme.LandmarkNames.Count; i++)
        {
            theme.LandmarkNames[i] = EnsureText(theme.LandmarkNames[i], $"地标{i + 1}");
        }

        for (var i = 0; i < theme.WardNamePool.Count; i++)
        {
            theme.WardNamePool[i] = EnsureText(theme.WardNamePool[i], $"坊市{i + 1}");
        }
    }

    private static string EnsureText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static PrefectureCityThemeConfig BuildFallbackTheme()
    {
        return new PrefectureCityThemeConfig
        {
            MapTitle = "周边郡图（开封城式）",
            CityTitle = "开封郡城",
            ForestName = "青岚森林",
            LakeName = "月湖",
            MountainName = "东岭山脉",
            FarmlandName = "汴梁农田",
            MainAvenueName = "御街",
            RiverGateName = "汴河渡口",
            InnerCityName = "内城",
            OuterWardsName = "外郭坊市",
            GateNames = new PrefectureGateNames
            {
                North = "宣德门",
                South = "朱雀门",
                East = "东水门",
                West = "西华门"
            },
            LandmarkNames =
            [
                "开封府衙",
                "州桥市集",
                "大相国寺",
                "汴河码头",
                "漕运官仓",
                "太学书院",
                "军营校场",
                "御街坊门"
            ],
            WardNamePool =
            [
                "里坊民居",
                "工匠作坊",
                "沿街商铺",
                "茶肆客栈",
                "盐引商号",
                "漕运仓铺",
                "布帛行肆",
                "药铺",
                "书肆",
                "酒楼"
            ]
        };
    }
}
