using System;
using System.Collections.Generic;
using System.Linq;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public sealed record SectTaskDefinition(
    SectTaskType TaskType,
    string DisplayName,
    string ShortName,
    string IconGlyph,
    string Description,
    JobType JobType,
    bool IsInternalTask,
    int WorkforcePerOrder,
    int PriorityOrder,
    double FoodYieldPerWorker,
    double ResearchYieldPerWorker,
    double GoldYieldPerWorker,
    double ContributionYieldPerWorker);

public sealed class SectTaskResolutionSnapshot
{
    public SectTaskResolutionSnapshot(
        IReadOnlyDictionary<SectTaskType, int> orderUnits,
        IReadOnlyDictionary<SectTaskType, int> requestedWorkersByTask,
        IReadOnlyDictionary<SectTaskType, int> resolvedWorkersByTask,
        IReadOnlyDictionary<JobType, int> resolvedByJob,
        int totalOrderUnits,
        int totalRequestedWorkers,
        int totalResolvedWorkers,
        int unassignedPopulation)
    {
        OrderUnits = orderUnits;
        RequestedWorkersByTask = requestedWorkersByTask;
        ResolvedWorkersByTask = resolvedWorkersByTask;
        ResolvedByJob = resolvedByJob;
        TotalOrderUnits = totalOrderUnits;
        TotalRequestedWorkers = totalRequestedWorkers;
        TotalResolvedWorkers = totalResolvedWorkers;
        UnassignedPopulation = unassignedPopulation;
    }

    public IReadOnlyDictionary<SectTaskType, int> OrderUnits { get; }

    public IReadOnlyDictionary<SectTaskType, int> RequestedWorkersByTask { get; }

    public IReadOnlyDictionary<SectTaskType, int> ResolvedWorkersByTask { get; }

    public IReadOnlyDictionary<JobType, int> ResolvedByJob { get; }

    public int TotalOrderUnits { get; }

    public int TotalRequestedWorkers { get; }

    public int TotalResolvedWorkers { get; }

    public int UnassignedPopulation { get; }
}

public static class SectTaskRules
{
    private const int MaxOrderUnitsPerTask = 99;

    private static readonly IReadOnlyList<SectTaskDefinition> Definitions =
    [
        new(
            SectTaskType.FieldDuty,
            "阵材采炼",
            "阵材",
            "🌾",
            "安排门人采集阵材、灵谷与基础物资，为天衍峰阵堂与庶务殿提供底盘供给。",
            JobType.Farmer,
            true,
            5,
            0,
            2.4,
            0,
            0,
            0.08),
        new(
            SectTaskType.WorkshopDuty,
            "阵枢营造",
            "阵枢",
            "🛠",
            "安排执事与匠役负责阵盘、傀儡、机关与峰内营造，是天衍峰阵堂的核心生产法旨。",
            JobType.Worker,
            true,
            4,
            1,
            0,
            0,
            0,
            0.12),
        new(
            SectTaskType.LogisticsPatrol,
            "巡山警戒",
            "巡山",
            "🕯",
            "安排巡山队、戒律哨与庶务值守，维持浮云宗山门秩序与边界警戒。",
            JobType.Worker,
            true,
            3,
            2,
            0,
            0,
            0,
            0.18),
        new(
            SectTaskType.ScriptureStudy,
            "阵法推演",
            "推演",
            "📜",
            "安排弟子在传法院与阵堂推演阵图、傀儡法式与战争沙盘。",
            JobType.Scholar,
            true,
            2,
            3,
            0,
            0.46,
            0,
            0.10),
        new(
            SectTaskType.SectCommerce,
            "总坊值守",
            "总坊",
            "💠",
            "负责青云总坊、峰内账册、内务兑换与附庸报备，会同时带来贡献点与少量灵石回流。",
            JobType.Merchant,
            true,
            2,
            4,
            0,
            0,
            0.26,
            0.45),
        new(
            SectTaskType.OuterTrade,
            "外事行商",
            "外事",
            "🧭",
            "由外事体系对接附庸据点与他宗商路，对外收益只以灵石结算。",
            JobType.Merchant,
            false,
            2,
            5,
            0,
            0,
            1.18,
            0)
    ];

    private static readonly IReadOnlyDictionary<SectTaskType, SectTaskDefinition> DefinitionLookup =
        Definitions.ToDictionary(static definition => definition.TaskType);

