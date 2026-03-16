using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

// 弟子名册生成与属性推导
public static class DiscipleRosterSystem
{
    // 一天对应的游戏分钟数
    private const int MinutesPerDay = 24 * 60;
    // 随机索引上限
    private const int RandomIndexUpperBound = 1_000_000;
    // 随机命名重试次数
    private const int RandomNameRetryLimit = 24;
    // 默认精英比例
    private const double DefaultEliteRate = 0.08;

    // 姓氏库
    private static readonly string[] Surnames =
    [
        "沈", "陆", "顾", "程", "许", "叶", "秦", "宁", "苏", "白", "韩", "林", "谢", "周", "温", "岑"
    ];

    // 名字首字库
    private static readonly string[] NameFirstChars =
    [
        "清", "玄", "景", "怀", "知", "云", "星", "若", "承", "照", "闻", "归", "静", "砚", "昭", "岚"
    ];

    // 名字次字库
    private static readonly string[] NameSecondChars =
    [
        "尘", "音", "川", "岳", "宁", "遥", "衡", "川", "晖", "岑", "澜", "松", "辞", "舟", "珩", "岐"
    ];

    // 通用性格词条
    private static readonly string[] CommonTraits =
    [
        "守纪稳当", "脚程轻快", "记事清楚", "耐心细密", "应变从容", "心气安定", "擅长配合", "能守长线"
    ];

    // 农务性格词条
    private static readonly string[] FarmerTraits =
    [
        "识地脉", "善照料灵植", "熟悉阵材库位", "晨课后耐久轮值", "擅看天候"
    ];

    // 工务性格词条
    private static readonly string[] WorkerTraits =
    [
        "阵基手稳", "擅修缮护山构件", "懂工序衔接", "能压住锻造节奏", "轮值响应快"
    ];

    // 商务性格词条
    private static readonly string[] MerchantTraits =
    [
        "记账快", "善交涉", "熟悉商路", "能稳住外事节奏", "识别供需差价"
    ];

    // 学术性格词条
    private static readonly string[] ScholarTraits =
    [
        "推演耐久", "静心久坐", "擅整理典册", "善总结阵图", "记忆力稳"
    ];

    // 预备性格词条
    private static readonly string[] ReserveTraits =
    [
        "轮值灵活", "服从度高", "乐于补位", "基础均衡", "适应不同堂口"
    ];

    // 构建当前宗门弟子名册
    public static IReadOnlyList<DiscipleProfile> BuildRoster(GameState sourceState)
    {
        var state = sourceState.Clone();
        PopulationRules.EnsureDefaults(state);
        SectGovernanceRules.EnsureDefaults(state);
        DiscipleDirectiveRules.EnsureDefaults(state);
        DiscipleEquipmentRules.EnsureDefaults(sourceState);

        var population = Math.Max(state.Population, 0);
        if (population <= 0)
        {
            return Array.Empty<DiscipleProfile>();
        }

        var jobAssignments = BuildJobAssignments(state, population);
        var ageBands = BuildAgeAssignments(state, population);
        var eliteSet = BuildEliteSet(state, population);
        var minuteOfDay = Modulo(state.GameMinutes, MinutesPerDay);
        var talentPlan = SectGovernanceRules.GetActiveTalentPlan(state);
        var direction = SectGovernanceRules.GetActiveDevelopmentDirection(state);
        var law = SectGovernanceRules.GetActiveLaw(state);

        var roster = new List<DiscipleProfile>(population);
        for (var index = 0; index < population; index++)
        {
            var jobType = jobAssignments[index];
            var ageBand = ageBands[index];
            var isElite = eliteSet.Contains(index);
            var directiveType = DiscipleDirectiveRules.GetDirective(state, index + 1);
            var age = ResolveAge(index, ageBand);
            var health = ResolveHealth(index, state, ageBand, isElite);
            var mood = ResolveMood(index, state, law);
            var potential = ResolvePotential(index, state, jobType, isElite, talentPlan, ageBand);
            var combat = ResolveCombat(index, state, jobType, isElite, talentPlan, direction, ageBand);
            var craft = ResolveCraft(index, state, jobType, isElite, direction);
            var insight = ResolveInsight(index, state, jobType, isElite, talentPlan, direction);
            var execution = ResolveExecution(index, state, jobType, law, mood);
            var contribution = ResolveContribution(index, isElite, execution, law);
            var realmTier = ResolveRealmTier(index, state, jobType, isElite, talentPlan, direction, ageBand);
            var currentAssignment = ResolveCurrentAssignment(state, jobType, ageBand, minuteOfDay, isElite, directiveType);
            var residenceName = ResolveResidenceName(state, jobType, ageBand, isElite);
            var linkedPeakSummary = ResolveLinkedPeakSummary(state, jobType, ageBand);
            var traitSummary = ResolveTraitSummary(index, jobType, ageBand, talentPlan, law);
            var note = ResolveNote(jobType, isElite, health, mood, potential, insight, currentAssignment, directiveType);
            var equipmentProfile = DiscipleEquipmentRules.GetOrCreateEquipmentProfile(
                sourceState,
                index + 1,
                jobType,
                ageBand,
                realmTier,
                isElite,
                directiveType);

            roster.Add(new DiscipleProfile(
                index + 1,
                BuildName(index),
                ResolveRankName(jobType, ageBand, isElite, potential),
                directiveType,
                jobType,
                ResolveDutyDisplayName(jobType),
                ResolveRealmName(realmTier),
                realmTier,
                ageBand,
                age,
                isElite,
                health,
                mood,
                potential,
                combat,
                craft,
                insight,
                execution,
                contribution,
                currentAssignment,
                residenceName,
                linkedPeakSummary,
                traitSummary,
                equipmentProfile,
                note));
        }

        return roster;
    }

