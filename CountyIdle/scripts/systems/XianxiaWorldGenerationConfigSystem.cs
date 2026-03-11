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
        NormalizeRuleProfiles(config);
    }

    private static XianxiaWorldGenerationConfig BuildFallbackConfig()
    {
        return new XianxiaWorldGenerationConfig();
    }

    private static void NormalizeRuleProfiles(XianxiaWorldGenerationConfig config)
    {
        config.RegionProfiles ??= [];
        config.PrimaryTypeSpawnRules ??= [];
        config.SecondaryTagSpawnRules ??= [];
        config.AdjacencyWeightRules ??= [];
        config.RarityProfiles ??= [];
        config.UnlockRules ??= [];
        config.CompanionSpawnRules ??= [];

        foreach (var region in config.RegionProfiles)
        {
            region.RegionId = region.RegionId?.Trim() ?? string.Empty;
            region.DisplayName = region.DisplayName?.Trim() ?? region.RegionId;
            region.CoverageWeight = Mathf.Clamp(region.CoverageWeight, 0f, 1f);
            NormalizeRange(region.SpiritualDensityRange, 0f, 1f);
            NormalizeRange(region.RoadDensityRange, 0f, 1f);
            NormalizeRange(region.ThreatBaseline, 0f, 1f);
            NormalizeWeightedValues(region.PrimaryTypeBias);
            region.RuinBias = Mathf.Clamp(region.RuinBias, 0f, 2f);
            region.MarketBias = Mathf.Clamp(region.MarketBias, 0f, 2f);
            region.UnlockTier = Math.Clamp(region.UnlockTier, 0, 2);
        }

        foreach (var rule in config.PrimaryTypeSpawnRules)
        {
            rule.PrimaryType = rule.PrimaryType?.Trim() ?? string.Empty;
            rule.BaseWeight = Mathf.Clamp(rule.BaseWeight, 0f, 4f);
            NormalizeWeightedValues(rule.RegionWeightMultiplier);
            NormalizeWeightedValues(rule.TerrainWeightMultiplier);
            NormalizeCurve(rule.SpiritualWeightCurve);
            NormalizeCurve(rule.RoadWeightCurve);
            NormalizeCurve(rule.ThreatWeightCurve);
            rule.MinHexDistance = Math.Clamp(rule.MinHexDistance, 0, 32);
            rule.SoftCapPerRegion = Math.Clamp(rule.SoftCapPerRegion, 0, 64);
            rule.GlobalCap = Math.Clamp(rule.GlobalCap, 0, 128);
            rule.UnlockTier = Math.Clamp(rule.UnlockTier, 0, 2);
            rule.VisibilityTier = Math.Clamp(rule.VisibilityTier, 0, 2);
        }

        foreach (var rule in config.SecondaryTagSpawnRules)
        {
            rule.PrimaryType = rule.PrimaryType?.Trim() ?? string.Empty;
            rule.SecondaryTag = rule.SecondaryTag?.Trim() ?? string.Empty;
            rule.BaseWeight = Mathf.Clamp(rule.BaseWeight, 0f, 4f);
            NormalizeWeightedValues(rule.RegionBias);
            NormalizeWeightedValues(rule.TerrainBias);
            rule.RequiresAdjacency ??= [];
            rule.AvoidsAdjacency ??= [];
            rule.UnlockTier = Math.Clamp(rule.UnlockTier, 0, 2);
            rule.RarityTier = string.IsNullOrWhiteSpace(rule.RarityTier) ? "Common" : rule.RarityTier.Trim();
        }

        foreach (var rule in config.AdjacencyWeightRules)
        {
            rule.SourceType = rule.SourceType?.Trim() ?? string.Empty;
            rule.TargetType = rule.TargetType?.Trim() ?? string.Empty;
            rule.WeightDelta = Mathf.Clamp(rule.WeightDelta, -2f, 2f);
            rule.Radius = Math.Clamp(rule.Radius, 1, 8);
            rule.RuleMode = string.IsNullOrWhiteSpace(rule.RuleMode) ? "Attract" : rule.RuleMode.Trim();
        }

        foreach (var profile in config.RarityProfiles)
        {
            profile.RarityTier = string.IsNullOrWhiteSpace(profile.RarityTier) ? "Common" : profile.RarityTier.Trim();
            profile.SpawnMultiplier = Mathf.Clamp(profile.SpawnMultiplier, 0f, 2f);
            profile.FogPriority = Math.Clamp(profile.FogPriority, 0, 100);
            NormalizeRange(profile.DiscoveryHintChance, 0f, 1f);
        }

        foreach (var rule in config.UnlockRules)
        {
            rule.UnlockTier = Math.Clamp(rule.UnlockTier, 0, 2);
            NormalizeRange(rule.MinSectReputation, 0f, 999f);
            NormalizeRange(rule.MinExpeditionDepth, 0f, 999f);
            NormalizeRange(rule.MinHeroPower, 0f, 99999f);
            rule.RequiredRumorTags ??= [];
            rule.RequiredFactionRelation ??= [];
        }

        foreach (var rule in config.CompanionSpawnRules)
        {
            rule.HostType = rule.HostType?.Trim() ?? string.Empty;
            rule.HostTag = rule.HostTag?.Trim() ?? string.Empty;
            rule.CompanionType = rule.CompanionType?.Trim() ?? string.Empty;
            rule.CompanionTag = rule.CompanionTag?.Trim() ?? string.Empty;
            NormalizeRange(rule.SpawnChance, 0f, 1f);
            rule.MinDistanceFromHost = Math.Clamp(rule.MinDistanceFromHost, 0, 8);
            rule.MaxDistanceFromHost = Math.Clamp(rule.MaxDistanceFromHost, rule.MinDistanceFromHost, 12);
        }
    }

    private static void NormalizeWeightedValues(System.Collections.Generic.List<WeightedStringValue>? values)
    {
        if (values == null)
        {
            return;
        }

        foreach (var entry in values)
        {
            entry.Key = entry.Key?.Trim() ?? string.Empty;
            entry.Weight = Mathf.Clamp(entry.Weight, 0f, 4f);
        }
    }

    private static void NormalizeRange(FloatRange? range, float minLimit, float maxLimit)
    {
        if (range == null)
        {
            return;
        }

        range.Min = Mathf.Clamp(range.Min, minLimit, maxLimit);
        range.Max = Mathf.Clamp(range.Max, range.Min, maxLimit);
    }

    private static void NormalizeCurve(SpawnWeightCurve? curve)
    {
        if (curve == null)
        {
            return;
        }

        curve.Low = Mathf.Clamp(curve.Low, 0f, 4f);
        curve.Mid = Mathf.Clamp(curve.Mid, 0f, 4f);
        curve.High = Mathf.Clamp(curve.High, 0f, 4f);
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
            SectMonsterThreatPenalty = source.SectMonsterThreatPenalty,
            RegionProfiles = CloneRegionProfiles(source.RegionProfiles),
            PrimaryTypeSpawnRules = ClonePrimaryTypeSpawnRules(source.PrimaryTypeSpawnRules),
            SecondaryTagSpawnRules = CloneSecondaryTagSpawnRules(source.SecondaryTagSpawnRules),
            AdjacencyWeightRules = CloneAdjacencyWeightRules(source.AdjacencyWeightRules),
            RarityProfiles = CloneRarityProfiles(source.RarityProfiles),
            UnlockRules = CloneUnlockRules(source.UnlockRules),
            CompanionSpawnRules = CloneCompanionSpawnRules(source.CompanionSpawnRules)
        };
    }

    private static System.Collections.Generic.List<WorldRegionProfile> CloneRegionProfiles(System.Collections.Generic.List<WorldRegionProfile>? source)
    {
        var result = new System.Collections.Generic.List<WorldRegionProfile>();
        if (source == null)
        {
            return result;
        }

        foreach (var region in source)
        {
            result.Add(new WorldRegionProfile
            {
                RegionId = region.RegionId,
                DisplayName = region.DisplayName,
                CoverageWeight = region.CoverageWeight,
                TerrainAffinity = [.. region.TerrainAffinity],
                SpiritualDensityRange = CloneRange(region.SpiritualDensityRange),
                RoadDensityRange = CloneRange(region.RoadDensityRange),
                ThreatBaseline = CloneRange(region.ThreatBaseline),
                PrimaryTypeBias = CloneWeightedValues(region.PrimaryTypeBias),
                RuinBias = region.RuinBias,
                MarketBias = region.MarketBias,
                UnlockTier = region.UnlockTier
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WorldPrimaryTypeSpawnRule> ClonePrimaryTypeSpawnRules(System.Collections.Generic.List<WorldPrimaryTypeSpawnRule>? source)
    {
        var result = new System.Collections.Generic.List<WorldPrimaryTypeSpawnRule>();
        if (source == null)
        {
            return result;
        }

        foreach (var rule in source)
        {
            result.Add(new WorldPrimaryTypeSpawnRule
            {
                PrimaryType = rule.PrimaryType,
                BaseWeight = rule.BaseWeight,
                RegionWeightMultiplier = CloneWeightedValues(rule.RegionWeightMultiplier),
                TerrainWeightMultiplier = CloneWeightedValues(rule.TerrainWeightMultiplier),
                SpiritualWeightCurve = CloneCurve(rule.SpiritualWeightCurve),
                RoadWeightCurve = CloneCurve(rule.RoadWeightCurve),
                ThreatWeightCurve = CloneCurve(rule.ThreatWeightCurve),
                MinHexDistance = rule.MinHexDistance,
                SoftCapPerRegion = rule.SoftCapPerRegion,
                GlobalCap = rule.GlobalCap,
                UnlockTier = rule.UnlockTier,
                VisibilityTier = rule.VisibilityTier
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WorldSecondaryTagSpawnRule> CloneSecondaryTagSpawnRules(System.Collections.Generic.List<WorldSecondaryTagSpawnRule>? source)
    {
        var result = new System.Collections.Generic.List<WorldSecondaryTagSpawnRule>();
        if (source == null)
        {
            return result;
        }

        foreach (var rule in source)
        {
            result.Add(new WorldSecondaryTagSpawnRule
            {
                PrimaryType = rule.PrimaryType,
                SecondaryTag = rule.SecondaryTag,
                BaseWeight = rule.BaseWeight,
                RegionBias = CloneWeightedValues(rule.RegionBias),
                TerrainBias = CloneWeightedValues(rule.TerrainBias),
                RequiresAdjacency = [.. rule.RequiresAdjacency],
                AvoidsAdjacency = [.. rule.AvoidsAdjacency],
                UnlockTier = rule.UnlockTier,
                RarityTier = rule.RarityTier,
                CanCompanionSpawn = rule.CanCompanionSpawn
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WorldAdjacencyWeightRule> CloneAdjacencyWeightRules(System.Collections.Generic.List<WorldAdjacencyWeightRule>? source)
    {
        var result = new System.Collections.Generic.List<WorldAdjacencyWeightRule>();
        if (source == null)
        {
            return result;
        }

        foreach (var rule in source)
        {
            result.Add(new WorldAdjacencyWeightRule
            {
                SourceType = rule.SourceType,
                TargetType = rule.TargetType,
                WeightDelta = rule.WeightDelta,
                Radius = rule.Radius,
                RuleMode = rule.RuleMode
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WorldRarityProfile> CloneRarityProfiles(System.Collections.Generic.List<WorldRarityProfile>? source)
    {
        var result = new System.Collections.Generic.List<WorldRarityProfile>();
        if (source == null)
        {
            return result;
        }

        foreach (var profile in source)
        {
            result.Add(new WorldRarityProfile
            {
                RarityTier = profile.RarityTier,
                SpawnMultiplier = profile.SpawnMultiplier,
                RevealByDefault = profile.RevealByDefault,
                FogPriority = profile.FogPriority,
                DiscoveryHintChance = CloneRange(profile.DiscoveryHintChance)
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WorldUnlockRule> CloneUnlockRules(System.Collections.Generic.List<WorldUnlockRule>? source)
    {
        var result = new System.Collections.Generic.List<WorldUnlockRule>();
        if (source == null)
        {
            return result;
        }

        foreach (var rule in source)
        {
            result.Add(new WorldUnlockRule
            {
                UnlockTier = rule.UnlockTier,
                MinSectReputation = CloneRange(rule.MinSectReputation),
                MinExpeditionDepth = CloneRange(rule.MinExpeditionDepth),
                MinHeroPower = CloneRange(rule.MinHeroPower),
                RequiredRumorTags = [.. rule.RequiredRumorTags],
                RequiredFactionRelation = [.. rule.RequiredFactionRelation]
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WorldCompanionSpawnRule> CloneCompanionSpawnRules(System.Collections.Generic.List<WorldCompanionSpawnRule>? source)
    {
        var result = new System.Collections.Generic.List<WorldCompanionSpawnRule>();
        if (source == null)
        {
            return result;
        }

        foreach (var rule in source)
        {
            result.Add(new WorldCompanionSpawnRule
            {
                HostType = rule.HostType,
                HostTag = rule.HostTag,
                CompanionType = rule.CompanionType,
                CompanionTag = rule.CompanionTag,
                SpawnChance = CloneRange(rule.SpawnChance),
                MinDistanceFromHost = rule.MinDistanceFromHost,
                MaxDistanceFromHost = rule.MaxDistanceFromHost
            });
        }

        return result;
    }

    private static System.Collections.Generic.List<WeightedStringValue> CloneWeightedValues(System.Collections.Generic.List<WeightedStringValue>? source)
    {
        var result = new System.Collections.Generic.List<WeightedStringValue>();
        if (source == null)
        {
            return result;
        }

        foreach (var entry in source)
        {
            result.Add(new WeightedStringValue
            {
                Key = entry.Key,
                Weight = entry.Weight
            });
        }

        return result;
    }

    private static FloatRange CloneRange(FloatRange? source)
    {
        if (source == null)
        {
            return new FloatRange();
        }

        return new FloatRange
        {
            Min = source.Min,
            Max = source.Max
        };
    }

    private static SpawnWeightCurve CloneCurve(SpawnWeightCurve? source)
    {
        if (source == null)
        {
            return new SpawnWeightCurve();
        }

        return new SpawnWeightCurve
        {
            Low = source.Low,
            Mid = source.Mid,
            High = source.High
        };
    }
}