    public static IReadOnlyList<SectTaskDefinition> GetOrderedDefinitions()
    {
        return Definitions;
    }

    public static SectTaskDefinition GetDefinition(SectTaskType taskType)
    {
        return DefinitionLookup[taskType];
    }

    public static void EnsureDefaults(GameState state)
    {
        SectGovernanceRules.EnsureDefaults(state);
        var shouldBootstrapOrders = state.TaskOrderUnits == null || state.TaskOrderUnits.Count == 0;
        EnsureTaskDictionaries(state);
        if (shouldBootstrapOrders)
        {
            state.TaskOrderUnits = BuildRecommendedOrders(state);
            EnsureTaskDictionaries(state);
        }

        ApplyResolvedSnapshot(state, BuildResolutionSnapshot(state));
    }

    public static void ResetToRecommended(GameState state)
    {
        SectGovernanceRules.EnsureDefaults(state);
        state.TaskOrderUnits = BuildRecommendedOrders(state);
        EnsureTaskDictionaries(state);
        ApplyResolvedSnapshot(state, BuildResolutionSnapshot(state));
    }

    public static SectTaskResolutionSnapshot BuildResolutionSnapshot(GameState state)
    {
        EnsureTaskDictionaries(state);
        IndustryRules.EnsureDefaults(state);
        PopulationRules.EnsureDefaults(state);

        var orderUnits = new Dictionary<SectTaskType, int>(Definitions.Count);
        var requestedWorkers = new Dictionary<SectTaskType, int>(Definitions.Count);
        var resolvedWorkers = new Dictionary<SectTaskType, int>(Definitions.Count);
        var resolvedByJob = new Dictionary<JobType, int>
        {
            [JobType.Farmer] = 0,
            [JobType.Worker] = 0,
            [JobType.Merchant] = 0,
            [JobType.Scholar] = 0
        };
        var remainingByJob = new Dictionary<JobType, int>
        {
            [JobType.Farmer] = IndustryRules.GetCapacity(state, JobType.Farmer),
            [JobType.Worker] = IndustryRules.GetCapacity(state, JobType.Worker),
            [JobType.Merchant] = IndustryRules.GetCapacity(state, JobType.Merchant),
            [JobType.Scholar] = IndustryRules.GetCapacity(state, JobType.Scholar)
        };

        var remainingPopulation = Math.Max(state.Population, 0);
        var totalOrderUnits = 0;
        var totalRequestedWorkers = 0;
        var totalResolvedWorkers = 0;

        foreach (var definition in Definitions)
        {
            var orderCount = GetOrderUnits(state, definition.TaskType);
            var requested = orderCount * definition.WorkforcePerOrder;
            var allowedByJob = remainingByJob[definition.JobType];
            var resolved = Math.Min(requested, Math.Min(remainingPopulation, allowedByJob));

            orderUnits[definition.TaskType] = orderCount;
            requestedWorkers[definition.TaskType] = requested;
            resolvedWorkers[definition.TaskType] = resolved;
            resolvedByJob[definition.JobType] += resolved;
            remainingByJob[definition.JobType] -= resolved;
            remainingPopulation -= resolved;

            totalOrderUnits += orderCount;
            totalRequestedWorkers += requested;
            totalResolvedWorkers += resolved;
        }

        return new SectTaskResolutionSnapshot(
            orderUnits,
            requestedWorkers,
            resolvedWorkers,
            resolvedByJob,
            totalOrderUnits,
            totalRequestedWorkers,
            totalResolvedWorkers,
            remainingPopulation);
    }

    public static void ApplyResolvedSnapshot(GameState state, SectTaskResolutionSnapshot snapshot)
    {
        EnsureTaskDictionaries(state);
        foreach (var definition in Definitions)
        {
            state.TaskResolvedWorkers[GetStorageKey(definition.TaskType)] =
                snapshot.ResolvedWorkersByTask.TryGetValue(definition.TaskType, out var resolved)
                    ? resolved
                    : 0;
        }

        IndustryRules.SetAssigned(state, JobType.Farmer, snapshot.ResolvedByJob[JobType.Farmer]);
        IndustryRules.SetAssigned(state, JobType.Worker, snapshot.ResolvedByJob[JobType.Worker]);
        IndustryRules.SetAssigned(state, JobType.Merchant, snapshot.ResolvedByJob[JobType.Merchant]);
        IndustryRules.SetAssigned(state, JobType.Scholar, snapshot.ResolvedByJob[JobType.Scholar]);
    }

