using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public readonly record struct SectNameEntry(string Key, string DefaultName, string Group, string Label);

public static class SectNamingRules
{
    public const int MaxNameLength = 12;

    public const string SectNameKey = "sect.name";

    public const string PeakQingyunKey = "peak.qingyun";
    public const string PeakTianyanKey = "peak.tianyan";
    public const string PeakTianshuKey = "peak.tianshu";
    public const string PeakTianjiKey = "peak.tianji";
    public const string PeakTiangongKey = "peak.tiangong";
    public const string PeakTianquanKey = "peak.tianquan";
    public const string PeakTianyuanKey = "peak.tianyuan";
    public const string PeakTianhengKey = "peak.tianheng";
    public const string PeakZhenyueKey = "peak.zhenyue";
    public const string PeakDandingKey = "peak.danding";
    public const string PeakShengongKey = "peak.shengong";
    public const string PeakYulingKey = "peak.yuling";
    public const string PeakCangjianKey = "peak.cangjian";
    public const string PeakWanfengKey = "peak.wanfeng";
    public const string PeakWuyingKey = "peak.wuying";
    public const string PeakYanxiaKey = "peak.yanxia";
    public const string PeakPillarKey = "peak.pillar";

    public const string HallInnerAffairsKey = "hall.inner_affairs";
    public const string HallExternalAffairsKey = "hall.external_affairs";
    public const string HallTransmissionKey = "hall.transmission";
    public const string HallFormationKey = "hall.formation";
    public const string HallAffairsKey = "hall.affairs";
    public const string HallAcademyKey = "hall.academy";
    public const string HallMarketKey = "hall.market";
    public const string HallFormationFieldKey = "hall.formation_field";
    public const string HallPuppetWorkshopKey = "hall.puppet_workshop";
    public const string HallLeisureKey = "hall.leisure";
    public const string HallSeedlingKey = "hall.seedling";
    public const string HallOuterDutyKey = "hall.outer_duty";
    public const string HallGeneralKey = "hall.general";
    public const string HallArrayLabKey = "hall.array_lab";
    public const string HallTheoryKey = "hall.theory";
    public const string HallArchiveKey = "hall.archive";
    public const string HallSimulationKey = "hall.simulation";
    public const string HallGeneralAffairsKey = "hall.general_affairs";
    public const string HallDiplomacyKey = "hall.diplomacy";
    public const string HallLectureKey = "hall.lecture";
    public const string HallMarketSquareKey = "hall.market_square";
    public const string HallSkyHarborKey = "hall.sky_harbor";
    public const string HallMatrixDockKey = "hall.matrix_dock";
    public const string HallStorehouseKey = "hall.storehouse";
    public const string HallResourceVaultKey = "hall.resource_vault";
    public const string HallSpiritBankKey = "hall.spirit_bank";
    public const string HallResearchKey = "hall.research";
    public const string HallLibraryKey = "hall.library";
    public const string HallTeachingInstituteKey = "hall.teaching_institute";
    public const string HallTrialKey = "hall.trial";
    public const string HallForgeKey = "hall.forge";
    public const string HallArrayBaseKey = "hall.array_base";
    public const string HallResourceDispatchKey = "hall.resource_dispatch";
    public const string HallBearMountainKey = "hall.bear_mountain";
    public const string HallBreakArrayKey = "hall.break_array";
    public const string HallDecreeKey = "hall.decree";
    public const string HallLawKey = "hall.law";
    public const string HallDrillKey = "hall.drill";
    public const string HallBathKey = "hall.bath";
    public const string HallCampKey = "hall.camp";
    public const string HallTacticsKey = "hall.tactics";
    public const string HallSigilKey = "hall.sigil";
    public const string HallTestKey = "hall.test";
    public const string HallReliefKey = "hall.relief";
    public const string HallBeastKey = "hall.beast";
    public const string HallMusicKey = "hall.music";
    public const string HallElixirKey = "hall.elixir";
    public const string HallHerbKey = "hall.herb";
    public const string HallBeastNurseryKey = "hall.beast_nursery";
    public const string HallMachineryKey = "hall.machinery";
    public const string HallMelodyKey = "hall.melody";
    public const string HallResonanceKey = "hall.resonance";
    public const string HallShadowKey = "hall.shadow";
    public const string HallDisciplineKey = "hall.discipline";
    public const string HallGuardKey = "hall.guard";
    public const string HallIntelKey = "hall.intel";
    public const string HallBodyKey = "hall.body";
    public const string HallPillKey = "hall.pill";
    public const string HallArtifactKey = "hall.artifact";
    public const string HallBeastWingKey = "hall.beast_wing";
    public const string HallSwordKey = "hall.sword";
    public const string HallSpellKey = "hall.spell";
    public const string HallShadowWingKey = "hall.shadow_wing";
    public const string HallToneKey = "hall.tone";

