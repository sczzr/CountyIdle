using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

/// <summary>
/// 外域府城的命名与主题配置（用于地图文案）。
/// </summary>
public sealed class PrefectureCityThemeConfig
{
    // 地图标题
    [JsonPropertyName("map_title")]
    public string MapTitle { get; set; } = "江陵府外域（附庸圈层）";

    // 城市标题
    [JsonPropertyName("city_title")]
    public string CityTitle { get; set; } = "云泽附庸坊城";

    // 森林名称
    [JsonPropertyName("forest_name")]
    public string ForestName { get; set; } = "青岚灵林";

    // 湖泊名称
    [JsonPropertyName("lake_name")]
    public string LakeName { get; set; } = "月魄湖";

    // 山脉名称
    [JsonPropertyName("mountain_name")]
    public string MountainName { get; set; } = "东岭灵脉";

    // 农田名称
    [JsonPropertyName("farmland_name")]
    public string FarmlandName { get; set; } = "云泽阵材圃";

    // 主干道名称
    [JsonPropertyName("main_avenue_name")]
    public string MainAvenueName { get; set; } = "问道长街";

    // 河门名称
    [JsonPropertyName("river_gate_name")]
    public string RiverGateName { get; set; } = "云津渡口";

    // 内坊名称
    [JsonPropertyName("inner_city_name")]
    public string InnerCityName { get; set; } = "内坊";

    // 外坊名称
    [JsonPropertyName("outer_wards_name")]
    public string OuterWardsName { get; set; } = "附庸坊廓";

    // 地标名称池
    [JsonPropertyName("landmark_names")]
    public List<string> LandmarkNames { get; set; } = [];

    // 坊廓命名池
    [JsonPropertyName("ward_name_pool")]
    public List<string> WardNamePool { get; set; } = [];

    // 城门名称配置
    [JsonPropertyName("gate_names")]
    public PrefectureGateNames GateNames { get; set; } = new();
}

/// <summary>
/// 外域城门名称配置。
/// </summary>
public sealed class PrefectureGateNames
{
    // 北门
    [JsonPropertyName("north")]
    public string North { get; set; } = "北门";

    // 南门
    [JsonPropertyName("south")]
    public string South { get; set; } = "南门";

    // 东门
    [JsonPropertyName("east")]
    public string East { get; set; } = "东门";

    // 西门
    [JsonPropertyName("west")]
    public string West { get; set; } = "西门";
}