    // 构建随机名册（用于预览/模拟）
    public static IReadOnlyList<DiscipleProfile> BuildRandomRoster(GameState sourceState, int count, int? seed = null)
    {
        var state = sourceState.Clone();
        PopulationRules.EnsureDefaults(state);
        SectGovernanceRules.EnsureDefaults(state);
        DiscipleDirectiveRules.EnsureDefaults(state);

        var safeCount = Math.Max(count, 0);
        if (safeCount == 0)
        {
            return Array.Empty<DiscipleProfile>();
        }

        var random = seed.HasValue ? new Random(seed.Value) : new Random();
        var virtualPopulation = Math.Max(state.Population, safeCount);
        var jobAssignments = virtualPopulation > 0
            ? BuildJobAssignments(state, virtualPopulation)
            : new List<JobType?> { null };
        var ageAssignments = virtualPopulation > 0
            ? BuildAgeAssignments(state, virtualPopulation)
            : new List<DiscipleAgeBand> { DiscipleAgeBand.Prime };
        var eliteRate = state.Population > 0
            ? Math.Clamp(state.ElitePopulation / (double)state.Population, 0.0, 1.0)
            : DefaultEliteRate;

        var minuteOfDay = Modulo(state.GameMinutes, MinutesPerDay);
        var talentPlan = SectGovernanceRules.GetActiveTalentPlan(state);
        var direction = SectGovernanceRules.GetActiveDevelopmentDirection(state);
        var law = SectGovernanceRules.GetActiveLaw(state);

        var roster = new List<DiscipleProfile>(safeCount);
        var usedNames = new HashSet<string>();

        for (var index = 0; index < safeCount; index++)
        {
            var indexSeed = RollRandomIndex(random, usedNames, out var name);
            var jobType = jobAssignments.Count > 0 ? jobAssignments[random.Next(jobAssignments.Count)] : null;
            var ageBand = ageAssignments.Count > 0 ? ageAssignments[random.Next(ageAssignments.Count)] : DiscipleAgeBand.Prime;
            var isElite = random.NextDouble() < eliteRate;
            var directiveType = DiscipleDirectiveType.None;
            var age = ResolveAge(indexSeed, ageBand);
            var health = ResolveHealth(indexSeed, state, ageBand, isElite);
            var mood = ResolveMood(indexSeed, state, law);
            var potential = ResolvePotential(indexSeed, state, jobType, isElite, talentPlan, ageBand);
            var combat = ResolveCombat(indexSeed, state, jobType, isElite, talentPlan, direction, ageBand);
            var craft = ResolveCraft(indexSeed, state, jobType, isElite, direction);
            var insight = ResolveInsight(indexSeed, state, jobType, isElite, talentPlan, direction);
            var execution = ResolveExecution(indexSeed, state, jobType, law, mood);
            var contribution = ResolveContribution(indexSeed, isElite, execution, law);
            var realmTier = ResolveRealmTier(indexSeed, state, jobType, isElite, talentPlan, direction, ageBand);
            var currentAssignment = ResolveCurrentAssignment(state, jobType, ageBand, minuteOfDay, isElite, directiveType);
            var residenceName = ResolveResidenceName(state, jobType, ageBand, isElite);
            var linkedPeakSummary = ResolveLinkedPeakSummary(state, jobType, ageBand);
            var traitSummary = ResolveTraitSummary(indexSeed, jobType, ageBand, talentPlan, law);
            var note = ResolveNote(jobType, isElite, health, mood, potential, insight, currentAssignment, directiveType);
            var equipmentProfile = DiscipleEquipmentRules.BuildPreviewEquipmentProfile(
                indexSeed,
                state,
                jobType,
                ageBand,
                realmTier,
                isElite,
                directiveType);

            roster.Add(new DiscipleProfile(
                index + 1,
                name,
                ResolveRankName(jobType, ageBand, isElite, potential),
                directiveType,
                jobType,
                ResolveDutyDisplayName(jobType),
                ResolveRealmName(realmTier),
                realmTier,
                ageBand,
                age,
                isElite,
                health,
                mood,
                potential,
                combat,
                craft,
                insight,
                execution,
                contribution,
                currentAssignment,
                residenceName,
                linkedPeakSummary,
                traitSummary,
                equipmentProfile,
                note));
        }

        return roster;
    }