    private static readonly IReadOnlyList<SectNameEntry> Entries =
    [
        new(SectNameKey, "浮云宗", "宗门", "宗门名"),

        new(PeakQingyunKey, "青云峰", "峰脉", "青云峰"),
        new(PeakTianyanKey, "天衍峰", "峰脉", "天衍峰"),
        new(PeakTianshuKey, "天枢峰", "峰脉", "天枢峰"),
        new(PeakTianjiKey, "天机峰", "峰脉", "天机峰"),
        new(PeakTiangongKey, "天工峰", "峰脉", "天工峰"),
        new(PeakTianquanKey, "天权峰", "峰脉", "天权峰"),
        new(PeakTianyuanKey, "天元峰", "峰脉", "天元峰"),
        new(PeakTianhengKey, "天衡峰", "峰脉", "天衡峰"),
        new(PeakZhenyueKey, "镇岳峰", "峰脉", "镇岳峰"),
        new(PeakDandingKey, "丹鼎峰", "峰脉", "丹鼎峰"),
        new(PeakShengongKey, "神工峰", "峰脉", "神工峰"),
        new(PeakYulingKey, "御灵峰", "峰脉", "御灵峰"),
        new(PeakCangjianKey, "藏剑峰", "峰脉", "藏剑峰"),
        new(PeakWanfengKey, "万法峰", "峰脉", "万法峰"),
        new(PeakWuyingKey, "无影峰", "峰脉", "无影峰"),
        new(PeakYanxiaKey, "烟霞峰", "峰脉", "烟霞峰"),
        new(PeakPillarKey, "其余支柱峰", "峰脉", "其余支柱峰"),

        new(HallInnerAffairsKey, "内务总殿", "堂口", "内务总殿"),
        new(HallExternalAffairsKey, "外事总殿", "堂口", "外事总殿"),
        new(HallTransmissionKey, "传功总殿", "堂口", "传功总殿"),
        new(HallFormationKey, "阵堂", "堂口", "阵堂"),
        new(HallAffairsKey, "庶务殿", "堂口", "庶务殿"),
        new(HallAcademyKey, "传法院", "堂口", "传法院"),
        new(HallMarketKey, "青云总坊", "堂口", "青云总坊"),
        new(HallFormationFieldKey, "阵材圃", "堂口", "阵材圃"),
        new(HallPuppetWorkshopKey, "傀儡工坊", "堂口", "傀儡工坊"),
        new(HallLeisureKey, "演阵台", "堂口", "演阵台"),
        new(HallSeedlingKey, "启蒙院", "堂口", "启蒙院"),
        new(HallOuterDutyKey, "外门轮值司", "堂口", "外门轮值司"),
        new(HallGeneralKey, "总枢殿", "堂口", "总枢殿"),
        new(HallArrayLabKey, "阵研阁", "堂口", "阵研阁"),
        new(HallTheoryKey, "理法堂", "堂口", "理法堂"),
        new(HallArchiveKey, "图录堂", "堂口", "图录堂"),
        new(HallSimulationKey, "推演堂", "堂口", "推演堂"),
        new(HallGeneralAffairsKey, "总务殿", "堂口", "总务殿"),
        new(HallDiplomacyKey, "鸿胪司", "堂口", "鸿胪司"),
        new(HallLectureKey, "讲法院", "堂口", "讲法院"),
        new(HallMarketSquareKey, "阵市", "堂口", "阵市"),
        new(HallSkyHarborKey, "浮空渡口", "堂口", "浮空渡口"),
        new(HallMatrixDockKey, "矩阵穿梭舟总站", "堂口", "矩阵穿梭舟总站"),
        new(HallStorehouseKey, "玄仓坪", "堂口", "玄仓坪"),
        new(HallResourceVaultKey, "资源总库", "堂口", "资源总库"),
        new(HallSpiritBankKey, "灵石钱庄", "堂口", "灵石钱庄"),
        new(HallResearchKey, "衍法阁", "堂口", "衍法阁"),
        new(HallLibraryKey, "藏书阁", "堂口", "藏书阁"),
        new(HallTeachingInstituteKey, "传功总院", "堂口", "传功总院"),
        new(HallTrialKey, "试炼幻境", "堂口", "试炼幻境"),
        new(HallForgeKey, "铸机阁", "堂口", "铸机阁"),
        new(HallArrayBaseKey, "阵基殿", "堂口", "阵基殿"),
        new(HallResourceDispatchKey, "资源调配司", "堂口", "资源调配司"),
        new(HallBearMountainKey, "承山堂", "堂口", "承山堂"),
        new(HallBreakArrayKey, "破阵营", "堂口", "破阵营"),
        new(HallDecreeKey, "天谕殿", "堂口", "天谕殿"),
        new(HallLawKey, "执法总堂", "堂口", "执法总堂"),
        new(HallDrillKey, "演武场", "堂口", "演武场"),
        new(HallBathKey, "药浴房", "堂口", "药浴房"),
        new(HallCampKey, "营部", "堂口", "营部"),
        new(HallTacticsKey, "战术阁", "堂口", "战术阁"),
        new(HallSigilKey, "制符坊", "堂口", "制符坊"),
        new(HallTestKey, "试法场", "堂口", "试法场"),
        new(HallReliefKey, "济世堂", "堂口", "济世堂"),
        new(HallBeastKey, "百兽苑", "堂口", "百兽苑"),
        new(HallMusicKey, "清音阁", "堂口", "清音阁"),
        new(HallElixirKey, "丹心庐", "堂口", "丹心庐"),
        new(HallHerbKey, "百草园", "堂口", "百草园"),
        new(HallBeastNurseryKey, "育兽房", "堂口", "育兽房"),
        new(HallMachineryKey, "机巧房", "堂口", "机巧房"),
        new(HallMelodyKey, "天籁居", "堂口", "天籁居"),
        new(HallResonanceKey, "鸣音坊", "堂口", "鸣音坊"),
        new(HallShadowKey, "暗部", "堂口", "暗部"),
        new(HallDisciplineKey, "风纪堂", "堂口", "风纪堂"),
        new(HallGuardKey, "影卫堂", "堂口", "影卫堂"),
        new(HallIntelKey, "天机阁", "堂口", "天机阁"),
        new(HallBodyKey, "炼体堂", "堂口", "炼体堂"),
        new(HallPillKey, "丹堂", "堂口", "丹堂"),
        new(HallArtifactKey, "器堂", "堂口", "器堂"),
        new(HallBeastWingKey, "灵兽堂", "堂口", "灵兽堂"),
        new(HallSwordKey, "剑堂", "堂口", "剑堂"),
        new(HallSpellKey, "法堂", "堂口", "法堂"),
        new(HallShadowWingKey, "影堂", "堂口", "影堂"),
        new(HallToneKey, "天音堂", "堂口", "天音堂")
    ];

