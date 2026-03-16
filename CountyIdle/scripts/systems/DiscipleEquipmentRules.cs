using System;
using System.Collections.Generic;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public static class DiscipleEquipmentRules
{
    private const int MinGearScore = 8;
    private const int MaxGearScore = 99;

    private static readonly string[] FarmerWeapons =
    [
        "药圃短镰",
        "青木长镰",
        "灵植竹刃"
    ];

    private static readonly string[] WorkerWeapons =
    [
        "承山铁锤",
        "护山长戟",
        "锻机短斧"
    ];

    private static readonly string[] MerchantWeapons =
    [
        "行远轻剑",
        "商路短弓",
        "驭风双匕"
    ];

    private static readonly string[] ScholarWeapons =
    [
        "明衍长剑",
        "符笔",
        "玄墨短刃"
    ];

    private static readonly string[] ReserveWeapons =
    [
        "制式短剑",
        "练习木刀"
    ];

    private static readonly string[] FarmerArmors =
    [
        "灵植布甲",
        "青木护衣"
    ];

    private static readonly string[] WorkerArmors =
    [
        "承山护铠",
        "玄铁护胸"
    ];

    private static readonly string[] MerchantArmors =
    [
        "行商轻甲",
        "御风软甲"
    ];

    private static readonly string[] ScholarArmors =
    [
        "青云道袍",
        "符纹护衣"
    ];

    private static readonly string[] ReserveArmors =
    [
        "制式皮甲",
        "门人护衣"
    ];

    private static readonly string[] RelicPool =
    [
        "护身玉佩",
        "清风铃",
        "灵火珠",
        "玄铁护心镜",
        "守山令符"
    ];

    private static readonly string[] EliteRelicPool =
    [
        "镇峰玉简",
        "青云印",
        "玄阙灵镜"
    ];

    private static readonly string[] TalismanPool =
    [
        "护心符",
        "御风符",
        "镇气符",
        "破邪符",
        "固元符"
    ];

    private static readonly string[] OuterMissionTalismans =
    [
        "御风符",
        "破邪符",
        "疾行符"
    ];

    public static void EnsureDefaults(GameState state)
    {
        state.DiscipleEquipmentProfiles ??= new Dictionary<int, DiscipleEquipmentProfile>();
    }

    public static void EnsureRosterEquipmentProfiles(GameState state)
    {
        EnsureDefaults(state);
        if (state.Population <= 0)
        {
            return;
        }

        _ = DiscipleRosterSystem.BuildRoster(state);
    }

    public static DiscipleEquipmentProfile GetOrCreateEquipmentProfile(
        GameState state,
        int discipleId,
        JobType? jobType,
        DiscipleAgeBand ageBand,
        int realmTier,
        bool isElite,
        DiscipleDirectiveType directiveType)
    {
        EnsureDefaults(state);
        if (state.DiscipleEquipmentProfiles.TryGetValue(discipleId, out var existing))
        {
            return existing;
        }

        var seed = BuildSeed(discipleId, realmTier, isElite, directiveType);
        var profile = BuildEquipmentProfile(seed, state, jobType, ageBand, realmTier, isElite, directiveType);
        state.DiscipleEquipmentProfiles[discipleId] = profile;
        return profile;
    }

    public static DiscipleEquipmentProfile BuildPreviewEquipmentProfile(
        int seed,
        GameState state,
        JobType? jobType,
        DiscipleAgeBand ageBand,
        int realmTier,
        bool isElite,
        DiscipleDirectiveType directiveType)
    {
        return BuildEquipmentProfile(seed, state, jobType, ageBand, realmTier, isElite, directiveType);
    }

    private static DiscipleEquipmentProfile BuildEquipmentProfile(
        int seed,
        GameState state,
        JobType? jobType,
        DiscipleAgeBand ageBand,
        int realmTier,
        bool isElite,
        DiscipleDirectiveType directiveType)
    {
        var baseScore = state.AvgGearScore + (realmTier * 2.4) + (isElite ? 6 : 0);
        baseScore += directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => 3,
            DiscipleDirectiveType.StewardCandidate => 1,
            _ => 0
        };

        if (ageBand == DiscipleAgeBand.Seedling)
        {
            baseScore -= 8;
        }

        var jitter = SampleSigned(seed, 19, 6);
        var score = Math.Clamp((int)Math.Round(baseScore + jitter), MinGearScore, MaxGearScore);
        var qualityTag = ResolveQualityTag(score);
        var weaponName = ResolveWeaponName(jobType, ageBand, seed);
        var armorName = ResolveArmorName(jobType, ageBand, seed);
        var relicName = ResolveRelicName(seed, isElite);
        var talismanName = ResolveTalismanName(seed, ageBand, directiveType);

        return new DiscipleEquipmentProfile(
            weaponName,
            armorName,
            relicName,
            talismanName,
            qualityTag,
            score);
    }

    private static string ResolveWeaponName(JobType? jobType, DiscipleAgeBand ageBand, int seed)
    {
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            return "启蒙木剑";
        }

        var pool = jobType switch
        {
            JobType.Farmer => FarmerWeapons,
            JobType.Worker => WorkerWeapons,
            JobType.Merchant => MerchantWeapons,
            JobType.Scholar => ScholarWeapons,
            _ => ReserveWeapons
        };

        return pool[StableHash(seed, 7) % pool.Length];
    }

    private static string ResolveArmorName(JobType? jobType, DiscipleAgeBand ageBand, int seed)
    {
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            return "启蒙护衣";
        }

        var pool = jobType switch
        {
            JobType.Farmer => FarmerArmors,
            JobType.Worker => WorkerArmors,
            JobType.Merchant => MerchantArmors,
            JobType.Scholar => ScholarArmors,
            _ => ReserveArmors
        };

        return pool[StableHash(seed, 11) % pool.Length];
    }

    private static string ResolveRelicName(int seed, bool isElite)
    {
        if (isElite)
        {
            return EliteRelicPool[StableHash(seed, 13) % EliteRelicPool.Length];
        }

        return RelicPool[StableHash(seed, 13) % RelicPool.Length];
    }

    private static string ResolveTalismanName(int seed, DiscipleAgeBand ageBand, DiscipleDirectiveType directiveType)
    {
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            return "启蒙护符";
        }

        var pool = directiveType == DiscipleDirectiveType.OuterMissionCandidate
            ? OuterMissionTalismans
            : TalismanPool;
        return pool[StableHash(seed, 17) % pool.Length];
    }

    private static string ResolveQualityTag(int score)
    {
        if (score >= 85)
        {
            return "传说";
        }

        if (score >= 72)
        {
            return "史诗";
        }

        if (score >= 58)
        {
            return "精良";
        }

        return "普通";
    }

    private static int BuildSeed(int discipleId, int realmTier, bool isElite, DiscipleDirectiveType directiveType)
    {
        var seed = discipleId * 997 + (realmTier * 71) + ((int)directiveType * 29);
        if (isElite)
        {
            seed += 233;
        }

        return seed;
    }

    private static int SampleSigned(int seed, int salt, int maxAbsoluteValue)
    {
        return (StableHash(seed, salt) % (maxAbsoluteValue * 2 + 1)) - maxAbsoluteValue;
    }

    private static int StableHash(int index, int salt)
    {
        unchecked
        {
            var hash = (index + 1) * 73856093;
            hash ^= (salt + 1) * 19349663;
            hash ^= (hash >> 13);
            hash *= 83492791;
            hash ^= (hash >> 16);
            return hash & int.MaxValue;
        }
    }
}