    // 按人口与岗位数量生成岗位序列
    private static List<JobType?> BuildJobAssignments(GameState state, int population)
    {
        var farmerCount = Math.Max(state.Farmers, 0);
        var workerCount = Math.Max(state.Workers, 0);
        var merchantCount = Math.Max(state.Merchants, 0);
        var scholarCount = Math.Max(state.Scholars, 0);
        var totalAssigned = farmerCount + workerCount + merchantCount + scholarCount;

        if (totalAssigned > population)
        {
            var scale = population / (double)Math.Max(totalAssigned, 1);
            farmerCount = (int)Math.Floor(farmerCount * scale);
            workerCount = (int)Math.Floor(workerCount * scale);
            merchantCount = (int)Math.Floor(merchantCount * scale);
            scholarCount = (int)Math.Floor(scholarCount * scale);

            var scaledTotal = farmerCount + workerCount + merchantCount + scholarCount;
            var remainders = new List<(JobType JobType, double Remainder)>
            {
                (JobType.Farmer, state.Farmers * scale - farmerCount),
                (JobType.Worker, state.Workers * scale - workerCount),
                (JobType.Merchant, state.Merchants * scale - merchantCount),
                (JobType.Scholar, state.Scholars * scale - scholarCount)
            };
            remainders.Sort((left, right) => right.Remainder.CompareTo(left.Remainder));
            var remainderCursor = 0;
            while (scaledTotal < population)
            {
                switch (remainders[remainderCursor % remainders.Count].JobType)
                {
                    case JobType.Farmer:
                        farmerCount++;
                        break;
                    case JobType.Worker:
                        workerCount++;
                        break;
                    case JobType.Merchant:
                        merchantCount++;
                        break;
                    case JobType.Scholar:
                        scholarCount++;
                        break;
                }

                scaledTotal++;
                remainderCursor++;
            }
        }

        var reserveCount = Math.Max(population - (farmerCount + workerCount + merchantCount + scholarCount), 0);
        var assignments = new List<JobType?>(population);
        AppendRepeated(assignments, JobType.Farmer, farmerCount);
        AppendRepeated(assignments, JobType.Worker, workerCount);
        AppendRepeated(assignments, JobType.Merchant, merchantCount);
        AppendRepeated(assignments, JobType.Scholar, scholarCount);
        AppendRepeated(assignments, null, reserveCount);

        return assignments
            .Select((jobType, index) => (
                JobType: jobType,
                SortKey: StableHash(index + (jobType.HasValue ? ((int)jobType.Value * 37) : 181), 71),
                Index: index))
            .OrderBy(static item => item.SortKey)
            .ThenBy(static item => item.Index)
            .Select(static item => item.JobType)
            .ToList();
    }

