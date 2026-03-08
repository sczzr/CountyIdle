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
        theme.MapTitle = EnsureText(theme.MapTitle, "外域态势（云泽外域）");
        theme.CityTitle = EnsureText(theme.CityTitle, "云泽外城");
        theme.ForestName = EnsureText(theme.ForestName, "青岚灵林");
        theme.LakeName = EnsureText(theme.LakeName, "月魄湖");
        theme.MountainName = EnsureText(theme.MountainName, "东岭灵脉");
        theme.FarmlandName = EnsureText(theme.FarmlandName, "云泽灵田");
        theme.MainAvenueName = EnsureText(theme.MainAvenueName, "问道长街");
        theme.RiverGateName = EnsureText(theme.RiverGateName, "云津渡口");
        theme.InnerCityName = EnsureText(theme.InnerCityName, "内坊");
        theme.OuterWardsName = EnsureText(theme.OuterWardsName, "山门坊市");

        theme.GateNames ??= new PrefectureGateNames();
        theme.GateNames.North = EnsureText(theme.GateNames.North, "北岚门");
        theme.GateNames.South = EnsureText(theme.GateNames.South, "南明门");
        theme.GateNames.East = EnsureText(theme.GateNames.East, "东云门");
        theme.GateNames.West = EnsureText(theme.GateNames.West, "西澜门");

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
            MapTitle = "外域态势（云泽外域）",
            CityTitle = "云泽外城",
            ForestName = "青岚灵林",
            LakeName = "月魄湖",
            MountainName = "东岭灵脉",
            FarmlandName = "云泽灵田",
            MainAvenueName = "问道长街",
            RiverGateName = "云津渡口",
            InnerCityName = "内坊",
            OuterWardsName = "山门坊市",
            GateNames = new PrefectureGateNames
            {
                North = "北岚门",
                South = "南明门",
                East = "东云门",
                West = "西澜门"
            },
            LandmarkNames =
            [
                "外务殿",
                "云津坊市",
                "观星台",
                "灵舟渡口",
                "储灵库",
                "藏经别院",
                "演武校场",
                "山门牌坊"
            ],
            WardNamePool =
            [
                "外门居舍",
                "炼器作坊",
                "沿街商铺",
                "论道茶寮",
                "行商栈舍",
                "储运库房",
                "布帛行肆",
                "药庐",
                "书阁",
                "酒楼"
            ]
        };
    }
}