    private static readonly IReadOnlyDictionary<string, string> DefaultNameLookup =
        Entries.ToDictionary(static entry => entry.Key, static entry => entry.DefaultName);

    private static readonly IReadOnlyList<SectNameEntry> ReplaceEntries =
        Entries.OrderByDescending(static entry => entry.DefaultName.Length).ToArray();

    public static IReadOnlyList<SectNameEntry> GetEntries()
    {
        return Entries;
    }

    public static IReadOnlyDictionary<string, string> BuildDefaultNameMap()
    {
        return Entries.ToDictionary(static entry => entry.Key, static entry => entry.DefaultName);
    }

    public static void EnsureDefaults(GameState state)
    {
        if (state.SectNameMap == null)
        {
            state.SectNameMap = new Dictionary<string, string>();
        }

        foreach (var entry in Entries)
        {
            if (!state.SectNameMap.TryGetValue(entry.Key, out var current) || string.IsNullOrWhiteSpace(current))
            {
                state.SectNameMap[entry.Key] = entry.DefaultName;
            }
        }
    }

    public static string GetDefaultName(string key)
    {
        return DefaultNameLookup.TryGetValue(key, out var name) ? name : key;
    }

    public static string GetName(GameState state, string key)
    {
        EnsureDefaults(state);
        return GetName(state.SectNameMap, key);
    }

    public static string GetName(IReadOnlyDictionary<string, string>? nameMap, string key)
    {
        if (nameMap != null &&
            nameMap.TryGetValue(key, out var name) &&
            !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return GetDefaultName(key);
    }

    public static string GetCompactName(IReadOnlyDictionary<string, string>? nameMap, string key, string defaultCompact)
    {
        var defaultName = GetDefaultName(key);
        var name = GetName(nameMap, key);
        return string.Equals(name, defaultName, StringComparison.Ordinal) ? defaultCompact : name;
    }

    public static string SanitizeName(string? rawValue)
    {
        var trimmed = (rawValue ?? string.Empty).Trim();
        trimmed = trimmed.Replace("\r", string.Empty).Replace("\n", string.Empty);
        if (trimmed.Length > MaxNameLength)
        {
            trimmed = trimmed[..MaxNameLength];
        }

        return trimmed;
    }

    public static void ApplyNames(GameState state, IReadOnlyDictionary<string, string> entries)
    {
        EnsureDefaults(state);
        foreach (var entry in entries)
        {
            var sanitized = SanitizeName(entry.Value);
            state.SectNameMap[entry.Key] = string.IsNullOrWhiteSpace(sanitized)
                ? GetDefaultName(entry.Key)
                : sanitized;
        }
    }

    public static string ReplaceKnownNames(GameState state, string text)
    {
        return ReplaceKnownNames(state?.SectNameMap, text);
    }

    public static string ReplaceKnownNames(IReadOnlyDictionary<string, string>? nameMap, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var output = text;
        foreach (var entry in ReplaceEntries)
        {
            var defaultName = entry.DefaultName;
            var customName = GetName(nameMap, entry.Key);
            if (string.Equals(defaultName, customName, StringComparison.Ordinal))
            {
                continue;
            }

            output = output.Replace(defaultName, customName, StringComparison.Ordinal);
        }

        return output;
    }
}
