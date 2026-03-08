using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CountyIdle.Models;

public sealed class PrefectureCityThemeConfig
{
    [JsonPropertyName("map_title")]
    public string MapTitle { get; set; } = "江陵府外域（附庸圈层）";

    [JsonPropertyName("city_title")]
    public string CityTitle { get; set; } = "云泽附庸坊城";

    [JsonPropertyName("forest_name")]
    public string ForestName { get; set; } = "青岚灵林";

    [JsonPropertyName("lake_name")]
    public string LakeName { get; set; } = "月魄湖";

    [JsonPropertyName("mountain_name")]
    public string MountainName { get; set; } = "东岭灵脉";

    [JsonPropertyName("farmland_name")]
    public string FarmlandName { get; set; } = "云泽阵材圃";

    [JsonPropertyName("main_avenue_name")]
    public string MainAvenueName { get; set; } = "问道长街";

    [JsonPropertyName("river_gate_name")]
    public string RiverGateName { get; set; } = "云津渡口";

    [JsonPropertyName("inner_city_name")]
    public string InnerCityName { get; set; } = "内坊";

    [JsonPropertyName("outer_wards_name")]
    public string OuterWardsName { get; set; } = "附庸坊廓";

    [JsonPropertyName("landmark_names")]
    public List<string> LandmarkNames { get; set; } = [];

    [JsonPropertyName("ward_name_pool")]
    public List<string> WardNamePool { get; set; } = [];

    [JsonPropertyName("gate_names")]
    public PrefectureGateNames GateNames { get; set; } = new();
}

public sealed class PrefectureGateNames
{
    [JsonPropertyName("north")]
    public string North { get; set; } = "北门";

    [JsonPropertyName("south")]
    public string South { get; set; } = "南门";

    [JsonPropertyName("east")]
    public string East { get; set; } = "东门";

    [JsonPropertyName("west")]
    public string West { get; set; } = "西门";
}