    // 生成年龄段分配序列
    private static List<DiscipleAgeBand> BuildAgeAssignments(GameState state, int population)
    {
        var seedlingCount = Math.Max(state.ChildPopulation, 0);
        var elderCount = Math.Max(state.ElderPopulation, 0);
        var adultCount = Math.Max(population - seedlingCount - elderCount, 0);
        var youngCount = adultCount / 2;
        var primeCount = adultCount - youngCount;

        var assignments = new List<DiscipleAgeBand>(population);
        AppendRepeated(assignments, DiscipleAgeBand.Seedling, seedlingCount);
        AppendRepeated(assignments, DiscipleAgeBand.Young, youngCount);
        AppendRepeated(assignments, DiscipleAgeBand.Prime, primeCount);
        AppendRepeated(assignments, DiscipleAgeBand.Elder, elderCount);

        while (assignments.Count < population)
        {
            assignments.Add(DiscipleAgeBand.Prime);
        }

        return assignments
            .Select((ageBand, index) => (
                AgeBand: ageBand,
                SortKey: StableHash(index + ((int)ageBand * 43), 131),
                Index: index))
            .OrderBy(static item => item.SortKey)
            .ThenBy(static item => item.Index)
            .Select(static item => item.AgeBand)
            .ToList();
    }

    // 构建精英弟子索引集合
    private static HashSet<int> BuildEliteSet(GameState state, int population)
    {
        var eliteCount = Math.Clamp(state.ElitePopulation, 0, population);
        return Enumerable.Range(0, population)
            .Select(index => (Index: index, Merit: StableHash(index, 911)))
            .OrderByDescending(static item => item.Merit)
            .Take(eliteCount)
            .Select(static item => item.Index)
            .ToHashSet();
    }

    // 计算弟子年龄
    private static int ResolveAge(int index, DiscipleAgeBand ageBand)
    {
        return ageBand switch
        {
            DiscipleAgeBand.Seedling => 11 + StableHash(index, 201) % 5,
            DiscipleAgeBand.Young => 16 + StableHash(index, 203) % 10,
            DiscipleAgeBand.Prime => 26 + StableHash(index, 205) % 18,
            DiscipleAgeBand.Elder => 46 + StableHash(index, 207) % 24,
            _ => 24
        };
    }

    // 计算健康值
    private static int ResolveHealth(
        int index,
        GameState state,
        DiscipleAgeBand ageBand,
        bool isElite)
    {
        var baseValue = 74 +
                        (state.Happiness * 0.12) +
                        (state.TechLevel * 2.0) -
                        (state.Threat * 0.16);

        if (ageBand == DiscipleAgeBand.Elder)
        {
            baseValue -= 7;
        }
        else if (ageBand == DiscipleAgeBand.Seedling)
        {
            baseValue += 4;
        }

        if (isElite)
        {
            baseValue += 4;
        }

        return Math.Clamp((int)Math.Round(baseValue + SampleSigned(index, 233, 8)), 24, 100);
    }

    // 计算心绪值
    private static int ResolveMood(
        int index,
        GameState state,
        SectLawType law)
    {
        var lawBonus = law switch
        {
            SectLawType.Benevolent => 6,
            SectLawType.OpenLectures => 4,
            SectLawType.Merit => 1,
            SectLawType.Discipline => -5,
            _ => 0
        };

        var baseValue = state.Happiness +
                        (state.Threat * 0.38) -
                        lawBonus;

        return Math.Clamp((int)Math.Round(baseValue + SampleSigned(index, 251, 9)), 18, 100);
    }