    public static int GetOrderUnits(GameState state, SectTaskType taskType)
    {
        EnsureTaskDictionaries(state);
        return state.TaskOrderUnits.TryGetValue(GetStorageKey(taskType), out var value)
            ? Math.Max(value, 0)
            : 0;
    }

    public static int SetOrderUnits(GameState state, SectTaskType taskType, int value)
    {
        EnsureTaskDictionaries(state);
        var clamped = Math.Clamp(value, 0, MaxOrderUnitsPerTask);
        state.TaskOrderUnits[GetStorageKey(taskType)] = clamped;
        return clamped;
    }

    public static int GetResolvedWorkers(GameState state, SectTaskType taskType)
    {
        EnsureTaskDictionaries(state);
        return state.TaskResolvedWorkers.TryGetValue(GetStorageKey(taskType), out var value)
            ? Math.Max(value, 0)
            : 0;
    }

    public static SectTaskType GetPrimaryTaskForJob(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => SectTaskType.FieldDuty,
            JobType.Worker => SectTaskType.WorkshopDuty,
            JobType.Merchant => SectTaskType.SectCommerce,
            JobType.Scholar => SectTaskType.ScriptureStudy,
            _ => SectTaskType.FieldDuty
        };
    }

    public static string GetJobButtonText(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "供养令",
            JobType.Worker => "峰务令",
            JobType.Merchant => "外务令",
            JobType.Scholar => "传承令",
            _ => "任务令"
        };
    }

    public static string BuildGovernanceHeadline(GameState state)
    {
        var snapshot = BuildResolutionSnapshot(state);
        var focusedItems = Definitions
            .Select(definition => new
            {
                Definition = definition,
                Orders = snapshot.OrderUnits[definition.TaskType]
            })
            .Where(item => item.Orders > 0)
            .OrderByDescending(item => item.Orders)
            .ThenBy(item => item.Definition.PriorityOrder)
            .Take(2)
            .ToArray();

        if (focusedItems.Length == 0)
        {
            return "尚未立定峰务方向";
        }

        return string.Join(" / ", focusedItems.Select(static item => $"{item.Definition.DisplayName}{GetDirectiveLevelText(item.Orders)}"));
    }

    public static string BuildGovernanceExecutionSummary(GameState state)
    {
        return BuildGovernanceExecutionSummary(BuildResolutionSnapshot(state));
    }

    public static string GetDirectiveLevelSummary(GameState state, SectTaskType taskType)
    {
        return GetDirectiveLevelText(GetOrderUnits(state, taskType));
    }

    public static string GetDirectiveExecutionSummary(GameState state, SectTaskType taskType)
    {
        var snapshot = BuildResolutionSnapshot(state);
        return BuildDirectiveExecutionText(
            snapshot.RequestedWorkersByTask[taskType],
            snapshot.ResolvedWorkersByTask[taskType]);
    }

    public static JobPanelInfo GetJobPanelInfo(GameState state, JobType jobType)
    {
        var snapshot = BuildResolutionSnapshot(state);
        var tasks = Definitions.Where(definition => definition.JobType == jobType).ToArray();
        var summaryParts = new List<string>(tasks.Length);
        var detailParts = new List<string>(tasks.Length);

        foreach (var definition in tasks)
        {
            var orderUnits = snapshot.OrderUnits[definition.TaskType];
            var requested = snapshot.RequestedWorkersByTask[definition.TaskType];
            var resolved = snapshot.ResolvedWorkersByTask[definition.TaskType];
            var levelText = GetDirectiveLevelText(orderUnits);
            var executionText = BuildDirectiveExecutionText(requested, resolved);

            if (orderUnits > 0)
            {
                summaryParts.Add($"{definition.ShortName}{levelText}");
            }

            detailParts.Add($"{definition.DisplayName}（{levelText} / {executionText}）");
        }

        var title = $"{GetJobIcon(jobType)} {GetJobDisplayName(jobType)}";
        var summary = summaryParts.Count > 0
            ? $"{SectOrganizationRules.GetLinkedPeakSummary(jobType)} · {BuildJobExecutionText(tasks, snapshot)}"
            : $"{SectOrganizationRules.GetLinkedPeakSummary(jobType)} · 尚未定调";
        var activeSupport = SectPeakSupportRules.GetActiveDefinition(state);
        var detail =
            $"当前方略：{string.Join("；", detailParts)}。{SectOrganizationRules.GetLinkedDepartmentDetail(jobType)} 当前峰脉协同：{activeSupport.DisplayName}（{activeSupport.ShortEffect}）。该职司由执事层按门人状态、场所条件与门规自动排班，宗主只需决定方向与轻重，不必逐人分配。请在“宗主中枢”中调整。";

        return new JobPanelInfo(
            jobType,
            GetJobDisplayName(jobType),
            title,
            summary,
            detail,
            GetJobButtonText(jobType));
    }

    public static string GetTaskListText(GameState state, SectTaskType taskType)
    {
        var definition = GetDefinition(taskType);
        var snapshot = BuildResolutionSnapshot(state);
        var orders = snapshot.OrderUnits[taskType];
        var requested = snapshot.RequestedWorkersByTask[taskType];
        var resolved = snapshot.ResolvedWorkersByTask[taskType];
        var levelText = GetDirectiveLevelText(orders);
        var executionText = BuildDirectiveExecutionText(requested, resolved);
        return $"{definition.IconGlyph} {definition.DisplayName} · {levelText} · {executionText}";
    }

    public static string BuildTaskDetailText(GameState state, SectTaskType taskType)
    {
        var definition = GetDefinition(taskType);
        var snapshot = BuildResolutionSnapshot(state);
        var orders = snapshot.OrderUnits[taskType];
        var requested = snapshot.RequestedWorkersByTask[taskType];
        var resolved = snapshot.ResolvedWorkersByTask[taskType];
        var levelText = GetDirectiveLevelText(orders);
        var executionText = BuildDirectiveExecutionText(requested, resolved);
        var outputParts = new List<string>(4);

        if (definition.FoodYieldPerWorker > 0)
        {
            outputParts.Add($"{MaterialSemanticRules.GetDisplayName(nameof(GameState.Food))}+");
        }

        if (definition.GoldYieldPerWorker > 0)
        {
            outputParts.Add($"{MaterialSemanticRules.GetDisplayName(nameof(GameState.Gold))}+");
        }

        if (definition.ContributionYieldPerWorker > 0)
        {
            outputParts.Add($"{MaterialSemanticRules.GetDisplayName(nameof(GameState.ContributionPoints))}+");
        }

        if (definition.ResearchYieldPerWorker > 0)
        {
            outputParts.Add("传承研修+");
        }

        var settlementText = definition.IsInternalTask
            ? "浮云宗内务：由内务总殿按贡献点 + 灵石双轨核算。"
            : "浮云宗外务：由外事总殿统一对外结算，只认灵石。";
        var quarterDecree = SectGovernanceRules.GetActiveQuarterDecreeDefinition(state);
        var ruleTreeSummary = SectRuleTreeRules.BuildActiveRuleSummary(state);
        var governanceText =
            $"宗主职责：决定是否试行、常设、重点或全力推进；具体人手与轮值由执事层按峰内条件自动协调。当前季度法令：{quarterDecree.DisplayName}（{quarterDecree.ShortEffect}）。当前门规纲目：{ruleTreeSummary}。";

        return
            $"{definition.Description}\n" +
            $"治理领域：{GetDirectiveDomainText(taskType)}\n" +
            $"所属职司：{GetJobDisplayName(definition.JobType)}\n" +
            $"当前侧重：{levelText}\n" +
            $"执事落实：{executionText}\n" +
            $"预计作用：{(outputParts.Count > 0 ? string.Join("、", outputParts) : "主要提供组织与内务支撑")}\n" +
            $"{settlementText}\n" +
            $"{governanceText}";
    }

    private static Dictionary<string, int> BuildRecommendedOrders(GameState state)
    {
        SectGovernanceRules.EnsureDefaults(state);
        var farmers = state.Farmers > 0
            ? state.Farmers
            : Math.Max((int)Math.Round(state.Population * 0.58), 20);
        var workers = state.Workers > 0
            ? state.Workers
            : Math.Max((int)Math.Round(state.Population * 0.21), 8);
        var merchants = state.Merchants > 0
            ? state.Merchants
            : Math.Max((int)Math.Round(state.Population * 0.10), 4);
        var scholars = state.Scholars > 0
            ? state.Scholars
            : Math.Max((int)Math.Round(state.Population * 0.07), 2);

        var workshopWorkers = workers > 0 ? Math.Max((int)Math.Round((workers * 0.64) / 4.0), 1) : 0;
        var resolvedWorkshopWorkers = workshopWorkers * GetDefinition(SectTaskType.WorkshopDuty).WorkforcePerOrder;
        var logisticsWorkers = Math.Max(workers - resolvedWorkshopWorkers, 0);

        var internalMerchants = (int)Math.Ceiling(merchants * 0.5);
        var outerMerchants = Math.Max(merchants - internalMerchants, 0);

        var orders = new Dictionary<string, int>
        {
            [GetStorageKey(SectTaskType.FieldDuty)] =
                farmers > 0 ? Math.Max((int)Math.Round(farmers / 5.0), 1) : 0,
            [GetStorageKey(SectTaskType.WorkshopDuty)] = workshopWorkers,
            [GetStorageKey(SectTaskType.LogisticsPatrol)] =
                logisticsWorkers > 0 ? Math.Max((int)Math.Round(logisticsWorkers / 3.0), 1) : 0,
            [GetStorageKey(SectTaskType.ScriptureStudy)] =
                scholars > 0 ? Math.Max((int)Math.Round(scholars / 2.0), 1) : 0,
            [GetStorageKey(SectTaskType.SectCommerce)] =
                internalMerchants > 0 ? Math.Max((int)Math.Round(internalMerchants / 2.0), 1) : 0,
            [GetStorageKey(SectTaskType.OuterTrade)] =
                outerMerchants > 0 ? Math.Max((int)Math.Round(outerMerchants / 2.0), 1) : 0
        };

        ApplyDevelopmentDirectionPreset(state, orders);
        return orders;
    }

    private static void EnsureTaskDictionaries(GameState state)
    {
        state.TaskOrderUnits ??= new Dictionary<string, int>();
        state.TaskResolvedWorkers ??= new Dictionary<string, int>();

        foreach (var definition in Definitions)
        {
            var key = GetStorageKey(definition.TaskType);
            if (!state.TaskOrderUnits.ContainsKey(key))
            {
                state.TaskOrderUnits[key] = 0;
            }

            if (!state.TaskResolvedWorkers.ContainsKey(key))
            {
                state.TaskResolvedWorkers[key] = 0;
            }
        }
    }

    private static string GetStorageKey(SectTaskType taskType)
    {
        return taskType.ToString();
    }

    private static string GetJobDisplayName(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "阵材供养体系",
            JobType.Worker => "峰务营造体系",
            JobType.Merchant => "总坊外务体系",
            JobType.Scholar => "传承研修体系",
            _ => "浮云宗治理体系"
        };
    }

    private static string GetJobIcon(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "🌾",
            JobType.Worker => "🛠",
            JobType.Merchant => "💰",
            JobType.Scholar => "📜",
            _ => "🕯"
        };
    }

    private static string GetDirectiveLevelText(int orderUnits)
    {
        return orderUnits switch
        {
            <= 0 => "未设",
            <= 2 => "试行",
            <= 5 => "常设",
            <= 9 => "重点",
            _ => "鼎力"
        };
    }

    private static string BuildDirectiveExecutionText(int requestedWorkers, int resolvedWorkers)
    {
        if (requestedWorkers <= 0)
        {
            return "待立令";
        }

        if (resolvedWorkers <= 0)
        {
            return "尚未落实";
        }

        var ratio = resolvedWorkers / (double)Math.Max(requestedWorkers, 1);
        if (ratio >= 0.95)
        {
            return "落实稳定";
        }

        if (ratio >= 0.6)
        {
            return "部分受限";
        }

        return "承压明显";
    }

    private static string BuildGovernanceExecutionSummary(SectTaskResolutionSnapshot snapshot)
    {
        if (snapshot.TotalOrderUnits <= 0)
        {
            return "尚待宗主定调";
        }

        if (snapshot.TotalResolvedWorkers <= 0 && snapshot.TotalRequestedWorkers > 0)
        {
            return "执事层尚未落令";
        }

        var constrainedCount = Definitions.Count(definition =>
            snapshot.RequestedWorkersByTask[definition.TaskType] > snapshot.ResolvedWorkersByTask[definition.TaskType]);

        return constrainedCount switch
        {
            0 => "执事层落实稳定",
            <= 2 => "执事层部分受限",
            _ => "执事层整体承压"
        };
    }

    private static string BuildJobExecutionText(
        IEnumerable<SectTaskDefinition> tasks,
        SectTaskResolutionSnapshot snapshot)
    {
        var constrained = tasks.Count(definition =>
            snapshot.RequestedWorkersByTask[definition.TaskType] > snapshot.ResolvedWorkersByTask[definition.TaskType]);

        return constrained == 0 ? "执事落实稳定" : "执事落实受限";
    }

    private static string GetDirectiveDomainText(SectTaskType taskType)
    {
        return taskType switch
        {
            SectTaskType.FieldDuty => "峰内供养",
            SectTaskType.WorkshopDuty => "营造建设",
            SectTaskType.LogisticsPatrol => "护山戒律",
            SectTaskType.ScriptureStudy => "传承研修",
            SectTaskType.SectCommerce => "峰内内务",
            SectTaskType.OuterTrade => "对外外务",
            _ => "峰务治理"
        };
    }

    private static string BuildCapacityFormula(GameState state, JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Agriculture)} {state.AgricultureBuildings}×{IndustryRules.ProductionPerAgricultureBuilding} + {SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Workshop)} {state.WorkshopBuildings}×{IndustryRules.ProductionPerWorkshopBuilding} = {IndustryRules.GetProductionCapacity(state)}",
            JobType.Worker => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Administration)} {state.AdministrationBuildings}×{IndustryRules.ManagementPerBuilding} = {IndustryRules.GetManagementCapacity(state)}",
            JobType.Merchant => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Trade)} {state.TradeBuildings}×{IndustryRules.CommercePerBuilding} = {IndustryRules.GetCommerceCapacity(state)}",
            JobType.Scholar => $"{SectMapSemanticRules.GetBuildingDisplayName(IndustryBuildingType.Research)} {state.ResearchBuildings}×{IndustryRules.ResearchPerBuilding} = {IndustryRules.GetResearchCapacity(state)}",
            _ => "容量 0"
        };
    }

    private static void ApplyDevelopmentDirectionPreset(GameState state, Dictionary<string, int> orders)
    {
        switch (SectGovernanceRules.GetActiveDevelopmentDirection(state))
        {
            case SectDevelopmentDirectionType.SupplyFirst:
                AdjustOrder(orders, SectTaskType.FieldDuty, 2);
                AdjustOrder(orders, SectTaskType.WorkshopDuty, 1);
                AdjustOrder(orders, SectTaskType.ScriptureStudy, -1);
                AdjustOrder(orders, SectTaskType.OuterTrade, -1);
                break;
            case SectDevelopmentDirectionType.DoctrineFirst:
                AdjustOrder(orders, SectTaskType.ScriptureStudy, 2);
                AdjustOrder(orders, SectTaskType.WorkshopDuty, 1);
                AdjustOrder(orders, SectTaskType.FieldDuty, -1);
                AdjustOrder(orders, SectTaskType.SectCommerce, -1);
                break;
            case SectDevelopmentDirectionType.DefenseFirst:
                AdjustOrder(orders, SectTaskType.LogisticsPatrol, 2);
                AdjustOrder(orders, SectTaskType.WorkshopDuty, 1);
                AdjustOrder(orders, SectTaskType.OuterTrade, -1);
                break;
            case SectDevelopmentDirectionType.OutreachFirst:
                AdjustOrder(orders, SectTaskType.SectCommerce, 1);
                AdjustOrder(orders, SectTaskType.OuterTrade, 2);
                AdjustOrder(orders, SectTaskType.FieldDuty, -1);
                AdjustOrder(orders, SectTaskType.LogisticsPatrol, -1);
                break;
        }
    }

    private static void AdjustOrder(Dictionary<string, int> orders, SectTaskType taskType, int delta)
    {
        var key = GetStorageKey(taskType);
        var current = orders.TryGetValue(key, out var value) ? value : 0;
        orders[key] = Math.Clamp(current + delta, 0, MaxOrderUnitsPerTask);
    }
}
