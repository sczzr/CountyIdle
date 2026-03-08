using System;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed class XianxiaWorldGenerationConfigSystem
{
    private const string ConfigPath = "res://data/xianxia_world_generation.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static XianxiaWorldGenerationConfig? _cachedConfig;

    public XianxiaWorldGenerationConfig GetConfig()
    {
        if (_cachedConfig != null)
        {
            return Clone(_cachedConfig);
        }

        _cachedConfig = TryLoadConfig() ?? BuildFallbackConfig();
        Normalize(_cachedConfig);
        return Clone(_cachedConfig);
    }

    private static XianxiaWorldGenerationConfig? TryLoadConfig()
    {
        if (!FileAccess.FileExists(ConfigPath))
        {
            GD.PushWarning($"Xianxia world generation config not found at {ConfigPath}, fallback will be used.");
            return null;
        }

        try
        {
            using var file = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            return JsonSerializer.Deserialize<XianxiaWorldGenerationConfig>(json, JsonOptions);
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Xianxia world generation config load failed: {exception.Message}, fallback will be used.");
            return null;
        }
    }

    private static void Normalize(XianxiaWorldGenerationConfig config)
    {
        config.Width = Math.Clamp(config.Width, 24, 128);
        config.Height = Math.Clamp(config.Height, 16, 96);
        config.GridLines = Math.Clamp(config.GridLines, 6, 16);
        config.UnitScale = Mathf.Clamp(config.UnitScale, 0.28f, 0.62f);
        config.MountainRangeCountMin = Math.Clamp(config.MountainRangeCountMin, 1, 12);
        config.MountainRangeCountMax = Math.Clamp(config.MountainRangeCountMax, config.MountainRangeCountMin, 16);
        config.RiverSourceCountMin = Math.Clamp(config.RiverSourceCountMin, 1, 18);
        config.RiverSourceCountMax = Math.Clamp(config.RiverSourceCountMax, config.RiverSourceCountMin, 20);
        config.MajorDragonVeinCountMin = Math.Clamp(config.MajorDragonVeinCountMin, 1, 8);
        config.MajorDragonVeinCountMax = Math.Clamp(config.MajorDragonVeinCountMax, config.MajorDragonVeinCountMin, 10);
        config.MinorDragonVeinCountMin = Math.Clamp(config.MinorDragonVeinCountMin, 1, 16);
        config.MinorDragonVeinCountMax = Math.Clamp(config.MinorDragonVeinCountMax, config.MinorDragonVeinCountMin, 20);
        config.WonderCountMin = Math.Clamp(config.WonderCountMin, 1, 20);
        config.WonderCountMax = Math.Clamp(config.WonderCountMax, config.WonderCountMin, 24);
        config.SectCandidateCount = Math.Clamp(config.SectCandidateCount, 3, 24);
        config.SettlementCount = Math.Clamp(config.SettlementCount, 2, 24);
        config.RuinCount = Math.Clamp(config.RuinCount, 2, 24);
        config.BaseTemperature = Mathf.Clamp(config.BaseTemperature, 0.10f, 0.90f);
        config.BaseMoisture = Mathf.Clamp(config.BaseMoisture, 0.10f, 0.90f);
        config.CliffThreshold = Math.Clamp(config.CliffThreshold, 6, 36);
        config.LakeThreshold = Mathf.Clamp(config.LakeThreshold, 0.08f, 0.40f);
        config.WorldTitle = string.IsNullOrWhiteSpace(config.WorldTitle) ? SectMapSemanticRules.GetWorldMapTitle() : config.WorldTitle;
    }

    private static XianxiaWorldGenerationConfig BuildFallbackConfig()
    {
        return new XianxiaWorldGenerationConfig();
    }

    private static XianxiaWorldGenerationConfig Clone(XianxiaWorldGenerationConfig source)
    {
        return new XianxiaWorldGenerationConfig
        {
            Seed = source.Seed,
            WorldTitle = source.WorldTitle,
            Width = source.Width,
            Height = source.Height,
            GridLines = source.GridLines,
            UnitScale = source.UnitScale,
            MountainRangeCountMin = source.MountainRangeCountMin,
            MountainRangeCountMax = source.MountainRangeCountMax,
            RiverSourceCountMin = source.RiverSourceCountMin,
            RiverSourceCountMax = source.RiverSourceCountMax,
            MajorDragonVeinCountMin = source.MajorDragonVeinCountMin,
            MajorDragonVeinCountMax = source.MajorDragonVeinCountMax,
            MinorDragonVeinCountMin = source.MinorDragonVeinCountMin,
            MinorDragonVeinCountMax = source.MinorDragonVeinCountMax,
            WonderCountMin = source.WonderCountMin,
            WonderCountMax = source.WonderCountMax,
            SectCandidateCount = source.SectCandidateCount,
            SettlementCount = source.SettlementCount,
            RuinCount = source.RuinCount,
            FloatingIslesEnabled = source.FloatingIslesEnabled,
            CorruptionEnabled = source.CorruptionEnabled,
            QiStormsEnabled = source.QiStormsEnabled,
            BaseTemperature = source.BaseTemperature,
            BaseMoisture = source.BaseMoisture,
            CliffThreshold = source.CliffThreshold,
            LakeThreshold = source.LakeThreshold,
            SectQiWeight = source.SectQiWeight,
            SectResourceWeight = source.SectResourceWeight,
            SectDefensibilityWeight = source.SectDefensibilityWeight,
            SectWaterAccessWeight = source.SectWaterAccessWeight,
            SectWonderWeight = source.SectWonderWeight,
            SectConnectivityWeight = source.SectConnectivityWeight,
            SectFertilityWeight = source.SectFertilityWeight,
            SectCorruptionPenalty = source.SectCorruptionPenalty,
            SectMonsterThreatPenalty = source.SectMonsterThreatPenalty
        };
    }
}