    // 计算潜力值
    private static int ResolvePotential(
        int index,
        GameState state,
        JobType? jobType,
        bool isElite,
        SectTalentPlanType talentPlan,
        DiscipleAgeBand ageBand)
    {
        var baseValue = 42 + (state.TechLevel * 6);

        if (isElite)
        {
            baseValue += 22;
        }

        if (ageBand == DiscipleAgeBand.Seedling)
        {
            baseValue += 8;
        }

        baseValue += talentPlan switch
        {
            SectTalentPlanType.RecruitDisciples => ageBand == DiscipleAgeBand.Seedling ? 10 : 2,
            SectTalentPlanType.ArrayScholarship => jobType == JobType.Scholar ? 11 : 1,
            SectTalentPlanType.StewardTraining => jobType == JobType.Worker ? 8 : 2,
            SectTalentPlanType.OuterMissions => jobType == JobType.Merchant ? 8 : 2,
            _ => 0
        };

        return Math.Clamp(baseValue + SampleSigned(index, 271, 12), 16, 100);
    }

    // 计算战力值
    private static int ResolveCombat(
        int index,
        GameState state,
        JobType? jobType,
        bool isElite,
        SectTalentPlanType talentPlan,
        SectDevelopmentDirectionType direction,
        DiscipleAgeBand ageBand)
    {
        var baseValue = GetJobBase(jobType, 46, 56, 40, 34, 28) +
                        (state.TechLevel * 5) +
                        (int)Math.Round(state.Threat * 0.30);

        if (direction == SectDevelopmentDirectionType.DefenseFirst)
        {
            baseValue += 8;
        }

        if (talentPlan == SectTalentPlanType.OuterMissions && jobType == JobType.Merchant)
        {
            baseValue += 7;
        }

        if (ageBand == DiscipleAgeBand.Seedling)
        {
            baseValue -= 10;
        }

        if (isElite)
        {
            baseValue += 18;
        }

        return Math.Clamp(baseValue + SampleSigned(index, 281, 11), 10, 100);
    }

    // 计算工艺值
    private static int ResolveCraft(
        int index,
        GameState state,
        JobType? jobType,
        bool isElite,
        SectDevelopmentDirectionType direction)
    {
        var baseValue = GetJobBase(jobType, 58, 71, 39, 45, 34) +
                        (state.WorkshopBuildings * 2) +
                        (state.WarehouseLevel * 3);

        if (direction == SectDevelopmentDirectionType.SupplyFirst)
        {
            baseValue += 8;
        }

        if (isElite)
        {
            baseValue += 6;
        }

        return Math.Clamp(baseValue + SampleSigned(index, 307, 9), 10, 100);
    }

    // 计算悟性值
    private static int ResolveInsight(
        int index,
        GameState state,
        JobType? jobType,
        bool isElite,
        SectTalentPlanType talentPlan,
        SectDevelopmentDirectionType direction)
    {
        var baseValue = GetJobBase(jobType, 42, 38, 48, 72, 36) +
                        (state.ResearchBuildings * 4) +
                        (state.TechLevel * 5);

        if (direction == SectDevelopmentDirectionType.DoctrineFirst)
        {
            baseValue += 9;
        }

        if (talentPlan == SectTalentPlanType.ArrayScholarship && jobType == JobType.Scholar)
        {
            baseValue += 12;
        }

        if (isElite)
        {
            baseValue += 7;
        }

        return Math.Clamp(baseValue + SampleSigned(index, 331, 10), 10, 100);
    }

    // 计算执行值
    private static int ResolveExecution(int index, GameState state, JobType? jobType, SectLawType law, int mood)
    {
        var baseValue = GetJobBase(jobType, 52, 64, 56, 49, 44) +
                        (state.AdministrationBuildings * 3) +
                        ((mood - 50) / 4);

        baseValue += law switch
        {
            SectLawType.Discipline => 10,
            SectLawType.Merit => 7,
            SectLawType.Benevolent => 3,
            SectLawType.OpenLectures => 1,
            _ => 0
        };

        return Math.Clamp(baseValue + SampleSigned(index, 353, 9), 10, 100);
    }

    // 计算贡献值
    private static int ResolveContribution(int index, bool isElite, int execution, SectLawType law)
    {
        var lawBonus = law == SectLawType.Merit ? 9 : 2;
        var baseValue = 22 + (execution / 2) + lawBonus + (isElite ? 14 : 0);
        return Math.Clamp(baseValue + SampleSigned(index, 367, 8), 8, 100);
    }

    // 计算境界阶
    private static int ResolveRealmTier(
        int index,
        GameState state,
        JobType? jobType,
        bool isElite,
        SectTalentPlanType talentPlan,
        SectDevelopmentDirectionType direction,
        DiscipleAgeBand ageBand)
    {
        var tier = 1 + Math.Max(state.TechLevel, 0);
        if (jobType == JobType.Scholar)
        {
            tier += 1;
        }

        if (ageBand == DiscipleAgeBand.Elder)
        {
            tier += 1;
        }

        if (direction == SectDevelopmentDirectionType.DoctrineFirst && jobType == JobType.Scholar)
        {
            tier += 1;
        }

        if (talentPlan == SectTalentPlanType.ArrayScholarship && jobType == JobType.Scholar)
        {
            tier += 1;
        }

        if (isElite)
        {
            tier += 2;
        }

        tier += StableHash(index, 389) % 2;
        return Math.Clamp(tier, 1, 7);
    }

    // 计算当前差使描述
    private static string ResolveCurrentAssignment(
        GameState state,
        JobType? jobType,
        DiscipleAgeBand ageBand,
        int minuteOfDay,
        bool isElite,
        DiscipleDirectiveType directiveType)
    {
        string assignment;
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            assignment = minuteOfDay switch
            {
                < 420 => "卯时温养灵息",
                < 720 => "启蒙晨课",
                < 1020 => "随堂观摩与基础差使",
                _ => "回舍温习与抄录"
            };

            return SectNamingRules.ReplaceKnownNames(state, assignment);
        }

        if (minuteOfDay < 300)
        {
            assignment = isElite ? "夜静行功" : "夜息回舍";
            return SectNamingRules.ReplaceKnownNames(state, AppendDirectiveSuffix(assignment, directiveType));
        }

        if (minuteOfDay < 480)
        {
            assignment = isElite ? "晨钟吐纳" : "点卯整队";
            return SectNamingRules.ReplaceKnownNames(state, AppendDirectiveSuffix(assignment, directiveType));
        }

        if (minuteOfDay < 900)
        {
            assignment = jobType switch
            {
                JobType.Farmer => "阵材圃轮值",
                JobType.Worker => "阵枢营造",
                JobType.Merchant => "总坊对牌",
                JobType.Scholar => "传法院推演",
                _ => "待命补位"
            };

            return SectNamingRules.ReplaceKnownNames(state, AppendDirectiveSuffix(assignment, directiveType));
        }

        if (minuteOfDay < 1140)
        {
            assignment = jobType switch
            {
                JobType.Farmer => "巡视地脉与药圃",
                JobType.Worker => "护山构件检修",
                JobType.Merchant => "商路议价与采办",
                JobType.Scholar => "讲法复盘与校勘",
                _ => "轮值巡舍"
            };

            return SectNamingRules.ReplaceKnownNames(state, AppendDirectiveSuffix(assignment, directiveType));
        }

        assignment = isElite ? "晚课收束与静修" : "归舍整理与晚修";
        return SectNamingRules.ReplaceKnownNames(state, AppendDirectiveSuffix(assignment, directiveType));
    }

    // 计算居住院舍名称
    private static string ResolveResidenceName(GameState state, JobType? jobType, DiscipleAgeBand ageBand, bool isElite)
    {
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            return SectNamingRules.ReplaceKnownNames(state, "启蒙院舍");
        }

        if (ageBand == DiscipleAgeBand.Elder)
        {
            return SectNamingRules.ReplaceKnownNames(state, "护峰别院");
        }

        if (isElite)
        {
            return SectNamingRules.ReplaceKnownNames(state, "真传静修院");
        }

        var residence = jobType switch
        {
            JobType.Farmer => "阵材圃轮值舍",
            JobType.Worker => "傀儡工坊值守舍",
            JobType.Merchant => "青云总坊客舍",
            JobType.Scholar => "传法院静修舍",
            _ => "外门居舍"
        };
        return SectNamingRules.ReplaceKnownNames(state, residence);
    }

    // 计算关联峰/堂口摘要
    private static string ResolveLinkedPeakSummary(GameState state, JobType? jobType, DiscipleAgeBand ageBand)
    {
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            return SectNamingRules.ReplaceKnownNames(state, "启蒙院 / 传功总院 / 天衍峰教习轮值");
        }

        if (!jobType.HasValue)
        {
            return SectNamingRules.ReplaceKnownNames(state, "庶务殿 / 外门轮值司 / 天衍峰巡值队");
        }

        return SectOrganizationRules.GetLinkedPeakSummary(state, jobType.Value);
    }

    // 计算性格词条汇总
    private static string ResolveTraitSummary(int index, JobType? jobType, DiscipleAgeBand ageBand, SectTalentPlanType talentPlan, SectLawType law)
    {
        var primaryTrait = PickTrait(jobType, index);
        var commonTrait = CommonTraits[StableHash(index, 433) % CommonTraits.Length];
        var planTrait = talentPlan switch
        {
            SectTalentPlanType.RecruitDisciples => ageBand == DiscipleAgeBand.Seedling ? "灵息活络" : "愿意带新苗",
            SectTalentPlanType.ArrayScholarship => jobType == JobType.Scholar ? "悟阵偏强" : "肯听讲法",
            SectTalentPlanType.StewardTraining => jobType == JobType.Worker ? "执行压得住" : "守序度高",
            SectTalentPlanType.OuterMissions => jobType == JobType.Merchant ? "外务适应快" : "临场不怯",
            _ => "状态平稳"
        };

        var lawTrait = law switch
        {
            SectLawType.Benevolent => "心气舒展",
            SectLawType.Discipline => "举止规整",
            SectLawType.Merit => "争先意识强",
            SectLawType.OpenLectures => "乐于问学",
            _ => "门风稳定"
        };

        return $"{primaryTrait} / {commonTrait} / {planTrait} / {lawTrait}";
    }

    // 生成备注说明
    private static string ResolveNote(
        JobType? jobType,
        bool isElite,
        int health,
        int mood,
        int potential,
        int insight,
        string currentAssignment,
        DiscipleDirectiveType directiveType)
    {
        string baseNote;
        if (health <= 44)
        {
            baseNote = "近期消耗偏高，建议优先补药养与静修时段。";
            return AppendDirectiveNote(baseNote, directiveType);
        }

        if (mood <= 42)
        {
            baseNote = "心绪起伏明显，宜暂缓高压差使，先稳住起居与讲法节奏。";
            return AppendDirectiveNote(baseNote, directiveType);
        }

        if (potential >= 82 && insight >= 72)
        {
            baseNote = "具备重点培养价值，适合纳入阵道深造或真传考察名单。";
            return AppendDirectiveNote(baseNote, directiveType);
        }

        if (isElite)
        {
            baseNote = "当前已列入峰内重点名册，可承担更高阶护山或推演职责。";
            return AppendDirectiveNote(baseNote, directiveType);
        }

        baseNote = $"当前以“{currentAssignment}”为主，整体状态平稳，可按既定方略继续历练。";
        return AppendDirectiveNote(baseNote, directiveType);
    }

    // 生成称谓（新苗/真传等）
    private static string ResolveRankName(JobType? jobType, DiscipleAgeBand ageBand, bool isElite, int potential)
    {
        if (ageBand == DiscipleAgeBand.Seedling)
        {
            return "新苗";
        }

        if (ageBand == DiscipleAgeBand.Elder)
        {
            return "守峰前辈";
        }

        if (isElite)
        {
            return "真传";
        }

        if (potential >= 74)
        {
            return "内门";
        }

        return jobType.HasValue ? "外门" : "候值";
    }

    // 生成职司显示名
    private static string ResolveDutyDisplayName(JobType? jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "阵材职司",
            JobType.Worker => "阵务职司",
            JobType.Merchant => "外事职司",
            JobType.Scholar => "推演职司",
            _ => "待命轮值"
        };
    }

    // 生成境界显示名
    private static string ResolveRealmName(int realmTier)
    {
        return realmTier switch
        {
            1 => "炼气二层",
            2 => "炼气四层",
            3 => "炼气六层",
            4 => "炼气圆满",
            5 => "筑基初境",
            6 => "筑基中境",
            7 => "筑基后境",
            _ => "凡俗根骨"
        };
    }

    // 生成弟子姓名
    private static string BuildName(int index)
    {
        var surname = Surnames[StableHash(index, 461) % Surnames.Length];
        var first = NameFirstChars[StableHash(index, 463) % NameFirstChars.Length];
        var second = NameSecondChars[StableHash(index, 467) % NameSecondChars.Length];
        return $"{surname}{first}{second}";
    }

    // 随机抽取不重复姓名索引
    private static int RollRandomIndex(Random random, HashSet<string> usedNames, out string name)
    {
        for (var attempt = 0; attempt < RandomNameRetryLimit; attempt++)
        {
            var index = random.Next(RandomIndexUpperBound);
            var candidateName = BuildName(index);
            if (usedNames.Add(candidateName))
            {
                name = candidateName;
                return index;
            }
        }

        var fallback = random.Next(RandomIndexUpperBound);
        name = BuildName(fallback);
        return fallback;
    }

    // 按岗位挑选性格词条
    private static string PickTrait(JobType? jobType, int index)
    {
        var pool = jobType switch
        {
            JobType.Farmer => FarmerTraits,
            JobType.Worker => WorkerTraits,
            JobType.Merchant => MerchantTraits,
            JobType.Scholar => ScholarTraits,
            _ => ReserveTraits
        };

        return pool[StableHash(index, 487) % pool.Length];
    }

    // 获取岗位对应的基础值
    private static int GetJobBase(JobType? jobType, int farmerValue, int workerValue, int merchantValue, int scholarValue, int reserveValue)
    {
        return jobType switch
        {
            JobType.Farmer => farmerValue,
            JobType.Worker => workerValue,
            JobType.Merchant => merchantValue,
            JobType.Scholar => scholarValue,
            _ => reserveValue
        };
    }

    // 生成稳定的有符号偏移
    private static int SampleSigned(int index, int salt, int maxAbsoluteValue)
    {
        return (StableHash(index, salt) % (maxAbsoluteValue * 2 + 1)) - maxAbsoluteValue;
    }

    // 稳定哈希生成器
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

    // 安全取模（允许负值）
    private static int Modulo(int value, int modulo)
    {
        if (modulo == 0)
        {
            return 0;
        }

        var remainder = value % modulo;
        return remainder < 0 ? remainder + modulo : remainder;
    }

    // 向集合追加重复值
    private static void AppendRepeated<T>(ICollection<T> list, T value, int count)
    {
        for (var i = 0; i < count; i++)
        {
            list.Add(value);
        }
    }

    // 为差使添加指令后缀
    private static string AppendDirectiveSuffix(string assignment, DiscipleDirectiveType directiveType)
    {
        return directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => $"{assignment} · 外务候补",
            DiscipleDirectiveType.StewardCandidate => $"{assignment} · 执事试值",
            _ => assignment
        };
    }

    // 为备注添加指令说明
    private static string AppendDirectiveNote(string baseNote, DiscipleDirectiveType directiveType)
    {
        var directiveNote = directiveType switch
        {
            DiscipleDirectiveType.OuterMissionCandidate => "现已被点为外务候补，历练与商路差使会优先吸纳其表现。",
            DiscipleDirectiveType.StewardCandidate => "现已被点为执事培养对象，庶务磨砺与内务回流会持续记录其执行表现。",
            _ => string.Empty
        };

        return string.IsNullOrWhiteSpace(directiveNote)
            ? baseNote
            : $"{baseNote} {directiveNote}";
    }
}
